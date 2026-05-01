///* ----------------------------------------------------------------
// CRIADO EM: 2024-06
// FEITO POR: Debora Carvalho
// DESCRIÇÃO: Manipula o input do jogador e dispara eventos que outros componentes podem assinar.
// ---------------------------------------------------------------- */
using UnityEngine;
using UnityEngine.InputSystem; 
using System; 

[RequireComponent(typeof(PlayerInput))]
public class PlayerInputHandler : MonoBehaviour
{
    // Eventos que os outros scripts do Player irão assinar
    public event Action<Vector2> OnMoveInput;
    public event Action OnAbilityInput;
    public event Action<bool> OnSprintInput;

    private PlayerInput _playerInput;
    private bool _isPaused = false;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
    }

    public void OnMove(InputValue value)
    {
        if (_isPaused) return;
        OnMoveInput?.Invoke(value.Get<Vector2>());
    }

    public void OnAbility(InputValue value)
    {
        if (_isPaused) return;

        if (value.isPressed)
        {
            OnAbilityInput?.Invoke();
        }
    }

    public void OnSprint(InputValue value)
    {
        if (_isPaused) return;
        OnSprintInput?.Invoke(value.isPressed);
    }

    private void HandlePauseChanged(bool paused)
    {
        _isPaused = paused;
    }
}