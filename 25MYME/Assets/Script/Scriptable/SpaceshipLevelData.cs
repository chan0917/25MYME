using System.Collections.Generic;
using UnityEngine;


#region ������ ��� �ý���
[System.Serializable]
public class LevelUpMaterial
{
    public ResourceType materialType;
    public int requiredAmount;

    [Header("���־� (���û���)")]
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
    [Header("������ ���")]
    public List<LevelUpMaterial> materials = new List<LevelUpMaterial>();

    [Header("�߰� ���� (���û���)")]
    public int goldCost = 0;
    public int minimumLevel = 1; // Ư�� ���� �̻󿡼��� ������ ����

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

#region ���ּ� ���� �� Ư�� �ɷ�
public enum SpaceshipStatType
{
    Health,         // ü��
    Speed,          // �̵��ӵ� (maxSpeed�� ����)
    CollectRange,   // ���� ����
    CollectSpeed    // ���� �ӵ�
}

public enum SpaceshipAbilityType
{
    None,
    GrappleArm,         // ������ �߰�
    AutoCollectDrone,   // �ڵ� ���� ���
    ShieldGenerator,    // ��ȣ�� ������
    FasterWarp,         // ���� ����
    LuckyCollector,     // ����� ������ (����� ����)
    ResourceMagnet,     // �ڿ� �ڼ� (���Ÿ� ����)
    EnergyEfficiency,   // ������ ȿ���� (�Ҹ� ����)
    AdvancedScanner     // ��� ��ĳ�� (��� � �߰߷� ����)
}

[System.Serializable]
public class StatBonus
{
    public SpaceshipStatType statType;
    public float bonusValue;
    public bool isPercentage = false; // false: ������, true: �ۼ�Ʈ

    [Header("���־� (���û���)")]
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
    public int level = 1; // �ɷ� ���� (�ߺ� ȹ�� �� ����)
    public bool isActive = true;

    [Header("�ɷ� ����")]
    public float effectValue = 1f; // �ɷ��� ȿ����
    public string abilityName;
    public string description;

    [Header("���־� (���û���)")]
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

#region ������ ���� �ý���
[System.Serializable]
public class LevelUpReward
{
    [Header("���� ���ʽ�")]
    public List<StatBonus> statBonuses = new List<StatBonus>();

    [Header("Ư�� �ɷ�")]
    public List<SpecialAbility> specialAbilities = new List<SpecialAbility>();

    [Header("�߰� ���� (���û���)")]
    public List<ResourceCost> bonusResources = new List<ResourceCost>();

    [Header("����")]
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
    [Header("���� ����")]
    public int level;
    public string levelName; // "�߽� ������", "���׶� ä����" ��
    public string description;

    [Header("������ �䱸����")]
    public LevelUpRequirement requirements;

    [Header("������ ����")]
    public LevelUpReward rewards;

    [Header("���־� (���û���)")]
    public Sprite levelIcon;
    public Color levelColor = Color.white;

    public bool CanLevelUp(ResourceManager resourceManager, int currentLevel)
    {
        // �ּ� ���� üũ
        if (currentLevel < requirements.minimumLevel) return false;

        // ��� üũ
        if (requirements.goldCost > 0 && !resourceManager.HasResource(ResourceType.Gold, requirements.goldCost))
            return false;

        // ��� üũ
        foreach (var material in requirements.materials)
        {
            if (!resourceManager.HasResource(material.materialType, material.requiredAmount))
                return false;
        }

        return true;
    }
}