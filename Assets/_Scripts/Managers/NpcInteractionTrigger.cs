using UnityEngine;

// Coloque esse script no mesmo GameObject do NpcQuestGiver.
// Exige um Collider2D marcado como "Is Trigger" no mesmo GameObject.
// O jogador precisa ter a tag "Player".
[RequireComponent(typeof(Collider2D), typeof(NpcQuestGiver))]
public class NpcInteractionTrigger : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private GameObject interactPrompt; // ex.: balão "Aperte E"

    private NpcQuestGiver questGiver;

    private void Awake()
    {
        questGiver = GetComponent<NpcQuestGiver>();
        GetComponent<Collider2D>().isTrigger = true;

        if (interactPrompt != null)
            interactPrompt.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        if (interactPrompt != null) interactPrompt.SetActive(true);
        questGiver.ShowPreview();

        var interaction = other.GetComponent<PlayerInteracion>();
        if (interaction != null) interaction.SetNpcInRange(questGiver);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        if (interactPrompt != null) interactPrompt.SetActive(false);
        questGiver.HidePreview();

        var interaction = other.GetComponent<PlayerInteracion>();
        if (interaction != null) interaction.ClearNpcInRange(questGiver);
    }
}
