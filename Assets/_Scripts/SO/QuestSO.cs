using UnityEngine;

[CreateAssetMenu(fileName = "Quest", menuName = "CasuloIndie/Quest")]
public class QuestSO : ScriptableObject
{
    [Header("Info")]
    public string questTitle;
    [TextArea] public string description;
    [TextArea] public string hint;

    [Header("Som Esperado")]
    public SoundProfile soundProfile;
    public float recordingTimeLimit = 5f;

    [Header("Recompensa")]
    public int rewardScore = 100;
}
