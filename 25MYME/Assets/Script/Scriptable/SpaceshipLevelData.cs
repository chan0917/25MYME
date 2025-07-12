using System.Collections.Generic;
using UnityEngine;


#region 레벨업 재료 시스템
[System.Serializable]
public class LevelUpMaterial
{
    public ResourceType materialType;
    public int requiredAmount;

    [Header("비주얼 (선택사항)")]
    public Sprite materialIcon;

    public LevelUpMaterial(ResourceType type, int amount)
    {
        materialType = type;
        requiredAmount = amount;
    }
}

[System.Serializable]
public class LevelUpRequirement
{
    [Header("레벨업 재료")]
    public List<LevelUpMaterial> materials = new List<LevelUpMaterial>();

    [Header("추가 조건 (선택사항)")]
    public int goldCost = 0;
    public int minimumLevel = 1; // 특정 레벨 이상에서만 레벨업 가능

    public void AddMaterial(ResourceType type, int amount)
    {
        materials.Add(new LevelUpMaterial(type, amount));
    }

    public bool HasMaterial(ResourceType type)
    {
        return materials.Exists(m => m.materialType == type);
    }

    public int GetRequiredAmount(ResourceType type)
    {
        var material = materials.Find(m => m.materialType == type);
        return material?.requiredAmount ?? 0;
    }
}
#endregion

#region 우주선 스탯 및 특수 능력
public enum SpaceshipStatType
{
    Health,         // 체력
    Speed,          // 이동속도 (maxSpeed에 적용)
    CollectRange,   // 수집 범위
    CollectSpeed    // 수집 속도
}

public enum SpaceshipAbilityType
{
    None,
    GrappleArm,         // 집게팔 추가
    AutoCollectDrone,   // 자동 수집 드론
    ShieldGenerator,    // 보호막 생성기
    FasterWarp,         // 빠른 워프
    LuckyCollector,     // 행운의 수집가 (드랍률 증가)
    ResourceMagnet,     // 자원 자석 (원거리 수집)
    EnergyEfficiency,   // 에너지 효율성 (소모량 감소)
    AdvancedScanner     // 고급 스캐너 (희귀 운석 발견률 증가)
}

[System.Serializable]
public class StatBonus
{
    public SpaceshipStatType statType;
    public float bonusValue;
    public bool isPercentage = false; // false: 고정값, true: 퍼센트

    [Header("비주얼 (선택사항)")]
    public string description;

    public StatBonus(SpaceshipStatType type, float value, bool percentage = false)
    {
        statType = type;
        bonusValue = value;
        isPercentage = percentage;
    }
}

[System.Serializable]
public class SpecialAbility
{
    public SpaceshipAbilityType abilityType;
    public int level = 1; // 능력 레벨 (중복 획득 시 증가)
    public bool isActive = true;

    [Header("능력 설정")]
    public float effectValue = 1f; // 능력의 효과값
    public string abilityName;
    public string description;

    [Header("비주얼 (선택사항)")]
    public Sprite abilityIcon;
    public Color abilityColor = Color.white;

    public SpecialAbility(SpaceshipAbilityType type, string name, string desc)
    {
        abilityType = type;
        abilityName = name;
        description = desc;
    }
}
#endregion

#region 레벨업 보상 시스템
[System.Serializable]
public class LevelUpReward
{
    [Header("스탯 보너스")]
    public List<StatBonus> statBonuses = new List<StatBonus>();

    [Header("특수 능력")]
    public List<SpecialAbility> specialAbilities = new List<SpecialAbility>();

    [Header("추가 보상 (선택사항)")]
    public List<ResourceCost> bonusResources = new List<ResourceCost>();

    [Header("설명")]
    public string rewardDescription;

    public void AddStatBonus(SpaceshipStatType statType, float value, bool isPercentage = false)
    {
        statBonuses.Add(new StatBonus(statType, value, isPercentage));
    }

    public void AddSpecialAbility(SpaceshipAbilityType abilityType, string name, string description)
    {
        specialAbilities.Add(new SpecialAbility(abilityType, name, description));
    }

    public void AddBonusResource(ResourceType type, int amount)
    {
        bonusResources.Add(new ResourceCost(type, amount));
    }
}
#endregion

[CreateAssetMenu(fileName = "SpaceshipLevelData", menuName = "Game/SpaceshipLevelData")]
public class SpaceshipLevelData : ScriptableObject
{
    [Header("레벨 정보")]
    public int level;
    public string levelName; // "견습 조종사", "베테랑 채굴자" 등
    public string description;

    [Header("레벨업 요구사항")]
    public LevelUpRequirement requirements;

    [Header("레벨업 보상")]
    public LevelUpReward rewards;

    [Header("비주얼 (선택사항)")]
    public Sprite levelIcon;
    public Color levelColor = Color.white;

    public bool CanLevelUp(ResourceManager resourceManager, int currentLevel)
    {
        // 최소 레벨 체크
        if (currentLevel < requirements.minimumLevel) return false;

        // 골드 체크
        if (requirements.goldCost > 0 && !resourceManager.HasResource(ResourceType.Gold, requirements.goldCost))
            return false;

        // 재료 체크
        foreach (var material in requirements.materials)
        {
            if (!resourceManager.HasResource(material.materialType, material.requiredAmount))
                return false;
        }

        return true;
    }
}