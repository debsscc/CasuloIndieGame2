// ----------------------------------------------------------------
// CRIADO EM: 2026-05
// DESCRIÇÃO: Controla o painel do diário do jogador.
// FEITO POR: Debs Carvalho
// ----------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DiaryPF : MonoBehaviour
{
    public static DiaryPF Instance { get; private set; }

    [Header("Entradas (arraste os DiaryEntrySO aqui)")]
    [SerializeField] private List<DiaryEntrySO> allEntries = new();

    [Header("Teste")]
    [Tooltip("Ativa para ver todas as entradas desbloqueadas no Play Mode")]
    [SerializeField] private bool unlockAllForTest = false;

    [Header("Painel Esquerdo — lista de entradas")]
    [SerializeField] private RectTransform entryListParent;

    [Header("Painel Direito — detalhe da entrada selecionada")]
    [SerializeField] private TMP_Text detailTitle;
    [SerializeField] private TMP_Text detailText;

    private const string SavePrefix = "Diary_";
    private readonly List<GameObject> spawnedButtons = new();

    // ----------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        // Adiciona VerticalLayoutGroup automaticamente no painel esquerdo
        if (entryListParent != null && entryListParent.GetComponent<VerticalLayoutGroup>() == null)
        {
            var vlg = entryListParent.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 8f;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.padding = new RectOffset(8, 8, 8, 8);
        }
    }

    private void OnEnable()
    {
        RefreshList();
    }

    // ----------------------------------------------------------------
    // API pública
    // ----------------------------------------------------------------

    /// <summary>
    /// Desbloqueia uma entrada pelo id. Chame por NPCs, triggers, etc.
    /// </summary>
    public void UnlockEntry(string entryId)
    {
        PlayerPrefs.SetInt(SavePrefix + entryId, 1);
        PlayerPrefs.Save();
        Debug.Log($"[DiaryPF] Entrada desbloqueada: {entryId}");
    }

    /// <summary>
    /// Atalho para desbloquear diretamente pelo SO (útil no Inspector / UnityEvent).
    /// </summary>
    public void UnlockEntry(DiaryEntrySO entry)
    {
        if (entry != null) UnlockEntry(entry.entryId);
    }

    // ----------------------------------------------------------------
    // Interno
    // ----------------------------------------------------------------

    private bool IsUnlocked(DiaryEntrySO entry) =>
        unlockAllForTest || PlayerPrefs.GetInt(SavePrefix + entry.entryId, 0) == 1;

    private void RefreshList()
    {
        foreach (var go in spawnedButtons)
            if (go != null) Destroy(go);
        spawnedButtons.Clear();

        ClearDetail();

        foreach (var entry in allEntries)
        {
            bool unlocked = IsUnlocked(entry);
            var btnGo = CreateEntryButton(entry.entryTitle, unlocked);

            if (unlocked)
            {
                var captured = entry;
                btnGo.GetComponent<Button>().onClick.AddListener(() => ShowDetail(captured));
            }

            spawnedButtons.Add(btnGo);
        }
    }

    // Cria um botão por código — sem precisar de prefab
    private GameObject CreateEntryButton(string label, bool unlocked)
    {
        var go = new GameObject(label, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(entryListParent, false);

        // Altura fixa via LayoutElement
        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 60f;
        le.flexibleWidth = 1f;

        // Fundo do botão: mais escuro se bloqueado
        go.GetComponent<Image>().color = unlocked
            ? new Color(0.15f, 0.15f, 0.15f, 0.85f)
            : new Color(0.08f, 0.08f, 0.08f, 0.4f);

        go.GetComponent<Button>().interactable = unlocked;

        // Texto do botão
        var textGo = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textGo.transform.SetParent(go.transform, false);
        var rt = textGo.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = new Vector2(-16f, 0f);
        rt.anchoredPosition = Vector2.zero;

        var tmp = textGo.GetComponent<TextMeshProUGUI>();
        tmp.text = unlocked ? label : "???";
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.fontSize = 24f;
        tmp.color = unlocked ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.6f);

        return go;
    }

    private void ShowDetail(DiaryEntrySO entry)
    {
        if (detailTitle != null) detailTitle.text = entry.entryTitle;
        if (detailText  != null) detailText.text  = entry.entryText;
    }

    private void ClearDetail()
    {
        if (detailTitle != null) detailTitle.text = string.Empty;
        if (detailText  != null) detailText.text  = string.Empty;
    }
}
