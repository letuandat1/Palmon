// Modified code. Safe to use.
using System;
using System.IO;
using UnityEngine;

public static class SaveLoadSystem
{
    private static string GetNormalSaveFilePath()
    {
        return Application.persistentDataPath + "/banhmi_savefile.json";
    }

    public static void SaveGameToFile(object caller, GameData data)
    {
        if (caller.GetType() != typeof(GameManager))
        {
            return;
        }

        string saveFilePath = GetNormalSaveFilePath();

        // Validate data before saving
        if (!ValidateGameData(data))
        {
            return;
        }

        // Create backup of existing save file (if it exists)
        if (File.Exists(saveFilePath))
        {
            string backupPath = saveFilePath + ".backup";
            File.Copy(saveFilePath, backupPath, true);
        }

        // Serialize the GameData object to JSON
        string json = JsonUtility.ToJson(data, true);

        // Write the JSON to the file
        File.WriteAllText(saveFilePath, json);

    }

    public static bool DoesSaveFileExist(object caller)
    {
        if (caller.GetType() != typeof(GameManager))
        {
            return false;
        }
        string saveFilePath = GetNormalSaveFilePath();
        return File.Exists(saveFilePath);
    }

    public static GameData LoadGameFromFile(object caller)
    {
        if (caller.GetType() != typeof(GameManager))
        {
            return null;
        }

        string saveFilePath = GetNormalSaveFilePath();

        bool fileExists = File.Exists(saveFilePath);

        if (fileExists)
        {
            try
            {
                string json = File.ReadAllText(saveFilePath);

                // Check if JSON is empty or corrupted
                if (string.IsNullOrEmpty(json) || json.Trim().Length == 0)
                {
                    return HandleCorruptedSave(new Exception("Empty save file"));
                }

                GameData data = JsonUtility.FromJson<GameData>(json);

                // Validate the loaded data
                if (!ValidateGameData(data))
                {
                    return HandleCorruptedSave(new Exception("Failed validation"));
                }

                return data;
            }
            catch (Exception ex)
            {
                return HandleCorruptedSave(ex);
            }
        }
        else
        {
            return null;
        }
    }

    public static void DeleteSaveFile(object caller)
    {
        if (caller.GetType() != typeof(GameManager))
        {
            return;
        }
        string saveFilePath = GetNormalSaveFilePath();
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
        }
    }

    private static bool ValidateGameData(GameData data)
    {
        if (data == null)
        {
            return false;
        }

        return true;
    }

    private static GameData CreateBackupGameData()
    {
        // Create a fresh GameData with safe defaults
        // GameData is now a regular class, not a MonoBehaviour
        GameData backupData = new();

        return backupData;
    }

    private static GameData HandleCorruptedSave(Exception ex)
    {
        // Try to create backup from existing save
        string backupPath = GetNormalSaveFilePath() + ".backup";
        if (File.Exists(backupPath))
        {
            string backupJson = File.ReadAllText(backupPath);
            GameData backupData = JsonUtility.FromJson<GameData>(backupJson);

            if (ValidateGameData(backupData))
            {
                return backupData;
            }
        }

        // Last resort: create fresh save
        return CreateBackupGameData();
    }
}