// ----------------------------------------------------------------
// CRIADO EM: 2026-05
// FEITO POR: Debs Carvalho
// DESCRIÇÃO: Registra comandos e funções Yarn para a quest de frutinhas do NPC2.
//
//            Comandos disponíveis no Yarn:
//              <<start_fruit_quest>>    — inicia a quest (estado 0→1)
//              <<complete_fruit_quest>> — conclui a quest após entrega (estado 2→3)
//
//            Funções disponíveis no Yarn:
//              get_fruit_quest_state()  — retorna int: 0=NãoIniciada, 1=Ativa,
//                                         2=ProntaParaEntregar, 3=Concluída
// ----------------------------------------------------------------

using UnityEngine;
using Yarn.Unity;

public class YarnFruitQuestCommands : MonoBehaviour
{
    [SerializeField] private DialogueRunner dialogueRunner;

    private void Start()
    {
        if (dialogueRunner == null)
            dialogueRunner = FindAnyObjectByType<DialogueRunner>();

        if (dialogueRunner == null)
        {
            Debug.LogError("[YarnFruitQuestCommands] DialogueRunner não encontrado na cena!", this);
            return;
        }

        dialogueRunner.AddCommandHandler("start_fruit_quest",    StartFruitQuest);
        dialogueRunner.AddCommandHandler("complete_fruit_quest", CompleteFruitQuest);
        dialogueRunner.AddFunction("get_fruit_quest_state",      GetFruitQuestState);
    }

    // ── Handlers ──────────────────────────────────────────────────

    private void StartFruitQuest()
    {
        if (FruitQuestManager.Instance == null)
        {
            Debug.LogError("[YarnFruitQuestCommands] FruitQuestManager não encontrado na cena!", this);
            return;
        }
        FruitQuestManager.Instance.StartQuest();
    }

    private void CompleteFruitQuest()
    {
        if (FruitQuestManager.Instance == null)
        {
            Debug.LogError("[YarnFruitQuestCommands] FruitQuestManager não encontrado na cena!", this);
            return;
        }
        FruitQuestManager.Instance.CompleteQuest();
    }

    private int GetFruitQuestState()
    {
        return FruitQuestManager.Instance?.QuestState ?? 0;
    }
}
