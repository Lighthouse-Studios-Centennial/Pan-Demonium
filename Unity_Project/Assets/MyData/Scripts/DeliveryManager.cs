using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class DeliveryManager : NetworkBehaviour
{
    public event EventHandler OnRecipeSpawned;
    public event EventHandler OnRecipeCompleted;

    public event EventHandler OnRecipeSuccess;
    public event EventHandler OnRecipeFailed;

    public static DeliveryManager Instance { get; private set; }

    [SerializeField] private RecipeListSO recipeListSO;
    [SerializeField] float recipeLifeTime = 60f;

    private List<RecipeSO> waitingRecipiesList = new();

    public int SuccessfulDeliveries { get; private set; }
    public int FailedDeliveries { get; private set; }

    private int waitingRecipeMax = 6;
    private float recipeTimer = 4f;
    private float recipeTimerMax = 4f;
    private int recipeIdCounter = 0;
    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        recipeLifeTime = GameManager.Instance.GetRecipeLifeTime();
        waitingRecipeMax = GameManager.Instance.GetWaitingRecipeMax();
        recipeTimerMax = GameManager.Instance.GetDeliverRecipeInterval();
        recipeTimer = recipeTimerMax;
    }
    private void Update()
    {
        if (!IsServer)
        {
            return;
        }

        if (!GameManager.Instance.IsGamePlaying())
            return;

        if (waitingRecipiesList.Count >= waitingRecipeMax)
            return;

        recipeTimer -= Time.deltaTime;
        if (recipeTimer < 0)
        {
            recipeTimer = recipeTimerMax;
            int waitingRecipeSOIndex = Random.Range(0, recipeListSO.Recipes.Count);
            DeliverRecipeServerRpc(waitingRecipeSOIndex);
        }
    }

    [ServerRpc]
    private void DeliverRecipeServerRpc(int waitingRecipeSOIndex)
    {
        DeliverRecipeClientRpc(waitingRecipeSOIndex);
    }

    [ClientRpc]
    private void DeliverRecipeClientRpc(int waitingRecipeSOIndex)
    {
        var waitingRecipeSO = Instantiate(recipeListSO.Recipes[waitingRecipeSOIndex]);
        waitingRecipeSO.recipeId = recipeIdCounter++;
        waitingRecipeSO.recipeLifeTime = recipeLifeTime;
        Debug.Log(waitingRecipeSO.recipeName);
        waitingRecipiesList.Add(waitingRecipeSO);

        OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
    }

    public void DelieverRecipe(PlateKitchenObject plateKitchenObject)
    {
        for (int i = 0; i< waitingRecipiesList.Count; i++)
        {
            var waitingRecipeSO = waitingRecipiesList[i];
            if(waitingRecipeSO.kitchenObjectSOList.Count == plateKitchenObject.GetKitchenObjectList().Count)
            {
                bool isValidDelievery = true;
                // Has same number of ingredient
                foreach (var requiredIngredient in waitingRecipeSO.kitchenObjectSOList)
                {
                    bool isFound = false;
                    foreach (var currentIngredient in plateKitchenObject.GetKitchenObjectList())
                    {
                        if(currentIngredient == requiredIngredient)
                        {
                            // Matches
                            isFound = true;
                            break;
                        }
                    }

                    if (!isFound)
                    {
                        // The recipe is not found on plate
                        isValidDelievery = false;
                    }
                }

                if (isValidDelievery)
                {
                    // Player delievered correct recipe!
                    DeliverCorrectRecipeServerRpc(i);
                    return;
                }
            }
        }

        // Player did not delievered correct recipe!
        DeliverIncorrectRecipeServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void DeliverIncorrectRecipeServerRpc()
    {
        GameManager.Instance.DeliveredIncorrectRecipeServerRpc();

        DeliverIncorrectRecipeClientRpc();
    }

    [ClientRpc]
    private void DeliverIncorrectRecipeClientRpc()
    {

        FailedDeliveries++;
        OnRecipeFailed?.Invoke(this, EventArgs.Empty);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DeliverCorrectRecipeServerRpc(int waitingRecipeSOIndex)
    {
        var recipeSO = waitingRecipiesList[waitingRecipeSOIndex];
        GameManager.Instance.DeliveredCorrectRecipeServerRpc(recipeSO.GetWorth(), recipeSO.passionValue);

        DeliverCorrectRecipeClientRpc(waitingRecipeSOIndex);
    }

    [ClientRpc]
    private void DeliverCorrectRecipeClientRpc(int waitingRecipeSOIndex)
    {

        SuccessfulDeliveries++;
        waitingRecipiesList.RemoveAt(waitingRecipeSOIndex);
        OnRecipeCompleted?.Invoke(this, EventArgs.Empty);
        OnRecipeSuccess?.Invoke(this, EventArgs.Empty);
    }

    [ServerRpc(RequireOwnership = true)]
    public void RecipeOutdatedServerRpc(int recipeId)
    {
        GameManager.Instance.DeliveredIncorrectRecipeServerRpc();

        int waitingRecipeSOIndex = waitingRecipiesList.FindIndex(x => x.recipeId == recipeId);
        RecipeOutdatedClientRpc(waitingRecipeSOIndex);
    }

    [ClientRpc]
    private void RecipeOutdatedClientRpc(int waitingRecipeSOIndex)
    {
        FailedDeliveries++;
        waitingRecipiesList.RemoveAt(waitingRecipeSOIndex);
        OnRecipeCompleted?.Invoke(this, EventArgs.Empty);
    }

    public List<RecipeSO> GetWaitingRecipeSOList()
    {
        return waitingRecipiesList;
    }

    public float GetRecipeLifeTime()
    {
        return recipeLifeTime;
    }
}
