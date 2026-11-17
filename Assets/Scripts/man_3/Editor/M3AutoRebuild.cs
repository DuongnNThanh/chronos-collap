using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Tự dựng lại Man_3 khi code dựng cảnh có phiên bản mới (Man3SceneBuilder.BuildVersion).
// Phiên bản đã dựng được ghi vào m3_build_stamp.txt (commit cùng cảnh), nên máy đồng đội
// mở dự án đã dựng sẵn sẽ KHÔNG bị dựng lại; chỉ khi ai đó tăng BuildVersion thì mới dựng lại.
[InitializeOnLoad]
public static class M3AutoRebuild
{
    public const string StampPath = "Assets/Scripts/man_3/Editor/m3_build_stamp.txt";

    static M3AutoRebuild()
    {
        EditorApplication.delayCall += Check;
        EditorApplication.playModeStateChanged += s =>
        {
            if (s == PlayModeStateChange.EnteredEditMode) EditorApplication.delayCall += Check;
        };
    }

    public static string ReadStamp()
    {
        return File.Exists(StampPath) ? File.ReadAllText(StampPath).Trim() : "";
    }

    public static void WriteStamp()
    {
        File.WriteAllText(StampPath, Man3SceneBuilder.BuildVersion);
        AssetDatabase.ImportAsset(StampPath);
    }

    private static void Check()
    {
        if (Application.isBatchMode || EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (ReadStamp() == Man3SceneBuilder.BuildVersion) return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += Check;
            return;
        }

        Debug.Log("[Man3] Có phiên bản dựng cảnh mới (" + Man3SceneBuilder.BuildVersion + ") – đang tự dựng lại Man_3...");
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        Man3SceneBuilder.BuildBatch();
    }
}
