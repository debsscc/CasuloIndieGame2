//------------------------
// Author: Debs Carvalho
// Data: 2026-05
// Description: Controller to handle feedback for dialogue choices based on character emotions;
//------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DialogueChoiceFeedbackController : MonoBehaviour
{
    [SerializeField] private float emotionDuration = 3f;
    [SerializeField] private EmotionTypeEnums gainEmotion = EmotionTypeEnums.Happy;
    [SerializeField] private EmotionTypeEnums loseEmotion = EmotionTypeEnums.Sad;
    [SerializeField] private EmotionTypeEnums loseFallbackEmotion = EmotionTypeEnums.Angry;

    private class RunnerBinding
    {
        public CharacterAnimatorRunner runner;
        public CharacterData data;
        public int previousScore;
        public Coroutine resetCoroutine;
        public Action<int> handler;
    }

    private readonly List<RunnerBinding> _bindings = new();



    private void Start()
    {
        var scoreDialogue = FindFirstObjectByType<ScoreRulesDialogue>();
        if (scoreDialogue == null)
        {
            return;
        }

        // Constrói dicionário characterId → CharacterData a partir dos bindings já configurados no ScoreRulesDialogue
        var idToData = new Dictionary<string, CharacterData>(StringComparer.OrdinalIgnoreCase);
        foreach (var b in scoreDialogue.characterBindings)
        {
            if (b == null || b.characterData == null) continue;
            var key = b.characterId?.Trim();
            if (string.IsNullOrEmpty(key)) continue;
            if (!idToData.ContainsKey(key))
                idToData[key] = b.characterData;
        }
        var runners = FindObjectsByType<CharacterAnimatorRunner>(FindObjectsSortMode.None);

        foreach (var runner in runners)
        {
            var characterId = runner.GetCharacterId()?.Trim();
            if (!idToData.TryGetValue(characterId, out var data) || data == null)
            {
                Debug.LogWarning($"[DialogueChoiceFeedbackController] CharacterData não encontrado para '{characterId}' — verifique o characterId no CharacterIdentity e os bindings do ScoreRulesDialogue.");
                continue;
            }

            var binding = new RunnerBinding
            {
                runner = runner,
                data = data,
                previousScore = data.RelationshipScore
            };

            binding.handler = newScore => OnScoreChanged(binding, newScore);
            data.OnRelationshipChanged += binding.handler;
            _bindings.Add(binding);
        }
    }

    private void OnScoreChanged(RunnerBinding binding, int newScore)
    {
        bool gained = newScore > binding.previousScore;
        binding.previousScore = newScore;

        if (binding.resetCoroutine != null)
            StopCoroutine(binding.resetCoroutine);

        binding.resetCoroutine = StartCoroutine(ApplyFeedbackDelayed(binding, gained));
    }

    public void CancelAllFeedback()
    {
        StopAllCoroutines();
        foreach (var b in _bindings)
            b.resetCoroutine = null;
    }

    private IEnumerator ApplyFeedbackDelayed(RunnerBinding binding, bool gained)
    {
        // Aguarda 1 frame para o DialogueEmotionController.ForceApplyCurrentEmotion rodar antes e n bugar
        yield return null;

        if (gained)
            PlayWithFallback(binding.runner, gainEmotion, EmotionTypeEnums.Normal);
        else
            PlayWithFallback(binding.runner, loseEmotion, loseFallbackEmotion);

        yield return new WaitForSeconds(emotionDuration);

        if (binding.runner != null)
            binding.runner.PlayAnimation(EmotionTypeEnums.Normal);

        binding.resetCoroutine = null;
    }

    private void PlayWithFallback(CharacterAnimatorRunner runner, EmotionTypeEnums first, EmotionTypeEnums fallback)
    {
        if (HasTrigger(runner, first))
        {
            runner.PlayAnimation(first);
        }
        else if (HasTrigger(runner, fallback))
        {
            runner.PlayAnimation(fallback);
        }
    }

    // mapeando p n dar problema (EmotionType → nome do estado DEFAULT no AnimatorController base)
    private static readonly Dictionary<EmotionTypeEnums, string> _emotionToStateName = new()
    {
        { EmotionTypeEnums.Normal,     "x"     },
        { EmotionTypeEnums.Angry,      "x"      },
        { EmotionTypeEnums.Happy,      "x"      },
        { EmotionTypeEnums.Excited,        "x"     },
        { EmotionTypeEnums.Sad,  "x"   },
        { EmotionTypeEnums.Surprised,    "x"    },
        { EmotionTypeEnums.Shy,     "x" },
        { EmotionTypeEnums.Flirty,    "x" },
        { EmotionTypeEnums.Worried,        "x"   },
        { EmotionTypeEnums.Thoughtful, "x"  },
    };

    private bool HasTrigger(CharacterAnimatorRunner runner, EmotionTypeEnums emotion)
    {
        var animator = runner.GetAnimator();
        if (animator == null) return false;

        // Se o animator usa OverrideController, verifica se o clip daquele estado ta posto ali ou n
        if (animator.runtimeAnimatorController is AnimatorOverrideController overrideCtrl)
        {
            if (_emotionToStateName.TryGetValue(emotion, out var stateName))
            {
                var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
                overrideCtrl.GetOverrides(overrides);

                foreach (var pair in overrides)
                {
                    if (pair.Key != null && pair.Key.name == stateName)
                        return pair.Value != null;
                }
            }
            // Estado não encontrado/sem clip nas overrides: cai para verificar o trigger no base controller
        }

        // verifica se o trigger existe como parâmetro (funciona p/ AnimatorController base e OverrideController)
        string triggerName = emotion.ToString();
        foreach (var param in animator.parameters)
        {
            if (param.type == AnimatorControllerParameterType.Trigger && param.name == triggerName)
                return true;
        }
        return false;
    }


    private void OnDestroy()
    {
        foreach (var binding in _bindings)
        {
            if (binding.data != null && binding.handler != null)
                binding.data.OnRelationshipChanged -= binding.handler;
        }
        _bindings.Clear();
    }
}
