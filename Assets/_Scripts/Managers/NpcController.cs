// ----------------------------------------------------------------
// CRIADO EM: 2026-05
// DESCRIÇÃO: Cérebro central de cada NPC. Gerencia o estado de voz, recebe o sinal de interação do player e aciona o menu de ações (NpcActionMenu) e o diálogo (Yarn DialogueRunner +
// FEITO POR: Debs Carvalho 
// ----------------------------------------------------------------

using UnityEngine;
using UnityEngine.Events;
using Yarn.Unity;

public class NpcController : MonoBehaviour
{
    // ── Estado ────────────────────────────────────────────────────
    [Header("Estado")]
    [Tooltip("Começa sem voz? Marque false se o NPC já tem voz desde o início")]
    [SerializeField] private bool hasVoice = false;

    // ── Dar a Voz ─────────────────────────────────────────────────
    [Header("Dar a Voz")]
    [Tooltip("Referência ao NpcVoiceGiver deste NPC (se aplicável)")]
    [SerializeField] private NpcVoiceGiver voiceGiver;

    // ── Diálogo ───────────────────────────────────────────────────
    [Header("Diálogo (Yarn)")]
    [Tooltip("Nó Yarn iniciado ao escolher 'Conversar'")]
    [SerializeField] private string yarnConversarNode;
    [Tooltip("DialogueRunner da cena (auto-buscado se não atribuído)")]
    [SerializeField] private DialogueRunner dialogueRunner;

    // ── Eventos ───────────────────────────────────────────────────
    [Header("Eventos")]
    public UnityEvent OnVoiceReceived;

    // =============================================================

    private void Awake()
    {
        if (voiceGiver == null)
            voiceGiver = GetComponent<NpcVoiceGiver>();
    }

    /// <summary>Chamado pelo PlayerInteracion quando o player pressiona E perto deste NPC.</summary>
    public void Interact()
    {
        var menu = NpcActionMenu.Instance;
        if (menu == null)
        {
            Debug.LogError("[NpcController] NpcActionMenu.Instance é null. Certifique-se que o prefab está na cena.", this);
            return;
        }

        menu.ShowMenu(this);
    }

    /// <summary>Cancela qualquer estado ativo (player saiu do range).</summary>
    public void Cancel()
    {
        NpcActionMenu.Instance?.Close();
    }

    /// <summary>
    /// Define o estado de voz do NPC.
    /// Wire NpcVoiceGiver.OnVoiceConfirmed → NpcController.SetHasVoice(true) via código,
    /// ou chame diretamente no HandleConfirmed.
    /// </summary>
    public void SetHasVoice(bool value)
    {
        hasVoice = value;
        if (value) OnVoiceReceived?.Invoke();
        Debug.Log($"[NpcController] {name} → hasVoice={value}");
    }

    public bool HasVoice      => hasVoice;
    public bool HasVoiceGiver => voiceGiver != null;

    // ── Callbacks do menu (chamados pelo NpcActionMenu) ───────────

    public void OnMenuDarVoz()
    {
        NpcActionMenu.Instance?.Close();

        if (voiceGiver == null)
        {
            Debug.LogWarning($"[NpcController] {name}: voiceGiver não atribuído.", this);
            return;
        }
        voiceGiver.TryOpenPopup();
    }

    public void OnMenuConversar()
    {
        NpcActionMenu.Instance?.Close();

        if (string.IsNullOrEmpty(yarnConversarNode))
        {
            Debug.LogWarning($"[NpcController] {name}: yarnConversarNode vazio.", this);
            return;
        }

        if (dialogueRunner == null)
            dialogueRunner = FindAnyObjectByType<DialogueRunner>();

        if (dialogueRunner == null)
        {
            Debug.LogError("[NpcController] DialogueRunner não encontrado na cena.", this);
            return;
        }

        NpcSpeechBubble.Instance?.TrackNpc(transform);
        dialogueRunner.StartDialogue(yarnConversarNode);
    }

    public void OnMenuSair()
    {
        NpcActionMenu.Instance?.Close();
    }
}
