using UnityEngine;
using UnityEditor;
using System.IO;

#if UNITY_EDITOR
public class SaveFileEditor : EditorWindow
{
    private static string saveFileName = "savegame.json"; // Default save file name

    [MenuItem("Tools/Delete Save File")]
    private static void DeleteSaveFile()
    {
        PlayerPrefs.DeleteAll();

        string path = Path.Combine(Application.persistentDataPath, saveFileName);

        if (File.Exists(path))
        {
            File.Delete(path);
           
            Debug.Log("Save file deleted: " + path);
        }
        else
        {
            Debug.LogWarning("Save file not found: " + path);
        }
    }

    [MenuItem("Tools/Refresh Save Folder")]
    private static void RefreshSaveFolder()
    {
        string path = Application.persistentDataPath;
        EditorUtility.RevealInFinder(path);
    }
}
#endif