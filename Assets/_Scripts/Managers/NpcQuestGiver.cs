//----------------------------------------------------------------
// CRIADO EM: 2026-05
// DESCRIÇÃO: Localizado no GameObject do NPC.
// Ele gerencia o fluxo: abrir quest → jogador grava → avalia → sucesso ou nova tentativa.
// ----------------------------------------------------------------

using UnityEngine;
using UnityEngine.Events;

public class NpcQuestGiver : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private SoundMatchChecker soundMatchChecker;
    [SerializeField] private QuestUI questUI;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private AudioPlayback audioPlayback;
    [SerializeField] private FootstepPlayer footstepPlayer;

    [Header("Quest")]
    [SerializeField] private QuestSO quest;

    [Header("Eventos")]
    public UnityEvent OnQuestStarted;
    public UnityEvent OnQuestCompleted;   // sucesso — avança diálogo, toca animação, etc.
    public UnityEvent OnQuestFailed;      // falhou — mostra hint, permite nova tentativa

    private bool questActive = false;

    // ----------------------------------------------------------------
    // Chamado pelo jogador ao interagir (PlayerInteracion, trigger, botão, etc.)
    // ----------------------------------------------------------------
    public void Interact()
    {
        if (quest == null)
        {
            Debug.LogWarning("[NpcQuestGiver] Nenhuma quest atribuída.");
            return;
        }

        if (!questActive)
            StartQuest();
    }

    // Chamado pelo NpcInteractionTrigger quando o player entra na área
    public void ShowPreview()
    {
        if (quest == null) return;
        questUI?.Show(quest.questTitle, quest.description);
    }

    // Chamado pelo NpcInteractionTrigger quando o player sai sem interagir
    public void HidePreview()
    {
        if (!questActive)
            questUI?.Hide();
    }

    private void StartQuest()
    {
        questActive = true;

        soundMatchChecker.SetActiveQuest(quest);
        soundMatchChecker.micRecorder.OnRecordingStarted.AddListener(OnRecordingStarted);
        soundMatchChecker.micRecorder.OnRecordingFinished.AddListener(OnRecordingFinished);
        // painel já está aberto pelo ShowPreview — não reabre

        soundMatchChecker.OnMatchSuccess.AddListener(HandleSuccess);
        soundMatchChecker.OnMatchFail.AddListener(HandleFail);

        OnQuestStarted?.Invoke();
        Debug.Log($"[NpcQuestGiver] Quest iniciada: {quest.questTitle}");
    }

    private void OnRecordingStarted()
    {
        playerMovement?.SetMovementBlocked(true);
    }

    private void OnRecordingFinished()
    {
        playerMovement?.SetMovementBlocked(false);
    }

    private void HandleSuccess(float score)
    {
        questActive = false;
        Unsubscribe();

        // Toca o som gravado de volta para o jogador ouvir
        audioPlayback?.PlayLastRecording();

        // Entrega o clip para o sistema de passos
        var clip = soundMatchChecker.micRecorder.LastRecordedClip;
        footstepPlayer?.SetFootstepClip(clip);

        questUI?.ShowSuccess(score);
        questUI?.HideAfterFeedback();

        Debug.Log($"[NpcQuestGiver] Quest concluída! Score: {score:P0}");
        OnQuestCompleted?.Invoke();
    }

    private void HandleFail(float score)
    {
        questUI?.ShowFail(score);
        questUI?.ShowHint(quest.hint);

        Debug.Log($"[NpcQuestGiver] Errou. Score: {score:P0}. Hint exibido.");
        OnQuestFailed?.Invoke();
    }

    // ----------------------------------------------------------------
    // Cancela a quest (ex.: jogador saiu da área do NPC)
    // ----------------------------------------------------------------
    public void CancelQuest()
    {
        if (!questActive) return;

        questActive = false;
        Unsubscribe();
        soundMatchChecker.ClearActiveQuest();

        questUI?.Hide();
        Debug.Log("[NpcQuestGiver] Quest cancelada.");
    }

    private void Unsubscribe()
    {
        soundMatchChecker.OnMatchSuccess.RemoveListener(HandleSuccess);
        soundMatchChecker.OnMatchFail.RemoveListener(HandleFail);
        soundMatchChecker.micRecorder.OnRecordingStarted.RemoveListener(OnRecordingStarted);
        soundMatchChecker.micRecorder.OnRecordingFinished.RemoveListener(OnRecordingFinished);
        playerMovement?.SetMovementBlocked(false);
    }

    private void OnDisable() => CancelQuest();
}
