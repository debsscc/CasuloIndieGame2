//------------------------
// Author: Debs Carvalho
// Data: 2026-05
// Description: Controller to handle character animations based on emotions;
//------------------------

using UnityEngine;

public class CharacterAnimatorRunner : MonoBehaviour
{
    [SerializeField] private CharacterIdentity characterIdentity;
    [SerializeField] private DialogueEmotionController dialogueEmotionController;
    [SerializeField] private Animator characterAnimator;

    private EmotionTypeEnums _currentEmotion = EmotionTypeEnums.Normal;

    void Start()
    {
        dialogueEmotionController.AddAnimatorRunner(this);

        if(!characterAnimator)
        {
            characterAnimator = GetComponent<Animator>();
        }

        PlayAnimation(EmotionTypeEnums.Normal);
    }


    public void PlayAnimation(EmotionTypeEnums animationTrigger, bool force = false)
    {
        if (!force && animationTrigger == _currentEmotion) return;

        // Reseta todos os triggers pendentes antes de ativar o novo,
        // prevenindo acúmulo durante navegação rápida entre opções.
        foreach (var param in characterAnimator.parameters)
            if (param.type == AnimatorControllerParameterType.Trigger)
                characterAnimator.ResetTrigger(param.name);

        characterAnimator.SetTrigger(animationTrigger.ToString());
        _currentEmotion = animationTrigger;
    }

    public void ResetCurrentEmotion()
    {
        _currentEmotion = EmotionTypeEnums.Normal;
    }

    public Animator GetAnimator() => characterAnimator;

    public string GetCharacterId()
    {
        return characterIdentity.characterId;
    }

    public void TestPlayAnimation(EmotionTypeEnums emotionType)
    {
        PlayAnimation(emotionType);
    }
}
