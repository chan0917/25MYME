using System.Collections.Generic;
using UnityEngine;

public enum ResourceType
{
    // �⺻ ȭ��
    None,
    Gold,
    Gem,

    // ����� ���� ����Ǵ� ��ȭ ����
    Crystal,        // ũ����Ż
    UpgradeStone,   // ��ȭ��
    ProtectionStone, // ��ȣ��
    LuckyCharm,     // ����� ����
    Equipment,      // ���
    Fragment,        // ����

    SclapMetal,
    Nickel,
    Ascied,
    Meteorite
}

public enum AsteroidType
{
    BasicAsteroid,    // �⺻ � (��ȭ�� ���)
    CrystalAsteroid,  // ũ����Ż � (ũ����Ż ���)
    RareAsteroid,     // ��� � (��ȣ�� ���)
    LegendaryAsteroid // ���� � (����� ���� ���)
}

[System.Serializable]
public class ResourceDrop
{
    [Header("��� �ڿ�")]
    public ResourceType resourceType;

    [Header("��� Ȯ�� �� ����")]
    [Range(0f, 1f)]
    public float dropChance = 1f;
    public int minAmount = 1;
    public int maxAmount = 3;

    [Header("���־� (���û���)")]
    public Sprite dropEffectSprite;
    public Color dropEffectColor = Color.white;

    // ���� �����ܰ� ������ ResourceDatabase���� �ڵ����� ������
    public Sprite GetIcon(ResourceDatabase database)
    {
        return database.GetResourceIcon(resourceType);
    }

    public Color GetColor(ResourceDatabase database)
    {
        return database.GetResourceColor(resourceType);
    }

    public int GetRandomAmount()
    {
        return Random.Range(minAmount, maxAmount + 1);
    }

    public bool ShouldDrop()
    {
        return Random.Range(0f, 1f) <= dropChance;
    }
}


[CreateAssetMenu(fileName = "AsteroidData", menuName = "Game/AsteroidData")]
public class AsteroidData : ScriptableObject
{
    [Header("�⺻ ����")]
    public string asteroidName;
    public AsteroidType asteroidType;
    public string description;

    [Header("�ʿ� ����")]
    public int requiredLevel = 1; // ä�� ������ �ּ� ����

    [Header("ä�� ����(������)")]
    public float miningTime = 3f; // ä�� �ҿ� �ð�
    public int energyCost = 10;   // ������ �Ҹ�

    [Header("���� ����")]
    public float mass = 1f; // ���� (������ٵ� ����)
    public float drag = 0.5f; // �巡�� (���û���)

    [Header("��� ���̺�")]
    public List<ResourceDrop> resourceDrops = new List<ResourceDrop>();

    [Header("���� ����")]
    [Range(0f, 1f)]
    public float spawnWeight = 1f; // ���� ���� �������� ����� Ȯ��
    [Range(0f, 100f)]
    public float minSpawnZone = 0f; // ���� ������ �ּ� ���� (%) - �߽ɿ����� �Ÿ�
    [Range(0f, 100f)]
    public float maxSpawnZone = 100f; // ���� ������ �ִ� ���� (%) - �߽ɿ����� �Ÿ�

    [Header("���־�")]
    public Sprite asteroidSprite;
    public Color asteroidColor = Color.white;

    [Header("����")]
    public AudioClip miningSound;
    public AudioClip depletedSound;

    // ������ �Ÿ��� �� ��� ���� ������ ���ԵǴ��� Ȯ��
    public bool CanSpawnAtDistance(float distancePercent)
    {
        return distancePercent >= minSpawnZone && distancePercent <= maxSpawnZone;
    }

    public List<ResourceDrop> GetDroppedResources()
    {
        List<ResourceDrop> droppedResources = new List<ResourceDrop>();

        foreach (var drop in resourceDrops)
        {
            if (drop.ShouldDrop())
            {
                var droppedResource = new ResourceDrop
                {
                    resourceType = drop.resourceType,
                    dropChance = 1f,
                    minAmount = drop.GetRandomAmount(),
                    maxAmount = drop.GetRandomAmount()
                };
                droppedResources.Add(droppedResource);
            }
        }

        return droppedResources;
    }
}