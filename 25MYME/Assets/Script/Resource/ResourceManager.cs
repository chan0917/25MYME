using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    [Header("����")]
    public List<ResourceData> allResourceData = new List<ResourceData>();
    public CraftingTable craftingTable;

    [Header("���� ���� �ڿ�")]
    public Dictionary<ResourceType, int> playerResources = new Dictionary<ResourceType, int>();

    [Header("�÷��̾� ����")]
    public int playerLevel = 1; // �÷��̾��� ä�� ����

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
        // ��� �ڿ��� 0���� �ʱ�ȭ
        foreach (ResourceType resourceType in System.Enum.GetValues(typeof(ResourceType)))
        {
            if (resourceType != ResourceType.None)
                playerResources[resourceType] = 0;
        }

        // ���� �ڿ� ����
        playerResources[ResourceType.Gold] = 1000;
    }

    #region �÷��̾� ���� ����
    public void SetPlayerLevel(int level)
    {
        playerLevel = level;
        Debug.Log($"�÷��̾� ������ {level}�� �����Ǿ����ϴ�.");
    }

    public int GetPlayerLevel()
    {
        return playerLevel;
    }

    public void LevelUp()
    {
        playerLevel++;
        Debug.Log($"������! ���� ����: {playerLevel}");
    }
    #endregion

    #region �ڿ� ���� �޼���
    public void AddResource(ResourceType type, int amount)
    {
        if (playerResources.ContainsKey(type))
            playerResources[type] += amount;
        else
            playerResources[type] = amount;

        Debug.Log($"{type} {amount}�� ȹ��! �� ������: {playerResources[type]}");
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
            Debug.Log($"{type} {amount}�� �Ҹ�! ���� ������: {playerResources[type]}");
            return true;
        }
        return false;
    }

    public bool ConsumeResources(MultiResourceCost cost)
    {
        // ���� ��� �ڿ��� ������� Ȯ��
        foreach (var resourceCost in cost.costs)
        {
            if (!HasResource(resourceCost.resourceType, resourceCost.amount))
                return false;
        }

        // ��� �ڿ� �Ҹ�
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

    #region ���� �ý���
    public bool CanCraft(ResourceType outputResource)
    {
        var recipe = craftingTable.GetRecipe(outputResource);
        if (recipe == null) return false;

        // ���� ��� Ȯ��
        if (recipe.craftingCost > 0 && !HasResource(recipe.craftingCostType, recipe.craftingCost))
            return false;

        // ��� Ȯ��
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

        // ��� �Ҹ�
        foreach (var input in recipe.inputResources)
        {
            ConsumeResource(input.resourceType, input.amount);
        }

        // ���� ��� �Ҹ�
        if (recipe.craftingCost > 0)
            ConsumeResource(recipe.craftingCostType, recipe.craftingCost);

        // ����� ȹ��
        AddResource(recipe.outputResource, recipe.outputAmount);

        Debug.Log($"{outputResource} {recipe.outputAmount}�� ���� �Ϸ�!");
        return true;
    }
    #endregion
}