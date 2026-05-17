//------------------------
// Author: Debs Carvalho
// Data: 2026-05
// Description: ScriptableObject to store character relationship bindings and associated data for dialogue system.;
//------------------------

[System.Serializable]
public class CharacterRelationshipBinding
{
    public string characterId;       // Nome exato como no Yarn (ex: "Sabrina")
    public CharacterData characterData; // ScriptableObject com pontuação
}