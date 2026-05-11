//----------------------------------------------------------------
// CRIADO EM: 2026-05
// DESCRIÇÃO: Localizado no GameObject do NPC.
// Ele gerencia o fluxo: abrir quest → jogador grava → avalia → sucesso ou nova tentativa.
// ----------------------------------------------------------------

using UnityEngine;
using UnityEngine.Events;

public class NpcQuestGiver : MonoBehaviour
{
    [Header("Identidade")]
    [SerializeField] private string npcId; // único por NPC, ex: "NPC_Floresta_01"

    [Header("Refs")]
    [SerializeField] private SoundMatchChecker soundMatchChecker;
    [SerializeField] private QuestUI questUI; // opcional: auto-resolve via QuestUI.Instance

    [Header("Quest")]
    [SerializeField] private QuestSO quest;

    [Header("Eventos")]
    public UnityEvent OnQuestStarted;
    public UnityEvent OnRecordingBegan;              // grave: bloqueie o player aqui
    public UnityEvent<AudioClip> OnQuestCompleted;   // passa o clip gravado aos ouvintes
    public UnityEvent OnQuestFailed;                 // libere o player aqui
    public UnityEvent OnQuestAlreadyDone;            // opcional: reação visual quando já concluída

    private bool questActive = false;
    private bool questCompleted = false;

    private static readonly string SavePrefix = "Quest_";

    private void Awake()
    {
        if (questUI == null)
            questUI = QuestUI.Instance;

        // Restaura estado salvo
        if (!string.IsNullOrEmpty(npcId))
            questCompleted = PlayerPrefs.GetInt(SavePrefix + npcId, 0) == 1;
    }

    public bool IsCompleted => questCompleted;

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

        if (questCompleted)
        {
            OnQuestAlreadyDone?.Invoke();
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
        soundMatchChecker.OnMatchSuccess.AddListener(HandleSuccess);
        soundMatchChecker.OnMatchFail.AddListener(HandleFail);

        OnQuestStarted?.Invoke();
        Debug.Log($"[NpcQuestGiver] Quest iniciada: {quest.questTitle}");
    }

    private void OnRecordingStarted()
    {
        OnRecordingBegan?.Invoke();
    }

    private void HandleSuccess(float score)
    {
        questActive = false;
        questCompleted = true;

        if (!string.IsNullOrEmpty(npcId))
            PlayerPrefs.SetInt(SavePrefix + npcId, 1);

        var clip = soundMatchChecker.micRecorder.LastRecordedClip;
        Unsubscribe();

        questUI?.ShowSuccess(score);
        questUI?.HideAfterFeedback();

        Debug.Log($"[NpcQuestGiver] Quest concluída! Score: {score:P0}");
        OnQuestCompleted?.Invoke(clip); // ouvintes recebem o clip: FootstepPlayer, AudioPlayback, etc.
    }

    private void HandleFail(float score)
    {
        questUI?.ShowFail(score);
        questUI?.ShowHint(quest.hint);

        Debug.Log($"[NpcQuestGiver] Errou. Score: {score:P0}. Hint exibido.");
        OnQuestFailed?.Invoke(); // ouvinte: PlayerMovement.SetMovementBlocked(false)
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
    }

    private void OnDisable() => CancelQuest();
}
