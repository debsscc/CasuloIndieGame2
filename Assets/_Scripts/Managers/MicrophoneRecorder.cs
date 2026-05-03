using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

// Captura áudio do microfone do jogador.
// Toggle: aperta para começar, aperta de novo (ou tempo esgota) para parar.
[RequireComponent(typeof(AudioSource))]
public class MicrophoneRecorder : MonoBehaviour
{
    [Header("Configurações de Gravação")]
    [Tooltip("Limite máximo de gravação em segundos. Configurável por NPC/quest via SetRecordingLimit().")]
    [SerializeField] private float recordingLimit = 5f;
    [SerializeField] private int sampleRate = 44100;
    private const Key recordKey = Key.E;

    [Header("Eventos")]
    public UnityEvent OnRecordingStarted;
    public UnityEvent OnRecordingFinished;

    // Habilitado pelo SoundMatchChecker quando uma quest está ativa
    public bool AllowRecording { get; set; } = false;
    public bool IsRecording { get; private set; }
    public AudioClip LastRecordedClip { get; private set; }
    public AudioSource MicAudioSource { get; private set; }
    public AudioClip MicClip => micClip;
    public string MicDeviceName => micDevice;
    public float RecordingLimit => recordingLimit;

    private float recordingTimer;
    private string micDevice;
    private AudioClip micClip;
    private int recordingStartSample;

    // Duração do buffer de loop (deve ser maior que qualquer recordingLimit possível)
    private const int loopBufferSeconds = 60;

    private void Start()
    {
        // Usa o AudioSource já existente no GameObject (o mesmo que o AudioVisualizer referencia na cena).
        MicAudioSource = GetComponent<AudioSource>();
        MicAudioSource.playOnAwake = false;
        MicAudioSource.loop = true;
        MicAudioSource.volume = 0.001f;

        if (Microphone.devices.Length == 0)
        {
            Debug.LogWarning("[MicrophoneRecorder] Nenhum microfone encontrado.");
            return;
        }
        micDevice = Microphone.devices[0];

        micClip = Microphone.Start(micDevice, true, loopBufferSeconds, sampleRate);
        MicAudioSource.clip = micClip;
        StartCoroutine(PlayWhenMicReady());
    }

    private System.Collections.IEnumerator PlayWhenMicReady()
    {
        // Espera até o microfone ter amostras reais (GetPosition > 0 ou -1 indica não iniciado)
        yield return new WaitUntil(() => Microphone.GetPosition(micDevice) > 0);
        MicAudioSource.Play();
        Debug.Log("[MicrophoneRecorder] AudioSource do mic iniciado.");
    }

    private void Update()
    {
        if (AllowRecording && Keyboard.current != null && Keyboard.current[recordKey].wasPressedThisFrame)
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
        if (IsRecording || Microphone.devices.Length == 0) return;

        IsRecording = true;
        recordingTimer = 0f;
        // Apenas marca a posição inicial no buffer — sem chamar Microphone.Start() de novo
        recordingStartSample = Microphone.GetPosition(micDevice);
        OnRecordingStarted?.Invoke();
        Debug.Log("[MicrophoneRecorder] Gravação iniciada.");
    }

    public void StopRecording()
    {
        if (!IsRecording) return;

        IsRecording = false;

        int samplesGravados = Mathf.CeilToInt(recordingTimer * sampleRate);
        samplesGravados = Mathf.Min(samplesGravados, loopBufferSeconds * sampleRate);

        // Extrai os samples gravados do buffer circular
        AudioClip clipFinal = AudioClip.Create("PlayerSound", samplesGravados, 1, sampleRate, false);
        float[] samples = new float[samplesGravados];
        micClip.GetData(samples, recordingStartSample);
        clipFinal.SetData(samples, 0);

        LastRecordedClip = clipFinal;
        Debug.Log($"[MicrophoneRecorder] Gravação finalizada. Duração: {recordingTimer:F2}s");
        OnRecordingFinished?.Invoke();
    }

    // Configura o limite de gravação (chamado por NPC ou sistema de quests).
    public void SetRecordingLimit(float seconds)
    {
        recordingLimit = Mathf.Max(0.5f, seconds);
    }
}
