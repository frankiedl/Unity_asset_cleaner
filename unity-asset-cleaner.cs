using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class AssetCleaner : EditorWindow
{
    private Vector2 scrollPosition;
    private Dictionary<string, bool> assetsToDelete = new Dictionary<string, bool>();
    private bool includeScripts = true;
    private bool includeTextures = true;
    private bool includeMaterials = true;
    private bool includeAudio = true;
    private bool includePrefabs = true;
    private bool includeCurrentScene = true;
    private bool analyzeResources = true;
    private bool createBackupBeforeDelete = true;
    private string backupFolderPath = "Assets/_DeletedAssetsBackup";
    private GUIStyle redXStyle;
    private bool selectAll = true;

    [MenuItem("Tools/Asset Cleaner")]
    public static void ShowWindow()
    {
        GetWindow<AssetCleaner>("Asset Cleaner");
    }

    private void OnEnable()
    {
        redXStyle = new GUIStyle();
        redXStyle.normal.textColor = Color.red;
        redXStyle.fontSize = 12;
        redXStyle.fontStyle = FontStyle.Bold;
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

        if (assetsToDelete.Count > 0)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Found {assetsToDelete.Count} potentially unused assets:", EditorStyles.boldLabel);
            
            if (GUILayout.Button(selectAll ? "Deselect All" : "Select All", GUILayout.Width(100)))
            {
                selectAll = !selectAll;
                foreach (var asset in assetsToDelete.Keys.ToList())
                {
                    assetsToDelete[asset] = selectAll;
                }
            }
            EditorGUILayout.EndHorizontal();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            foreach (var assetEntry in assetsToDelete.ToList())
            {
                EditorGUILayout.BeginHorizontal();
                
                // Checkbox
                bool isMarked = EditorGUILayout.Toggle(assetEntry.Value, GUILayout.Width(20));
                if (isMarked != assetEntry.Value)
                {
                    assetsToDelete[assetEntry.Key] = isMarked;
                }

                // Asset preview
                Object assetObject = AssetDatabase.LoadAssetAtPath<Object>(assetEntry.Key);
                EditorGUILayout.ObjectField(assetObject, typeof(Object), false);
                
                // Red X button
                if (GUILayout.Button("✕", redXStyle, GUILayout.Width(20)))
                {
                    assetsToDelete[assetEntry.Key] = false;
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();

            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Delete Marked Assets", GUILayout.Height(30)))
            {
                DeleteMarkedAssets();
            }
            GUI.backgroundColor = Color.white;

            // Show count of marked assets
            int markedCount = assetsToDelete.Count(x => x.Value);
            if (markedCount > 0)
            {
                EditorGUILayout.HelpBox($"Assets marked for deletion: {markedCount}", MessageType.Info);
            }
        }
    }

    private void DeleteMarkedAssets()
    {
        if (!assetsToDelete.Any(x => x.Value))
        {
            EditorUtility.DisplayDialog("No Assets Selected", 
                "Please select assets to delete first.", 
                "OK");
            return;
        }

        if (EditorUtility.DisplayDialog("Confirm Delete",
            $"Are you sure you want to delete {assetsToDelete.Count(x => x.Value)} assets?" +
            (createBackupBeforeDelete ? "\nA backup will be created before deletion." : "\nThis action cannot be undone!"),
            "Delete",
            "Cancel"))
        {
            foreach (var asset in assetsToDelete.Where(x => x.Value).ToList())
            {
                if (createBackupBeforeDelete)
                {
                    CreateBackup(asset.Key);
                }
                AssetDatabase.DeleteAsset(asset.Key);
                assetsToDelete.Remove(asset.Key);
            }
            AssetDatabase.Refresh();
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
        assetsToDelete.Clear();
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
            var resourcesFolder = "Assets/Resources";
            if (Directory.Exists(resourcesFolder))
            {
                string[] resourcesAssets = AssetDatabase.FindAssets("", new[] { resourcesFolder });
                foreach (string guid in resourcesAssets)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    usedAssets.Add(assetPath);
                }
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

                assetsToDelete.Add(asset, true);
            }
        }
    }
}
