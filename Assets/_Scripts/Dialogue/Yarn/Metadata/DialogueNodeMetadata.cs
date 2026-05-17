using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Dialogue/Node Metadata")]
public class DialogueNodeMetadata : ScriptableObject
{
    public string yarnNodeName;

    public Sprite portrait;

    public string expressionName;

    public List<string> eventsBeforeNode;
    public List<string> eventsAfterNode;

    public bool overrideChoicesUI;
}
