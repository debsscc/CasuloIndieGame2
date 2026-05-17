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

    [Header("Debug")]
    [Tooltip("Reseta o estado salvo desta quest ao iniciar o Play Mode (só para testes)")]
    [SerializeField] private bool resetQuestOnStart = false;

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

        if (string.IsNullOrEmpty(npcId))
            Debug.LogWarning("[NpcQuestGiver] npcId está vazio! Defina um ID único no Inspector para salvar o estado corretamente.", this);

        // Restaura estado salvo
        if (!string.IsNullOrEmpty(npcId))
        {
            if (resetQuestOnStart)
                PlayerPrefs.DeleteKey(SavePrefix + npcId);

            questCompleted = PlayerPrefs.GetInt(SavePrefix + npcId, 0) == 1;
        }
    }

    public bool IsCompleted => questCompleted;

    // ----------------------------------------------------------------
    // Chamado pelo jogador ao interagir (PlayerInteracion, trigger, botão, etc.)
    // ----------------------------------------------------------------
    public void Interact()
    {
        Debug.Log($"[NpcQuestGiver] Interact() chamado. questActive={questActive} questCompleted={questCompleted} quest={quest?.questTitle ?? "NULL"}", this);

        if (quest == null)
        {
            Debug.LogError("[NpcQuestGiver] quest (QuestSO) é NULL. Atribua no Inspector do NPC1.prefab.", this);
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
    // Não abre mais o painel automaticamente — o painel só abre ao pressionar E
    public void ShowPreview() { }

    // Chamado pelo NpcInteractionTrigger quando o player sai sem interagir
    public void HidePreview()
    {
        if (!questActive)
            questUI?.Hide();
    }

    private void StartQuest()
    {
        questActive = true;

        // Auto-resolve questUI se ainda null
        if (questUI == null) questUI = QuestUI.Instance;
        if (questUI == null)
            Debug.LogError("[NpcQuestGiver] QuestUI é NULL. Verifique se QuestUI existe na cena.", this);

        questUI?.Show(quest.questTitle, quest.description);
        Debug.Log($"[NpcQuestGiver] StartQuest: abrindo painel '{quest.questTitle}'. questUI={questUI}, soundMatchChecker={soundMatchChecker}", this);

        if (soundMatchChecker == null)
        {
            Debug.LogError("[NpcQuestGiver] soundMatchChecker é NULL. Atribua no Inspector do NPC1.prefab.", this);
            return;
        }

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
