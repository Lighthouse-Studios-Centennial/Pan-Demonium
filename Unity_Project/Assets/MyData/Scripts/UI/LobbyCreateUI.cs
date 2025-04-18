using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyCreateUI : MonoBehaviour
{
    [SerializeField] private Button createPublicBtn;
    [SerializeField] private Button createPrivateBtn;
    [SerializeField] private Button closeBtn;
    [SerializeField] private TMP_InputField lobbyNameInput;

    private string lobbyName = string.Empty;

    private void Awake()
    {
        lobbyName = $"Lobby #{Random.Range(0, int.MaxValue)}";
        lobbyNameInput.text = lobbyName;

        closeBtn.onClick.AddListener(Hide);

        createPrivateBtn.onClick.AddListener(() =>
        {
            KitchenGameLobby.Instance.CreateLobby(lobbyName, true);
        });
        
        createPublicBtn.onClick.AddListener(() =>
        {
            KitchenGameLobby.Instance.CreateLobby(lobbyName, false);
        });

        lobbyNameInput.onValueChanged.AddListener(newLobbyName =>
        {
            lobbyName = newLobbyName;
        });
    }

    private void Start()
    {
        Hide();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        createPublicBtn.Select();
    }
}
