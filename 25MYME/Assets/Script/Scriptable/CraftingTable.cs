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

    public void AddCost(ResourceType type, int amount)
    {
        costs.Add(new ResourceCost(type, amount));
    }

    public bool HasCost(ResourceType type)
    {
        return costs.Exists(c => c.resourceType == type);
    }

    public int GetCost(ResourceType type)
    {
        var cost = costs.Find(c => c.resourceType == type);
        return cost?.amount ?? 0;
    }
}

[System.Serializable]
public class CraftingRecipe
{
    [Header("제작 결과")]
    public ResourceType outputResource;
    public int outputAmount = 1;

    [Header("필요 재료")]
    public List<ResourceCost> inputResources = new List<ResourceCost>();

    [Header("제작 설정")]
    public float craftingTime = 5f;
    public int craftingCost = 0; // 추가 비용 (골드 등)
    public ResourceType craftingCostType = ResourceType.Gold;

    [Header("비주얼 (선택사항)")]
    public Sprite craftingIcon;
    public string craftingDescription;
}


[CreateAssetMenu(fileName = "CraftingTable", menuName = "Game/CraftingTable")]
public class CraftingTable : ScriptableObject
{
    [Header("제작법 목록")]
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