using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class AssetCleaner : EditorWindow
{
    private Vector2 scrollPosition;
    private List<string> unusedAssets = new List<string>();
    private bool includeScripts = true;
    private bool includeTextures = true;
    private bool includeMaterials = true;
    private bool includeAudio = true;
    private bool includePrefabs = true;
    private bool includeCurrentScene = true;
    private bool analyzeResources = true;
    private bool createBackupBeforeDelete = true;
    private string backupFolderPath = "Assets/_DeletedAssetsBackup";

    [MenuItem("Tools/Asset Cleaner")]
    public static void ShowWindow()
    {
        GetWindow<AssetCleaner>("Asset Cleaner");
    }

    private void OnGUI()
    {
        GUILayout.Label("Asset Cleaner", EditorStyles.boldLabel);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Analysis Options:", EditorStyles.boldLabel);
        
        includeCurrentScene = EditorGUILayout.Toggle("Include Current Scene", includeCurrentScene);
        analyzeResources = EditorGUILayout.Toggle("Check Resources Folder", analyzeResources);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Asset Types to Check:", EditorStyles.boldLabel);
        
        includeScripts = EditorGUILayout.Toggle("Scripts", includeScripts);
        includeTextures = EditorGUILayout.Toggle("Textures", includeTextures);
        includeMaterials = EditorGUILayout.Toggle("Materials", includeMaterials);
        includeAudio = EditorGUILayout.Toggle("Audio", includeAudio);
        includePrefabs = EditorGUILayout.Toggle("Prefabs", includePrefabs);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Backup Options:", EditorStyles.boldLabel);
        createBackupBeforeDelete = EditorGUILayout.Toggle("Create Backup Before Deleting", createBackupBeforeDelete);
        if (createBackupBeforeDelete)
        {
            EditorGUI.indentLevel++;
            backupFolderPath = EditorGUILayout.TextField("Backup Folder", backupFolderPath);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Make sure to backup your project before deleting assets!", MessageType.Warning);

        if (GUILayout.Button("Find Unused Assets"))
        {
            FindUnusedAssets();
        }

        EditorGUILayout.Space();

        if (unusedAssets.Count > 0)
        {
            EditorGUILayout.LabelField($"Found {unusedAssets.Count} potentially unused assets:", EditorStyles.boldLabel);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            foreach (string asset in unusedAssets.ToList())
            {
                EditorGUILayout.BeginHorizontal();
                
                Object assetObject = AssetDatabase.LoadAssetAtPath<Object>(asset);
                EditorGUILayout.ObjectField(assetObject, typeof(Object), false);
                
                if (GUILayout.Button("Delete", GUILayout.Width(60)))
                {
                    DeleteAssetWithBackup(asset);
                }
                
                if (GUILayout.Button("Keep", GUILayout.Width(60)))
                {
                    unusedAssets.Remove(asset);
                    GUIUtility.ExitGUI();
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndScrollView();
        }
    }

    private void DeleteAssetWithBackup(string assetPath)
    {
        if (EditorUtility.DisplayDialog("Confirm Delete",
            $"Are you sure you want to delete {Path.GetFileName(assetPath)}?" +
            (createBackupBeforeDelete ? "\nA backup will be created before deletion." : "\nThis action cannot be undone!"),
            "Delete",
            "Cancel"))
        {
            if (createBackupBeforeDelete)
            {
                CreateBackup(assetPath);
            }
            AssetDatabase.DeleteAsset(assetPath);
            unusedAssets.Remove(assetPath);
            GUIUtility.ExitGUI();
        }
    }

    private void CreateBackup(string assetPath)
    {
        try
        {
            // Ensure backup folder exists
            if (!Directory.Exists(backupFolderPath))
            {
                Directory.CreateDirectory(backupFolderPath);
                AssetDatabase.Refresh();
            }

            // Create timestamp subfolder
            string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string backupSubFolder = Path.Combine(backupFolderPath, timestamp);
            if (!Directory.Exists(backupSubFolder))
            {
                Directory.CreateDirectory(backupSubFolder);
            }

            // Get relative path within Assets folder
            string relativePath = assetPath.Replace("Assets/", "");
            string targetPath = Path.Combine(backupSubFolder, relativePath);

            // Create necessary subdirectories
            Directory.CreateDirectory(Path.GetDirectoryName(targetPath));

            // Copy the asset
            File.Copy(assetPath, targetPath);
            
            // If it's a material or prefab, we need to copy its dependencies too
            if (Path.GetExtension(assetPath).ToLower() == ".mat" || Path.GetExtension(assetPath).ToLower() == ".prefab")
            {
                string[] dependencies = AssetDatabase.GetDependencies(assetPath, true);
                foreach (string dependency in dependencies)
                {
                    if (dependency != assetPath)
                    {
                        string depRelativePath = dependency.Replace("Assets/", "");
                        string depTargetPath = Path.Combine(backupSubFolder, depRelativePath);
                        Directory.CreateDirectory(Path.GetDirectoryName(depTargetPath));
                        File.Copy(dependency, depTargetPath, true);
                    }
                }
            }

            AssetDatabase.Refresh();
            Debug.Log($"Backup created at: {backupSubFolder}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error creating backup: {e.Message}");
            if (!EditorUtility.DisplayDialog("Backup Failed",
                "Failed to create backup. Do you want to proceed with deletion anyway?",
                "Delete Without Backup",
                "Cancel"))
            {
                return;
            }
        }
    }

    private void FindUnusedAssets()
    {
        unusedAssets.Clear();
        HashSet<string> usedAssets = new HashSet<string>();
        
        // Get all scenes in build settings
        var scenePaths = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToList();

        // Add current scene if it's not already included
        if (includeCurrentScene)
        {
            string currentScenePath = EditorSceneManager.GetActiveScene().path;
            if (!string.IsNullOrEmpty(currentScenePath) && !scenePaths.Contains(currentScenePath))
            {
                scenePaths.Add(currentScenePath);
            }
        }

        // Analyze all scenes
        foreach (string scenePath in scenePaths)
        {
            if (string.IsNullOrEmpty(scenePath)) continue;
            
            // Get all dependencies from the scene
            string[] dependencies = AssetDatabase.GetDependencies(scenePath, true);
            foreach (string dependency in dependencies)
            {
                usedAssets.Add(dependency);
            }
        }

        // Check Resources folder if enabled
        if (analyzeResources)
        {
            string[] resourcesAssets = AssetDatabase.FindAssets("", new[] { "Assets/Resources" });
            foreach (string guid in resourcesAssets)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                usedAssets.Add(assetPath);
            }
        }

        // Get all project assets
        string[] allAssets = AssetDatabase.GetAllAssetPaths();

        // Check each asset
        foreach (string asset in allAssets)
        {
            // Skip built-in assets, folders, and scenes
            if (!asset.StartsWith("Assets/") || 
                string.IsNullOrEmpty(Path.GetExtension(asset)) || 
                asset.EndsWith(".unity"))
                continue;

            bool shouldCheck = false;
            string extension = Path.GetExtension(asset).ToLower();

            // Check based on file type and user preferences
            if (includeScripts && extension == ".cs") shouldCheck = true;
            else if (includeTextures && (extension == ".png" || extension == ".jpg" || extension == ".jpeg" || extension == ".tga")) shouldCheck = true;
            else if (includeMaterials && extension == ".mat") shouldCheck = true;
            else if (includeAudio && (extension == ".mp3" || extension == ".wav" || extension == ".ogg")) shouldCheck = true;
            else if (includePrefabs && extension == ".prefab") shouldCheck = true;

            if (shouldCheck && !usedAssets.Contains(asset))
            {
                // Double check for materials - look for references in scene objects
                if (extension == ".mat")
                {
                    bool materialInUse = false;
                    var allGameObjects = Resources.FindObjectsOfTypeAll<GameObject>();
                    foreach (var go in allGameObjects)
                    {
                        var renderers = go.GetComponentsInChildren<Renderer>(true);
                        foreach (var renderer in renderers)
                        {
                            if (renderer.sharedMaterials.Any(m => m != null && AssetDatabase.GetAssetPath(m) == asset))
                            {
                                materialInUse = true;
                                break;
                            }
                        }
                        if (materialInUse) break;
                    }
                    if (materialInUse) continue;
                }

                unusedAssets.Add(asset);
            }
        }
    }
}
