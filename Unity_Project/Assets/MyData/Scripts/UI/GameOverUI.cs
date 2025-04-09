using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] GameObject[] showGOs;
    [SerializeField] private TextMeshProUGUI deliveredRecipeText;
    [SerializeField] private TextMeshProUGUI recipeEarningText;
    [SerializeField] private TextMeshProUGUI passionMeterText;
    [SerializeField] private TextMeshProUGUI failedRecipeText;
    [SerializeField] private TextMeshProUGUI failedRecipeEarningText;
    [SerializeField] private TextMeshProUGUI totalEarningText;
    [SerializeField] private Button continueBtn;

    private void Start()
    {
        GameManager.Instance.OnStateChanged += GameManager_OnStateChanged;
        continueBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.Shutdown();
            Loader.LoadScene(Loader.Scene.MainMenu);
        });
        Hide();
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnStateChanged -= GameManager_OnStateChanged;
    }

    private void GameManager_OnStateChanged(object sender, System.EventArgs e)
    {
        if (GameManager.Instance.IsGameOver())
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
        deliveredRecipeText.text = DeliveryManager.Instance.SuccessfulDeliveries.ToString();
        recipeEarningText.text = GameManager.Instance.RecipeEarnings.ToString();
        passionMeterText.text = GameManager.Instance.PassionMeterBonus.ToString();
        failedRecipeText.text = DeliveryManager.Instance.FailedDeliveries.ToString();
        var failEarning = GameManager.Instance.GetWrongRecipePenaltyForMoney() * DeliveryManager.Instance.FailedDeliveries;
        failedRecipeEarningText.text = (failEarning > 0 ? "-" : "") + (GameManager.Instance.GetWrongRecipePenaltyForMoney() * DeliveryManager.Instance.FailedDeliveries).ToString();
        totalEarningText.text = GameManager.Instance.GetTotalEarning().ToString();

        foreach (var go in showGOs)
        {
            go.SetActive(false);
        }

        gameObject.SetActive(true);

        StartCoroutine(ShowGOsOneByOne(0.5f));
    }

    IEnumerator ShowGOsOneByOne(float delayTime)
    {
        foreach (var go in showGOs)
        {
            go.SetActive(true);
            yield return new WaitForSeconds(delayTime);
        }
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
