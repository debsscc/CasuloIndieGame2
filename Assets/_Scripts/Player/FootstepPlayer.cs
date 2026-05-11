//--=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
// CRIADO EM: 2026-05
// DESCRIÇÃO: Toca o som de passos do player. O clip é definido pelo NpcQuestGiver após a quest de caminhada ser concluída.
// ----------------------------------------------------------------

using System.Collections;
using UnityEngine;

// Fica no mesmo GameObject que o Player.
[RequireComponent(typeof(AudioSource), typeof(PlayerMovement))]
public class FootstepPlayer : MonoBehaviour
{
    [Header("Configurações")]
    [SerializeField] private float stepInterval = 0.4f;       // segundos entre cada passo
    [SerializeField] private float runStepInterval = 0.25f;   // intervalo menor ao correr
    [SerializeField] [Range(0f, 1f)] private float volume = 1f;

    private AudioSource audioSource;
    private PlayerMovement playerMovement;
    private AudioClip footstepClip;

    private bool isMoving = false;
    private bool isRunning = false;
    private Coroutine stepCoroutine;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void OnEnable()
    {
        playerMovement.OnMovement += HandleMovement;
        playerMovement.OnStop     += HandleStop;
        playerMovement.OnSprintChanged += HandleSprint;
    }

    private void OnDisable()
    {
        playerMovement.OnMovement -= HandleMovement;
        playerMovement.OnStop     -= HandleStop;
        playerMovement.OnSprintChanged -= HandleSprint;
    }

    // Chamado pelo NpcQuestGiver.OnQuestCompleted via código ou Inspector.
    // Só aceita o clip uma vez — após a quest ser concluída, fica salvo permanentemente.
    public void SetFootstepClip(AudioClip clip)
    {
        if (footstepClip != null) return;
        footstepClip = clip;
        Debug.Log("[FootstepPlayer] Clip de passo definido: " + clip?.name);
    }

    private void HandleMovement(Vector2 _)
    {
        if (isMoving) return;
        isMoving = true;
        stepCoroutine = StartCoroutine(StepLoop());
    }

    private void HandleStop()
    {
        isMoving = false;
        if (stepCoroutine != null)
        {
            StopCoroutine(stepCoroutine);
            stepCoroutine = null;
        }
    }

    private void HandleSprint(bool running)
    {
        isRunning = running;
    }

    private IEnumerator StepLoop()
    {
        while (isMoving)
        {
            PlayStep();
            yield return new WaitForSeconds(isRunning ? runStepInterval : stepInterval);
        }
    }

    private void PlayStep()
    {
        if (footstepClip == null || audioSource == null) return;
        audioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
        audioSource.PlayOneShot(footstepClip, volume);
    }
}
