using System;
using UnityEngine;

[CreateAssetMenu(fileName = "PassionMeterSO", menuName = "Scriptable Objects/PassionMeterSO")]
public class PassionMeterSO : ScriptableObject
{
    public PassionMeterLevelSetting[] passionMeterLevelSettings;
    public int passionMeterLevelCount => passionMeterLevelSettings.Length;
    public PassionMeterLevelSetting GetPassionMeterLevelSetting(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= passionMeterLevelCount)
        {
            Debug.LogError($"Invalid level index: {levelIndex}. Returning default PassionMeterLevelSetting.");
            return passionMeterLevelSettings[0];
        }
        return passionMeterLevelSettings[levelIndex];
    }
}

[Serializable]
public class PassionMeterLevelSetting
{
    public float minPassion;
    public float maxPassion;
    public float passionLostPerSecond;
    public float passionMultiplier;
}