using System;
using Unity.Netcode;
using UnityEngine;

public class LevelSelector : MonoBehaviour
{
    [SerializeField] private GameObject levelSelectorUI;
    [SerializeField] private GameObject[] gameLevels;

    private void OnEnable()
    {
        KitchenGameMultiplayer.Instance.OnGameLevelIndexChanged += KitchenGameMultiplayer_OnGameLevelIndexChanged;
        Debug.Log("LevelSelector enabled");
    }

    private void OnDisable()
    {
        KitchenGameMultiplayer.Instance.OnGameLevelIndexChanged -= KitchenGameMultiplayer_OnGameLevelIndexChanged;
    }

    private void KitchenGameMultiplayer_OnGameLevelIndexChanged(int selectedLevelIndex)
    {
        SelectGameLevel(selectedLevelIndex);
        Debug.Log("Selected level index: " + selectedLevelIndex);
    }

    void Start()
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            levelSelectorUI.SetActive(false);
            return;
        }

        KitchenGameMultiplayer.Instance.ChangeGameLevel(0);
    }

    public int GetGameLevelCount()
    {
        return gameLevels.Length;
    }

    private void SelectGameLevel(int levelId)
    {
        for (int i = 0; i < gameLevels.Length; i++)
        {
            gameLevels[i].SetActive(i == levelId);
        }
    }
}
