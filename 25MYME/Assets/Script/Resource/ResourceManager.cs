using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    [Header("설정")]
    public List<ResourceData> allResourceData = new List<ResourceData>();
    public CraftingTable craftingTable;

    [Header("현재 보유 자원")]
    public Dictionary<ResourceType, int> playerResources = new Dictionary<ResourceType, int>();

    [Header("플레이어 레벨")]
    public int playerLevel = 1; // 플레이어의 채굴 레벨

    private Dictionary<ResourceType, ResourceData> resourceDataTable;

    void Start()
    {
        InitializeResourceTable();
        InitializePlayerResources();
    }

    void InitializeResourceTable()
    {
        resourceDataTable = new Dictionary<ResourceType, ResourceData>();
        foreach (var resourceData in allResourceData)
        {
            resourceDataTable[resourceData.type] = resourceData;
        }
    }

    void InitializePlayerResources()
    {
        // 모든 자원을 0으로 초기화
        foreach (ResourceType resourceType in System.Enum.GetValues(typeof(ResourceType)))
        {
            if (resourceType != ResourceType.None)
                playerResources[resourceType] = 0;
        }

        // 시작 자원 설정
        playerResources[ResourceType.Gold] = 1000;
    }

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

    #region 자원 관리 메서드
    public void AddResource(ResourceType type, int amount)
    {
        if (playerResources.ContainsKey(type))
            playerResources[type] += amount;
        else
            playerResources[type] = amount;

        Debug.Log($"{type} {amount}개 획득! 총 보유량: {playerResources[type]}");
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
            Debug.Log($"{type} {amount}개 소모! 남은 보유량: {playerResources[type]}");
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
        return resourceDataTable.ContainsKey(type) ? resourceDataTable[type] : null;
    }
    #endregion

    #region 제작 시스템
    public bool CanCraft(ResourceType outputResource)
    {
        var recipe = craftingTable.GetRecipe(outputResource);
        if (recipe == null) return false;

        // 제작 비용 확인
        if (recipe.craftingCost > 0 && !HasResource(recipe.craftingCostType, recipe.craftingCost))
            return false;

        // 재료 확인
        foreach (var input in recipe.inputResources)
        {
            if (!HasResource(input.resourceType, input.amount))
                return false;
        }

        return true;
    }

    public bool TryCraft(ResourceType outputResource)
    {
        if (!CanCraft(outputResource)) return false;

        var recipe = craftingTable.GetRecipe(outputResource);

        // 재료 소모
        foreach (var input in recipe.inputResources)
        {
            ConsumeResource(input.resourceType, input.amount);
        }

        // 제작 비용 소모
        if (recipe.craftingCost > 0)
            ConsumeResource(recipe.craftingCostType, recipe.craftingCost);

        // 결과물 획득
        AddResource(recipe.outputResource, recipe.outputAmount);

        Debug.Log($"{outputResource} {recipe.outputAmount}개 제작 완료!");
        return true;
    }
    #endregion
}