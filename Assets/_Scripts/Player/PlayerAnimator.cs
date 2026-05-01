///* ----------------------------------------------------------------
// CRIADO EM: 2026-05
// DESCRIÇÃO: Controla as animações do jogador com base nos eventos
//            disparados pelo PlayerMovement.
// ---------------------------------------------------------------- */

using UnityEngine;

[RequireComponent(typeof(Animator), typeof(SpriteRenderer), typeof(PlayerMovement))]
public class PlayerAnimator : MonoBehaviour
{
    private static readonly int IsMoving     = Animator.StringToHash("isMoving");
    private static readonly int IsRunning    = Animator.StringToHash("isRunning");
    private static readonly int IsWalkingUp  = Animator.StringToHash("isWalkingUp");
    private static readonly int IsWalkingDown = Animator.StringToHash("isWalkingDown");

    private Animator        _animator;
    private SpriteRenderer  _spriteRenderer;
    private PlayerMovement  _movement;

    void Awake()
    {
        _animator       = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _movement       = GetComponent<PlayerMovement>();
    }

    private void OnEnable()
    {
        _movement.OnMovement      += HandleMovement;
        _movement.OnStop          += HandleStop;
        _movement.OnSprintChanged += HandleSprint;
    }

    private void OnDisable()
    {
        _movement.OnMovement      -= HandleMovement;
        _movement.OnStop          -= HandleStop;
        _movement.OnSprintChanged -= HandleSprint;
    }

    private void HandleMovement(Vector2 dir)
    {
        _animator.SetBool(IsMoving, true);

        bool isVertical = Mathf.Abs(dir.y) > Mathf.Abs(dir.x);

        if (isVertical)
        {
            // WalkUp / WalkDown: sem flip
            _spriteRenderer.flipX = false;
            _animator.SetBool(IsWalkingUp,   dir.y > 0);
            _animator.SetBool(IsWalkingDown, dir.y < 0);
        }
        else
        {
            // Walk horizontal: aplica flip conforme direção
            _spriteRenderer.flipX = dir.x < 0;
            _animator.SetBool(IsWalkingUp,   false);
            _animator.SetBool(IsWalkingDown, false);
        }
    }

    private void HandleStop()
    {
        _animator.SetBool(IsMoving,     false);
        _animator.SetBool(IsRunning,    false);
        _animator.SetBool(IsWalkingUp,  false);
        _animator.SetBool(IsWalkingDown, false);
    }

    private void HandleSprint(bool sprinting)
    {
        _animator.SetBool(IsRunning, sprinting);
    }
}
