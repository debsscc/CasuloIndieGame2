using UnityEngine;
using UnityEngine.Events;

// Reproduz de volta o AudioClip gravado pelo MicrophoneRecorder.
[RequireComponent(typeof(AudioSource))]
public class AudioPlayback : MonoBehaviour
{
    [Header("Eventos")]
    public UnityEvent OnPlaybackStarted;
    public UnityEvent<AudioClip> OnPlaybackFinished;

    private AudioSource audioSource;
    private AudioClip currentClip;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    // Reproduz o clip gravado. Sempre conectado ao OnRecordingFinished do MicrophoneRecorder.
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
