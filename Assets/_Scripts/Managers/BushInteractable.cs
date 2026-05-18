// ----------------------------------------------------------------
// CRIADO EM: 2026-05
// FEITO POR: Debs Carvalho
// DESCRIÇÃO: Arbusto interagível. O player aperta E quando em range e as frutinhas caem.
//            Funciona como IInteractable, registrado no PlayerInteracion.
//            Requer: Collider2D (trigger) para zona de interação.
// ----------------------------------------------------------------

using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BushInteractable : MonoBehaviour, IInteractable
{
    [Header("Frutas")]
    [Tooltip("Prefab do FruitItem que será instanciado ao interagir")]
    [SerializeField] private GameObject fruitItemPrefab;
    [SerializeField] private int fruitsToSpawn = 5;
    [Tooltip("Raio de dispersão ao redor do arbusto")]
    [SerializeField] private float spawnRadius = 0.5f;
    [Tooltip("Força do impulso aplicado a cada fruta ao spawnar")]
    [SerializeField] private float spawnForce = 3.5f;

    [Header("Visual")]
    [Tooltip("GameObject com o prompt 'Aperte E' (opcional)")]
    [SerializeField] private GameObject interactPrompt;
    [Tooltip("Animator do arbusto para a animação de tremida (opcional)")]
    [SerializeField] private Animator bushAnimator;
    [Tooltip("Nome do trigger de tremida no Animator")]
    [SerializeField] private string shakeTrigger = "Shake";

    private bool playerInRange    = false;
    private bool hasBeenHarvested = false;

    // ── Trigger de proximidade ────────────────────────────────────

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;

        var playerInteraction = other.GetComponentInParent<PlayerInteracion>();
        playerInteraction?.SetInteractableInRange(this);

        RefreshPrompt();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;

        var playerInteraction = other.GetComponentInParent<PlayerInteracion>();
        playerInteraction?.ClearInteractableInRange(this);

        if (interactPrompt != null)
            interactPrompt.SetActive(false);
    }

    private void RefreshPrompt()
    {
        if (interactPrompt == null) return;

        bool questActive = FruitQuestManager.Instance != null
                           && FruitQuestManager.Instance.IsActive
                           && !hasBeenHarvested;

        interactPrompt.SetActive(playerInRange && questActive);
    }

    // ── IInteractable ─────────────────────────────────────────────

    public void Interact()
    {
        var manager = FruitQuestManager.Instance;
        if (manager == null || !manager.IsActive || hasBeenHarvested) return;

        hasBeenHarvested = true;

        if (interactPrompt != null)
            interactPrompt.SetActive(false);

        // Tremida do arbusto
        if (bushAnimator != null && !string.IsNullOrEmpty(shakeTrigger))
            bushAnimator.SetTrigger(shakeTrigger);

        SpawnFruits();
    }

    public void Cancel() { }

    // ── Spawn das frutas ──────────────────────────────────────────

    private void SpawnFruits()
    {
        if (fruitItemPrefab == null)
        {
            Debug.LogWarning("[BushInteractable] fruitItemPrefab não atribuído! Atribua o prefab FruitItem no Inspector.", this);
            return;
        }

        for (int i = 0; i < fruitsToSpawn; i++)
        {
            // Posição inicial no topo do arbusto com pequena dispersão horizontal
            float offsetX = Random.Range(-spawnRadius, spawnRadius);
            Vector2 spawnPos = (Vector2)transform.position + new Vector2(offsetX, 0.3f);

            GameObject fruit = Instantiate(fruitItemPrefab, spawnPos, Quaternion.identity);

            var rb = fruit.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // Impulso em arco (para cima e para os lados)
                Vector2 impulse = new Vector2(
                    Random.Range(-1f, 1f),
                    Random.Range(1.5f, 3f)
                ) * spawnForce;

                rb.AddForce(impulse, ForceMode2D.Impulse);
            }
        }
    }
}
