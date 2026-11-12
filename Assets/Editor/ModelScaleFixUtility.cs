using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public static class ModelScaleFixUtility
{
    private static readonly string[] ModelPaths = new string[]
    {
        "Assets/Models/Big Bich.fbx",
        "Assets/Models/Connected Circle.fbx",
        "Assets/Models/Fence Part.fbx",
        "Assets/Models/Hexagon.fbx",
        "Assets/Models/Portal.fbx",
        "Assets/Models/Road Main Element.fbx",
        "Assets/Models/Romb.fbx",
        "Assets/Models/Splash.fbx",
        "Assets/Models/Testo Loca.fbx",
        "Assets/Models/Tube.fbx"
    };

    static ModelScaleFixUtility()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
        EditorApplication.delayCall += CheckAndFixAll;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            EditorApplication.delayCall += CheckAndFixAll;
        }
    }

    [MenuItem("Tools/Fix FBX Model Scales")]
    public static void FixAllExplicit()
    {
        ApplyFix(force: true);
        LogAllBounds();
    }

    [MenuItem("Tools/Log FBX Model Bounds")]
    public static void LogAllBounds()
    {
        Debug.Log("=== FBX MODEL BOUNDS AUDIT ===");
        foreach (var path in ModelPaths)
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (go == null || importer == null) continue;

            var filters = go.GetComponentsInChildren<MeshFilter>(true);
            foreach (var mf in filters)
            {
                if (mf.sharedMesh != null)
                {
                    var size = mf.sharedMesh.bounds.size;
                    Debug.Log($"[ModelBounds] {path} | Mesh: {mf.sharedMesh.name} | Size: {size} (Scale: {importer.globalScale}, FileUnits: {importer.useFileUnits})");
                }
            }
        }
    }

    public static void CheckAndFixAll()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }
        ApplyFix(force: false);
    }

    private static void ApplyFix(bool force)
    {
        int fixedCount = 0;
        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (var path in ModelPaths)
            {
                var importer = AssetImporter.GetAtPath(path) as ModelImporter;
                if (importer == null)
                {
                    Debug.LogWarning($"[ModelScaleFix] Importer not found for: {path}");
                    continue;
                }

                bool needsFix = force || importer.useFileUnits != false || Mathf.Abs(importer.globalScale - 0.1f) > 0.0001f;

                if (needsFix)
                {
                    importer.useFileUnits = false;
                    importer.globalScale = 0.1f;
                    EditorUtility.SetDirty(importer);
                    importer.SaveAndReimport();
                    fixedCount++;
                    Debug.Log($"[ModelScaleFix] Reimported '{path}' with useFileUnits=false, globalScale=0.1f");
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }

        if (fixedCount > 0)
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            Debug.Log($"[ModelScaleFix] Completed reimport of {fixedCount} models. Scales are now restored!");
        }
        else
        {
            Debug.Log("[ModelScaleFix] All FBX models are already correctly configured (useFileUnits=false, globalScale=0.1).");
        }
    }
}
