using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

// Font tiếng Việt riêng cho màn 3.
// - Font dự phòng mặc định của TextMeshPro chỉ có atlas 512x512 -> chữ có dấu bị mờ/thiếu.
// - Ở đây: lấy mẫu 120pt, đệm 14px, atlas 4096x4096 (đủ cho toàn bộ chữ tiếng Việt trên 1 atlas),
//   nạp sẵn mọi ký tự, và chỉnh material cho nét dày + khử răng cưa mềm để chữ rõ cả khi màn hình nhỏ.
public static class M3FontMaker
{
    private const string SourceFont = "Assets/TextMesh Pro/Fonts/LiberationSans.ttf";
    private const string Folder = "Assets/Sprites/man_3/Fonts";
    private const string FontAssetPath = Folder + "/M3_VietFont SDF.asset";

    private const int SamplingSize = 120;
    private const int Padding = 14;
    private const int AtlasSize = 4096;

    private const string Lower = "aáàảãạăắằẳẵặâấầẩẫậbcdđeéèẻẽẹêếềểễệfghiíìỉĩịjklmnoóòỏõọôốồổỗộơớờởỡợpqrstuúùủũụưứừửữựvwxyýỳỷỹỵz";
    private const string Extra = "0123456789 .,:;!?…'\"“”‘’-–—()[]/\\+*=%&#@<>·•_»«";

    public static string AllCharacters => Lower + Lower.ToUpperInvariant() + Extra;

    public static TMP_FontAsset GetOrCreate()
    {
        if (!Directory.Exists(Folder)) Directory.CreateDirectory(Folder);

        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);

        // Font cũ tạo với thông số thấp hơn -> xóa để tạo lại (cảnh được dựng lại ngay sau nên không mất tham chiếu)
        if (font != null && (font.atlasWidth != AtlasSize || font.atlasPadding != Padding || Mathf.RoundToInt(font.faceInfo.pointSize) != SamplingSize))
        {
            AssetDatabase.DeleteAsset(FontAssetPath);
            font = null;
        }

        if (font == null)
        {
            var source = AssetDatabase.LoadAssetAtPath<Font>(SourceFont);
            font = TMP_FontAsset.CreateFontAsset(source, SamplingSize, Padding, GlyphRenderMode.SDFAA, AtlasSize, AtlasSize, AtlasPopulationMode.Dynamic, true);
            font.name = "M3_VietFont SDF";
            AssetDatabase.CreateAsset(font, FontAssetPath);

            font.material.name = font.name + " Material";
            AssetDatabase.AddObjectToAsset(font.material, font);
            font.atlasTextures[0].name = font.name + " Atlas";
            AssetDatabase.AddObjectToAsset(font.atlasTextures[0], font);
        }

        // Nạp sẵn chữ – chỉ khi còn thiếu
        if (!font.HasCharacters(AllCharacters, out List<char> notYet))
        {
            font.TryAddCharacters(new string(notYet.ToArray()), out string _);
            if (!font.HasCharacters(AllCharacters, out List<char> stillMissing))
                Debug.LogWarning("[M3FontMaker] Font thiếu ký tự: " + new string(stillMissing.ToArray()));
        }

        // Giữ chữ đã nạp sẵn khi build game (mặc định TMP xóa dữ liệu font động lúc build)
        var so = new SerializedObject(font);
        SerializedProperty clearOnBuild = so.FindProperty("m_ClearDynamicDataOnBuild");
        if (clearOnBuild != null)
        {
            clearOnBuild.boolValue = false;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // Material dùng chung cho mọi chữ màn 3:
        // - FaceDilate: nét dày hơn LiberationSans gốc (vốn mảnh) -> chữ nhỏ vẫn đậm, dễ đọc
        // - Sharpness âm: mép chữ khử răng cưa mềm hơn khi giao diện bị thu nhỏ
        Material m = font.material;
        m.SetFloat(ShaderUtilities.ID_FaceDilate, 0.14f);
        m.SetFloat(ShaderUtilities.ID_Sharpness, -0.25f);
        m.SetFloat(ShaderUtilities.ID_OutlineWidth, 0f);
        m.DisableKeyword(ShaderUtilities.Keyword_Outline);
        m.DisableKeyword(ShaderUtilities.Keyword_Underlay);
        EditorUtility.SetDirty(m);
        EditorUtility.SetDirty(font);

        // Material viền cũ (bản trước) không còn dùng
        AssetDatabase.DeleteAsset(Folder + "/M3_VietFont SDF - Outline.mat");

        AssetDatabase.SaveAssets();
        return font;
    }
}
