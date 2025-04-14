using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    public static InputHandler Instance { get; private set; }

    private const string PLAYER_PREF_BINDING_JSON = "Input Bindings";

    public enum Bindings
    {
        Move_Up,
        Move_Down,
        Move_Left,
        Move_Right,
        Interact,
        Interact_Alt,
        Dash,
        Throw,
        Pause,

        Move_Gamepad,
        Interact_Gamepad,
        Interact_Alt_Gamepad,
        Dash_Gamepad,
        Throw_Gamepad,
        Pause_Gamepad,
    }

    private PlayerInputMap inputActions;
    public event EventHandler OnInteractAction;
    public event EventHandler OnInteractAltAction;
    public event EventHandler OnThrowAction;
    public event EventHandler OnPauseAction;
    public event EventHandler OnRebindBindingCompleted;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        inputActions = new PlayerInputMap();

        if (PlayerPrefs.HasKey(PLAYER_PREF_BINDING_JSON))
        {
            inputActions.LoadBindingOverridesFromJson(PlayerPrefs.GetString(PLAYER_PREF_BINDING_JSON));
        }

        inputActions.Player.Enable();
    }

    private void OnEnable()
    {
        inputActions.Player.Interact.performed += OnInteract;
        inputActions.Player.InteractAlt.performed += OnInteractAlt;
        inputActions.Player.Throw.performed += OnThrow;
        inputActions.Player.Pause.performed += Pause_performed;
    }

    private void Pause_performed(InputAction.CallbackContext obj)
    {
        OnPauseAction?.Invoke(this, EventArgs.Empty);
    }

    private void OnInteractAlt(InputAction.CallbackContext obj)
    {
        OnInteractAltAction?.Invoke(this, EventArgs.Empty);
    }

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        OnInteractAction?.Invoke(this, EventArgs.Empty);
    }

    private void OnThrow(InputAction.CallbackContext ctx)
    {
        OnThrowAction?.Invoke(this, EventArgs.Empty);
    }

    private void OnDisable()
    {
        inputActions.Player.Interact.performed -= OnInteract;
        inputActions.Player.InteractAlt.performed -= OnInteractAlt;
        inputActions.Player.Pause.performed -= Pause_performed;
    }

    private void OnDestroy()
    {
        inputActions.Dispose();
    }

    public Vector2 GetInputVector()
    {
        Vector2 moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        return moveInput;
    }

    public bool IsDashTriggered()
    {
        return inputActions.Player.Dash.triggered;
    }
    public void RebindBindings(Bindings bindings, Action onRebindCompleted)
    {
        InputAction inputAction;
        int bindingIndex; // array index defined in input action asset under specific action

        switch (bindings)
        {
            default:
            case Bindings.Move_Up:
                inputAction = inputActions.Player.Move;
                bindingIndex = 1;
                break;
            case Bindings.Move_Down:
                inputAction = inputActions.Player.Move;
                bindingIndex = 3;
                break;
            case Bindings.Move_Left:
                inputAction = inputActions.Player.Move;
                bindingIndex = 5;
                break;
            case Bindings.Move_Right:
                inputAction = inputActions.Player.Move;
                bindingIndex = 7;
                break;
            case Bindings.Interact:
                inputAction = inputActions.Player.Interact;
                bindingIndex = 0;
                break;
            case Bindings.Interact_Alt:
                inputAction = inputActions.Player.InteractAlt;
                bindingIndex = 0;
                break;
            case Bindings.Dash:
                inputAction = inputActions.Player.Dash;
                bindingIndex = 0;
                break;
            case Bindings.Throw:
                inputAction = inputActions.Player.Throw;
                bindingIndex = 0;
                break;
            case Bindings.Pause:
                inputAction = inputActions.Player.Pause;
                bindingIndex = 0;
                break;
            case Bindings.Move_Gamepad:
                inputAction = inputActions.Player.Move;
                bindingIndex = 9;
                break;
            case Bindings.Interact_Gamepad:
                inputAction = inputActions.Player.Interact;
                bindingIndex = 1;
                break;
            case Bindings.Interact_Alt_Gamepad:
                inputAction = inputActions.Player.InteractAlt;
                bindingIndex = 1;
                break;
            case Bindings.Pause_Gamepad:
                inputAction = inputActions.Player.Pause;
                bindingIndex = 1;
                break;
            case Bindings.Dash_Gamepad:
                inputAction = inputActions.Player.Dash;
                bindingIndex = 1;
                break;
            case Bindings.Throw_Gamepad:
                inputAction = inputActions.Player.Throw;
                bindingIndex = 1;
                break;
        }

        inputActions.Player.Disable();

        inputAction.PerformInteractiveRebinding(bindingIndex)
            .WithControlsExcluding("<Gamepad>/rightTriggerButton")
            .WithControlsExcluding("<Gamepad>/leftTriggerButton")
            .OnComplete(callback =>
        {
            inputActions.Player.Enable();
            Debug.Log(callback.action.bindings[bindingIndex].ToString());
            callback.Dispose();

            var bindingsJson = inputActions.SaveBindingOverridesAsJson();
            PlayerPrefs.SetString(PLAYER_PREF_BINDING_JSON, bindingsJson);
            PlayerPrefs.Save();
            OnRebindBindingCompleted?.Invoke(this, EventArgs.Empty);
            onRebindCompleted();
        }).Start();
    }

    public void ResetToDefaultBindings()
    {
        inputActions.Player.Disable();

        inputActions.RemoveAllBindingOverrides();
        PlayerPrefs.DeleteKey(PLAYER_PREF_BINDING_JSON);

        inputActions.Player.Enable();

        Debug.Log(inputActions.Player.Dash.ToString());
    }

    public string GetBindingsText(Bindings bindings)
    {
        switch (bindings)
        {
            case Bindings.Move_Up:
                return inputActions.Player.Move.GetBindingDisplayString(bindingIndex: 1);
            case Bindings.Move_Down:
                return inputActions.Player.Move.GetBindingDisplayString(bindingIndex: 3);
            case Bindings.Move_Left:
                return inputActions.Player.Move.GetBindingDisplayString(bindingIndex: 5);
            case Bindings.Move_Right:
                return inputActions.Player.Move.GetBindingDisplayString(bindingIndex: 7);
            case Bindings.Interact:
                return inputActions.Player.Interact.GetBindingDisplayString(bindingIndex: 0);
            case Bindings.Interact_Alt:
                return inputActions.Player.InteractAlt.GetBindingDisplayString(bindingIndex: 0);
            case Bindings.Dash:
                return inputActions.Player.Dash.GetBindingDisplayString(bindingIndex: 0);
            case Bindings.Throw:
                return inputActions.Player.Throw.GetBindingDisplayString(bindingIndex: 0);
            case Bindings.Pause:
                return inputActions.Player.Pause.GetBindingDisplayString(bindingIndex: 0);
            case Bindings.Move_Gamepad:
                return inputActions.Player.Move.GetBindingDisplayString(bindingIndex: 9);
            case Bindings.Interact_Gamepad:
                return inputActions.Player.Interact.GetBindingDisplayString(bindingIndex: 1);
            case Bindings.Interact_Alt_Gamepad:
                return inputActions.Player.InteractAlt.GetBindingDisplayString(bindingIndex: 1);
            case Bindings.Pause_Gamepad:
                return inputActions.Player.Pause.GetBindingDisplayString(bindingIndex: 1);
            case Bindings.Dash_Gamepad:
                return inputActions.Player.Dash.GetBindingDisplayString(bindingIndex: 1);
            case Bindings.Throw_Gamepad:
                return inputActions.Player.Throw.GetBindingDisplayString(bindingIndex: 1);
            default:
                return string.Empty;
        }
    }
}
