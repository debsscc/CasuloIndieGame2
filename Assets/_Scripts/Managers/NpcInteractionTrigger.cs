//------------------------
// Author: Debs Carvalho
// Data: 2026-05
// Description: Component to handle NPC interaction triggers;
//------------------------

using UnityEngine;
using UnityEngine.Events;


[RequireComponent(typeof(Collider2D), typeof(NpcController))]
public class NpcInteractionTrigger : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private GameObject interactPrompt; // ex.: balão "Aperte E"
    [SerializeField] private Collider2D triggerCollider;

    [Header("Eventos")]
    public UnityEvent OnPlayerEntered;
    public UnityEvent OnPlayerLeft;

    private NpcController npcController;

    private void Awake()
    {
        npcController = GetComponent<NpcController>();

        if (triggerCollider == null)
        {
            foreach (var col in GetComponents<Collider2D>())
            {
                if (col.isTrigger) { triggerCollider = col; break; }
            }

            if (triggerCollider == null)
                Debug.LogWarning("[NpcInteractionTrigger] Nenhum Collider2D trigger encontrado.", this);
        }

        if (interactPrompt != null)
            interactPrompt.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        var interaction = other.GetComponentInParent<PlayerInteracion>();
        if (interaction == null)
            Debug.LogError("[NpcInteractionTrigger] PlayerInteracion não encontrado no player!", this);
        else
            interaction.SetNpcInRange(npcController);

        if (interactPrompt != null) interactPrompt.SetActive(true);
        OnPlayerEntered?.Invoke();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        other.GetComponentInParent<PlayerInteracion>()?.ClearNpcInRange(npcController);

        if (interactPrompt != null) interactPrompt.SetActive(false);
        OnPlayerLeft?.Invoke();
    }
}
