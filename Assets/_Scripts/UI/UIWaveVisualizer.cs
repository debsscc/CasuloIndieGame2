// ----------------------------------------------------------------
// CRIADO EM: 2026-05
// DESCRIÇÃO: Versão UI do AudioVisualizer — usa RectTransform[] bars (Images)
//            ao invés de Transform[] 3D. Escala a altura (sizeDelta.y) de cada
//            barra com base na amplitude do microfone em tempo real.
//            Mesma lógica do AudioVisualizer (AudioLoudnessRecorder.cs) da cena Menu.
// FEITO POR: Debs Carvalho
// ----------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;

public class UIWaveVisualizer : MonoBehaviour
{
    [Header("Refs")]
    public MicrophoneRecorder micRecorder;

    [Header("Barras UI")]
    [Tooltip("Arraste aqui os RectTransforms das barras do painel de wave")]
    public RectTransform[] bars;

    [Header("Configurações")]
    [Tooltip("Multiplicador de amplitude → altura em pixels")]
    public float amplification = 400f;
    [Tooltip("Altura mínima de cada barra em pixels (estado idle)")]
    public float minHeight = 4f;
    [Tooltip("Velocidade de suavização")]
    public float smoothSpeed = 20f;

    private float[] micSamples;
    private float[] currentHeights;
    private float[] targetHeights;

    private void Awake()
    {
        if (micRecorder == null)
            micRecorder = FindAnyObjectByType<MicrophoneRecorder>();

        // Corrige o HorizontalLayoutGroup: alinha na base e libera altura individual
        var hlg = GetComponent<HorizontalLayoutGroup>();
        if (hlg != null)
        {
            hlg.childAlignment        = TextAnchor.LowerLeft;  // ancora barras no fundo
            hlg.childForceExpandHeight = false;                 // respeita sizeDelta.y de cada barra
        }

        if (bars != null && bars.Length > 0)
        {
            currentHeights = new float[bars.Length];
            targetHeights  = new float[bars.Length];
            for (int i = 0; i < bars.Length; i++)
            {
                currentHeights[i] = minHeight;

                if (bars[i] == null) continue;

                // Altura inicial
                var size = bars[i].sizeDelta;
                size.y   = minHeight;
                bars[i].sizeDelta = size;
            }
        }
    }

    private void Update()
    {
        if (bars == null || bars.Length == 0) return;

        UpdateTargetHeights();
        SmoothBars();
    }

    // Lê o buffer do microfone (mesma lógica do AudioVisualizer.UpdateBarsFromMic)
    private void UpdateTargetHeights()
    {
        if (micRecorder == null || micRecorder.MicClip == null)
        {
            ResetTargets();
            return;
        }

        int pos = Microphone.GetPosition(micRecorder.MicDeviceName);
        if (pos <= 0)
        {
            ResetTargets();
            return;
        }

        int clipSamples = micRecorder.MicClip.samples;
        int windowSize  = Mathf.Min(micRecorder.MicClip.frequency / 10, clipSamples); // 100 ms

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

            float amplitude  = sum / bandSize;
            targetHeights[i] = Mathf.Max(minHeight, amplitude * amplification);
        }
    }

    // Lerp suave igual ao visual do menu
    private void SmoothBars()
    {
        for (int i = 0; i < bars.Length; i++)
        {
            if (bars[i] == null) continue;

            currentHeights[i] = Mathf.Lerp(currentHeights[i], targetHeights[i],
                                             Time.deltaTime * smoothSpeed);

            var size  = bars[i].sizeDelta;
            size.y    = currentHeights[i];
            bars[i].sizeDelta = size;
        }
    }

    private void ResetTargets()
    {
        if (targetHeights == null) return;
        for (int i = 0; i < targetHeights.Length; i++)
            targetHeights[i] = minHeight;
    }

    // Chamado pelo NpcVoicePopup quando o popup fecha
    public void ResetBars() => ResetTargets();
}
