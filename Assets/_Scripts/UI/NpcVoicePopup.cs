// ----------------------------------------------------------------
// CRIADO EM: 2026-05
// DESCRIÇÃO: Popup "DAR A VOZ A NPC". Gerencia 3 estados:
//   Ready     → mostra instrução + botão de gravar + botão cancelar
//   Recording → gravação ativa (2 s automáticos), wave reagindo ao mic
//   Recorded  → mostra botões: Ouvir | Regravar | Confirmar
//
// Animação pop idêntica à QuestUI (SmoothStep alpha + scale).
// Connecte os eventos a PlayerMovement / FootstepPlayer / etc. no Inspector.
// ----------------------------------------------------------------

using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class NpcVoicePopup : MonoBehaviour
{
    public static NpcVoicePopup Instance { get; private set; }

    // ── Refs ──────────────────────────────────────────────────────
    [Header("Refs")]
    [SerializeField] private MicrophoneRecorder micRecorder;
    [SerializeField] private AudioPlayback      audioPlayback;
    [SerializeField] private UIWaveVisualizer   waveVisualizer;

    // ── Painel / animação ─────────────────────────────────────────
    [Header("Painel")]
    [SerializeField] private RectTransform panelRect;
    [SerializeField] private float         animDuration = 0.25f;

    // ── Grupos de UI por estado ───────────────────────────────────
    [Header("Grupos de estado")]
    [Tooltip("Visível nos estados Ready e Recording (botão de gravar + instrução)")]
    [SerializeField] private GameObject readyGroup;
    [Tooltip("Overlay / indicador opcional mostrado SÓ durante Recording")]
    [SerializeField] private GameObject recordingOverlay;
    [Tooltip("Visível no estado Recorded (Ouvir | Regravar | Confirmar)")]
    [SerializeField] private GameObject recordedGroup;

    // ── Gravação ──────────────────────────────────────────────────
    [Header("Gravação")]
    [Tooltip("Duração fixa da gravação em segundos")]
    [SerializeField] private float recordingDuration = 2f;

    // ── Eventos públicos ──────────────────────────────────────────
    [Header("Eventos")]
    public UnityEvent<AudioClip> OnVoiceConfirmed;   // wire: FootstepPlayer.SetFootstepClip
    public UnityEvent             OnPopupCanceled;    // wire: NpcVoiceGiver

    // ── Estado interno ────────────────────────────────────────────
    private enum State { Hidden, Ready, Recording, Recorded }
    private State state = State.Hidden;

    private CanvasGroup canvasGroup;
    private Coroutine   panelCoroutine;
    private AudioClip   lastClip;

    // =============================================================

    private void Awake()
    {
        Instance = this;

        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha          = 0f;
        canvasGroup.interactable   = false;
        canvasGroup.blocksRaycasts = true;  // bloqueia interação com o mundo desde o início

        if (panelRect == null) panelRect = GetComponent<RectTransform>();

        ApplyGroupsForState(State.Hidden);
    }

    // ── API pública ───────────────────────────────────────────────

    // Chamado pelo NpcVoiceGiver para abrir o popup
    public void OpenPopup()
    {
        if (state != State.Hidden) return;
        SetState(State.Ready);
        StartPanelAnim(open: true);
    }

    // Botão de gravar — wira no Inspector ao Button.onClick
    public void OnRecordButtonPressed()
    {
        if (state != State.Ready && state != State.Recorded) return;
        lastClip = null;
        StartRecording();
    }

    // Botão de ouvir
    public void OnPlaybackButtonPressed()
    {
        if (state != State.Recorded || lastClip == null) return;
        audioPlayback?.PlayRecordedAudio(lastClip);
    }

    // Botão de regravar
    public void OnRerecordButtonPressed()
    {
        if (state != State.Recorded) return;
        lastClip = null;
        SetState(State.Ready);
    }

    // Botão de confirmar
    public void OnConfirmButtonPressed()
    {
        if (state != State.Recorded || lastClip == null) return;

        var clip = lastClip;
        ClosePopup();
        OnVoiceConfirmed?.Invoke(clip);
    }

    // Botão de cancelar
    public void OnCancelButtonPressed()
    {
        if (state == State.Hidden) return;

        // Se estiver gravando, garante parada limpa
        if (state == State.Recording && micRecorder != null && micRecorder.IsRecording)
        {
            micRecorder.OnRecordingFinished.RemoveListener(HandleRecordingFinished);
            micRecorder.StopRecording();
        }

        ClosePopup();
        OnPopupCanceled?.Invoke();
    }

    // ── Fluxo interno ─────────────────────────────────────────────

    private void StartRecording()
    {
        if (micRecorder == null)
        {
            Debug.LogWarning("[NpcVoicePopup] MicrophoneRecorder não atribuído.");
            return;
        }

        micRecorder.SetRecordingLimit(recordingDuration);
        micRecorder.OnRecordingFinished.AddListener(HandleRecordingFinished);
        micRecorder.StartRecording();

        SetState(State.Recording);
    }

    // Chamado pelo MicrophoneRecorder quando a gravação termina (pelo timer interno dele)
    private void HandleRecordingFinished()
    {
        micRecorder.OnRecordingFinished.RemoveListener(HandleRecordingFinished);

        if (state != State.Recording) return;

        lastClip = micRecorder.LastRecordedClip;
        SetState(State.Recorded);
    }

    private void ClosePopup()
    {
        SetState(State.Hidden);
        StartPanelAnim(open: false);
        waveVisualizer?.ResetBars();
    }

    private void SetState(State newState)
    {
        state = newState;
        ApplyGroupsForState(newState);
    }

    private void ApplyGroupsForState(State s)
    {
        bool isReady     = s == State.Ready;
        bool isRecording = s == State.Recording;
        bool isRecorded  = s == State.Recorded;

        // readyGroup fica visível tanto em Ready quanto em Recording
        if (readyGroup      != null) readyGroup.SetActive(isReady || isRecording);
        if (recordingOverlay != null) recordingOverlay.SetActive(isRecording);
        if (recordedGroup   != null) recordedGroup.SetActive(isRecorded);
    }

    // ── Animação pop (idêntica à QuestUI) ─────────────────────────

    private void StartPanelAnim(bool open)
    {
        if (panelCoroutine != null) StopCoroutine(panelCoroutine);
        panelCoroutine = StartCoroutine(PanelAnimRoutine(open));
    }

    private IEnumerator PanelAnimRoutine(bool open)
    {
        float   fromAlpha = canvasGroup.alpha;
        float   toAlpha   = open ? 1f : 0f;
        Vector3 fromScale = panelRect.localScale;
        Vector3 toScale   = open ? Vector3.one : new Vector3(0.85f, 0.85f, 1f);

        if (open)
        {
            panelRect.localScale       = new Vector3(0.85f, 0.85f, 1f);
            fromScale                  = panelRect.localScale; // corrige fromScale após reset
            canvasGroup.interactable   = true;  // botões clicáveis imediatamente
            canvasGroup.blocksRaycasts = true;  // já era true desde Awake
        }

        float elapsed = 0f;
        while (elapsed < animDuration)
        {
            elapsed          += Time.deltaTime;
            float t           = Mathf.SmoothStep(0f, 1f, elapsed / animDuration);
            canvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, t);
            panelRect.localScale = Vector3.Lerp(fromScale, toScale, t);
            yield return null;
        }

        canvasGroup.alpha    = toAlpha;
        panelRect.localScale = toScale;

        if (!open)
        {
            // ao fechar: desbloqueia o mundo
            canvasGroup.interactable   = false;
            canvasGroup.blocksRaycasts = false;
        }
        // ao abrir: interactable/blocksRaycasts já foram setados no início da animação

        panelCoroutine = null;
    }
}
