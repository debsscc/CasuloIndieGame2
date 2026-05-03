///* ----------------------------------------------------------------
 // CRIADO EM: 2026-05
 // DESCRIÇÃO: Utilitário pra analisar um AudioClip gravado e extrair características do som.

using UnityEngine;

public struct SoundAnalysisResult
{
    public float averageLoudness;   // RMS normalizado 0-1
    public float duration;          // segundos
    public FrequencyBand dominantBand;
    public float zeroCrossingRate;  // valor bruto ZCR
}

// Usa RMS para volume e Zero Crossing Rate como proxy de frequência:
//   ZCR baixo  (<0.05)  = sons graves (rugidos, roncos)
//   ZCR médio  (<=0.20) = sons médios (fala normal)
//   ZCR alto   (>0.20)  = sons agudos (apitos, gritos finos)
public static class SoundAnalyzer
{
    private const float ZCR_BASS_THRESHOLD = 0.05f;
    private const float ZCR_MID_THRESHOLD  = 0.20f;

    public static SoundAnalysisResult Analyze(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("[SoundAnalyzer] Clip nulo.");
            return default;
        }

        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        float rms = ComputeRMS(samples);
        float zcr = ComputeZCR(samples);

        FrequencyBand band;
        if (zcr <= ZCR_BASS_THRESHOLD)
            band = FrequencyBand.Bass;
        else if (zcr <= ZCR_MID_THRESHOLD)
            band = FrequencyBand.Mid;
        else
            band = FrequencyBand.Treble;

        return new SoundAnalysisResult
        {
            averageLoudness   = rms,
            duration          = (float)clip.samples / clip.frequency,
            dominantBand      = band,
            zeroCrossingRate  = zcr
        };
    }

    // RMS (Root Mean Square) — mede o volume médio do clip.
    private static float ComputeRMS(float[] samples)
    {
        if (samples.Length == 0) return 0f;
        float sum = 0f;
        for (int i = 0; i < samples.Length; i++)
            sum += samples[i] * samples[i];
        return Mathf.Sqrt(sum / samples.Length);
    }

    // ZCR (Zero Crossing Rate) — conta quantas vezes o sinal troca de sinal por amostra.
    // Usado como proxy de frequência: sons graves têm ZCR baixo, sons agudos têm ZCR alto.
    private static float ComputeZCR(float[] samples)
    {
        if (samples.Length < 2) return 0f;
        int crossings = 0;
        for (int i = 1; i < samples.Length; i++)
        {
            if ((samples[i - 1] >= 0f && samples[i] < 0f) ||
                (samples[i - 1] <  0f && samples[i] >= 0f))
                crossings++;
        }
        return (float)crossings / samples.Length;
    }
}
