// ----------------------------------------------------------------
// CRIADO EM: 2026-05
// FEITO POR: Debs Carvalho
// DESCRIÇÃO: Singleton que gerencia o estado da quest de coletar frutinhas para o NPC2.
//            Estados: 0=NãoIniciada, 1=Ativa, 2=ProntaParaEntregar, 3=Concluída
// ----------------------------------------------------------------

using UnityEngine;
using UnityEngine.Events;

public class FruitQuestManager : MonoBehaviour
{
    public static FruitQuestManager Instance { get; private set; }

    [Header("Config")]
    [SerializeField] private int fruitsRequired = 5;
    [SerializeField] private string saveKey = "FruitQuest_NPC2";

    [Header("Debug")]
    [Tooltip("Reseta o estado salvo ao entrar no Play Mode (só para testes)")]
    [SerializeField] private bool resetOnStart = false;

    [Header("Eventos")]
    public UnityEvent<int> OnFruitCollected;   // passa a contagem atual
    public UnityEvent OnReadyToDeliver;         // quando coletou todas as frutas
    public UnityEvent OnQuestCompleted;         // após entregar ao NPC

    // Estado: 0=NãoIniciada | 1=Ativa | 2=ProntaParaEntregar | 3=Concluída
    private int questState = 0;
    private int fruitsCollected = 0;

    public int QuestState        => questState;
    public int FruitsCollected   => fruitsCollected;
    public int FruitsRequired    => fruitsRequired;
    public bool IsActive         => questState == 1;
    public bool IsReadyToDeliver => questState == 2;
    public bool IsCompleted      => questState == 3;

    // ── Ciclo de vida ────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (resetOnStart)
        {
            PlayerPrefs.DeleteKey(saveKey);
            PlayerPrefs.DeleteKey(saveKey + "_fruits");
        }

        questState      = PlayerPrefs.GetInt(saveKey, 0);
        fruitsCollected = PlayerPrefs.GetInt(saveKey + "_fruits", 0);
    }

    // ── API pública ──────────────────────────────────────────────

    /// <summary>Inicia a quest (chamado via Yarn <<start_fruit_quest>>).</summary>
    public void StartQuest()
    {
        if (questState != 0)
        {
            Debug.Log("[FruitQuestManager] StartQuest ignorado: quest já iniciada ou concluída.", this);
            return;
        }

        questState      = 1;
        fruitsCollected = 0;
        Save();
        Debug.Log("[FruitQuestManager] Quest de frutinhas iniciada!");
    }

    /// <summary>Registra a coleta de uma fruta pelo player.</summary>
    public void CollectFruit()
    {
        if (questState != 1) return;

        fruitsCollected++;
        Save();
        OnFruitCollected?.Invoke(fruitsCollected);
        Debug.Log($"[FruitQuestManager] Fruta coletada: {fruitsCollected}/{fruitsRequired}");

        if (fruitsCollected >= fruitsRequired)
        {
            questState = 2;
            Save();
            OnReadyToDeliver?.Invoke();
            Debug.Log("[FruitQuestManager] Todas as frutas coletadas! Volte ao NPC2.");
        }
    }

    /// <summary>Conclui a quest após entregar as frutas ao NPC (chamado via Yarn <<complete_fruit_quest>>).</summary>
    public void CompleteQuest()
    {
        if (questState != 2)
        {
            Debug.LogWarning("[FruitQuestManager] CompleteQuest chamado fora de hora (estado não é 2).", this);
            return;
        }

        questState      = 3;
        fruitsCollected = 0;
        Save();
        OnQuestCompleted?.Invoke();
        Debug.Log("[FruitQuestManager] Quest de frutinhas concluída!");
    }

    // ── Persistência ─────────────────────────────────────────────

    private void Save()
    {
        PlayerPrefs.SetInt(saveKey, questState);
        PlayerPrefs.SetInt(saveKey + "_fruits", fruitsCollected);
        PlayerPrefs.Save();
    }
}
