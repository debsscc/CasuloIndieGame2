///* ----------------------------------------------------------------
// CRIADO EM: 2026-05
// DESCRIÇÃO: MonoBehaviour que escuta o OnRecordingFinished, roda o analyzer e dispara OnMatchSuccess ou OnMatchFail com o score
// ---------------------------------------------------------------- */


using UnityEngine;
using UnityEngine.Events;

// Ouve o MicrophoneRecorder, analisa o clip gravado e compara com a QuestSO ativa.
// Colocar no mesmo GameObject que o MicrophoneRecorder.
public class SoundMatchChecker : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] public MicrophoneRecorder micRecorder;

    [Header("Quest Ativa")]
    [SerializeField] private QuestSO activeQuest;

    [Header("Configurações")]
    [Range(0f, 1f)]
    [Tooltip("Score mínimo (0-1) para considerar acerto.")]
    public float successThreshold = 0.6f;

    [Header("Eventos")]
    public UnityEvent<float> OnMatchResult;   // sempre dispara com o score
    public UnityEvent<float> OnMatchSuccess;  // score >= successThreshold
    public UnityEvent<float> OnMatchFail;     // score < successThreshold

    public SoundAnalysisResult LastAnalysis { get; private set; }
    public float LastScore { get; private set; }

    private void Awake()
    {
        if (micRecorder == null)
            micRecorder = GetComponent<MicrophoneRecorder>();
    }

    private void OnEnable()
    {
        if (micRecorder != null)
            micRecorder.OnRecordingFinished.AddListener(OnRecordingFinished);
    }

    private void OnDisable()
    {
        if (micRecorder != null)
            micRecorder.OnRecordingFinished.RemoveListener(OnRecordingFinished);
    }

    // Chamado pelo NPC ou sistema de diálogo para definir a quest atual
    public void SetActiveQuest(QuestSO quest)
    {
        activeQuest = quest;
        if (micRecorder != null && quest != null)
        {
            micRecorder.SetRecordingLimit(quest.recordingTimeLimit);
            micRecorder.AllowRecording = true;
        }
    }

    public void ClearActiveQuest()
    {
        activeQuest = null;
        if (micRecorder != null)
        {
            micRecorder.AllowRecording = false;
            if (micRecorder.IsRecording) micRecorder.StopRecording();
        }
    }

    private void OnRecordingFinished()
    {
        if (activeQuest == null || activeQuest.soundProfile == null)
        {
            Debug.LogWarning("[SoundMatchChecker] Nenhuma quest ativa ou sem SoundProfile.");
            return;
        }

        LastAnalysis = SoundAnalyzer.Analyze(micRecorder.LastRecordedClip);
        LastScore    = ComputeScore(LastAnalysis, activeQuest.soundProfile);

        Debug.Log($"[SoundMatchChecker] Quest: '{activeQuest.questTitle}' | " +
                  $"Score: {LastScore:P0} | Loudness: {LastAnalysis.averageLoudness:F3} | " +
                  $"Banda: {LastAnalysis.dominantBand} | Duração: {LastAnalysis.duration:F2}s");

        // Bloqueia nova gravação até a próxima quest
        micRecorder.AllowRecording = false;

        OnMatchResult?.Invoke(LastScore);

        if (LastScore >= successThreshold)
            OnMatchSuccess?.Invoke(LastScore);
        else
            OnMatchFail?.Invoke(LastScore);
    }

    private float ComputeScore(SoundAnalysisResult result, SoundProfile profile)
    {
        float loudnessScore = ScoreRange(result.averageLoudness, profile.minLoudness, profile.maxLoudness);
        float durationScore = ScoreRange(result.duration,        profile.minDuration,  profile.maxDuration);
        float freqScore     = profile.expectedFrequency == FrequencyBand.Any
                                ? 1f
                                : (result.dominantBand == profile.expectedFrequency ? 1f : 0f);

        float totalWeight = profile.loudnessWeight + profile.durationWeight + profile.frequencyWeight;
        if (totalWeight <= 0f) return 0f;

        return (loudnessScore * profile.loudnessWeight +
                durationScore * profile.durationWeight +
                freqScore     * profile.frequencyWeight) / totalWeight;
    }

    // Retorna 1.0 se dentro do range, decai linearmente fora (até 0 em 50% além do range)
    private static float ScoreRange(float value, float min, float max)
    {
        if (max <= min) return 1f;
        if (value >= min && value <= max) return 1f;

        float tolerance = (max - min) * 0.5f;
        if (value < min) return Mathf.Max(0f, 1f - (min - value) / tolerance);
        return Mathf.Max(0f, 1f - (value - max) / tolerance);
    }
}
