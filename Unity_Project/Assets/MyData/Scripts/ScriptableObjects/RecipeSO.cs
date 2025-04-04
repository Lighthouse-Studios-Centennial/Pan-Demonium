using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

[CreateAssetMenu()]
public class RecipeSO : ScriptableObject
{
    public string recipeName;
    public List<KitchenObjectSO> kitchenObjectSOList;

    [HideInInspector]public int recipeId;
    [HideInInspector]public float recipeLifeTime;
    [HideInInspector]public float maxRecipeLifeTime;

    public int maxWorth;
    public int midWorth;
    public int minWorth;
    public int passionValue;

    public int GetWorth()
    {
        if (recipeLifeTime > maxRecipeLifeTime * 0.66f)
        {
            return maxWorth;
        }
        else if (recipeLifeTime > maxRecipeLifeTime * 0.33f)
        {
            return midWorth;
        }
        else
        {
            return minWorth;
        }
    }

    public void SetMaxRecipeLifeTime(float maxRecipeLifeTime)
    {
        this.maxRecipeLifeTime = maxRecipeLifeTime;
        recipeLifeTime = maxRecipeLifeTime;
    }
}
