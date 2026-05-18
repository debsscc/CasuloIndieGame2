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

    // Interagíveis não-NPC (ex.: BushInteractable)
    private IInteractable currentInteractable;

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

    // Chamado por qualquer IInteractable (ex.: BushInteractable) quando o player entra na área
    public void SetInteractableInRange(IInteractable interactable)
    {
        currentInteractable = interactable;
    }

    // Chamado por qualquer IInteractable quando o player sai da área
    public void ClearInteractableInRange(IInteractable interactable)
    {
        if (currentInteractable == interactable)
        {
            currentInteractable.Cancel();
            currentInteractable = null;
        }
    }

    private void HandleInteract()
    {
        // Não abre o menu se o balão de diálogo estiver ativo
        if (NpcSpeechBubble.Instance != null && NpcSpeechBubble.Instance.IsVisible) return;

        // Não abre o menu se o popup de voz estiver aberto
        if (NpcVoicePopup.Instance != null && NpcVoicePopup.Instance.IsOpen) return;

        // NPC tem prioridade sobre outros interagíveis
        if (currentNpc != null)
        {
            Debug.Log($"[PlayerInteracion] HandleInteract → NPC: {currentNpc.name}");
            currentNpc.Interact();
            return;
        }

        if (currentInteractable != null)
        {
            Debug.Log($"[PlayerInteracion] HandleInteract → Interactable: {currentInteractable}");
            currentInteractable.Interact();
        }
    }
}
