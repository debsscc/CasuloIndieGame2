using UnityEngine;

public enum FrequencyBand { Any, Bass, Mid, Treble }

[CreateAssetMenu(fileName = "SoundProfile", menuName = "CasuloIndie/Sound Profile")]
public class SoundProfile : ScriptableObject
{
    [Header("Volume")]
    [Range(0f, 1f)] public float minLoudness = 0f;
    [Range(0f, 1f)] public float maxLoudness = 1f;

    [Header("Duração")]
    public float minDuration = 0f;
    public float maxDuration = 5f;

    [Header("Frequência")]
    public FrequencyBand expectedFrequency = FrequencyBand.Any;

    [Header("Pesos da Pontuação")]
    [Range(0f, 1f)] public float loudnessWeight  = 0.4f;
    [Range(0f, 1f)] public float durationWeight  = 0.3f;
    [Range(0f, 1f)] public float frequencyWeight = 0.3f;
}
