using UnityEngine;
using UnityEditor;
using TMPro;
using System.Collections.Generic;

#if UNITY_EDITOR
/// <summary>
/// TextMeshPro 한글 폰트 자동 업데이트 유틸리티
/// 에디터에서 한글이 깨질 때 자동으로 폰트 아틀라스를 업데이트합니다.
/// </summary>
public class TMPFontAutoUpdater : EditorWindow
{
    private TMP_FontAsset fontAsset;
    private bool autoUpdate = false;

    [MenuItem("Tools/TMP Font Auto Updater")]
    public static void ShowWindow()
    {
        GetWindow<TMPFontAutoUpdater>("TMP Font Updater");
    }

    private void OnGUI()
    {
        GUILayout.Label("TextMeshPro 한글 폰트 업데이터", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        fontAsset = (TMP_FontAsset)EditorGUILayout.ObjectField("Font Asset", fontAsset, typeof(TMP_FontAsset), false);

        EditorGUILayout.Space();

        if (GUILayout.Button("모든 한글 폰트 Dynamic으로 설정"))
        {
            SetAllKoreanFontsToDynamic();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("선택된 폰트 Dynamic으로 설정"))
        {
            if (fontAsset != null)
            {
                SetFontToDynamic(fontAsset);
                EditorUtility.SetDirty(fontAsset);
                AssetDatabase.SaveAssets();
                Debug.Log($"✅ {fontAsset.name}를 Dynamic으로 설정했습니다.");
            }
            else
            {
                Debug.LogWarning("폰트를 먼저 선택해주세요.");
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "한글 폰트가 깨지는 경우:\n" +
            "1. '모든 한글 폰트 Dynamic으로 설정' 버튼 클릭\n" +
            "2. 또는 특정 폰트를 선택하고 '선택된 폰트 Dynamic으로 설정' 클릭\n\n" +
            "Dynamic으로 설정하면 필요한 글자가 런타임에 자동으로 추가됩니다.",
            MessageType.Info);
    }

    private void SetAllKoreanFontsToDynamic()
    {
        string[] fontPaths = new string[]
        {
            "Assets/TextMesh Pro/Resources/Fonts & Materials/NotoSansKR-Black SDF.asset",
            "Assets/TextMesh Pro/Resources/Fonts & Materials/NotoSansKR-Bold SDF.asset",
            "Assets/TextMesh Pro/Resources/Fonts & Materials/NotoSansKR-ExtraBold SDF.asset",
            "Assets/TextMesh Pro/Resources/Fonts & Materials/Maplestory Light SDF.asset",
            "Assets/TextMesh Pro/Resources/Fonts & Materials/Maplestory Bold SDF.asset"
        };

        int successCount = 0;
        foreach (string path in fontPaths)
        {
            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (font != null)
            {
                SetFontToDynamic(font);
                EditorUtility.SetDirty(font);
                successCount++;
                Debug.Log($"✅ {font.name}를 Dynamic으로 설정했습니다.");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"✅ 총 {successCount}개의 한글 폰트를 Dynamic으로 설정했습니다.");
        EditorUtility.DisplayDialog("완료", $"{successCount}개의 한글 폰트를 Dynamic으로 설정했습니다.\n이제 한글이 깨지지 않습니다!", "확인");
    }

    private void SetFontToDynamic(TMP_FontAsset font)
    {
        if (font == null) return;

        SerializedObject so = new SerializedObject(font);

        SerializedProperty atlasPopulationMode = so.FindProperty("m_AtlasPopulationMode");
        if (atlasPopulationMode != null)
        {
            atlasPopulationMode.intValue = 1;
        }

        SerializedProperty clearDynamicDataOnBuild = so.FindProperty("m_ClearDynamicDataOnBuild");
        if (clearDynamicDataOnBuild != null)
        {
            clearDynamicDataOnBuild.boolValue = false;
        }

        so.ApplyModifiedProperties();
    }
}

/// <summary>
/// 플레이 모드 진입 시 자동으로 한글 폰트를 Dynamic으로 설정
/// </summary>
[InitializeOnLoad]
public class TMPFontAutoSetup
{
    static TMPFontAutoSetup()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            EnsureKoreanFontsAreDynamic();
        }
    }

    private static void EnsureKoreanFontsAreDynamic()
    {
        string[] fontPaths = new string[]
        {
            "Assets/TextMesh Pro/Resources/Fonts & Materials/NotoSansKR-Black SDF.asset",
            "Assets/TextMesh Pro/Resources/Fonts & Materials/NotoSansKR-Bold SDF.asset",
            "Assets/TextMesh Pro/Resources/Fonts & Materials/NotoSansKR-ExtraBold SDF.asset",
            "Assets/TextMesh Pro/Resources/Fonts & Materials/Maplestory Light SDF.asset",
            "Assets/TextMesh Pro/Resources/Fonts & Materials/Maplestory Bold SDF.asset"
        };

        foreach (string path in fontPaths)
        {
            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (font != null)
            {
                SetFontAtlasPopulationMode(font);
            }
        }
    }

    private static void SetFontAtlasPopulationMode(TMP_FontAsset font)
    {
        if (font == null) return;

        SerializedObject so = new SerializedObject(font);
        SerializedProperty atlasPopulationMode = so.FindProperty("m_AtlasPopulationMode");

        if (atlasPopulationMode != null && atlasPopulationMode.intValue != 1)
        {
            atlasPopulationMode.intValue = 1;

            SerializedProperty clearDynamicDataOnBuild = so.FindProperty("m_ClearDynamicDataOnBuild");
            if (clearDynamicDataOnBuild != null)
            {
                clearDynamicDataOnBuild.boolValue = false;
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(font);
        }
    }
}
#endif
