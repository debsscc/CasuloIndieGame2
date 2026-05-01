///* ----------------------------------------------------------------
// CRIADO EM: 2024-06
// REVISADO POR: Debora Carvalho
// DESCRIÇÃO: Controla o movimento do jogador.
// ---------------------------------------------------------------- */

using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(PlayerInputHandler))]
public class PlayerMovement : MonoBehaviour
{
    //-----DustParticle-------
    public ParticleSystem dustParticle; 
    private Rigidbody2D _rb;
    private PlayerInputHandler _input;
    
    private Vector2 _moveDirection;

    public event Action<bool> OnFlipSprite;

    // Evento disparado quando o jogador começa a se mover
    public event Action OnMovement;
    // Evento disparado quando o jogador para
    public event Action OnStop;

    [SerializeField] private float speed = 5f;
    [SerializeField] private float runSpeedMultiplier = 1.5f;

    private bool _isMoving = false;
    private bool facingRight = false;
    private bool _isRunning = false;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _input = GetComponent<PlayerInputHandler>();
    }

    private void OnEnable()
    {
        _input.OnMoveInput += HandleMoveInput;
        _input.OnSprintInput += HandleSprintInput;
    }

    private void OnDisable()
    {
        _input.OnMoveInput -= HandleMoveInput;
        _input.OnSprintInput -= HandleSprintInput;
    }

    public void HandleMoveInput(Vector2 direction)
    {
        _moveDirection = direction;
        
        // Dispara eventos OnMovement / OnStop quando o estado de movimento muda
        bool movingNow = direction.sqrMagnitude > 0.0001f;
        if (movingNow && !_isMoving)
        {
            OnMovement?.Invoke();
            _isMoving = true;
        }
        else if (!movingNow && _isMoving)
        {
            OnStop?.Invoke();
            _isMoving = false;
        }

        //-------Flip w/ Dust Particle (VERIFICAR SE VAI SER ASSIM MEMO--------
        if (_moveDirection.x > 0 && !facingRight)
        {
            OnFlipSprite?.Invoke(true);
            facingRight = true;
            CreateDust();
        }
        else if (_moveDirection.x < 0 && facingRight)
        {
            OnFlipSprite?.Invoke(false);
            facingRight = false;
            CreateDust();
        }
    }
    

    public void HandleSprintInput(bool sprinting)
    {
        _isRunning = sprinting;
    }

    void FixedUpdate()
    {
        float currentSpeed = _isRunning ? speed * runSpeedMultiplier : speed;
        _rb.linearVelocity = _moveDirection * currentSpeed;
    }

    void CreateDust()
    {
        if (dustParticle != null)
        {
            dustParticle.Play();
        }
    }

}