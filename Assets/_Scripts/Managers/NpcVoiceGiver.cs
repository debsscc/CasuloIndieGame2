//----------------------------------------------------------------
// CRIADO EM: 2026-05
// FEITO POR: Debora Carvalho
// DESCRIÇÃO: Localizado no GameObject do NPC.
// Ele gerencia o fluxo: abrir quest → jogador grava → avalia → sucesso ou nova tentativa.
// ----------------------------------------------------------------

using System.Collections;
using UnityEngine;

// Garante que Awake/Start deste script rodem APÓS o PlayerMovement (ordem padrão = 0)
[DefaultExecutionOrder(10)]
public class NpcVoiceGiver : MonoBehaviour
{
    [Header("Popup")]
    [SerializeField] private NpcVoicePopup voicePopup;

    [Header("Comportamento")]
    [Tooltip("Abre o popup automaticamente ao iniciar a cena")]
    [SerializeField] private bool openOnSceneStart = true;
    [Tooltip("Delay em segundos antes de abrir o popup (dá tempo do fade-in)")]
    [SerializeField] private float autoOpenDelay = 0.8f;

    [Header("Resultado da gravação")]
    [Tooltip("Arraste o FootstepPlayer do Player aqui para aplicar o clip gravado")]
    [SerializeField] private FootstepPlayer footstepPlayer;

    // ── Referências do player ─────────────────────────────────────
    [Header("Player")]
    [Tooltip("Arraste o GameObject do Player aqui")]
    [SerializeField] private PlayerMovement    playerMovement;
    [Tooltip("Arraste o GameObject do Player aqui (mesmo objeto)")]
    [SerializeField] private PlayerInputHandler playerInput;
    private bool                playerInRange = false;
    private bool                questDone     = false;

    // =============================================================

    private void Start()
    {
        // Auto-resolve o popup se não foi atribuído
        if (voicePopup == null)
            voicePopup = NpcVoicePopup.Instance;

        if (voicePopup != null)
        {
            voicePopup.OnVoiceConfirmed.AddListener(HandleConfirmed);
            voicePopup.OnPopupCanceled.AddListener(HandleCanceled);
        }

        if (openOnSceneStart)
            StartCoroutine(AutoOpenRoutine());
    }

    private void OnDestroy()
    {
        if (voicePopup != null)
        {
            voicePopup.OnVoiceConfirmed.RemoveListener(HandleConfirmed);
            voicePopup.OnPopupCanceled.RemoveListener(HandleCanceled);
        }

        if (playerInput != null)
            playerInput.OnInteractInput -= HandleInteract;
    }

    // ── Trigger do NPC ────────────────────────────────────────────

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;
        CachePlayerComponents(other);

        // Subscreve ao input de interação do player para reabrir após cancelar
        if (playerInput != null)
            playerInput.OnInteractInput += HandleInteract;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;

        if (playerInput != null)
            playerInput.OnInteractInput -= HandleInteract;
    }

    // ── Interação (E key enquanto em range) ───────────────────────

    // Chamado pelo PlayerInputHandler.OnInteractInput
    private void HandleInteract() => TryOpenPopup();

    // Também pode ser chamado via UnityEvent de NpcInteractionTrigger.OnPlayerEntered
    public void TryOpenPopup()
    {
        if (questDone) return;
        voicePopup?.OpenPopup();
        BlockPlayer(true);
    }

    // ── Callbacks do popup ────────────────────────────────────────

    private void HandleConfirmed(AudioClip clip)
    {
        questDone = true;

        // Define o clip ANTES de liberar o movimento — evita primeiro passo sem som
        if (footstepPlayer == null)
            footstepPlayer = FindAnyObjectByType<FootstepPlayer>();

        if (footstepPlayer != null)
            footstepPlayer.SetFootstepClip(clip);
        else
            Debug.LogWarning("[NpcVoiceGiver] FootstepPlayer não encontrado na cena.", this);

        BlockPlayer(false);

        // Desinscreve input — quest concluída, não precisa mais reabrir
        if (playerInput != null)
            playerInput.OnInteractInput -= HandleInteract;

        Debug.Log("[NpcVoiceGiver] Voz confirmada. Clip: " + clip?.name);
    }

    private void HandleCanceled()
    {
        // Movimento permanece bloqueado — player precisa re-interagir para gravar
        Debug.Log("[NpcVoiceGiver] Popup cancelado. Movimento ainda bloqueado.");
    }

    // ── Internos ──────────────────────────────────────────────────

    private IEnumerator AutoOpenRoutine()
    {
        yield return new WaitForSeconds(autoOpenDelay);

        if (questDone) yield break;

        // Garante referência ao player mesmo que ainda não tenha entrado no trigger
        if (playerMovement == null)
        {
            var pm = FindAnyObjectByType<PlayerMovement>();
            if (pm != null)
            {
                playerMovement = pm;
                playerInput    = pm.GetComponent<PlayerInputHandler>();
                if (playerInput != null)
                    playerInput.OnInteractInput += HandleInteract;
            }
        }

        // Reforça bloqueio (já aplicado no Awake, mas re-aplica após delay p/ garantir)
        BlockPlayer(true);

        voicePopup?.OpenPopup();
    }

    private void CachePlayerComponents(Collider2D playerCollider)
    {
        if (playerMovement == null)
            playerMovement = playerCollider.GetComponent<PlayerMovement>();

        if (playerInput == null)
            playerInput = playerCollider.GetComponent<PlayerInputHandler>();
    }

    private void BlockPlayer(bool block)
    {
        if (playerMovement == null)
            playerMovement = FindAnyObjectByType<PlayerMovement>();
        if (playerInput == null && playerMovement != null)
            playerInput = playerMovement.GetComponent<PlayerInputHandler>();

        if (playerMovement == null)
        {
            Debug.LogError("[NpcVoiceGiver] PlayerMovement não encontrado! Arraste no Inspector.", this);
            return;
        }

        // Bloqueia nas duas camadas: input (fonte) + física (segurança)
        playerInput?.SetMovementInputBlocked(block);
        playerMovement.SetMovementBlocked(block);
        Debug.Log($"[NpcVoiceGiver] Movimento {(block ? "BLOQUEADO" : "LIBERADO")}");
    }
}
