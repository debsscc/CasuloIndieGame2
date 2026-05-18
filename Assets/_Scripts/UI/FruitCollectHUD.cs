// ----------------------------------------------------------------
// CRIADO EM: 2026-05
// FEITO POR: Debs Carvalho
// DESCRIÇÃO: HUD simples que mostra o contador de frutinhas coletadas (X/5).
//            Aparece quando a quest está ativa e some quando concluída.
//
//            Setup: adicione este script a um GameObject de UI que contenha
//            um TMP_Text. Conecte os eventos do FruitQuestManager nos campos
//            OnFruitCollected → FruitCollectHUD.UpdateCount
//            OnReadyToDeliver → FruitCollectHUD.ShowReadyMessage
//            OnQuestCompleted → FruitCollectHUD.Hide
// ----------------------------------------------------------------

using UnityEngine;
using TMPro;

public class FruitCollectHUD : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private TMP_Text counterText;
    [Tooltip("Texto exibido quando o player coletou todas as frutas")]
    [SerializeField] private TMP_Text statusText;

    [Header("Mensagens")]
    [SerializeField] private string collectingFormat = "Frutinhas: {0}/{1}";
    [SerializeField] private string readyMessage     = "Volte ao NPC! 🍒";

    // ── Ciclo de vida ─────────────────────────────────────────────

    private void Awake()
    {
        if (hudPanel != null)
            hudPanel.SetActive(false);

        if (statusText != null)
            statusText.gameObject.SetActive(false);
    }

    private void Start()
    {
        // Se a quest já estava ativa (cena recarregada), exibe o HUD
        var manager = FruitQuestManager.Instance;
        if (manager == null) return;

        if (manager.IsActive)
            UpdateCount(manager.FruitsCollected);
        else if (manager.IsReadyToDeliver)
            ShowReadyMessage();
    }

    // ── API (conectar no Inspector via UnityEvent do FruitQuestManager) ──

    /// <summary>Atualiza o contador. Conecte ao FruitQuestManager.OnFruitCollected.</summary>
    public void UpdateCount(int current)
    {
        if (hudPanel != null)
            hudPanel.SetActive(true);

        if (counterText != null)
        {
            int required = FruitQuestManager.Instance != null
                ? FruitQuestManager.Instance.FruitsRequired
                : 5;

            counterText.text = string.Format(collectingFormat, current, required);
        }

        if (statusText != null)
            statusText.gameObject.SetActive(false);
    }

    /// <summary>Mostra mensagem de "volte ao NPC". Conecte ao FruitQuestManager.OnReadyToDeliver.</summary>
    public void ShowReadyMessage()
    {
        if (hudPanel != null)
            hudPanel.SetActive(true);

        if (statusText != null)
        {
            statusText.text = readyMessage;
            statusText.gameObject.SetActive(true);
        }

        if (counterText != null)
            counterText.gameObject.SetActive(false);
    }

    /// <summary>Esconde o HUD. Conecte ao FruitQuestManager.OnQuestCompleted.</summary>
    public void Hide()
    {
        if (hudPanel != null)
            hudPanel.SetActive(false);
    }
}
