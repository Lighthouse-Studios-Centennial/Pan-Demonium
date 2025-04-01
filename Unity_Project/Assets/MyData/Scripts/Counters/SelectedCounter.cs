using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedCounter : MonoBehaviour
{
    [SerializeField] private BaseCounter baseCounter;
    [SerializeField] private GameObject[] selectedVisualGO;

    private void Start()
    {
        if (PlayerController.LocalInstance)
            PlayerController.LocalInstance.OnSelectedCounterChanged += OnSelectedCounterChanged;
        else
            PlayerController.OnAnyPlayerSpawned += PlayerController_OnAnyPlayerSpawned;
    }

    private void OnDestroy()
    {
        if (PlayerController.LocalInstance)
            PlayerController.LocalInstance.OnSelectedCounterChanged -= OnSelectedCounterChanged;
        else
            PlayerController.OnAnyPlayerSpawned -= PlayerController_OnAnyPlayerSpawned;
    }

    private void PlayerController_OnAnyPlayerSpawned(object sender, System.EventArgs e)
    {
        if (PlayerController.LocalInstance)
        {
            PlayerController.LocalInstance.OnSelectedCounterChanged -= OnSelectedCounterChanged;
            PlayerController.LocalInstance.OnSelectedCounterChanged += OnSelectedCounterChanged;
        }
    }

    private void OnSelectedCounterChanged(object sender, PlayerController.OnSelectedCounterChangedEventArgs e)
    {
        if(e.selectedCounter == baseCounter)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    private void Show()
    {
        foreach(GameObject go in selectedVisualGO)
        {
            go.SetActive(true);
        }
    }

    private void Hide()
    {
        Debug.Log(transform.name);
        foreach (GameObject go in selectedVisualGO)
        {
            go.SetActive(false);
        }
    }
}
