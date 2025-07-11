using System.Collections.Generic;
using UnityEngine;

public class AsteroidPoolManager : MonoBehaviour
{
    [Header("� Ǯ ����")]
    public List<AsteroidData> asteroidDataList = new List<AsteroidData>();
    public GameObject asteroidPrefab; // �⺻ � ������
    public Transform asteroidParent; // ����� �θ� ������Ʈ

    [Header("���� ����")]
    public int maxAsteroids = 20; // �ִ� � ��
    public float spawnRadius = 50f; // ���� �ݰ�
    public Vector2 spawnInterval = new Vector2(3f, 8f); // ���� ���� (�ּ�, �ִ�)

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
        // Ǯ �ʱ�ȭ - �̸� ����� �����س���
        for (int i = 0; i < maxAsteroids * 2; i++)
        {
            GameObject asteroid = Instantiate(asteroidPrefab, asteroidParent);
            asteroid.SetActive(false);
            asteroidPool.Enqueue(asteroid);
        }
    }

    void SpawnRandomAsteroid()
    {
        // �÷��̾� ������ �´� � ������ ���͸�
        var resourceManager = FindAnyObjectByType<ResourceManager>();
        if (resourceManager == null) return;

        var availableAsteroids = GetAvailableAsteroids(resourceManager.GetPlayerLevel());
        if (availableAsteroids.Count == 0) return;

        // ����ġ ��� ���� ����
        var selectedData = GetWeightedRandomAsteroid(availableAsteroids);

        // Ǯ���� � ��������
        GameObject asteroidObj = GetPooledAsteroid();
        if (asteroidObj == null) return;

        // � ����
        var asteroid = asteroidObj.GetComponent<Asteroid>();
        asteroid.Initialize(selectedData);

        // ���� ��ġ�� ����
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

        return availableAsteroids[0]; // �⺻��
    }

    GameObject GetPooledAsteroid()
    {
        if (asteroidPool.Count > 0)
        {
            return asteroidPool.Dequeue();
        }

        // Ǯ�� ��������� ���� ����
        return Instantiate(asteroidPrefab, asteroidParent);
    }

    Vector3 GetRandomSpawnPosition()
    {
        // ���� �������� ���� ��ġ ����
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
