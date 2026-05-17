// ----------------------------------------------------------------
// CRIADO EM: 2026-05
// DESCRIÇÃO: Menu de ações que flutua acima do NPC quando o player pressiona E.
//
// Setup na cena:
//   1. Adicione este script + CanvasGroup a um GameObject filho do Canvas
//   2. Atribua menuPanel (o RectTransform do painel/bolha visual)
//   3. Arraste os botões já existentes na hierarquia:
//      btnSair, btnConversar, btnDarVoz
//   4. Os botões são mostrados/escondidos conforme o estado do NPC —
//      NÃO são criados dinamicamente, apenas ativados/desativados.
//   5. O painel flutua acima do NPC automaticamente.
// ----------------------------------------------------------------

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class NpcActionMenu : MonoBehaviour
{
    public static NpcActionMenu Instance { get; private set; }

    // ── Refs ──────────────────────────────────────────────────────
    [Header("Painel")]
    [Tooltip("RectTransform do painel/bolha — filho deste GameObject")]
    [SerializeField] private RectTransform menuPanel;

    [Header("Botões (arraste da hierarquia)")]
    [SerializeField] private Button btnSair;
    [SerializeField] private Button btnConversar;
    [SerializeField] private Button btnDarVoz;

    // ── Posição ───────────────────────────────────────────────────
    [Header("Posição")]
    [Tooltip("Deslocamento em unidades de mundo acima do pivot do NPC")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.8f, 0f);

    // ── Animação ──────────────────────────────────────────────────
    [Header("Animação")]
    [SerializeField] private float animDuration = 0.2f;

    // ── Estado interno ────────────────────────────────────────────
    private CanvasGroup canvasGroup;
    private Canvas      parentCanvas;
    private Camera      cam;
    private Transform   trackedNpc;
    private bool        isOpen;
    private Coroutine   animCoroutine;
    private NpcController currentNpc;

    // =============================================================

    private void Awake()
    {
        Instance = this;

        canvasGroup  = GetComponent<CanvasGroup>();
        parentCanvas = GetComponentInParent<Canvas>();
        cam          = Camera.main;

        canvasGroup.alpha          = 0f;
        canvasGroup.interactable   = true;
        canvasGroup.blocksRaycasts = false;  // invisível → não bloqueia

        if (menuPanel != null)
            menuPanel.localScale = new Vector3(0.85f, 0.85f, 1f);
    }

    private void LateUpdate()
    {
        if (!isOpen || trackedNpc == null) return;
        UpdatePosition();
    }

    // ── API pública ───────────────────────────────────────────────

    /// <summary>
    /// Abre o menu acima do NPC. Mostra os botões de acordo com o estado do NPC.
    /// </summary>
    public void ShowMenu(NpcController npc)
    {
        if (isOpen) CloseImmediate();

        currentNpc = npc;
        trackedNpc = npc.transform;

        // Configura visibilidade dos botões
        SetButtonVisible(btnDarVoz,    !npc.HasVoice && npc.HasVoiceGiver);
        SetButtonVisible(btnConversar,  npc.HasVoice);
        SetButtonVisible(btnSair,       true);

        // Registra callbacks (limpa listeners velhos antes)
        SetupButton(btnSair,      npc.OnMenuSair);
        SetupButton(btnConversar, npc.OnMenuConversar);
        SetupButton(btnDarVoz,    npc.OnMenuDarVoz);

        isOpen = true;
        canvasGroup.blocksRaycasts = true;
        UpdatePosition();
        PlayAnim(open: true);
    }

    /// <summary>Fecha o menu com animação.</summary>
    public void Close()
    {
        if (!isOpen) return;
        isOpen                     = false;
        trackedNpc                 = null;
        currentNpc                 = null;
        canvasGroup.blocksRaycasts = false;
        PlayAnim(open: false);
    }

    // ── Internos ──────────────────────────────────────────────────

    private void CloseImmediate()
    {
        isOpen     = false;
        trackedNpc = null;
        currentNpc = null;
        if (animCoroutine != null) { StopCoroutine(animCoroutine); animCoroutine = null; }
        canvasGroup.alpha          = 0f;
        canvasGroup.blocksRaycasts = false;
    }

    private static void SetButtonVisible(Button btn, bool visible)
    {
        if (btn != null) btn.gameObject.SetActive(visible);
    }

    private static void SetupButton(Button btn, System.Action callback)
    {
        if (btn == null) return;
        btn.onClick.RemoveAllListeners();
        if (callback != null) btn.onClick.AddListener(() => callback());
    }

    private void UpdatePosition()
    {
        if (trackedNpc == null || menuPanel == null) return;
        if (cam == null) cam = Camera.main;
        if (cam == null) return;
        if (parentCanvas == null) parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas == null) return;

        Vector3 screenPos = cam.WorldToScreenPoint(trackedNpc.position + worldOffset);
        if (screenPos.z < 0f) { menuPanel.gameObject.SetActive(false); return; }
        menuPanel.gameObject.SetActive(true);

        // World point no plano do canvas — funciona em qualquer renderMode
        // e independe de onde menuPanel está na hierarquia
        Camera uiCam = parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : cam;
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                parentCanvas.transform as RectTransform,
                screenPos,
                uiCam,
                out Vector3 worldPoint))
        {
            menuPanel.position = worldPoint;
        }
    }

    private void PlayAnim(bool open)
    {
        if (animCoroutine != null) StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(AnimRoutine(open));
    }

    private IEnumerator AnimRoutine(bool open)
    {
        float   fromAlpha = canvasGroup.alpha;
        float   toAlpha   = open ? 1f : 0f;
        Vector3 fromScale = menuPanel != null ? menuPanel.localScale : Vector3.one;
        Vector3 toScale   = open ? Vector3.one : new Vector3(0.85f, 0.85f, 1f);

        if (open && menuPanel != null)
        {
            menuPanel.localScale = new Vector3(0.85f, 0.85f, 1f);
            fromScale            = menuPanel.localScale;
        }

        float elapsed = 0f;
        while (elapsed < animDuration)
        {
            elapsed          += Time.deltaTime;
            float t           = Mathf.SmoothStep(0f, 1f, elapsed / animDuration);
            canvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, t);
            if (menuPanel != null)
                menuPanel.localScale = Vector3.Lerp(fromScale, toScale, t);
            yield return null;
        }

        canvasGroup.alpha = toAlpha;
        if (menuPanel != null) menuPanel.localScale = toScale;

        animCoroutine = null;
    }
}
