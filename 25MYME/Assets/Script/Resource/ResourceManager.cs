using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    [Header("����")]
    public ResourceDatabase resourceDatabase; // �߾� �����ͺ��̽� ����

    [Header("���� ���� �ڿ�")]
    public Dictionary<ResourceType, int> playerResources = new Dictionary<ResourceType, int>();

    [Header("�÷��̾� ����")]
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

        playerResources[ResourceType.Gold] = 1000; // ���� �ڿ�
    }

    #region �ڿ� ���� �޼���
    public void AddResource(ResourceType type, int amount)
    {
        if (playerResources.ContainsKey(type))
            playerResources[type] += amount;
        else
            playerResources[type] = amount;

        string resourceName = resourceDatabase.GetResourceName(type);
        Debug.Log($"{resourceName} {amount}�� ȹ��! �� ������: {playerResources[type]}");
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
            Debug.Log($"{resourceName} {amount}�� �Ҹ�! ���� ������: {playerResources[type]}");
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
}
