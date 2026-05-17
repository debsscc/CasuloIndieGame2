// ----------------------------------------------------------------
// CRIADO EM: 2026-05
// DESCRIÇÃO: Cérebro central de cada NPC. Recebe o Interact() do player,
//   abre o NpcActionMenu com as opções corretas para o estado atual do NPC
//   e roteia cada escolha para o sistema correspondente.
//
// Estados:
//   sem voz → botões: "Dar a Voz" (se tem voiceGiver) + "Sair"
//   com voz → botões: "Conversar" + "Sair"
//
// Setup:
//   1. Adicione NpcController ao NPC (junto com NpcInteractionTrigger)
//   2. Se o NPC tem mecânica de voz, atribua voiceGiver
//   3. Preencha yarnConversarNode com o nó Yarn para "Conversar"
//   4. Após a voz ser confirmada, chame SetHasVoice(true) — wire via NpcVoiceGiver
// ----------------------------------------------------------------

using UnityEngine;
using UnityEngine.Events;

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

    // ── Eventos ───────────────────────────────────────────────────
    [Header("Eventos")]
    public UnityEvent OnVoiceReceived;

    // =============================================================

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

        // TODO: integrar YarnSpinner com posição do NPC
        // YarnDialogueRunner.Instance?.StartDialogue(yarnConversarNode, transform);
        Debug.Log($"[NpcController] Iniciar Yarn: '{yarnConversarNode}'");
    }

    public void OnMenuSair()
    {
        NpcActionMenu.Instance?.Close();
    }
}
