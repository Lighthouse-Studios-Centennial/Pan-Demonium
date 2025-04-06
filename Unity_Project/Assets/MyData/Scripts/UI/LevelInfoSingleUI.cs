using UnityEngine;
using UnityEngine.UI;

public class LevelInfoSingleUI : MonoBehaviour
{
    [SerializeField] private GameObject selectedGO;

    [SerializeField] private int levelIndex = -1;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() => KitchenGameMultiplayer.Instance.ChangeGameLevel(levelIndex));
    }

    private void OnEnable()
    {
        KitchenGameMultiplayer.Instance.OnGameLevelIndexChanged += KitchenGameMultiplayer_OnGameLevelIndexChanged;
        Debug.Log("LevelInfoSingleUI enabled");
    }

    private void OnDisable()
    {
        KitchenGameMultiplayer.Instance.OnGameLevelIndexChanged -= KitchenGameMultiplayer_OnGameLevelIndexChanged;
    }

    private void KitchenGameMultiplayer_OnGameLevelIndexChanged(int selectedLevelIndex)
    {
        selectedGO.SetActive(selectedLevelIndex == levelIndex);
        Debug.Log("Selected level index from Single ui: " + selectedLevelIndex);
    }
}
