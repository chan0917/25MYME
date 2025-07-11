using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ResourceCost
{
    public ResourceType resourceType;
    public int amount;

    public ResourceCost(ResourceType type, int cost)
    {
        resourceType = type;
        amount = cost;
    }
}

[System.Serializable]
public class MultiResourceCost
{
    public List<ResourceCost> costs = new List<ResourceCost>();

    // ��� + ��ȭ�� ���� ����
    public static MultiResourceCost CreateGoldAndStone(int gold, int stone)
    {
        var cost = new MultiResourceCost();
        cost.costs.Add(new ResourceCost(ResourceType.Gold, gold));
        cost.costs.Add(new ResourceCost(ResourceType.UpgradeStone, stone));
        return cost;
    }
}

[System.Serializable]
public class CraftingRecipe
{
    [Header("���� ���")]
    public ResourceType outputResource;
    public int outputAmount = 1;

    [Header("�ʿ� ���")]
    public List<ResourceCost> inputResources = new List<ResourceCost>();

    [Header("���� ����")]
    public float craftingTime = 5f;
    public int craftingCost = 0; // �߰� ��� (��� ��)
    public ResourceType craftingCostType = ResourceType.Gold;

    [Header("���־� (���û���)")]
    public Sprite craftingIcon;
    public string craftingDescription;
}


[CreateAssetMenu(fileName = "CraftingTable", menuName = "Game/CraftingTable")]
public class CraftingTable : ScriptableObject
{
    [Header("���۹� ���")]
    public List<CraftingRecipe> recipes = new List<CraftingRecipe>();

    public CraftingRecipe GetRecipe(ResourceType outputResource)
    {
        return recipes.Find(r => r.outputResource == outputResource);
    }

    public List<CraftingRecipe> GetRecipesUsingResource(ResourceType inputResource)
    {
        return recipes.FindAll(r => r.inputResources.Exists(input => input.resourceType == inputResource));
    }
}