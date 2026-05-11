// ----------------------------------------------------------------
// CRIADO EM: 2026-05
// DESCRIÇÃO: ScriptableObject que define uma entrada do diário (informação nova da história).
// ----------------------------------------------------------------

using UnityEngine;

[CreateAssetMenu(fileName = "DiaryEntry", menuName = "CasuloIndie/DiaryEntry")]
public class DiaryEntrySO : ScriptableObject
{
    [Header("Identificação")]
    public string entryId;   // único por entrada, ex: "DIARY_01"

    [Header("Conteúdo")]
    public string entryTitle;
    [TextArea(3, 8)] public string entryText;
}
