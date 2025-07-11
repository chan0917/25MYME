using UnityEngine;

public class Asteroid : MonoBehaviour
{
    [Header("운석 설정")]
    public AsteroidData asteroidData;

    [Header("현재 상태")]
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

        // 레벨 체크
        if (resourceManager.GetPlayerLevel() < asteroidData.requiredLevel)
        {
            Debug.Log($"레벨이 부족합니다! 필요 레벨: {asteroidData.requiredLevel}, 현재 레벨: {resourceManager.GetPlayerLevel()}");
            return false;
        }

        // 에너지 체크
        if (!resourceManager.HasResource(ResourceType.Gold, asteroidData.energyCost))
        {
            Debug.Log("에너지가 부족합니다!");
            return false;
        }

        // 에너지 소모
        resourceManager.ConsumeResource(ResourceType.Gold, asteroidData.energyCost);

        // 자원 드랍
        var droppedResources = asteroidData.GetDroppedResources();
        foreach (var drop in droppedResources)
        {
            resourceManager.AddResource(drop.resourceType, drop.minAmount);
        }

        // 수집 완료 처리
        isCollected = true;
        Debug.Log($"{asteroidData.asteroidName} 수집 완료!");

        // 수집 이펙트 또는 애니메이션 실행 후 풀로 반환
        StartCoroutine(CollectAndReturn());

        return true;
    }

    System.Collections.IEnumerator CollectAndReturn()
    {
        // 수집 애니메이션/이펙트 (0.5초)
        float animTime = 0.5f;
        Vector3 startScale = transform.localScale;

        while (animTime > 0)
        {
            animTime -= Time.deltaTime;
            float progress = 1f - (animTime / 0.5f);
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, progress);
            yield return null;
        }

        // 풀로 반환
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
            transform.localScale = Vector3.one; // 스케일 초기화
        }
    }

    // 마우스 클릭으로 수집 (테스트용)
    void OnMouseDown()
    {
        var resourceManager = FindAnyObjectByType<ResourceManager>();
        if (resourceManager != null)
        {
            TryCollect(resourceManager);
        }
    }
}
