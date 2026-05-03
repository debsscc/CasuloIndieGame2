//----------------------------------------------------------------
// CRIADO EM: 2026-05
// DESCRIÇÃO: Recebe o sinal de interação do PlayerInputHandler e repassa para o NPC em range.
// ----------------------------------------------------------------

using UnityEngine;


[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerInteracion : MonoBehaviour
{
    private PlayerInputHandler inputHandler;
    private NpcQuestGiver currentNpc;

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
    public void SetNpcInRange(NpcQuestGiver npc) => currentNpc = npc;

    // Chamado pelo NpcInteractionTrigger quando o player sai da área
    public void ClearNpcInRange(NpcQuestGiver npc)
    {
        if (currentNpc == npc)
        {
            currentNpc.CancelQuest();
            currentNpc = null;
        }
    }

    private void HandleInteract()
    {
        currentNpc?.Interact();
    }
}
