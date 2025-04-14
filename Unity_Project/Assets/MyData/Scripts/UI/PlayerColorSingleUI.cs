using UnityEngine;
using UnityEngine.UI;

public class PlayerColorSingleUI : MonoBehaviour
{
    [SerializeField] private int colorId;
    [SerializeField] private Image colorImage;
    [SerializeField] private GameObject selectedGO;
    [SerializeField] private Color selectedColor = Color.red * (Color.green * .5f);
    [SerializeField] private Color unselectedColor = Color.white;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(() => KitchenGameMultiplayer.Instance.ChangePlayerColor(colorId));

        KitchenGameMultiplayer.Instance.OnPlayerDataNetworkListChanged += KitchenGameMultiplayer_OnPlayerDataNetworkListChanged;
        colorImage.color = KitchenGameMultiplayer.Instance.GetPlayerColor(colorId);
        UpdateIsSelected();
    }

    private void OnDestroy()
    {
        KitchenGameMultiplayer.Instance.OnPlayerDataNetworkListChanged -= KitchenGameMultiplayer_OnPlayerDataNetworkListChanged;
    }

    public void SetColorId(int colorId)
    {
        this.colorId = colorId;
        colorImage.color = KitchenGameMultiplayer.Instance.GetPlayerColor(colorId);
        UpdateIsSelected();
    }

    private void KitchenGameMultiplayer_OnPlayerDataNetworkListChanged(object sender, System.EventArgs e)
    {
        UpdateIsSelected();
    }

    public void UpdateIsSelected()
    {
        if (KitchenGameMultiplayer.Instance.GetLocalPlayerData().colorId == colorId)
        {
            selectedGO.GetComponent<Image>().color = selectedColor;
        }
        else
        {
            selectedGO.GetComponent<Image>().color = unselectedColor;
        }
    }
}
