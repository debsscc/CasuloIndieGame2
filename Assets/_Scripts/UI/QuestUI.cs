//-----------------------------------------------------------------
// CRIADO EM: 2026-05
// DESCRIÇÃO: UI para mostrar título, descrição, feedback de sucesso/falha e hints da quest.
// ----------------------------------------------------------------

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class QuestUI : MonoBehaviour
{
    [Header("Painel")]
    [SerializeField] private RectTransform panelRect;

    [Header("Textos")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text hintText;
    [SerializeField] private GameObject hintPanel;

    [Header("Feedback")]
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private Image feedbackBackground;
    [SerializeField] private Color successColor = new Color(0.2f, 0.8f, 0.3f, 0.9f);
    [SerializeField] private Color failColor    = new Color(0.9f, 0.2f, 0.2f, 0.9f);

    [Header("Animação")]
    [SerializeField] private float panelAnimDuration = 0.25f;
    [SerializeField] private float typewriterSpeed   = 0.03f;  // segundos por caractere
    [SerializeField] private float feedbackDuration  = 1.8f;

    private CanvasGroup canvasGroup;
    private Coroutine typewriterCoroutine;
    private Coroutine feedbackCoroutine;
    private Coroutine panelCoroutine;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha          = 0f;
        canvasGroup.interactable   = false;
        canvasGroup.blocksRaycasts = false;

        if (panelRect == null) panelRect = GetComponent<RectTransform>();
        if (hintPanel != null) hintPanel.SetActive(false);
        if (feedbackBackground != null) feedbackBackground.gameObject.SetActive(false);
    }

    // ----------------------------------------------------------------
    // API pública — chamada pelo NpcQuestGiver
    // ----------------------------------------------------------------

    public void Show(string title, string description)
    {
        if (titleText != null)  titleText.text = title;
        if (hintPanel != null)  hintPanel.SetActive(false);
        if (feedbackBackground != null) feedbackBackground.gameObject.SetActive(false);

        StartTypewriter(descriptionText, description);
        StartPanelAnim(open: true);
    }

    public void ShowHint(string hint)
    {
        if (hintPanel != null) hintPanel.SetActive(true);
        StartTypewriter(hintText, hint);
    }

    //mensagenzinha de sucesso
    public void ShowSuccess(float score)
    {
        string msg = score >= 0.9f ? "Perfeito!" : score >= 0.75f ? "Muito bom!" : "Conseguiu!";
        ShowFeedback(msg + $"  {score:P0}", successColor);
    }

    //mensagenzinha de falha e core
    public void ShowFail(float score)
    {
        ShowFeedback($"Quase! Tente de novo  {score:P0}", failColor);
    }

    public void Hide()
    {
        StartPanelAnim(open: false);
    }

    // Espera o feedback terminar, depois fecha o painel
    public void HideAfterFeedback()
    {
        StartCoroutine(HideAfterFeedbackRoutine());
    }

    private IEnumerator HideAfterFeedbackRoutine()
    {
        // 0.15 fade in + feedbackDuration + 0.25 fade out
        yield return new WaitForSeconds(0.15f + feedbackDuration + 0.25f);
        Hide();
    }

    // ----------------------------------------------------------------
    // Internos
    // ----------------------------------------------------------------

    private void StartTypewriter(TMP_Text target, string text)
    {
        if (target == null) return;
        if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
        typewriterCoroutine = StartCoroutine(TypewriterRoutine(target, text));
    }

    private IEnumerator TypewriterRoutine(TMP_Text target, string fullText)
    {
        target.text = "";
        foreach (char c in fullText)
        {
            target.text += c;
            yield return new WaitForSeconds(typewriterSpeed);
        }
        typewriterCoroutine = null;
    }

    private void ShowFeedback(string message, Color color)
    {
        if (feedbackBackground == null || feedbackText == null) return;

        if (feedbackCoroutine != null) StopCoroutine(feedbackCoroutine);
        feedbackCoroutine = StartCoroutine(FeedbackRoutine(message, color));
    }

    private IEnumerator FeedbackRoutine(string message, Color color)
    {
        feedbackText.text             = message;
        feedbackBackground.color      = color;
        feedbackBackground.gameObject.SetActive(true);

        // Fade in
        yield return FadeImage(feedbackBackground, 0f, color.a, 0.15f);
        yield return new WaitForSeconds(feedbackDuration);
        // Fade out
        yield return FadeImage(feedbackBackground, color.a, 0f, 0.25f);

        feedbackBackground.gameObject.SetActive(false);
        feedbackCoroutine = null;
    }

    //animacao de abrir/fechar o painel da quest
    private void StartPanelAnim(bool open)
    {
        if (panelCoroutine != null) StopCoroutine(panelCoroutine);
        panelCoroutine = StartCoroutine(PanelAnimRoutine(open));
    }

    //anima o alpha e escale p ser tipo um "POP" apareci :)
    private IEnumerator PanelAnimRoutine(bool open)
    {
        float fromAlpha = canvasGroup.alpha;
        float toAlpha   = open ? 1f : 0f;
        Vector3 fromScale = panelRect.localScale;
        Vector3 toScale   = open ? Vector3.one : new Vector3(0.85f, 0.85f, 1f);

        if (open)
        {
            panelRect.localScale           = new Vector3(0.85f, 0.85f, 1f);
            canvasGroup.interactable       = false;
            canvasGroup.blocksRaycasts     = false;
        }
        //a variavel elapsed é nativa da unity e serve pra contar o tempo que passou desde o início da animação, e a gente usa ela pra calcular o progresso da animação (de 0 a 1) e aplicar uma curva de suavização com Mathf.SmoothStep.
        float elapsed = 0f;
        while (elapsed < panelAnimDuration)
        {
            elapsed += Time.deltaTime;
            //aq o mathf.smoothstep p elapsed! 
            //suaviza a interpolação da animação
            float t = Mathf.SmoothStep(0f, 1f, elapsed / panelAnimDuration);
            canvasGroup.alpha        = Mathf.Lerp(fromAlpha, toAlpha, t);
            panelRect.localScale     = Vector3.Lerp(fromScale, toScale, t);
            yield return null;
        }

        canvasGroup.alpha      = toAlpha;
        panelRect.localScale   = toScale;

        if (open)
        {
            canvasGroup.interactable   = true;
            canvasGroup.blocksRaycasts = true;
        }
        else
        {
            canvasGroup.interactable   = false;
            canvasGroup.blocksRaycasts = false;
        }

        panelCoroutine = null;
    }

    //fade de imagem usada pro feedback de sucesso/falha do quest
    private IEnumerator FadeImage(Image img, float from, float to, float duration)
    {
        float elapsed = 0f;
        Color c = img.color;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(from, to, elapsed / duration);
            img.color = c;
            yield return null;
        }
        c.a = to;
        img.color = c;
    }
}
