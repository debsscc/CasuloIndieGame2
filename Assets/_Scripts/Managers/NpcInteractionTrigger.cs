using UnityEngine;
using UnityEngine.Events;

// O NPC deve ter DOIS Collider2D:
//   1) Um Collider2D normal (Is Trigger = false) → colisão física com o player
//   2) Um Collider2D com Is Trigger = true → zona de interação (atribua em "Trigger Collider")
// O jogador precisa ter a tag "Player" p q isso dê certo tb.
[RequireComponent(typeof(Collider2D), typeof(NpcQuestGiver))]
public class NpcInteractionTrigger : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private GameObject interactPrompt; // ex.: balão "Aperte E"
    [SerializeField] private Collider2D triggerCollider; // arraste aqui o Collider2D com Is Trigger = true

    [Header("Eventos")]
    public UnityEvent OnPlayerEntered; // wire: PlayerInteracion.SetNpcInRange (via Inspector)
    public UnityEvent OnPlayerLeft;    // wire: PlayerInteracion.ClearNpcInRange (via Inspector)

    private NpcQuestGiver questGiver;

    private void Awake()
    {
        questGiver = GetComponent<NpcQuestGiver>();

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

        Debug.Log($"[NpcInteractionTrigger] Player entrou no trigger. other={other.name}", this);

        var interaction = other.GetComponentInParent<PlayerInteracion>();
        if (interaction == null)
            Debug.LogError("[NpcInteractionTrigger] PlayerInteracion NAO encontrado no player! Verifique se o componente existe no GameObject do player.", this);
        else
            interaction.SetNpcInRange(questGiver);

        if (interactPrompt != null) interactPrompt.SetActive(true);
        OnPlayerEntered?.Invoke();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        other.GetComponentInParent<PlayerInteracion>()?.ClearNpcInRange(questGiver);

        if (interactPrompt != null) interactPrompt.SetActive(false);
        questGiver.HidePreview();
        OnPlayerLeft?.Invoke();
    }
}
