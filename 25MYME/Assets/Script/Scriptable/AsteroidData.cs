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
    Fragment        // ����
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

    [Header("ä�� ����")]
    public float miningTime = 3f; // ä�� �ҿ� �ð�
    public int energyCost = 10;   // ������ �Ҹ�

    [Header("��� ���̺�")]
    public List<ResourceDrop> resourceDrops = new List<ResourceDrop>();

    [Header("���� Ȯ��")]
    [Range(0f, 1f)]
    public float spawnWeight = 1f; // ���� ����ġ (�������� ���� ����)

    [Header("���־� (���û���)")]
    public Sprite asteroidSprite;
    public Color asteroidColor = Color.white;

    [Header("���� (���û���)")]
    public AudioClip miningSound;
    public AudioClip depletedSound;

    public List<ResourceDrop> GetDroppedResources()
    {
        List<ResourceDrop> droppedResources = new List<ResourceDrop>();

        foreach (var drop in resourceDrops)
        {
            if (drop.ShouldDrop())
            {
                // ����Ǵ� �ڿ��� ���纻 ���� (���� ����)
                var droppedResource = new ResourceDrop
                {
                    resourceType = drop.resourceType,
                    dropChance = 1f, // �̹� ��� Ȯ��
                    minAmount = drop.GetRandomAmount(),
                    maxAmount = drop.GetRandomAmount(),
                    dropEffectSprite = drop.dropEffectSprite,
                    dropEffectColor = drop.dropEffectColor
                };
                droppedResources.Add(droppedResource);
            }
        }

        return droppedResources;
    }
}