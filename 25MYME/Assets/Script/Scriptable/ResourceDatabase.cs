using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ResourceDatabase", menuName = "Game/ResourceDatabase")]
public class ResourceDatabase : ScriptableObject
{
    [Header("모든 리소스 데이터")]
    public List<ResourceData> allResources = new List<ResourceData>();

    private Dictionary<ResourceType, ResourceData> resourceLookup;

    void OnEnable()
    {
        BuildLookupTable();
    }

    public void BuildLookupTable()
    {
        resourceLookup = new Dictionary<ResourceType, ResourceData>();
        foreach (var resource in allResources)
        {
            if (resource != null)
                resourceLookup[resource.type] = resource;
        }
    }

    public ResourceData GetResourceData(ResourceType type)
    {
        if (resourceLookup == null) BuildLookupTable();
        return resourceLookup.ContainsKey(type) ? resourceLookup[type] : null;
    }

    public Sprite GetResourceIcon(ResourceType type)
    {
        var data = GetResourceData(type);
        return data?.icon;
    }

    public string GetResourceName(ResourceType type)
    {
        var data = GetResourceData(type);
        return data?.resourceName ?? type.ToString();
    }

    public Color GetResourceColor(ResourceType type)
    {
        var data = GetResourceData(type);
        return data?.displayColor ?? Color.white;
    }

    public float GetResourceEffect(ResourceType type)
    {
        var data = GetResourceData(type);
        return data?.effectValue ?? 0f;
    }

    public string FormatResourceAmount(ResourceType type, int amount)
    {
        var data = GetResourceData(type);
        string format = data?.displayFormat ?? "{0:N0}";
        return string.Format(format, amount);
    }

#if UNITY_EDITOR
    public void AutoCreateMissingResources()
    {
        var createdResources = new List<string>();

        foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
        {
            if (type == ResourceType.None) continue;

            if (resourceLookup == null) BuildLookupTable();

            if (!resourceLookup.ContainsKey(type))
            {
                var newResource = CreateInstance<ResourceData>();
                newResource.type = type;
                newResource.resourceName = GetFriendlyName(type);
                newResource.name = $"Resource_{type}";

                // 기본 색상 설정
                SetDefaultColor(newResource, type);

                allResources.Add(newResource);
                createdResources.Add(type.ToString());

                // 에셋으로 저장
                string folderPath = "Assets/GameData/Resources";
                if (!UnityEditor.AssetDatabase.IsValidFolder(folderPath))
                {
                    UnityEditor.AssetDatabase.CreateFolder("Assets/GameData", "Resources");
                }

                string assetPath = $"{folderPath}/Resource_{type}.asset";
                UnityEditor.AssetDatabase.CreateAsset(newResource, assetPath);
            }
        }

        if (createdResources.Count > 0)
        {
            BuildLookupTable();
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();

            Debug.Log($"생성된 리소스: {string.Join(", ", createdResources)}");
        }
        else
        {
            Debug.Log("모든 리소스가 이미 존재합니다.");
        }
    }

    string GetFriendlyName(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Gold: return "골드";
            case ResourceType.Gem: return "젬";
            case ResourceType.Crystal: return "크리스탈";
            case ResourceType.UpgradeStone: return "강화석";
            case ResourceType.ProtectionStone: return "보호석";
            case ResourceType.LuckyCharm: return "행운의 부적";
            case ResourceType.Equipment: return "장비";
            case ResourceType.Fragment: return "파편";
            case ResourceType.SclapMetal: return "고철";
            case ResourceType.Nickel: return "니켈";
            case ResourceType.Ascied: return "염석";
            case ResourceType.Meteorite: return "운철";
            default: return type.ToString();
        }
    }

    void SetDefaultColor(ResourceData resource, ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Gold:
                resource.displayColor = new Color(1f, 0.8f, 0f); // 금색
                break;
            case ResourceType.Gem:
                resource.displayColor = new Color(0.5f, 0f, 1f); // 보라색
                break;
            case ResourceType.Crystal:
                resource.displayColor = new Color(0f, 0.8f, 1f); // 하늘색
                break;
            case ResourceType.UpgradeStone:
                resource.displayColor = new Color(0.7f, 0.7f, 0.7f); // 회색
                break;
            case ResourceType.ProtectionStone:
                resource.displayColor = new Color(0f, 1f, 0f); // 초록색
                break;
            case ResourceType.LuckyCharm:
                resource.displayColor = new Color(1f, 0.5f, 0f); // 주황색
                break;
            case ResourceType.Equipment:
                resource.displayColor = new Color(0.8f, 0.4f, 0.2f); // 갈색
                break;
            case ResourceType.Fragment:
                resource.displayColor = new Color(0.9f, 0.9f, 0.9f); // 밝은 회색
                break;
            default:
                resource.displayColor = Color.white;
                break;
        }
    }
#endif
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(ResourceDatabase))]
public class ResourceDatabaseEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(20);

        ResourceDatabase database = (ResourceDatabase)target;

        GUILayout.BeginVertical("box");
        GUILayout.Label("리소스 데이터베이스 관리", UnityEditor.EditorStyles.boldLabel);

        if (GUILayout.Button("누락된 리소스 자동 생성", GUILayout.Height(30)))
        {
            database.AutoCreateMissingResources();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("리소스 목록 새로고침"))
        {
            database.BuildLookupTable();
            UnityEditor.EditorUtility.SetDirty(database);
        }
        
        if (GUILayout.Button("리소스 목록 초기화"))
        {
            database.allResources.Clear();
        }

        GUILayout.Space(10);

        // 현재 상태 표시
        GUILayout.Label("현재 상태:", UnityEditor.EditorStyles.boldLabel);

        int totalTypes = System.Enum.GetValues(typeof(ResourceType)).Length - 1; // None 제외
        int currentResources = database.allResources?.Count ?? 0;

        if (currentResources < totalTypes)
        {
            GUILayout.Label($"⚠️ 누락된 리소스: {totalTypes - currentResources}개",
                           new GUIStyle(GUI.skin.label) { normal = { textColor = Color.red } });
        }
        else
        {
            GUILayout.Label($"✅ 모든 리소스 생성 완료 ({currentResources}개)",
                           new GUIStyle(GUI.skin.label) { normal = { textColor = Color.green } });
        }

        GUILayout.EndVertical();

        // 리소스 미리보기
        if (database.allResources != null && database.allResources.Count > 0)
        {
            GUILayout.Space(10);
            GUILayout.BeginVertical("box");
            GUILayout.Label("리소스 미리보기", UnityEditor.EditorStyles.boldLabel);

            foreach (var resource in database.allResources)
            {
                if (resource != null)
                {
                    GUILayout.BeginHorizontal();

                    // 색상 표시
                    var oldColor = GUI.color;
                    GUI.color = resource.displayColor;
                    GUILayout.Label("●", GUILayout.Width(20));
                    GUI.color = oldColor;

                    // 리소스 정보
                    GUILayout.Label($"{resource.resourceName} ({resource.type})");

                    // 아이콘 상태
                    if (resource.icon != null)
                    {
                        GUILayout.Label("🖼️", GUILayout.Width(20));
                    }
                    else
                    {
                        GUILayout.Label("❌", GUILayout.Width(20));
                    }

                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.EndVertical();
        }
    }
}
#endif