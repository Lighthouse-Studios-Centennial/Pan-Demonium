using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System.Runtime.CompilerServices;

public class RecipeSingleUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI recipeNameText;
    [SerializeField] private Transform recipeIconContainer;
    [SerializeField] private Transform recipeIconTemplate;
    [SerializeField] private Image timerBar;
    [SerializeField] private Color timerBarMaxColor;
    [SerializeField] private Color timerBarMinColor;

    [Header("Animations")]
    [SerializeField] DOTweenAnimation shakeAnimation;
    [SerializeField] DOTweenAnimation completeAnimation;
    [SerializeField] DOTweenAnimation outDatedAnimation;

    private bool isActivated = false;
    RecipeSO recipeSO;

    private void Awake()
    {
        recipeIconTemplate.gameObject.SetActive(false);
    }

    public void SetRecipeSO(RecipeSO recipeSO)
    {
        this.recipeSO = recipeSO;
        recipeNameText.text = recipeSO.recipeName;

        foreach (Transform child in recipeIconContainer)
        {
            if (child == recipeIconTemplate)
                continue;

            Destroy(child.gameObject);
        }

        foreach (var recipe in recipeSO.kitchenObjectSOList)
        {
            var iconUI = Instantiate(recipeIconTemplate, recipeIconContainer);
            iconUI.gameObject.SetActive(true);
            iconUI.GetChild(0).GetComponent<Image>().sprite = recipe.icon;
        }
    }

    public void SetLifeTime(float lifeTime)
    {
        recipeSO.SetMaxRecipeLifeTime(lifeTime);
    }

    public void Activate()
    {
        gameObject.SetActive(true);
        isActivated = true;
    }

    public int GetRecipeId()
    {
        return recipeSO.recipeId;
    }

    private void Update()
    {
        if (!isActivated || GameManager.Instance.IsGameOver())
            return;

        recipeSO.recipeLifeTime -= Time.deltaTime;
        float progress = recipeSO.recipeLifeTime / recipeSO.maxRecipeLifeTime;

        timerBar.fillAmount = progress;
        timerBar.color = Color.Lerp(timerBarMinColor, timerBarMaxColor, progress);

        if(progress < 0.33f)
        {
            shakeAnimation.DOPlay();
        }

        if (recipeSO.recipeLifeTime <= 0)
        {
            DeliveryManager.Instance.RecipeOutdatedServerRpc(recipeSO.recipeId);
            isActivated = false;
        }
    }

    public void Deactivate()
    {
        if(recipeSO.recipeLifeTime > 0)
        {
            // Recipe is completed
            completeAnimation.gameObject.SetActive(true);
        }
        else
        {
            // Recipe is outdated
            outDatedAnimation.gameObject.SetActive(true);
        }
    }

    public void DestroyGO()
    {
        Destroy(gameObject);
    }
}
