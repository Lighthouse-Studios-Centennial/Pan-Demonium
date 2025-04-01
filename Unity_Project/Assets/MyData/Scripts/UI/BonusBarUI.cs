using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class BonusBarUI : MonoBehaviour
{
    [SerializeField] Image bonusBar;
    [SerializeField] TextMeshProUGUI mulittiplierText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.Instance.GetPassionMeterReactive().Subscribe(x =>
        {
            // if(!GameManager.Instance.IsGamePlaying()) return;

            // Debug.Log("Passion Meter: " + bonusBar.fillAmount);
            // bonusBar.fillAmount = GameManager.Instance.GetPassionMeterNormalized();
            // mulittiplierText.text = "x" + GameManager.Instance.GetPassionMeterMultiplier().ToString("F1");
        }).AddTo(this);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (!GameManager.Instance.IsGamePlaying()) return;

        bonusBar.fillAmount = GameManager.Instance.GetPassionMeterNormalized();
        mulittiplierText.text = "x" + GameManager.Instance.GetPassionMeterMultiplier().ToString("F1");
    }

}
