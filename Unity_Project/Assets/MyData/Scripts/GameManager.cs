using System;
using System.Collections.Generic;
using UniRx;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public event EventHandler OnStateChanged;
    public event Action<bool> OnLocalPauseStatusChanged;
    public event Action<bool> OnMultiplayerPauseStateChanged;
    public event EventHandler OnLocalPlayerReadyChanged;

    private enum State
    {
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        GameOver,
    }

    [SerializeField] private Transform playerPrefab;
    [SerializeField] private LevelSettingSO levelSettingSO;
    [SerializeField] private GameObject[] levels;

    private PassionMeterLevelSetting currentPassionMeterLevelSetting;
    private int currentPassionMeterLevelSettingIndex = 0;

    private NetworkVariable<State> state = new(State.WaitingToStart);
    private bool isLocalPlayerReady;

    private NetworkVariable<float> countdownToStartCountdown = new(3f); // Default
    //private float countdownToStartCountdown = 1f; // Testing
    private NetworkVariable<float> gameplayTimer = new(0f);
    private NetworkVariable<int> totalEarnings = new(0);
    private NetworkVariable<float> passionMeter = new(0f);
    private ReactiveProperty<int> earningReactive = new(0);
    private ReactiveProperty<int> passionMeterReactive = new(0);
    private float maxGameplayTimer = 1 * 60f; // Default
    //private float maxGameplayTimer = 15 * 60f; // Testing
    private bool isLocalGamePaused = false;
    private NetworkVariable<bool> isGamePaused = new(false);

    private Dictionary<ulong, bool> playerReadyDictionary;
    private Dictionary<ulong, bool> playerPausedDictionary;
    private bool autoTestPauseStateAfterDisconnect;

    // Scoring
    public int RecipeEarnings = 0;
    public int PassionMeterBonus = 0;
    
    private void Awake()
    {
        Instance = this;

        playerReadyDictionary = new();
        playerPausedDictionary = new();
    }

    private void Start()
    {
        InputHandler.Instance.OnPauseAction += InputHandler_OnPauseAction;
        InputHandler.Instance.OnInteractAction += InputHandler_OnInteractAction;

        totalEarnings.OnValueChanged += (previousValue, newValue) =>
        {
            // Update UI or perform any action when total earnings change
            earningReactive.Value = newValue;
        };

        passionMeter.OnValueChanged += (previousValue, newValue) =>
        {
            // Update UI or perform any action when passion meter changes
            UpdateCurrentPassionMeterSetting();
            passionMeterReactive.Value = (int)newValue;
        };

        maxGameplayTimer = levelSettingSO.levelGameTime;
        UpdateCurrentPassionMeterSetting();

        // Enable Game Environment
        int selectedLevelIndex = KitchenGameMultiplayer.Instance.GetCurrentGameLevelIndex();

        if (selectedLevelIndex < levels.Length)
        {
            Debug.Log($"Selected Level Index: {selectedLevelIndex}");
        }
        else
        {
            Debug.LogError($"Selected Level Index {selectedLevelIndex} is out of bounds. Defaulting to 0.");
            selectedLevelIndex = 0;
        }

        for (int i = 0; i < levels.Length; i++)
        {
            levels[i].SetActive(i == selectedLevelIndex);
        }
    }

    public override void OnNetworkSpawn()
    {
        state.OnValueChanged += OnStateValueChanged;
        isGamePaused.OnValueChanged += OnGamePausedValueChanged;

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        }
    }

    private void SceneManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            var player = Instantiate(playerPrefab);
            var playerNetworkObject = player.GetComponent<NetworkObject>();
            playerNetworkObject.SpawnAsPlayerObject(clientId, true);
        }
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        if (IsServer)
        {
            autoTestPauseStateAfterDisconnect = true;
        }
    }

    private void OnGamePausedValueChanged(bool previousValue, bool newValue)
    {
        Time.timeScale = newValue ? 0f : 1f;
        OnMultiplayerPauseStateChanged?.Invoke(newValue);
    }

    private void OnStateValueChanged(State previousValue, State newValue)
    {
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void InputHandler_OnInteractAction(object sender, EventArgs e)
    {
        if (state.Value == State.WaitingToStart)
        {
            isLocalPlayerReady = true;

            OnLocalPlayerReadyChanged?.Invoke(this, EventArgs.Empty);

            SetPlayerReadyServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playerReadyDictionary[serverRpcParams.Receive.SenderClientId] = true;

        bool allClientsAreReady = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!playerReadyDictionary.ContainsKey(clientId) || !playerReadyDictionary[clientId])
            {
                allClientsAreReady = false;
                break;
            }
        }

        if (allClientsAreReady)
        {
            state.Value = State.CountdownToStart;
        }
    }

    private void InputHandler_OnPauseAction(object sender, EventArgs e)
    {
        ToggleGamePauseStatus();
    }

    public void ToggleGamePauseStatus()
    {
        isLocalGamePaused = !isLocalGamePaused;
        if (isLocalGamePaused)
        {
            SetGamePauseServerRpc();
        }
        else
        {
            SetGameUnpauseServerRpc();
        }
        OnLocalPauseStatusChanged?.Invoke(isLocalGamePaused);
    }

    private void Update()
    {
        if (!IsServer)
        {
            return;
        }

        switch (state.Value)
        {
            case State.WaitingToStart:
                break;

            case State.CountdownToStart:
                countdownToStartCountdown.Value -= Time.deltaTime;
                if (countdownToStartCountdown.Value < 0f)
                {
                    state.Value = State.GamePlaying;
                    gameplayTimer.Value = maxGameplayTimer;
                }
                break;

            case State.GamePlaying:
                gameplayTimer.Value -= Time.deltaTime;
                if (gameplayTimer.Value < 0f)
                {
                    state.Value = State.GameOver;
                }
                HandlePassionLostPerSecond();
                break;

            case State.GameOver:
                break;

            default:
                break;
        }
    }

    private void LateUpdate()
    {
        if (autoTestPauseStateAfterDisconnect)
        {
            autoTestPauseStateAfterDisconnect = false;
            TestPauseState();
        }
    }

    public bool IsGamePlaying() => state.Value == State.GamePlaying;

    public bool IsGameCountdownToStart() => state.Value == State.CountdownToStart;

    public bool IsGameOver() => state.Value == State.GameOver;

    public bool IsWaitingToStart() => state.Value == State.WaitingToStart;

    public float GetCountdownToStartTimer() => countdownToStartCountdown.Value;

    public bool IsLocalPlayerReady() => isLocalPlayerReady;

    public float GetPlayingTimerNormalized() => (gameplayTimer.Value / maxGameplayTimer);
    public float GetPlayingTimer() => gameplayTimer.Value;

    public ReactiveProperty<int> GetEarningReactive() => earningReactive;
    public ReactiveProperty<int> GetPassionMeterReactive() => passionMeterReactive;
    public float GetPassionMeterNormalized() => (passionMeter.Value / currentPassionMeterLevelSetting.maxPassion);
    public float GetPassionMeterMultiplier() => currentPassionMeterLevelSetting.passionMultiplier;
    public float GetRecipeLifeTime() => levelSettingSO.recipeLifeTime;
    public int GetWaitingRecipeMax() => levelSettingSO.waitingRecipeMax;
    public float GetDeliverRecipeInterval() => levelSettingSO.deliverRecipeInterval;
    public int GetWrongRecipePenaltyForMoney() => levelSettingSO.wrongRecipePenaltyForMoney;
    public int GetTotalEarning() => totalEarnings.Value;


    [ServerRpc(RequireOwnership = false)]
    private void SetGamePauseServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playerPausedDictionary[serverRpcParams.Receive.SenderClientId] = true;

        TestPauseState();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetGameUnpauseServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playerPausedDictionary[serverRpcParams.Receive.SenderClientId] = false;

        TestPauseState();
    }

    [ServerRpc(RequireOwnership = false)]
    public void DeliveredCorrectRecipeServerRpc(int worth, int passionValue)
    {
        var totalEarning = worth * (int)currentPassionMeterLevelSetting.passionMultiplier;
        PassionMeterBonus += totalEarning - worth;
        RecipeEarnings += worth;
        totalEarnings.Value += totalEarning;
        passionMeter.Value += passionValue;
    }

    [ServerRpc(RequireOwnership = false)]
    public void DeliveredIncorrectRecipeServerRpc()
    {
        var passionValue = passionMeter.Value;
        if (passionValue > levelSettingSO.wrongRecipePenaltyForPassionMeter)
        {
            passionMeter.Value = passionValue - levelSettingSO.wrongRecipePenaltyForPassionMeter;
        }
        else
        {
            passionMeter.Value = 0f;
        }

        totalEarnings.Value -= levelSettingSO.wrongRecipePenaltyForMoney;
    }

    private void TestPauseState()
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (playerPausedDictionary.ContainsKey(clientId) && playerPausedDictionary[clientId])
            {
                // atleast 1 player paused the game
                isGamePaused.Value = true;
                return;
            }
        }

        isGamePaused.Value = false;
        // all players are unpaused
    }

    private void UpdateCurrentPassionMeterSetting()
    {
        if (currentPassionMeterLevelSetting == null)
        {
            currentPassionMeterLevelSetting = levelSettingSO.passionMeterSO.GetPassionMeterLevelSetting(currentPassionMeterLevelSettingIndex);
        }
        else
        {
            if (passionMeter.Value >= currentPassionMeterLevelSetting.maxPassion)
            {
                currentPassionMeterLevelSettingIndex++;
                if (currentPassionMeterLevelSettingIndex < levelSettingSO.passionMeterSO.passionMeterLevelCount)
                {
                    currentPassionMeterLevelSetting = levelSettingSO.passionMeterSO.GetPassionMeterLevelSetting(currentPassionMeterLevelSettingIndex);
                    passionMeter.Value += 50f;
                }
                else
                {
                    // max passion reached
                }
            }
            else if (passionMeter.Value < currentPassionMeterLevelSetting.minPassion)
            {
                currentPassionMeterLevelSettingIndex--;
                if (currentPassionMeterLevelSettingIndex >= 0)
                {
                    currentPassionMeterLevelSetting = levelSettingSO.passionMeterSO.GetPassionMeterLevelSetting(currentPassionMeterLevelSettingIndex);
                }
                else
                {
                    // min passion reached
                }
            }
        }
    }

    private void HandlePassionLostPerSecond()
    {
        if (currentPassionMeterLevelSetting != null)
        {
            var pmv = passionMeter.Value;
            var lost = currentPassionMeterLevelSetting.passionLostPerSecond * Time.deltaTime;
            if (pmv >= lost)
            {
                passionMeter.Value -= lost;
            }
            else
            {
                passionMeter.Value = 0;
            }
        }
    }
}
