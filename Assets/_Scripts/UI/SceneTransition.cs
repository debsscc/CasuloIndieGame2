///* ----------------------------------------------------------------
// Controla o fade in/out para transição entre cenas.
//            Requer um CanvasGroup com uma imagem preta cobrindo a tela.
// ---------------------------------------------------------------- */

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : Singleton<SceneTransition>
{
    [SerializeField] private CanvasGroup fadeCanvas;
    [SerializeField] private float fadeDuration = 0.5f;

    protected override void Awake()
    {
        base.Awake();
        if (fadeCanvas != null)
        {
            fadeCanvas.alpha = 1f;
            fadeCanvas.blocksRaycasts = true;
        }
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FadeIn();
    }

    public void FadeIn()
    {
        StartCoroutine(FadeRoutine(1f, 0f, blockAfter: false));
    }

    public void FadeOutAndLoad(string sceneName)
    {
        StartCoroutine(FadeOutLoadRoutine(sceneName));
    }

    private IEnumerator FadeOutLoadRoutine(string sceneName)
    {
        yield return FadeRoutine(0f, 1f, blockAfter: true);
        SceneManager.LoadScene(sceneName);
        //toda vez que uma cena é carregada, o método OnSceneLoaded é chamado, e ele automaticamente inicia o fade in. 
    }

    private IEnumerator FadeRoutine(float from, float to, bool blockAfter)
    {
        if (fadeCanvas == null) yield break;

        fadeCanvas.blocksRaycasts = true;
        fadeCanvas.alpha = from;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeCanvas.alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
            yield return null;
        }

        fadeCanvas.alpha = to;
        fadeCanvas.blocksRaycasts = blockAfter;
    }
}
