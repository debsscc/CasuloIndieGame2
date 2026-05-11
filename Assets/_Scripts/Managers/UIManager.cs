//----------------------------------------------------------------
// CRIADO EM: 2026-05
// DESCRIÇÃO: Gerencia os painéis de UI (pause, diário, mapa). Singleton. O painel de quest é controlado pelo NpcQuestGiverSÓ
// ----------------------------------------------------------------

using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Painéis")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject diarioPanel;
    [SerializeField] private GameObject mapaPanel;
    // QuestPanel não entra aqui — é controlado pelo NpcQuestGiver

    [Header("Sons")]
    [SerializeField] private AudioSource uiAudioSource;
    [SerializeField] private AudioClip diarioOpenSound;

    private GameObject currentOpenPanel;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start(){
        if (pausePanel   != null) pausePanel.SetActive(false);
        if (diarioPanel  != null) diarioPanel.SetActive(false);
        if (mapaPanel    != null) mapaPanel.SetActive(false);
        currentOpenPanel = null;
    }

    private void OpenPanel(GameObject panel){
        CloseAll();
        panel.SetActive(true);
        currentOpenPanel = panel;
    }

    private void ClosePanel(GameObject panel){
        panel.SetActive(false);
        if (currentOpenPanel == panel) currentOpenPanel = null;
    }
    public void TogglePause(){
        if (currentOpenPanel == pausePanel) {
            ClosePanel(pausePanel);
            Time.timeScale = 1f;
        }
        else {
            OpenPanel(pausePanel);
            Time.timeScale = 0f;
        }
    }
    public void ToggleDiario(){
        if (currentOpenPanel == diarioPanel) ClosePanel(diarioPanel);
        else
        {
            OpenPanel(diarioPanel);
            if (uiAudioSource != null && diarioOpenSound != null)
                uiAudioSource.PlayOneShot(diarioOpenSound);
        }
    }
    public void ToggleMapa(){
        if (currentOpenPanel == mapaPanel) ClosePanel(mapaPanel);
        else OpenPanel(mapaPanel);
    }

    public void CloseAll(){
        if (currentOpenPanel != null)
            currentOpenPanel.SetActive(false);
        currentOpenPanel = null;
    }
}
