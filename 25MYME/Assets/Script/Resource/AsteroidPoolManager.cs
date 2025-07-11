using System.Collections.Generic;
using UnityEngine;

public class AsteroidPoolManager : MonoBehaviour
{
    [Header("운석 풀 설정")]
    public List<AsteroidData> asteroidDataList = new List<AsteroidData>();
    public GameObject asteroidPrefab; // 기본 운석 프리팹
    public Transform asteroidParent; // 운석들의 부모 오브젝트

    [Header("생성 설정")]
    public int maxAsteroids = 20; // 최대 운석 수
    public float spawnRadius = 50f; // 생성 반경
    public Vector2 spawnInterval = new Vector2(3f, 8f); // 생성 간격 (최소, 최대)

    private List<GameObject> activeAsteroids = new List<GameObject>();
    private Queue<GameObject> asteroidPool = new Queue<GameObject>();
    private float nextSpawnTime = 0f;

    void Start()
    {
        InitializePool();
        SetNextSpawnTime();
    }

    void Update()
    {
        if (Time.time >= nextSpawnTime && activeAsteroids.Count < maxAsteroids)
        {
            SpawnRandomAsteroid();
            SetNextSpawnTime();
        }

        CleanupDestroyedAsteroids();
    }

    void InitializePool()
    {
        // 풀 초기화 - 미리 운석들을 생성해놓음
        for (int i = 0; i < maxAsteroids * 2; i++)
        {
            GameObject asteroid = Instantiate(asteroidPrefab, asteroidParent);
            asteroid.SetActive(false);
            asteroidPool.Enqueue(asteroid);
        }
    }

    void SpawnRandomAsteroid()
    {
        // 플레이어 레벨에 맞는 운석 데이터 필터링
        var resourceManager = FindAnyObjectByType<ResourceManager>();
        if (resourceManager == null) return;

        var availableAsteroids = GetAvailableAsteroids(resourceManager.GetPlayerLevel());
        if (availableAsteroids.Count == 0) return;

        // 가중치 기반 랜덤 선택
        var selectedData = GetWeightedRandomAsteroid(availableAsteroids);

        // 풀에서 운석 가져오기
        GameObject asteroidObj = GetPooledAsteroid();
        if (asteroidObj == null) return;

        // 운석 설정
        var asteroid = asteroidObj.GetComponent<Asteroid>();
        asteroid.Initialize(selectedData);

        // 랜덤 위치에 생성
        Vector3 spawnPosition = GetRandomSpawnPosition();
        asteroidObj.transform.position = spawnPosition;
        asteroidObj.SetActive(true);

        activeAsteroids.Add(asteroidObj);
    }

    List<AsteroidData> GetAvailableAsteroids(int playerLevel)
    {
        return asteroidDataList.FindAll(data => data.requiredLevel <= playerLevel);
    }

    AsteroidData GetWeightedRandomAsteroid(List<AsteroidData> availableAsteroids)
    {
        float totalWeight = 0f;
        foreach (var data in availableAsteroids)
        {
            totalWeight += data.spawnWeight;
        }

        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        foreach (var data in availableAsteroids)
        {
            currentWeight += data.spawnWeight;
            if (randomValue <= currentWeight)
                return data;
        }

        return availableAsteroids[0]; // 기본값
    }

    GameObject GetPooledAsteroid()
    {
        if (asteroidPool.Count > 0)
        {
            return asteroidPool.Dequeue();
        }

        // 풀이 비어있으면 새로 생성
        return Instantiate(asteroidPrefab, asteroidParent);
    }

    Vector3 GetRandomSpawnPosition()
    {
        // 원형 영역에서 랜덤 위치 생성
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        return new Vector3(randomCircle.x, 0.5f, randomCircle.y);
    }

    void SetNextSpawnTime()
    {
        float interval = Random.Range(spawnInterval.x, spawnInterval.y);
        nextSpawnTime = Time.time + interval;
    }

    void CleanupDestroyedAsteroids()
    {
        for (int i = activeAsteroids.Count - 1; i >= 0; i--)
        {
            if (activeAsteroids[i] == null || !activeAsteroids[i].activeInHierarchy)
            {
                activeAsteroids.RemoveAt(i);
            }
        }
    }

    public void ReturnAsteroidToPool(GameObject asteroid)
    {
        asteroid.SetActive(false);
        asteroidPool.Enqueue(asteroid);
        activeAsteroids.Remove(asteroid);
    }
}
