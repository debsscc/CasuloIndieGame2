using UnityEngine;
using UnityEngine.InputSystem;

// Coloque no DataManager ou num GameObject dedicado em ----MANAGERS----.
// Lê o Escape e coordena UIManager + PlayerInputHandler.
public class PauseController : MonoBehaviour
{
    [SerializeField] private PlayerInputHandler playerInputHandler;

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            Toggle();
    }

    public void Toggle()
    {
        UIManager.Instance.TogglePause();

        bool isPaused = Time.timeScale == 0f;
        playerInputHandler?.SetPaused(isPaused);
    }

    // Chamável por botão "Resume" do PausePanel no Inspector
    public void Resume()
    {
        if (Time.timeScale == 0f)
            Toggle();
    }

    // Chamável por botão "Quit" do PausePanel no Inspector
    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
