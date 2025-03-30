using System.Collections.Generic;
using UnityEngine;

public class DeliveryManagerUI : MonoBehaviour
{
    [SerializeField] private Transform recipeContainer;
    [SerializeField] private Transform recipeTemplate;

    bool isRecipeSpawned = false;
    List<RecipeSO> curWaitingRecipiesList = new List<RecipeSO>();
    List<RecipeSingleUI> curRecipeUIList = new List<RecipeSingleUI>();

    private void Start()
    {
        recipeTemplate.gameObject.SetActive(false);
        DeliveryManager.Instance.OnRecipeSpawned += DeliveryManager_OnRecipeSpawned;
        DeliveryManager.Instance.OnRecipeCompleted += DeliveryManager_OnRecipeCompleted;
    }

    private void DeliveryManager_OnRecipeCompleted(object sender, System.EventArgs e)
    {
        UpdateVisual();
    }

    private void DeliveryManager_OnRecipeSpawned(object sender, System.EventArgs e)
    {
        UpdateVisual();
    }

    public void UpdateVisual()
    {
        var waitingRecipiesList = DeliveryManager.Instance.GetWaitingRecipeSOList();

        for(int i = 0; i < curRecipeUIList.Count; i++)
        {
            if (waitingRecipiesList.Exists(x => x.recipeId == curRecipeUIList[i].GetRecipeId()))
            {
                continue;
            }
            curRecipeUIList[i].Deactivate();
            curRecipeUIList.RemoveAt(i);
            i--;
        }

        for (int i = 0; i < waitingRecipiesList.Count; i++)
        {
            if(curRecipeUIList.Exists(x => x.GetRecipeId() == waitingRecipiesList[i].recipeId))
            {
                continue;
            }
            var waitingRecipeUI = Instantiate(recipeTemplate, recipeContainer).GetComponent<RecipeSingleUI>();
            waitingRecipeUI.SetRecipeSO(waitingRecipiesList[i]);
            waitingRecipeUI.SetLifeTime(DeliveryManager.Instance.GetRecipeLifeTime());
            waitingRecipeUI.Activate();

            curRecipeUIList.Add(waitingRecipeUI);
        }
    }
}
