//------------------------
// Author: Debs Carvalho
// Data: 2026-05
// Description: ScriptableObject to store character data and relationships with other characters;
//------------------------

using UnityEngine;
using System.Collections.Generic;
using System;

[CreateAssetMenu(fileName = "NPCData", menuName = "Game/NPC Data")]
public class CharacterData : ScriptableObject
{
    [SerializeField] public int _initialRelationshipScore = 10;
    [SerializeField] public int _maxRelationshipScore;
    [SerializeField] private int _relationshipScore = 0;

    //public MapData favoritePlace; 
    //public TraitsData traits;
    public List<int> _relationshipTresholds;

    public event Action<int> OnRelationshipChanged;

    public int RelationshipScore
    {
        get => _relationshipScore;
        set
        {
            _relationshipScore = value;
            OnRelationshipChanged?.Invoke(value);
        }
    }
    

    public void OnEnable()
    {
        _relationshipScore = _initialRelationshipScore;
    }

    public void ResetScore()
    {
        _relationshipScore = _initialRelationshipScore;
        OnRelationshipChanged?.Invoke(_relationshipScore);
    }

    // public bool LikesItem(ItemsSO item)
    // {
    //     if (item == null) return false;

    //     if (favoriteItems == null || favoriteItems.Count == 0)
    //         return false;

    //     if (favoriteItems.Contains(item))
    //         return true;

    //     // Fallback: compare by name in case different asset instances are used
    //     foreach (var fav in favoriteItems)
    //     {
    //         if (fav != null && fav.name == item.name)
    //             return true;
    //     }

    //     return false; 
    // }
}

// [Serializable]
// //public class TraitsData
// {
//     public bool isRebel;
//     public bool isCorrect;
//     public bool isReckless;
//     public bool isBrave;
//     public bool isFearful;
//     public bool isCautious;
//     public bool isLeader;
//     public bool isFollower;
// }

