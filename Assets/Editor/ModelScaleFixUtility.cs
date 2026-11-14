using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public static class ModelScaleFixUtility
{
    public struct ModelConfig
    {
        public float TargetScale;
        public string Description;

        public ModelConfig(float scale, string description)
        {
            TargetScale = scale;
            Description = description;
        }
    }

    public static readonly Dictionary<string, ModelConfig> ModelConfigs = new Dictionary<string, ModelConfig>()
    {
        { "Assets/Models/Big Bich.fbx", new ModelConfig(0.1f, "Creep Xeroboro 5 (Collider ~5.6m)") },
        { "Assets/Models/Hexagon.fbx", new ModelConfig(0.1f, "Creep Xeroboro 4 (Collider ~3.4m)") },
        { "Assets/Models/Tube.fbx", new ModelConfig(0.1f, "Creep Xeroboro 3 (Collider ~2.2m)") },
        { "Assets/Models/Romb.fbx", new ModelConfig(0.1f, "Creep Xeroboro 2 (Collider ~1.4m)") },
        { "Assets/Models/Portal.fbx", new ModelConfig(0.75f, "Portal Variant (Collider 3.0m)") },
        { "Assets/Models/Fence Part.fbx", new ModelConfig(1.0f, "Fence / Wall section (~1.72m)") },
        { "Assets/Models/Road Main Element.fbx", new ModelConfig(1.0f, "Road Element (10m x 15m)") },
        { "Assets/Models/Testo Loca.fbx", new ModelConfig(1.0f, "Arena Ground Mesh (~85m x 87m)") },
        { "Assets/Models/Connected Circle.fbx", new ModelConfig(1.0f, "Connected Circle mesh") },
        { "Assets/Models/Splash.fbx", new ModelConfig(1.0f, "Splash mesh") }
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
        DumpBoundsToFile();
    }

    [MenuItem("Tools/Dump FBX Bounds To File")]
    public static void DumpBoundsToFile()
    {
        var filePath = Application.dataPath + "/bounds_report.txt";
        System.IO.File.WriteAllText(filePath, GetBoundsReport());
        Debug.Log("[ModelScaleFix] Dumped bounds report to: " + filePath);
    }

    [MenuItem("Tools/Log FBX Model Bounds")]
    public static void LogAllBounds()
    {
        Debug.Log(GetBoundsReport());
    }

    public static string GetBoundsReport()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("=== FBX MODEL BOUNDS REPORT ===");
        foreach (var kvp in ModelConfigs)
        {
            var path = kvp.Key;
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (go == null || importer == null) continue;

            var filters = go.GetComponentsInChildren<MeshFilter>(true);
            foreach (var mf in filters)
            {
                if (mf.sharedMesh != null)
                {
                    var size = mf.sharedMesh.bounds.size;
                    sb.AppendLine($"{path} | Mesh: {mf.sharedMesh.name} | Size: ({size.x:F2}, {size.y:F2}, {size.z:F2}) | Scale: {importer.globalScale} | useFileScale: {importer.useFileScale} | useFileUnits: {importer.useFileUnits} | {kvp.Value.Description}");
                }
            }
        }
        return sb.ToString();
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
            foreach (var kvp in ModelConfigs)
            {
                var path = kvp.Key;
                var config = kvp.Value;
                var importer = AssetImporter.GetAtPath(path) as ModelImporter;
                if (importer == null)
                {
                    Debug.LogWarning($"[ModelScaleFix] Importer not found for: {path}");
                    continue;
                }

                bool needsFix = force 
                    || importer.useFileScale != false 
                    || importer.useFileUnits != false 
                    || Mathf.Abs(importer.globalScale - config.TargetScale) > 0.0001f;

                if (needsFix)
                {
                    importer.useFileScale = false;
                    importer.useFileUnits = false;
                    importer.globalScale = config.TargetScale;

                    var so = new SerializedObject(importer);
                    var useFileScaleProp = so.FindProperty("m_UseFileScale");
                    if (useFileScaleProp != null)
                    {
                        useFileScaleProp.boolValue = false;
                    }
                    var useFileUnitsProp = so.FindProperty("m_UseFileUnits");
                    if (useFileUnitsProp != null)
                    {
                        useFileUnitsProp.boolValue = false;
                    }
                    var globalScaleProp = so.FindProperty("m_GlobalScale");
                    if (globalScaleProp != null)
                    {
                        globalScaleProp.floatValue = config.TargetScale;
                    }
                    so.ApplyModifiedProperties();

                    EditorUtility.SetDirty(importer);
                    importer.SaveAndReimport();
                    fixedCount++;
                    Debug.Log($"[ModelScaleFix] Reimported '{path}' with useFileScale=false, useFileUnits=false, globalScale={config.TargetScale} ({config.Description})");
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
            Debug.Log("[ModelScaleFix] All FBX models are already correctly configured.");
        }
    }
}
