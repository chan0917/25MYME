using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#region 우주선 스탯 관리자
[System.Serializable]
public class SpaceshipStats
{
    [Header("기본 스탯")]
    public float baseHealth = 100f;
    public float baseSpeed = 20f;
    public float baseCollectRange = 5f;
    public float baseCollectSpeed = 1f;

    [Header("현재 보너스")]
    public Dictionary<SpaceshipStatType, float> statBonuses = new Dictionary<SpaceshipStatType, float>();
    public Dictionary<SpaceshipStatType, float> percentBonuses = new Dictionary<SpaceshipStatType, float>();

    public void InitializeStats()
    {
        foreach (SpaceshipStatType statType in System.Enum.GetValues(typeof(SpaceshipStatType)))
        {
            statBonuses[statType] = 0f;
            percentBonuses[statType] = 0f;
        }
    }

    public void AddStatBonus(StatBonus bonus)
    {
        if (bonus.isPercentage)
            percentBonuses[bonus.statType] += bonus.bonusValue;
        else
            statBonuses[bonus.statType] += bonus.bonusValue;
    }

    public float GetFinalStat(SpaceshipStatType statType)
    {
        float baseStat = GetBaseStat(statType);
        float flatBonus = statBonuses.ContainsKey(statType) ? statBonuses[statType] : 0f;
        float percentBonus = percentBonuses.ContainsKey(statType) ? percentBonuses[statType] : 0f;

        return (baseStat + flatBonus) * (1f + percentBonus / 100f);
    }

    private float GetBaseStat(SpaceshipStatType statType)
    {
        switch (statType)
        {
            case SpaceshipStatType.Health: return baseHealth;
            case SpaceshipStatType.Speed: return baseSpeed;
            case SpaceshipStatType.CollectRange: return baseCollectRange;
            case SpaceshipStatType.CollectSpeed: return baseCollectSpeed;
            default: return 0f;
        }
    }
}
#endregion

public class SpaceshipLevelManager : MonoBehaviour
{
    [Header("레벨 데이터")]
    public List<SpaceshipLevelData> levelDataList = new List<SpaceshipLevelData>();

    [Header("현재 상태")]
    public int currentLevel = 1;
    public SpaceshipStats stats = new SpaceshipStats();
    public List<SpecialAbility> activeAbilities = new List<SpecialAbility>();

    [Header("참조")]
    public ResourceManager resourceManager;
    public SpaceshipController spaceshipController;
    public GrappleArmSystem grappleArmSystem;

    [Header("현재 스탯값")]
    public float currentHealth = 100f;
    public float maxHealth = 100f;

    private Dictionary<int, SpaceshipLevelData> levelDataTable;


    void Start()
    {
        InitializeLevelTable();
        stats.InitializeStats();

        if (resourceManager == null)
            resourceManager = FindAnyObjectByType<ResourceManager>();

        if (spaceshipController == null)
            spaceshipController = FindAnyObjectByType<SpaceshipController>();

        if (grappleArmSystem == null)
            grappleArmSystem = FindAnyObjectByType<GrappleArmSystem>();

        ApplyStatsToSpaceship();

        // 초기 체력 설정
        maxHealth = stats.GetFinalStat(SpaceshipStatType.Health);
        currentHealth = maxHealth;
    }

    void InitializeLevelTable()
    {
        levelDataTable = new Dictionary<int, SpaceshipLevelData>();
        foreach (var levelData in levelDataList)
        {
            levelDataTable[levelData.level] = levelData;
        }
    }

    public bool CanLevelUp()
    {
        var nextLevelData = GetNextLevelData();
        if (nextLevelData == null) return false;

        return nextLevelData.CanLevelUp(resourceManager, currentLevel);
    }

    public bool TryLevelUp()
    {
        if (!CanLevelUp()) return false;

        var nextLevelData = GetNextLevelData();

        // 재료 소모
        foreach (var material in nextLevelData.requirements.materials)
        {
            resourceManager.ConsumeResource(material.materialType, material.requiredAmount);
        }

        // 골드 소모
        if (nextLevelData.requirements.goldCost > 0)
            resourceManager.ConsumeResource(ResourceType.Gold, nextLevelData.requirements.goldCost);

        // 레벨업 적용
        ApplyLevelUpRewards(nextLevelData.rewards);
        currentLevel++;

        // 플레이어 레벨도 동기화
        resourceManager.SetPlayerLevel(currentLevel);

        // 우주선에 새로운 스탯 적용
        ApplyStatsToSpaceship();

        // 체력 회복 (레벨업 시 풀 체력)
        maxHealth = stats.GetFinalStat(SpaceshipStatType.Health);
        currentHealth = maxHealth;

        Debug.Log($"우주선 레벨업! 현재 레벨: {currentLevel} - {nextLevelData.levelName}");
        return true;
    }

    void ApplyLevelUpRewards(LevelUpReward rewards)
    {
        // 스탯 보너스 적용
        foreach (var statBonus in rewards.statBonuses)
        {
            stats.AddStatBonus(statBonus);
        }

        // 특수 능력 추가/업그레이드
        foreach (var newAbility in rewards.specialAbilities)
        {
            var existingAbility = activeAbilities.Find(a => a.abilityType == newAbility.abilityType);
            if (existingAbility != null)
            {
                // 기존 능력 레벨업
                existingAbility.level++;
                existingAbility.effectValue += newAbility.effectValue;
            }
            else
            {
                // 새 능력 추가
                activeAbilities.Add(newAbility);
            }
        }

        // 보너스 자원 지급
        foreach (var bonusResource in rewards.bonusResources)
        {
            resourceManager.AddResource(bonusResource.resourceType, bonusResource.amount);
        }
    }

    // 우주선에 스탯 적용 (우주선 스크립트 수정 없이)
    void ApplyStatsToSpaceship()
    {
        if (spaceshipController == null) return;

        // Speed 스탯을 우주선의 maxSpeed에 적용
        float finalSpeed = stats.GetFinalStat(SpaceshipStatType.Speed);

        // Reflection을 사용해서 우주선의 maxSpeed 값 변경
        var field = typeof(SpaceshipController).GetField("maxSpeed");
        if (field != null)
        {
            field.SetValue(spaceshipController, finalSpeed);
            Debug.Log($"우주선 속도 적용: {finalSpeed}");
        }

        grappleArmSystem.settings.maxRange = stats.GetFinalStat(SpaceshipStatType.CollectRange);
        Debug.Log($"우주선 고리 적용: {stats.GetFinalStat(SpaceshipStatType.CollectRange)}");

        // 다른 스탯들도 필요에 따라 적용 가능
        // 예: thrustForce, rotationSpeed 등
    }

    public SpaceshipLevelData GetCurrentLevelData()
    {
        return levelDataTable.ContainsKey(currentLevel) ? levelDataTable[currentLevel] : null;
    }

    public SpaceshipLevelData GetNextLevelData()
    {
        int nextLevel = currentLevel + 1;
        return levelDataTable.ContainsKey(nextLevel) ? levelDataTable[nextLevel] : null;
    }

    public bool HasAbility(SpaceshipAbilityType abilityType)
    {
        return activeAbilities.Exists(a => a.abilityType == abilityType && a.isActive);
    }

    public SpecialAbility GetAbility(SpaceshipAbilityType abilityType)
    {
        return activeAbilities.Find(a => a.abilityType == abilityType && a.isActive);
    }

    public float GetStatValue(SpaceshipStatType statType)
    {
        return stats.GetFinalStat(statType);
    }

    public List<LevelUpMaterial> GetNextLevelRequirements()
    {
        var nextLevel = GetNextLevelData();
        return nextLevel?.requirements.materials ?? new List<LevelUpMaterial>();
    }

    // 체력 관리
    public void TakeDamage(float damage)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);
        Debug.Log($"데미지 {damage} 받음. 현재 체력: {currentHealth}/{maxHealth}");
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        Debug.Log($"체력 {amount} 회복. 현재 체력: {currentHealth}/{maxHealth}");
    }

    public bool IsDead()
    {
        return currentHealth <= 0;
    }

    public float GetHealthRatio()
    {
        return maxHealth > 0 ? currentHealth / maxHealth : 0f;
    }

#if UNITY_EDITOR
    public void AutoLoadLevelData()
    {
        levelDataList.Clear();

        // GameData/SpaceshipLevel 폴더에서 모든 SpaceshipLevelData 찾기
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:SpaceshipLevelData", new[] { "Assets/GameData/SpaceshipLevel" });

        foreach (string guid in guids)
        {
            string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            SpaceshipLevelData levelData = UnityEditor.AssetDatabase.LoadAssetAtPath<SpaceshipLevelData>(assetPath);

            if (levelData != null)
            {
                levelDataList.Add(levelData);
            }
        }

        // 레벨 순으로 정렬
        levelDataList.Sort((a, b) => a.level.CompareTo(b.level));

        UnityEditor.EditorUtility.SetDirty(this);

        Debug.Log($"레벨 데이터 자동 로드 완료: {levelDataList.Count}개 발견");
    }

    public void CreateNewLevelData()
    {
        // 다음 레벨 번호 찾기
        int nextLevel = 2; // 기본값 레벨 2부터
        if (levelDataList.Count > 0)
        {
            nextLevel = levelDataList.Max(l => l.level) + 1;
        }

        var newLevelData = ScriptableObject.CreateInstance<SpaceshipLevelData>();

        // 기본값 설정
        newLevelData.level = nextLevel;
        newLevelData.levelName = $"레벨 {nextLevel}";
        newLevelData.description = $"우주선을 레벨 {nextLevel}로 업그레이드합니다.";

        // 기본 요구사항 설정
        newLevelData.requirements = new LevelUpRequirement();
        newLevelData.requirements.goldCost = nextLevel * 1000;
        newLevelData.requirements.AddMaterial(ResourceType.UpgradeStone, nextLevel * 2);

        // 기본 보상 설정
        newLevelData.rewards = new LevelUpReward();
        newLevelData.rewards.AddStatBonus(SpaceshipStatType.Health, 20f); // 체력 +20
        newLevelData.rewards.AddStatBonus(SpaceshipStatType.Speed, 2f);   // 속도 +2
        newLevelData.rewards.rewardDescription = $"체력 +20, 속도 +2";

        // 폴더 확인 및 생성
        string folderPath = "Assets/GameData/SpaceshipLevel";
        if (!UnityEditor.AssetDatabase.IsValidFolder(folderPath))
        {
            if (!UnityEditor.AssetDatabase.IsValidFolder("Assets/GameData"))
            {
                UnityEditor.AssetDatabase.CreateFolder("Assets", "GameData");
            }
            UnityEditor.AssetDatabase.CreateFolder("Assets/GameData", "SpaceshipLevel");
        }

        // 파일 저장
        string fileName = $"SpaceshipLevel_{nextLevel:D2}";
        string assetPath = $"{folderPath}/{fileName}.asset";

        UnityEditor.AssetDatabase.CreateAsset(newLevelData, assetPath);
        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.AssetDatabase.Refresh();

        // 즉시 선택
        UnityEditor.Selection.activeObject = newLevelData;
        UnityEditor.EditorGUIUtility.PingObject(newLevelData);

        Debug.Log($"새 레벨 데이터 생성: {assetPath}");

        // 자동으로 리스트에 추가
        AutoLoadLevelData();
    }
#endif
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(SpaceshipLevelManager))]
public class SpaceshipLevelManagerEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(20);

        SpaceshipLevelManager manager = (SpaceshipLevelManager)target;

        GUILayout.BeginVertical("box");
        GUILayout.Label("🚀 우주선 레벨 데이터 관리", UnityEditor.EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("GameData/SpaceshipLevel\n자동 로드", GUILayout.Height(50)))
        {
            manager.AutoLoadLevelData();
        }

        if (GUILayout.Button("새 레벨 생성", GUILayout.Height(50)))
        {
            manager.CreateNewLevelData();
        }

        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        // 현재 상태 표시
        if (manager.levelDataList != null && manager.levelDataList.Count > 0)
        {
            GUILayout.Label($"✅ 로드된 레벨: {manager.levelDataList.Count}개",
                           new GUIStyle(GUI.skin.label) { normal = { textColor = Color.green } });

            GUILayout.Space(5);
            GUILayout.Label("레벨 목록:", UnityEditor.EditorStyles.boldLabel);

            for (int i = 0; i < manager.levelDataList.Count && i < 8; i++)
            {
                var levelData = manager.levelDataList[i];
                if (levelData != null)
                {
                    GUILayout.BeginHorizontal();

                    // 레벨
                    GUILayout.Label($"Lv.{levelData.level}", GUILayout.Width(40));

                    // 이름
                    GUILayout.Label(levelData.levelName, GUILayout.Width(100));

                    // 골드 비용
                    GUILayout.Label($"골드:{levelData.requirements.goldCost}", GUILayout.Width(80));

                    // 재료 수
                    GUILayout.Label($"재료:{levelData.requirements.materials.Count}개", GUILayout.Width(60));

                    GUILayout.EndHorizontal();
                }
            }

            if (manager.levelDataList.Count > 8)
            {
                GUILayout.Label($"... 외 {manager.levelDataList.Count - 8}개 더");
            }
        }
        else
        {
            GUILayout.Label("⚠️ 로드된 레벨이 없습니다.",
                           new GUIStyle(GUI.skin.label) { normal = { textColor = Color.yellow } });
        }

        GUILayout.EndVertical();

        // 현재 스탯 표시
        if (Application.isPlaying)
        {
            GUILayout.Space(10);
            GUILayout.BeginVertical("box");
            GUILayout.Label("📊 현재 우주선 스탯", UnityEditor.EditorStyles.boldLabel);

            GUILayout.Label($"🔋 체력: {manager.currentHealth:F0}/{manager.maxHealth:F0} ({manager.GetHealthRatio() * 100:F0}%)");
            GUILayout.Label($"🚀 속도: {manager.GetStatValue(SpaceshipStatType.Speed):F1}");
            GUILayout.Label($"📡 수집 범위: {manager.GetStatValue(SpaceshipStatType.CollectRange):F1}");
            GUILayout.Label($"⚡ 수집 속도: {manager.GetStatValue(SpaceshipStatType.CollectSpeed):F1}");

            GUILayout.Space(5);
            GUILayout.Label($"🎯 활성 능력: {manager.activeAbilities.Count}개");

            GUILayout.EndVertical();
        }

        // 폴더 정보
        GUILayout.Space(10);
        GUILayout.BeginVertical("box");
        GUILayout.Label("📁 폴더 정보", UnityEditor.EditorStyles.boldLabel);
        GUILayout.Label("레벨 데이터 위치: Assets/GameData/SpaceshipLevel/");
        GUILayout.EndVertical();
    }
}
#endif
