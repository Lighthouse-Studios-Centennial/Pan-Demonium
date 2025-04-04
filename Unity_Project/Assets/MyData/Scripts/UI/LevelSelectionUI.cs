using System;
using UnityEngine;

public class LevelSelectionUI : MonoBehaviour
{
    [SerializeField] private LevelSelector levelSelector;
    [SerializeField] private LevelInfoSingleUI levelInfoTemplate;
    [SerializeField] private RectTransform levelInfoContainer;

    private void Awake()
    {
        Initialize(levelSelector.GetGameLevelCount());
    }

    private void Initialize(int levelCount)
    {
        for (int i = 0; i < levelCount; i++)
        {
            LevelInfoSingleUI levelInfo = Instantiate(levelInfoTemplate, levelInfoContainer);
            levelInfo.SetLevelIndex(i);
        }

        levelInfoTemplate.gameObject.SetActive(false);
    }
}
