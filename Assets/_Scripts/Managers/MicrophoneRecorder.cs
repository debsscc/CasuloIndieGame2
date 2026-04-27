using UnityEngine;
using UnityEngine.Events;

// Captura áudio do microfone do jogador.
// Toggle: aperta para começar, aperta de novo (ou tempo esgota) para parar.
public class MicrophoneRecorder : MonoBehaviour
{
    [Header("Configurações de Gravação")]
    [Tooltip("Limite máximo de gravação em segundos. Configurável por NPC/quest via SetRecordingLimit().")]
    [SerializeField] private float recordingLimit = 5f;
    [SerializeField] private KeyCode recordKey = KeyCode.E;
    [SerializeField] private int sampleRate = 44100;

    [Header("Eventos")]
    public UnityEvent OnRecordingStarted;
    public UnityEvent<AudioClip> OnRecordingFinished;

    public bool IsRecording { get; private set; }
    public float RecordingLimit => recordingLimit;

    private float recordingTimer;
    private string micDevice;
    private AudioClip micClip;

    private void Start()
    {
        if (Microphone.devices.Length == 0)
        {
            Debug.LogWarning("[MicrophoneRecorder] Nenhum microfone encontrado.");
            return;
        }
        micDevice = Microphone.devices[0];
    }

    private void Update()
    {
        if (Input.GetKeyDown(recordKey))
        {
            if (!IsRecording)
                StartRecording();
            else
                StopRecording();
        }

        if (IsRecording)
        {
            recordingTimer += Time.deltaTime;
            if (recordingTimer >= recordingLimit)
                StopRecording();
        }
    }

    public void StartRecording()
    {
        if (Microphone.devices.Length == 0) return;

        IsRecording = true;
        recordingTimer = 0f;
        micClip = Microphone.Start(micDevice, false, Mathf.CeilToInt(recordingLimit), sampleRate);
        OnRecordingStarted?.Invoke();
        Debug.Log("[MicrophoneRecorder] Gravação iniciada.");
    }

    public void StopRecording()
    {
        if (!IsRecording) return;

        IsRecording = false;

        int samplesGravados = Mathf.CeilToInt(recordingTimer * sampleRate);
        Microphone.End(micDevice);

        // Recorta só o trecho gravado para não ter silêncio no final
        AudioClip clipFinal = AudioClip.Create("PlayerSound", samplesGravados, 1, sampleRate, false);
        float[] samples = new float[samplesGravados];
        micClip.GetData(samples, 0);
        clipFinal.SetData(samples, 0);

        Debug.Log($"[MicrophoneRecorder] Gravação finalizada. Duração: {recordingTimer:F2}s");
        OnRecordingFinished?.Invoke(clipFinal);
    }

    // Configura o limite de gravação (chamado por NPC ou sistema de quests).
    public void SetRecordingLimit(float seconds)
    {
        recordingLimit = Mathf.Max(0.5f, seconds);
    }
}
