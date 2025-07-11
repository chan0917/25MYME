using UnityEngine;

public class Asteroid : MonoBehaviour
{
    [Header("� ����")]
    public AsteroidData asteroidData;

    [Header("���� ����")]
    public bool isCollected = false;

    private SpriteRenderer spriteRenderer;
    private AsteroidPoolManager poolManager;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        poolManager = FindAnyObjectByType<AsteroidPoolManager>();
    }

    public void Initialize(AsteroidData data)
    {
        asteroidData = data;
        isCollected = false;
        UpdateVisual();
    }

    public bool TryCollect(ResourceManager resourceManager)
    {
        if (isCollected) return false;

        // ���� üũ
        if (resourceManager.GetPlayerLevel() < asteroidData.requiredLevel)
        {
            Debug.Log($"������ �����մϴ�! �ʿ� ����: {asteroidData.requiredLevel}, ���� ����: {resourceManager.GetPlayerLevel()}");
            return false;
        }

        // ������ üũ
        if (!resourceManager.HasResource(ResourceType.Gold, asteroidData.energyCost))
        {
            Debug.Log("�������� �����մϴ�!");
            return false;
        }

        // ������ �Ҹ�
        resourceManager.ConsumeResource(ResourceType.Gold, asteroidData.energyCost);

        // �ڿ� ���
        var droppedResources = asteroidData.GetDroppedResources();
        foreach (var drop in droppedResources)
        {
            resourceManager.AddResource(drop.resourceType, drop.minAmount);
        }

        // ���� �Ϸ� ó��
        isCollected = true;
        Debug.Log($"{asteroidData.asteroidName} ���� �Ϸ�!");

        // ���� ����Ʈ �Ǵ� �ִϸ��̼� ���� �� Ǯ�� ��ȯ
        StartCoroutine(CollectAndReturn());

        return true;
    }

    System.Collections.IEnumerator CollectAndReturn()
    {
        // ���� �ִϸ��̼�/����Ʈ (0.5��)
        float animTime = 0.5f;
        Vector3 startScale = transform.localScale;

        while (animTime > 0)
        {
            animTime -= Time.deltaTime;
            float progress = 1f - (animTime / 0.5f);
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, progress);
            yield return null;
        }

        // Ǯ�� ��ȯ
        if (poolManager != null)
            poolManager.ReturnAsteroidToPool(gameObject);
        else
            gameObject.SetActive(false);
    }

    public void ReturnAsteroid()
    {
        if (poolManager != null)
            poolManager.ReturnAsteroidToPool(gameObject);
    }

    void UpdateVisual()
    {
        if (spriteRenderer && asteroidData)
        {
            spriteRenderer.sprite = asteroidData.asteroidSprite;
            spriteRenderer.color = asteroidData.asteroidColor;
            transform.localScale = Vector3.one; // ������ �ʱ�ȭ
        }
    }

    // ���콺 Ŭ������ ���� (�׽�Ʈ��)
    void OnMouseDown()
    {
        var resourceManager = FindAnyObjectByType<ResourceManager>();
        if (resourceManager != null)
        {
            TryCollect(resourceManager);
        }
    }
}
