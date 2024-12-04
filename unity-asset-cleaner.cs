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
    private Dictionary<string, bool> scenesToInclude = new Dictionary<string, bool>();
    private bool includeScripts = true;
    private bool includeTextures = true;
    private bool includeMaterials = true;
    private bool includeAudio = true;
    private bool includePrefabs = true;
    private bool createBackupBeforeDelete = true;
    private bool deleteEmptyFolders = true;
    private string backupFolderPath = "Assets/_DeletedAssetsBackup";
    private GUIStyle redXStyle;
    private bool selectAll = true;
    private bool sceneFoldout = true;

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
        RefreshSceneList();
    }

    private string GetCurrentScriptPath()
    {
        var scriptGUID = AssetDatabase.FindAssets("t:Script AssetCleaner").FirstOrDefault();
        if (scriptGUID != null)
        {
            return AssetDatabase.GUIDToAssetPath(scriptGUID);
        }
        return null;
    }

    private void RefreshSceneList()
    {
        scenesToInclude.Clear();

        // Añadir la escena actual
        string currentScene = EditorSceneManager.GetActiveScene().path;
        if (!string.IsNullOrEmpty(currentScene))
        {
            scenesToInclude[currentScene] = true;
        }

        // Añadir todas las escenas en build settings
        foreach (var scene in EditorBuildSettings.scenes)
        {
            if (!scenesToInclude.ContainsKey(scene.path))
            {
                scenesToInclude[scene.path] = scene.enabled;
            }
        }

        // Buscar todas las escenas en el proyecto
        string[] allScenes = AssetDatabase.FindAssets("t:Scene");
        foreach (string guid in allScenes)
        {
            string scenePath = AssetDatabase.GUIDToAssetPath(guid);
            if (!scenesToInclude.ContainsKey(scenePath))
            {
                scenesToInclude[scenePath] = false;
            }
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("Asset Cleaner", EditorStyles.boldLabel);

        EditorGUILayout.Space();
        sceneFoldout = EditorGUILayout.Foldout(sceneFoldout, "Scenes to Preserve:", true);
        if (sceneFoldout)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Select All Scenes"))
            {
                foreach (var key in scenesToInclude.Keys.ToList())
                {
                    scenesToInclude[key] = true;
                }
            }
            if (GUILayout.Button("Deselect All Scenes"))
            {
                foreach (var key in scenesToInclude.Keys.ToList())
                {
                    scenesToInclude[key] = false;
                }
            }
            if (GUILayout.Button("Refresh Scene List"))
            {
                RefreshSceneList();
            }
            EditorGUILayout.EndHorizontal();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(150));
            foreach (var scene in scenesToInclude.ToList())
            {
                EditorGUILayout.BeginHorizontal();
                bool isIncluded = EditorGUILayout.Toggle(scene.Value, GUILayout.Width(20));
                if (isIncluded != scene.Value)
                {
                    scenesToInclude[scene.Key] = isIncluded;
                }
                EditorGUILayout.LabelField(scene.Key);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Asset Types to Check:", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        includeScripts = EditorGUILayout.Toggle("Scripts", includeScripts);
        includeTextures = EditorGUILayout.Toggle("Textures", includeTextures);
        includeMaterials = EditorGUILayout.Toggle("Materials", includeMaterials);
        includeAudio = EditorGUILayout.Toggle("Audio", includeAudio);
        includePrefabs = EditorGUILayout.Toggle("Prefabs", includePrefabs);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Cleaning Options:", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        createBackupBeforeDelete = EditorGUILayout.Toggle("Create Backup Before Deleting", createBackupBeforeDelete);
        deleteEmptyFolders = EditorGUILayout.Toggle("Delete Empty Folders", deleteEmptyFolders);
        
        if (createBackupBeforeDelete)
        {
            EditorGUI.indentLevel++;
            backupFolderPath = EditorGUILayout.TextField("Backup Folder", backupFolderPath);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Make sure to backup your project before deleting assets!", MessageType.Warning);

        if (GUILayout.Button("Find Unused Assets", GUILayout.Height(30)))
        {
            FindUnusedAssets();
        }

        DrawAssetList();
    }

    private void DrawAssetList()
    {
        if (assetsToDelete.Count > 0)
        {
            EditorGUILayout.Space();
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

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            foreach (var assetEntry in assetsToDelete.ToList())
            {
                EditorGUILayout.BeginHorizontal();
                
                bool isMarked = EditorGUILayout.Toggle(assetEntry.Value, GUILayout.Width(20));
                if (isMarked != assetEntry.Value)
                {
                    assetsToDelete[assetEntry.Key] = isMarked;
                }

                Object assetObject = AssetDatabase.LoadAssetAtPath<Object>(assetEntry.Key);
                EditorGUILayout.ObjectField(assetObject, typeof(Object), false);
                
                if (GUILayout.Button("✕", redXStyle, GUILayout.Width(20)))
                {
                    assetsToDelete[assetEntry.Key] = false;
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Delete Marked Assets", GUILayout.Height(30)))
            {
                DeleteMarkedAssets();
            }
            GUI.backgroundColor = Color.white;

            int markedCount = assetsToDelete.Count(x => x.Value);
            if (markedCount > 0)
            {
                EditorGUILayout.HelpBox($"Assets marked for deletion: {markedCount}", MessageType.Info);
            }
        }
    }

    private void FindUnusedAssets()
    {
        assetsToDelete.Clear();
        HashSet<string> usedAssets = new HashSet<string>();
        
        // Proteger el script actual
        string currentScriptPath = GetCurrentScriptPath();
        if (currentScriptPath != null)
        {
            usedAssets.Add(currentScriptPath);
        }
        
        // Analizar las escenas seleccionadas
        foreach (var sceneEntry in scenesToInclude.Where(s => s.Value))
        {
            if (string.IsNullOrEmpty(sceneEntry.Key)) continue;
            
            string[] dependencies = AssetDatabase.GetDependencies(sceneEntry.Key, true);
            foreach (string dependency in dependencies)
            {
                usedAssets.Add(dependency);
            }
        }

        // Get all project assets
        string[] allAssets = AssetDatabase.GetAllAssetPaths();

        // Check each asset
        foreach (string asset in allAssets)
        {
            if (!asset.StartsWith("Assets/") || 
                string.IsNullOrEmpty(Path.GetExtension(asset)) || 
                asset.EndsWith(".unity") ||
                asset == currentScriptPath)
                continue;

            bool shouldCheck = false;
            string extension = Path.GetExtension(asset).ToLower();

            if (includeScripts && extension == ".cs") shouldCheck = true;
            else if (includeTextures && (extension == ".png" || extension == ".jpg" || extension == ".jpeg" || extension == ".tga")) shouldCheck = true;
            else if (includeMaterials && extension == ".mat") shouldCheck = true;
            else if (includeAudio && (extension == ".mp3" || extension == ".wav" || extension == ".ogg")) shouldCheck = true;
            else if (includePrefabs && extension == ".prefab") shouldCheck = true;

            if (shouldCheck && !usedAssets.Contains(asset))
            {
                assetsToDelete.Add(asset, true);
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

        string currentScriptPath = GetCurrentScriptPath();
        if (currentScriptPath != null && assetsToDelete.Any(x => x.Value && x.Key == currentScriptPath))
        {
            EditorUtility.DisplayDialog("Warning", 
                "Cannot delete the Asset Cleaner script while it's running.", 
                "OK");
            return;
        }

        if (EditorUtility.DisplayDialog("Confirm Delete",
            $"Are you sure you want to delete {assetsToDelete.Count(x => x.Value)} assets?" +
            (createBackupBeforeDelete ? "\nA backup will be created before deletion." : "\nThis action cannot be undone!"),
            "Delete",
            "Cancel"))
        {
            if (createBackupBeforeDelete)
            {
                foreach (var asset in assetsToDelete.Where(x => x.Value))
                {
                    CreateBackup(asset.Key);
                }
            }

            foreach (var asset in assetsToDelete.Where(x => x.Value).ToList())
            {
                AssetDatabase.DeleteAsset(asset.Key);
                assetsToDelete.Remove(asset.Key);
            }

            if (deleteEmptyFolders)
            {
                DeleteEmptyFolders("Assets");
            }

            AssetDatabase.Refresh();
        }
    }

    private void DeleteEmptyFolders(string startPath)
    {
        if (!Directory.Exists(startPath)) return;

        foreach (var dir in Directory.GetDirectories(startPath))
        {
            DeleteEmptyFolders(dir);
        }

        if (startPath != "Assets" && Directory.GetFiles(startPath).Length == 0 && 
            Directory.GetDirectories(startPath).Length == 0)
        {
            Directory.Delete(startPath);
            string metaFile = startPath + ".meta";
            if (File.Exists(metaFile))
            {
                File.Delete(metaFile);
            }
        }
    }

    private void CreateBackup(string assetPath)
    {
        try
        {
            if (!Directory.Exists(backupFolderPath))
            {
                Directory.CreateDirectory(backupFolderPath);
                AssetDatabase.Refresh();
            }

            string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string backupSubFolder = Path.Combine(backupFolderPath, timestamp);
            
            if (!Directory.Exists(backupSubFolder))
            {
                Directory.CreateDirectory(backupSubFolder);
            }

            string relativePath = assetPath.Replace("Assets/", "");
            string targetPath = Path.Combine(backupSubFolder, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
            File.Copy(assetPath, targetPath);

            // Backup dependencies for materials and prefabs
            if (Path.GetExtension(assetPath).ToLower() == ".mat" || 
                Path.GetExtension(assetPath).ToLower() == ".prefab")
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

            Debug.Log($"Backup created at: {backupSubFolder}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error creating backup for {assetPath}: {e.Message}");
        }
    }
}
