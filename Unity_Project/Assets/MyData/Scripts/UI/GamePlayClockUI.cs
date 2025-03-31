using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class GamePlayClockUI : MonoBehaviour
{
    [SerializeField] private GameObject clockAlermObj;
    [SerializeField] private Image clockImg;
    [SerializeField] private TextMeshProUGUI timerText;

    private Sequence clockAlermTween;

    private void Awake()
    {
        clockAlermTween = DOTween.Sequence();
        clockAlermTween.Append(clockAlermObj.transform.DORotate(new Vector3(0, 0, 10), 0.1f).SetEase(Ease.Linear));
        clockAlermTween.Append(clockAlermObj.transform.DORotate(new Vector3(0, 0, -20), 0.2f).SetEase(Ease.Linear));
        clockAlermTween.Append(clockAlermObj.transform.DORotate(new Vector3(0, 0, 20), 0.2f).SetEase(Ease.Linear));
        clockAlermTween.Append(clockAlermObj.transform.DORotate(new Vector3(0, 0, -10), 0.1f).SetEase(Ease.Linear));
        clockAlermTween.AppendInterval(0.5f);
        clockAlermTween.SetLoops(-1);

        clockAlermTween.Pause();
    }

    private void Update()
    {
        if (!GameManager.Instance.IsGamePlaying()) return;

        clockImg.fillAmount = GameManager.Instance.GetPlayingTimerNormalized();
        timerText.text = string.Format("{0:00}:{1:00}", Mathf.FloorToInt(GameManager.Instance.GetPlayingTimer() / 60f), Mathf.FloorToInt(GameManager.Instance.GetPlayingTimer() % 60f));

        if (GameManager.Instance.GetPlayingTimer() <= 30f)
        {
            if (!clockAlermTween.IsPlaying())
            {
                clockAlermTween.Play();
            }
        }
        else
        {
            if (clockAlermTween.IsPlaying())
            {
                clockAlermTween.Pause();
            }
        }
    }
}
