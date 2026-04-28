using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FrequencyFocusWindow
{
    Full    = 1,
    Half    = 2,
    Quarter = 4,
    Eighth  = 8
}

// Visualiza o áudio do microfone em tempo real.
public class AudioVisualizer : MonoBehaviour
{
    [Header("Refs")]
    public AudioSource audioSource;
    public MicrophoneRecorder micRecorder;
    public Transform [] bars;

    [Header("Settings")]
    public FrequencyFocusWindow frequencyFocusWindow;
    public float amplification = 1.0f;
    public float baseHeight = 0.0f;
    public FFTWindow fFTWindow;
    public bool useDecibels;

    [Header("State")]
    public float [] spectrumData;

    private float[] micSamples;

    void Awake()
    {
        spectrumData = new float[4096];
    }

    void Start()
    {
        // Auto-encontra o MicrophoneRecorder na cena se não foi atribuído no Inspector.
        if (micRecorder == null)
            micRecorder = FindAnyObjectByType<MicrophoneRecorder>();

        if (micRecorder != null)
            Debug.Log("[AudioVisualizer] MicrophoneRecorder encontrado: " + micRecorder.name);
        else
            Debug.LogWarning("[AudioVisualizer] MicrophoneRecorder não encontrado!");
    }

    void Update()
    {
        if (micRecorder != null && micRecorder.MicClip != null)
            UpdateBarsFromMic();
        else
            UpdateBarsFromAudioSource();
    }

    // Lê diretamente do micClip — independente do volume do AudioSource.
    private void UpdateBarsFromMic()
    {
        int pos = Microphone.GetPosition(micRecorder.MicDeviceName);
        if (pos <= 0) return;

        int clipSamples = micRecorder.MicClip.samples;
        int windowSize = Mathf.Min(micRecorder.MicClip.frequency / 10, clipSamples); // 100ms

        if (micSamples == null || micSamples.Length != windowSize)
            micSamples = new float[windowSize];

        int startPos = pos - windowSize;
        if (startPos < 0) startPos += clipSamples;

        micRecorder.MicClip.GetData(micSamples, startPos);

        int bandSize = windowSize / bars.Length;
        if (bandSize <= 0) return;

        for (int i = 0; i < bars.Length; i++)
        {
            float sum = 0f;
            int start = i * bandSize;
            for (int j = 0; j < bandSize; j++)
                sum += Mathf.Abs(micSamples[start + j]);

            float amplitude = sum / bandSize;
            var scale = bars[i].localScale;
            scale.y = amplitude * amplification + baseHeight;
            bars[i].localScale = scale;
        }
    }

    // Fallback: usa GetSpectrumData do AudioSource (para audio não-microfone).
    private void UpdateBarsFromAudioSource()
    {
        if (audioSource == null) return;
        audioSource.GetSpectrumData(spectrumData, 0, fFTWindow);
        int blockSize = spectrumData.Length / bars.Length / (int)frequencyFocusWindow;
        if (blockSize <= 0) return;

        for (int i = 0; i < bars.Length; i++)
        {
            float average = 0f;
            for (int j = 0; j < blockSize; j++)
                average += spectrumData[i * blockSize + j];
            average /= blockSize;

            float amplituded = Mathf.Clamp(average, 1e-7f, 1f);
            var scale = bars[i].localScale;
            if (useDecibels)
            {
                float dBFS = 20f * Mathf.Log10(amplituded);
                scale.y = (dBFS + 140f) / 140f * amplification + baseHeight;
            }
            else
            {
                scale.y = average * amplification + baseHeight;
            }
            bars[i].localScale = scale;
        }
    }
}

