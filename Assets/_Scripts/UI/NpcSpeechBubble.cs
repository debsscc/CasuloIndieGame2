#nullable enable
// ----------------------------------------------------------------
// CRIADO EM: 2026-05
// FEITO POR: Debs Carvalho
// DESCRIÇÃO: Balão de diálogo único que segue o NPC falante no mundo.
// ----------------------------------------------------------------

using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using Yarn.Unity;

public class NpcSpeechBubble : DialoguePresenterBase
{
    public static NpcSpeechBubble? Instance { get; private set; }

    // ── UI ────────────────────────────────────────────────────────
    [Header("UI")]
    [Tooltip("Painel/bolha raiz (filho deste GO)")]
    [SerializeField] private RectTransform? bubblePanel;
    [Tooltip("Texto do nome do personagem )")]
    [SerializeField] private TextMeshProUGUI? speakerText;
    [Tooltip("Texto da linha de diálogo")]
    [SerializeField] private TextMeshProUGUI? lineText;
    [Tooltip("Ícone 'pressione para continuar'")]
    [SerializeField] private GameObject? continueIndicator;

    // ── Posição ───────────────────────────────────────────────────
    [Header("Posição")]
    [Tooltip("Deslocamento em unidades de mundo acima do NPC")]
    //o 3.5f é a altura da bolha em relação ao pivot do NPC, ajustado para ficar acima da cabeça
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 5.5f, 0f);

    // ── Typewriter ────────────────────────────────────────────────
    [Header("Typewriter")]
    [Tooltip("Segundos entre cada caractere")]
    [SerializeField] private float typeSpeed = 0.04f;

    // ── Referências ───────────────────────────────────────────────
    [Header("Referências")]
    [Tooltip("DialogueRunner da cena (auto-buscado se null)")]
    [SerializeField] private DialogueRunner? dialogueRunner;
    [Tooltip("PlayerMovement para bloquear/desbloquear ao conversar")]
    [SerializeField] private PlayerMovement? playerMovement;

    // ── Estado interno ────────────────────────────────────────────
    private Canvas?    parentCanvas;
    private Camera?    cam;
    private Transform? trackedNpc;
    private bool       isVisible;

    public bool IsVisible => isVisible;

    // =============================================================

    private void Awake()
    {
        Instance     = this;
        parentCanvas = GetComponentInParent<Canvas>();
        cam          = Camera.main;

        if (dialogueRunner == null)
            dialogueRunner = FindAnyObjectByType<DialogueRunner>();

        if (playerMovement == null)
            playerMovement = FindAnyObjectByType<PlayerMovement>();

        if (bubblePanel != null) bubblePanel.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!isVisible || dialogueRunner == null) return;

        var kb = Keyboard.current;
        if (kb == null) return;

        // E ou Espaço: apressar o typewriter OU avançar para a próxima linha
        if (kb.eKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame)
            dialogueRunner.RequestNextLine();
    }

    private void LateUpdate()
    {
        if (!isVisible || trackedNpc == null || bubblePanel == null) return;
        if (cam == null) cam = Camera.main;
        if (cam == null || parentCanvas == null) return;

        Vector3 screenPos = cam.WorldToScreenPoint(trackedNpc.position + worldOffset);
        if (screenPos.z < 0f) { bubblePanel.gameObject.SetActive(false); return; }
        bubblePanel.gameObject.SetActive(true);

        // Mesma técnica do NpcActionMenu: world point no plano do canvas
        Camera? uiCam = parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : cam;
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                parentCanvas.transform as RectTransform,
                screenPos, uiCam, out Vector3 worldPoint))
        {
            bubblePanel.position = worldPoint;
        }
    }

    // ── API pública ───────────────────────────────────────────────

    /// Define qual NPC o balão deve seguir.
    /// Chama ANTES de DialogueRunner.StartDialogue().
    public void TrackNpc(Transform npc)
    {
        trackedNpc = npc;
    }

    // ── DialoguePresenterBase ─────────────────────────────────────

    public override YarnTask OnDialogueStartedAsync()
    {
        isVisible = true;
        if (bubblePanel != null) bubblePanel.gameObject.SetActive(true);
        if (continueIndicator != null) continueIndicator.SetActive(false);
        playerMovement?.SetMovementBlocked(true);
        return YarnTask.CompletedTask;
    }

    public override YarnTask OnDialogueCompleteAsync()
    {
        isVisible  = false;
        trackedNpc = null;
        if (bubblePanel != null) bubblePanel.gameObject.SetActive(false);
        playerMovement?.SetMovementBlocked(false);
        return YarnTask.CompletedTask;
    }

    public override async YarnTask RunLineAsync(LocalizedLine line, LineCancellationToken token)
    {
        // Nome do personagem
        string speaker = line.CharacterName ?? "";
        if (speakerText != null)
        {
            speakerText.text = speaker;
            speakerText.gameObject.SetActive(!string.IsNullOrEmpty(speaker));
        }

        // Texto da linha (sem o prefixo "NPC:")
        string text = line.TextWithoutCharacterName.Text;

        if (continueIndicator != null) continueIndicator.SetActive(false);

        // Efeito typewriter — HurryUpToken pula direto ao fim
        if (lineText != null)
        {
            lineText.text = "";
            foreach (char c in text)
            {
                if (token.HurryUpToken.IsCancellationRequested)
                {
                    lineText.text = text;
                    break;
                }
                lineText.text += c;
                await YarnTask.Delay(
                    Mathf.RoundToInt(typeSpeed * 1000),
                    token.HurryUpToken)
                    .SuppressCancellationThrow();
            }
        }

        if (continueIndicator != null) continueIndicator.SetActive(true);

        // Aguarda player avançar (pressionar E / clicar no LineAdvancer)
        await YarnTask.WaitUntilCanceled(token.NextLineToken)
            .SuppressCancellationThrow();

        if (continueIndicator != null) continueIndicator.SetActive(false);
    }

    public override YarnTask<DialogueOption?> RunOptionsAsync(
        DialogueOption[] dialogueOptions,
        CancellationToken cancellationToken)
    {
        return YarnTask<DialogueOption?>.FromResult(null);
    }
}
