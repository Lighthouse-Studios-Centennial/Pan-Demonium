using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitingForOtherPlayerUI : MonoBehaviour
{
    private void Start()
    {
        if (KitchenGameMultiplayer.playMultiplayer)
        {
            GameManager.Instance.OnLocalPlayerReadyChanged += GameManager_OnLocalPlayerReadyChanged;
            GameManager.Instance.OnStateChanged += GameManager_OnStateChanged;
        }

        Hide();
    }

    private void GameManager_OnStateChanged(object sender, System.EventArgs e)
    {
        if (GameManager.Instance.IsGameCountdownToStart())
        {
            Hide();
        }
    }

    private void GameManager_OnLocalPlayerReadyChanged(object sender, System.EventArgs e)
    {
        if (GameManager.Instance.IsLocalPlayerReady())
        {
            Show();
        }
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
