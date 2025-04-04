using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class EarningUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI earningText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.Instance.GetEarningReactive().Subscribe(x =>
        {
            earningText.text = x.ToString();
        }).AddTo(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
