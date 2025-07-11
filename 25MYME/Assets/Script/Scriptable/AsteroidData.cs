using System.Collections.Generic;
using UnityEngine;

public enum ResourceType
{
    // 기본 화폐
    None,
    Gold,
    Gem,

    // 운석에서 직접 드랍되는 강화 재료들
    Crystal,        // 크리스탈
    UpgradeStone,   // 강화석
    ProtectionStone, // 보호석
    LuckyCharm,     // 행운의 부적
    Equipment,      // 장비
    Fragment        // 파편
}

public enum AsteroidType
{
    BasicAsteroid,    // 기본 운석 (강화석 드랍)
    CrystalAsteroid,  // 크리스탈 운석 (크리스탈 드랍)
    RareAsteroid,     // 희귀 운석 (보호석 드랍)
    LegendaryAsteroid // 전설 운석 (행운의 부적 드랍)
}

[System.Serializable]
public class ResourceDrop
{
    [Header("드랍 자원")]
    public ResourceType resourceType;

    [Header("드랍 확률 및 수량")]
    [Range(0f, 1f)]
    public float dropChance = 1f;
    public int minAmount = 1;
    public int maxAmount = 3;

    [Header("비주얼 (선택사항)")]
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
    [Header("기본 정보")]
    public string asteroidName;
    public AsteroidType asteroidType;
    public string description;

    [Header("필요 레벨")]
    public int requiredLevel = 1; // 채굴 가능한 최소 레벨

    [Header("채굴 설정")]
    public float miningTime = 3f; // 채굴 소요 시간
    public int energyCost = 10;   // 에너지 소모량

    [Header("드랍 테이블")]
    public List<ResourceDrop> resourceDrops = new List<ResourceDrop>();

    [Header("생성 확률")]
    [Range(0f, 1f)]
    public float spawnWeight = 1f; // 생성 가중치 (높을수록 자주 생성)

    [Header("비주얼 (선택사항)")]
    public Sprite asteroidSprite;
    public Color asteroidColor = Color.white;

    [Header("사운드 (선택사항)")]
    public AudioClip miningSound;
    public AudioClip depletedSound;

    public List<ResourceDrop> GetDroppedResources()
    {
        List<ResourceDrop> droppedResources = new List<ResourceDrop>();

        foreach (var drop in resourceDrops)
        {
            if (drop.ShouldDrop())
            {
                // 드랍되는 자원의 복사본 생성 (수량 재계산)
                var droppedResource = new ResourceDrop
                {
                    resourceType = drop.resourceType,
                    dropChance = 1f, // 이미 드랍 확정
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