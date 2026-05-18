// ----------------------------------------------------------------
// CRIADO EM: 2026-05
// FEITO POR: Debs Carvalho
// DESCRIÇÃO: Frutinha coletável que spawna do arbusto.
//            Cai com física (Rigidbody2D + gravidade), depois de um delay
//            habilita o trigger de coleta — player passa por cima e coleta.
//
//            Setup do prefab:
//              - SpriteRenderer (sprite da fruta)
//              - Rigidbody2D  (gravityScale ~2, collision detection: Continuous)
//              - CircleCollider2D (isTrigger=false, para física com o chão)
//              - CircleCollider2D (isTrigger=true,  para coleta pelo player) ← assign em collectTrigger
// ----------------------------------------------------------------

using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class FruitItem : MonoBehaviour
{
    [Header("Coleta")]
    [Tooltip("Collider trigger exclusivo para coleta (separado do collider de física)")]
    [SerializeField] private Collider2D collectTrigger;
    [Tooltip("Tempo após spawn antes de poder ser coletada (evita coleta instantânea)")]
    [SerializeField] private float collectDelay = 0.6f;
    [Tooltip("Tempo máximo até a fruta ser destruída se não coletada")]
    [SerializeField] private float autoDestroyTime = 25f;

    // ── Ciclo de vida ────────────────────────────────────────────

    private void Start()
    {
        // O trigger de coleta começa desativado
        if (collectTrigger != null)
            collectTrigger.enabled = false;

        StartCoroutine(EnableCollectionAfterDelay());
        Destroy(gameObject, autoDestroyTime);
    }

    private IEnumerator EnableCollectionAfterDelay()
    {
        yield return new WaitForSeconds(collectDelay);

        if (collectTrigger != null)
            collectTrigger.enabled = true;
    }

    // ── Coleta ────────────────────────────────────────────────────

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        Collect();
    }

    private void Collect()
    {
        FruitQuestManager.Instance?.CollectFruit();
        Destroy(gameObject);
    }
}
