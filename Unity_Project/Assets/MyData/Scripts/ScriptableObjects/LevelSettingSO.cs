using UnityEngine;

[CreateAssetMenu(fileName = "LevelSettingSO", menuName = "Scriptable Objects/LevelSettingSO")]
public class LevelSettingSO : ScriptableObject
{
    public int levelId;
    public string levelName;
    public int levelGameTime; // in seconds
    public float recipeLifeTime; // in seconds
    public int waitingRecipeMax;
    public float deliverRecipeInterval; // in seconds
    public int wrongRecipePenaltyForPassionMeter; // in seconds
    public PassionMeterSO passionMeterSO;
}
