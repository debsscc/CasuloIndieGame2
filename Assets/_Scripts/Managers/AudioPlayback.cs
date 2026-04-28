using UnityEngine;
using UnityEngine.Events;

// Reproduz de volta o AudioClip gravado pelo MicrophoneRecorder.
public class AudioPlayback : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private MicrophoneRecorder micRecorder;

    [Header("Eventos")]
    public UnityEvent OnPlaybackStarted;
    public UnityEvent<AudioClip> OnPlaybackFinished;

    private AudioSource audioSource;

    private AudioClip currentClip;

    private void Awake()
    {
        // Cria um AudioSource dedicado para playback, separado do AudioSource
        // do MicrophoneRecorder que fica no mesmo GameObject
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // Auto-resolve a referência se não foi atribuída no Inspector.
        if (micRecorder == null)
            micRecorder = GetComponent<MicrophoneRecorder>();
    }

    // Chamado pelo OnRecordingFinished via Inspector
    public void PlayLastRecording()
    {
        if (micRecorder != null)
            PlayRecordedAudio(micRecorder.LastRecordedClip);
    }

    public void PlayRecordedAudio(AudioClip clip)
    {
        if (clip == null) return;

        currentClip = clip;
        audioSource.clip = clip;
        audioSource.Play();
        OnPlaybackStarted?.Invoke();
        Debug.Log("[AudioPlayback] Reproduzindo gravação...");

        StartCoroutine(WaitForPlaybackEnd(clip.length));
    }

    private System.Collections.IEnumerator WaitForPlaybackEnd(float duration)
    {
        yield return new WaitForSeconds(duration);
        OnPlaybackFinished?.Invoke(currentClip);
        Debug.Log("[AudioPlayback] Reprodução finalizada.");
    }

    public AudioClip GetCurrentClip() => currentClip;
}
