using UnityEngine;
using UnityEngine.SceneManagement;

public class menuManager : MonoBehaviour
{
    private const string GameScene = "Game";

    // ─── Botões principais ───────────────────────────────────────────

    public void StartGame()
    {
        if (SceneTransition.instance != null)
            SceneTransition.instance.FadeOutAndLoad(GameScene);
        else
            SceneManager.LoadScene(GameScene);
    }

    public void OpenSettings()
    {
        // ativar painel de configurações
        Debug.Log("Configurações abertas.");
    }

    public void OpenCredits()
    {
        // ativar painel de créditos
        Debug.Log("Créditos abertos.");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
