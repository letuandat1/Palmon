using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

/// <summary>
/// Central coordinator for game systems and save/load operations
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField] private Vector3 firstPlayPosition;
    [SerializeField] private Quaternion firstPlayRotation;
    [SerializeField] private ToolSO startingTool;

    [Header("Game Data")]
    [SerializeField] private GameData activeGameData;

    [Header("Initial Data")]


    // Other systems
    [Header("Other Systems")]
    public bool IsGameJustLaunched { get; private set; } = true;
    public void SetIsGameJustLaunchedFalse()
    {
        IsGameJustLaunched = false;
    }

    // Events
    // public System.Action<int> OnGameDayChanged;

    #region Unity Lifecycle

    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        LoadOrCreateGameData();
    }

    #endregion

    #region Load Data

    private void LoadOrCreateGameData()
    {
        // Try to load from save file first
        var loadedData = SaveLoadSystem.LoadGameFromFile(this);
        if (loadedData != null)
        {
            activeGameData = loadedData;
        }
        else
        {
            var newPlayerData = new PlayerSaveData(100, 100, firstPlayPosition, firstPlayRotation);
            var newToolData = new ToolSaveData(startingTool.ID, 0);
            // Create new game data
            activeGameData = new GameData(newPlayerData, newToolData);
            Debug.Log("No save file found. Created new game data.");
            // Initialize other systems as needed
        }
        // Log active game data for debugging
        Debug.Log(activeGameData);
    }

    public PlayerSaveData GetActivePlayerSaveData()
    {
        if (activeGameData != null)
        {
            return activeGameData.PlayerData;
        }
        return null;
    }

    public ToolSaveData GetActiveToolSaveData()
    {
        if (activeGameData != null)
        {
            return activeGameData.ToolData;
        }
        return null;
    }

    #endregion

    #region Save Data

    public void SaveGameData()
    {
        if (activeGameData != null)
        {
            // Update tool data
            var toolSaveData = Player.Instance.GetToolSaveData();
            if (toolSaveData == null)
            {
                throw new System.Exception("Player returned null ToolSaveData during save operation.");
            }
            activeGameData.SetToolData(toolSaveData, this);

            // Update player data
            var playerSaveData = Player.Instance.GetPlayerSaveData();
            if (playerSaveData == null)
            {
                throw new System.Exception("Player returned null PlayerSaveData during save operation.");
            }
            activeGameData.SetPlayerData(playerSaveData, this);

            // ✅ SAVE TO FILE
            SaveLoadSystem.SaveGameToFile(this, activeGameData);
        }
    }

    #endregion

    #region Auto-Save Events

    private bool CheckActiveSceneIsGameplay()
    {
        // Implement scene check logic here if needed
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == SceneNameManager.SCENE_GAMEPLAY)
        {
            return true;
        }
        return false;
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (!CheckActiveSceneIsGameplay())
        {
            return;
        }
        // Only auto-save if we have valid game data AND game is actually running
        if (pauseStatus && activeGameData != null && Time.time > 5f) // 5 seconds after start
        {
            SaveGameData();
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!CheckActiveSceneIsGameplay())
        {
            return;
        }
        // Only auto-save if losing focus AND game has been running for a while
        if (!hasFocus && activeGameData != null && Time.time > 5f) // 5 seconds after start
        {
            SaveGameData();
        }
    }

    private void OnApplicationQuit()
    {
        if (!CheckActiveSceneIsGameplay())
        {
            return;
        }
        if (activeGameData != null)
        {
            SaveGameData();
        }
    }

    private void OnDestroy()
    {
        if (!CheckActiveSceneIsGameplay())
        {
            return;
        }
        // Safety save if GameManager is being destroyed unexpectedly
        if (activeGameData != null && Instance == this)
        {
            SaveGameData();
        }
    }

    #endregion

    [ContextMenu("Debug: Clear Save Data")]
    private void ClearSaveData()
    {
        SaveLoadSystem.DeleteSaveFile(this);
        Debug.Log("Save data cleared.");
    }
}