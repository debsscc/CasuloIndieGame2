//----------------------------------------------------------------
// CRIADO EM: 2026-05
// FEITO POR: Debs Carvalho
// DESCRIÇÃO: Recebe o sinal de interação do PlayerInputHandler e repassa para o NPC em range.
// ----------------------------------------------------------------

using UnityEngine;


[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerInteracion : MonoBehaviour
{
    private PlayerInputHandler inputHandler;
    private NpcController currentNpc;

    private void Awake()
    {
        inputHandler = GetComponent<PlayerInputHandler>();
    }

    private void OnEnable()
    {
        inputHandler.OnInteractInput += HandleInteract;
    }

    private void OnDisable()
    {
        inputHandler.OnInteractInput -= HandleInteract;
    }

    // Chamado pelo NpcInteractionTrigger quando o player entra na área
    public void SetNpcInRange(NpcController npc)
    {
        currentNpc = npc;
        Debug.Log($"[PlayerInteracion] NPC em range: {npc?.name}");
    }

    // Chamado pelo NpcInteractionTrigger quando o player sai da área
    public void ClearNpcInRange(NpcController npc)
    {
        if (currentNpc == npc)
        {
            currentNpc.Cancel();
            currentNpc = null;
        }
    }

    private void HandleInteract()
    {
        // Não abre o menu se o balão de diálogo estiver ativo
        if (NpcSpeechBubble.Instance != null && NpcSpeechBubble.Instance.IsVisible) return;

        // Não abre o menu se o popup de voz estiver aberto
        if (NpcVoicePopup.Instance != null && NpcVoicePopup.Instance.IsOpen) return;

        string npcName = currentNpc != null ? currentNpc.name : "NULL";
        Debug.Log($"[PlayerInteracion] HandleInteract chamado. currentNpc={npcName}");
        currentNpc?.Interact();
    }
}
