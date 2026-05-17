//------------------------
// Author: Debs Carvalho
// Data: 2026-05
// Description: Controller to manage speech bubbles and character emotions in dialogue interactions and conversations with NPCS;
//------------------------

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpeechBubbleController : MonoBehaviour
{
    public Image backgroundImage;
    public Image emotionImage;
    public Animator emotionAnimator;
    public TextMeshProUGUI dialogueText;

    // ── Tracking de posição ───────────────────────────────────────
    [Header("Posição (seguir NPC)")]
    [Tooltip("Deslocamento em unidades de mundo acima do NPC")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2.2f, 0f);

    private Canvas    parentCanvas;
    private Camera    cam;
    private Transform trackedTarget;
    private RectTransform rectT;

    private void Awake()
    {
        parentCanvas = GetComponentInParent<Canvas>();
        cam          = Camera.main;
        rectT        = GetComponent<RectTransform>();
    }

// -──────────────────────────────────────────────────────────────
// LateUpdate para seguir o NPC. Se o NPC estiver atrás da câmera, esconde a bolha. 
//Função de conversão de posição de mundo para tela, e depois para local dentro do canvas.
// -──────────────────────────────────────────────────────────────

    private void LateUpdate()
    {
        if (trackedTarget == null || cam == null || rectT == null) return;

        Vector3 screenPos = cam.WorldToScreenPoint(trackedTarget.position + worldOffset);
        if (screenPos.z < 0f) { gameObject.SetActive(false); return; }

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentCanvas.transform as RectTransform,
                screenPos,
                parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : cam,
                out Vector2 local))
        {
            rectT.localPosition = local;
        }
    }

    // ─────────────────────────────────────────────────────────────

    public void TrackTarget(Transform npcTransform)
    {
        trackedTarget = npcTransform;
    }

    public void ShowSpeech(string text, Sprite emotionSprite, AnimationClip emotionAnim, bool useAnimation)
    {
        dialogueText.text = text;

        if (useAnimation && emotionAnim != null && emotionAnimator != null)
        {
            emotionAnimator.enabled = true;
            emotionAnimator.Play(emotionAnim.name);
            emotionImage.enabled = false;
        }
        else if (emotionSprite != null)
        {
            emotionAnimator.enabled = false;
            emotionImage.sprite = emotionSprite;
            emotionImage.enabled = true;
        }
        else
        {
            emotionAnimator.enabled = false;
            emotionImage.enabled = false;
        }

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        trackedTarget = null;
        gameObject.SetActive(false);
    }
}