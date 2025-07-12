using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    [Header("설정")]
    public ResourceDatabase resourceDatabase; // 중앙 데이터베이스 참조

    [Header("현재 보유 자원")]
    public Dictionary<ResourceType, int> playerResources = new Dictionary<ResourceType, int>();

    [Header("플레이어 레벨")]
    public int playerLevel = 1;

    void Start()
    {
        InitializePlayerResources();
    }

    void InitializePlayerResources()
    {
        foreach (ResourceType resourceType in System.Enum.GetValues(typeof(ResourceType)))
        {
            if (resourceType != ResourceType.None)
                playerResources[resourceType] = 0;
        }

        playerResources[ResourceType.Gold] = 1000; // 시작 자원
    }

    #region 자원 관리 메서드
    public void AddResource(ResourceType type, int amount)
    {
        if (playerResources.ContainsKey(type))
            playerResources[type] += amount;
        else
            playerResources[type] = amount;

        string resourceName = resourceDatabase.GetResourceName(type);
        Debug.Log($"{resourceName} {amount}개 획득! 총 보유량: {playerResources[type]}");
    }

    public bool HasResource(ResourceType type, int amount)
    {
        return playerResources.ContainsKey(type) && playerResources[type] >= amount;
    }

    public bool ConsumeResource(ResourceType type, int amount)
    {
        if (HasResource(type, amount))
        {
            playerResources[type] -= amount;
            string resourceName = resourceDatabase.GetResourceName(type);
            Debug.Log($"{resourceName} {amount}개 소모! 남은 보유량: {playerResources[type]}");
            return true;
        }
        return false;
    }

    public bool ConsumeResources(MultiResourceCost cost)
    {
        // 먼저 모든 자원이 충분한지 확인
        foreach (var resourceCost in cost.costs)
        {
            if (!HasResource(resourceCost.resourceType, resourceCost.amount))
                return false;
        }

        // 모든 자원 소모
        foreach (var resourceCost in cost.costs)
        {
            ConsumeResource(resourceCost.resourceType, resourceCost.amount);
        }

        return true;
    }

    public int GetResourceAmount(ResourceType type)
    {
        return playerResources.ContainsKey(type) ? playerResources[type] : 0;
    }

    public ResourceData GetResourceData(ResourceType type)
    {
        return resourceDatabase.GetResourceData(type);
    }

    public Sprite GetResourceIcon(ResourceType type)
    {
        return resourceDatabase.GetResourceIcon(type);
    }

    public string GetResourceDisplayText(ResourceType type, int amount)
    {
        return resourceDatabase.FormatResourceAmount(type, amount);
    }
    #endregion

    #region 플레이어 레벨 관리
    public void SetPlayerLevel(int level)
    {
        playerLevel = level;
        Debug.Log($"플레이어 레벨이 {level}로 설정되었습니다.");
    }

    public int GetPlayerLevel()
    {
        return playerLevel;
    }

    public void LevelUp()
    {
        playerLevel++;
        Debug.Log($"레벨업! 현재 레벨: {playerLevel}");
    }
    #endregion
}
