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
    public event Action OnInteractInput;

    private PlayerInput _playerInput;
    private InputAction _sprintAction;
    private bool _isPaused = false;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _sprintAction = _playerInput.actions["Sprint"];
    }

    private void OnEnable()
    {
        _sprintAction.performed += OnSprintPerformed;
        _sprintAction.canceled  += OnSprintCanceled;
    }

    private void OnDisable()
    {
        _sprintAction.performed -= OnSprintPerformed;
        _sprintAction.canceled  -= OnSprintCanceled;
    }

    private void OnSprintPerformed(InputAction.CallbackContext ctx)
    {
        if (!_isPaused) OnSprintInput?.Invoke(true);
    }

    private void OnSprintCanceled(InputAction.CallbackContext ctx)
    {
        OnSprintInput?.Invoke(false);
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

    public void OnInteract(InputValue value)
    {
        if (_isPaused) return;
        if (value.isPressed) OnInteractInput?.Invoke();
    }

    // Mantido para compatibilidade com SendMessages, mas o sprint agora usa callbacks diretos
    public void OnSprint(InputValue value) { }

    public void SetPaused(bool paused)
    {
        _isPaused = paused;
    }
}