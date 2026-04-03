using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Core game progression data (non-shop related)
/// ShopManager handles all economy and unlock data separately
/// </summary>
[System.Serializable]
public class GameData
{
    [Header("Tool Data")]
    [SerializeField] private ToolSaveData toolData;

    [Header("Player Stats")]
    [SerializeField] private PlayerSaveData playerData;

    // Public properties for access (Unity will serialize the private fields)
    public ToolSaveData ToolData => toolData;
    public PlayerSaveData PlayerData => playerData;

    /// <summary>
    /// Default constructor - creates GameData with safe initial values
    /// </summary>
    public GameData()
    {
        toolData = new ToolSaveData();
        playerData = new PlayerSaveData();
    }

    public GameData(PlayerSaveData playerData, ToolSaveData toolData)
    {
        this.toolData = toolData;
        this.playerData = playerData;
    }

    // Tool data management
    internal bool SetToolData(ToolSaveData data, GameManager editor)
    {
        if (!ValidateEditor(editor))
        {
            throw new System.UnauthorizedAccessException("Only GameManager can modify tool data.");
        }
        toolData = data;
        return true;
    }

    // Player data management
    internal bool SetPlayerData(PlayerSaveData data, GameManager editor)
    {
        if (!ValidateEditor(editor))
        {
            throw new System.UnauthorizedAccessException("Only GameManager can modify player data.");
        }
        playerData = data;
        return true;
    }

    /// <summary>
    /// Validate that only GameManager can edit this data
    /// </summary>
    private bool ValidateEditor(object editor)
    {
        if (editor is GameManager)
        {
            return true;
        }

        return false;
    }
}