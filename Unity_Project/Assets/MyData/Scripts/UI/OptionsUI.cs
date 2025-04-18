using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionsUI : MonoBehaviour
{
    public static OptionsUI Instance { get; private set; }

    [SerializeField] private Button sfxBtn;
    [SerializeField] private Button musicBtn;

    [SerializeField] private TextMeshProUGUI sfxText;
    [SerializeField] private TextMeshProUGUI musicText;

    [Header("Key Rebindings")]
    [SerializeField] private GameObject pressToRebindUI;
    [SerializeField] private Button resetToDefaultBindingBtn;

    [Header("Rebinding Buttons")]
    [SerializeField] private Button moveUpBtn;
    [SerializeField] private Button moveDownBtn;
    [SerializeField] private Button moveLeftBtn;
    [SerializeField] private Button moveRightBtn;
    [SerializeField] private Button interactBtn;
    [SerializeField] private Button interactBtn_Gamepad;
    [SerializeField] private Button interactAltBtn;
    [SerializeField] private Button interactAltBtn_Gamepad;
    [SerializeField] private Button pauseBtn;
    [SerializeField] private Button dashBtn;
    [SerializeField] private Button dashBtn_Gamepad;
    [SerializeField] private Button throwBtn;
    [SerializeField] private Button throwBtn_Gamepad;

    [Header("Rebinding Texts")]
    [SerializeField] private TextMeshProUGUI moveUpText;
    [SerializeField] private TextMeshProUGUI moveDownText;
    [SerializeField] private TextMeshProUGUI moveLeftText;
    [SerializeField] private TextMeshProUGUI moveRightText;
    [SerializeField] private TextMeshProUGUI interactText;
    [SerializeField] private TextMeshProUGUI interactText_Gamepad;
    [SerializeField] private TextMeshProUGUI interactAltText;
    [SerializeField] private TextMeshProUGUI interactAltText_Gamepad;
    [SerializeField] private TextMeshProUGUI pauseText;
    [SerializeField] private TextMeshProUGUI dashText;
    [SerializeField] private TextMeshProUGUI dashText_Gamepad;
    [SerializeField] private TextMeshProUGUI throwText;
    [SerializeField] private TextMeshProUGUI throwText_Gamepad;

    private void Awake()
    {
        Instance = this;
        sfxBtn.onClick.AddListener(() =>
        {
            AudioManager.Instance.ChangeVolume();
            UpdateVisuals();
        });

        musicBtn.onClick.AddListener(() =>
        {
            MusicManager.Instance.ChangeVolume();
            UpdateVisuals();
        });

        resetToDefaultBindingBtn.onClick.AddListener(() => 
        {
            InputHandler.Instance.ResetToDefaultBindings();
            UpdateVisuals();
        });

        moveUpBtn.onClick.AddListener(() => RebindBindings(InputHandler.Bindings.Move_Up));
        moveDownBtn.onClick.AddListener(() => RebindBindings(InputHandler.Bindings.Move_Down));
        moveLeftBtn.onClick.AddListener(() => RebindBindings(InputHandler.Bindings.Move_Left));
        moveRightBtn.onClick.AddListener(() => RebindBindings(InputHandler.Bindings.Move_Right));
        interactBtn.onClick.AddListener(() => RebindBindings(InputHandler.Bindings.Interact));
        interactAltBtn.onClick.AddListener(() => RebindBindings(InputHandler.Bindings.Interact_Alt));
        pauseBtn.onClick.AddListener(() => RebindBindings(InputHandler.Bindings.Pause));

        interactBtn_Gamepad.onClick.AddListener(() => RebindBindings(InputHandler.Bindings.Interact_Gamepad));
        interactAltBtn_Gamepad.onClick.AddListener(() => RebindBindings(InputHandler.Bindings.Interact_Alt_Gamepad));

        throwBtn.onClick.AddListener(() => RebindBindings(InputHandler.Bindings.Throw));
        throwBtn_Gamepad.onClick.AddListener(() => RebindBindings(InputHandler.Bindings.Throw_Gamepad));

        dashBtn.onClick.AddListener(() => RebindBindings(InputHandler.Bindings.Dash));
        dashBtn_Gamepad.onClick.AddListener(() => RebindBindings(InputHandler.Bindings.Dash_Gamepad));
    }

    private void Start()
    {
        GameManager.Instance.OnLocalPauseStatusChanged += GameManager_OnPauseStatusChanged;

        UpdateVisuals();

        HidePressToRebindUI();
        Hide();
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnLocalPauseStatusChanged -= GameManager_OnPauseStatusChanged;
    }

    private void GameManager_OnPauseStatusChanged(bool isPaused)
    {
        if (!isPaused)
        {
            Hide();
        }
    }

    private void UpdateVisuals()
    {
        sfxText.text = $"Sound Effects: {Mathf.RoundToInt(AudioManager.Instance.GetVolume() * 10)}";
        musicText.text = $"Music: {Mathf.RoundToInt(MusicManager.Instance.GetVolume() * 10)}";

        moveUpText.text = InputHandler.Instance.GetBindingsText(InputHandler.Bindings.Move_Up);
        moveDownText.text = InputHandler.Instance.GetBindingsText(InputHandler.Bindings.Move_Down);
        moveLeftText.text = InputHandler.Instance.GetBindingsText(InputHandler.Bindings.Move_Left);
        moveRightText.text = InputHandler.Instance.GetBindingsText(InputHandler.Bindings.Move_Right);
        interactText.text = InputHandler.Instance.GetBindingsText(InputHandler.Bindings.Interact);
        interactAltText.text = InputHandler.Instance.GetBindingsText(InputHandler.Bindings.Interact_Alt);
        pauseText.text = InputHandler.Instance.GetBindingsText(InputHandler.Bindings.Pause);

        interactText_Gamepad.text = InputHandler.Instance.GetBindingsText(InputHandler.Bindings.Interact_Gamepad);
        interactAltText_Gamepad.text = InputHandler.Instance.GetBindingsText(InputHandler.Bindings.Interact_Alt_Gamepad);

        throwText.text = InputHandler.Instance.GetBindingsText(InputHandler.Bindings.Throw);
        throwText_Gamepad.text = InputHandler.Instance.GetBindingsText(InputHandler.Bindings.Throw_Gamepad);

        dashText.text = InputHandler.Instance.GetBindingsText(InputHandler.Bindings.Dash);
        dashText_Gamepad.text = InputHandler.Instance.GetBindingsText(InputHandler.Bindings.Dash_Gamepad);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        sfxBtn.Select();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void ShowPressToRebindUI()
    {
        pressToRebindUI.SetActive(true);
    }

    public void HidePressToRebindUI()
    {
        pressToRebindUI.SetActive(false);
    }

    private void RebindBindings(InputHandler.Bindings bindings)
    {
        ShowPressToRebindUI();
        InputHandler.Instance.RebindBindings(bindings, () =>
        {
            HidePressToRebindUI();
            UpdateVisuals();
        });
    }
}
