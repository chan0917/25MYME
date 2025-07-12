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

    [Header("구간별 스폰 설정")]
    [Range(0f, 100f)]
    public float safeZone = 10f; // 0~10%: 스폰 안함 (위험지대)
    [Range(0f, 1f)]
    public float legendaryZoneWeight = 0.1f; // 중심부 전설 구간 가중치
    [Range(0f, 1f)]
    public float rareZoneWeight = 0.5f; // 중간 희귀 구간 가중치  
    [Range(0f, 1f)]
    public float commonZoneWeight = 1f; // 외곽 일반 구간 가중치

    [Header("스폰 타이밍")]
    public Vector2 spawnInterval = new Vector2(3f, 8f); // 생성 간격 (최소, 최대)
    public int initalSpawn = 30;

    private List<GameObject> activeAsteroids = new List<GameObject>();
    private Queue<GameObject> asteroidPool = new Queue<GameObject>();
    private float nextSpawnTime = 0f;

    void Start()
    {
        InitializePool();
        SetNextSpawnTime();

        for (int i = 0; i < initalSpawn; i++)
        {
            SpawnRandomAsteroid();
        }
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

        // 1단계: 랜덤 위치 결정
        Vector3 spawnPosition = GetRandomSpawnPosition();
        float distanceFromCenter = Vector2.Distance(Vector2.zero, new Vector2(spawnPosition.x, spawnPosition.z));
        float distancePercent = (distanceFromCenter / spawnRadius) * 100f;

        if (distancePercent <= safeZone)
        {
            Debug.Log($"중심지대 ({distancePercent:F1}% ≤ {safeZone}%): 스폰 안함");
            return;
        }


        // 2단계: 해당 구간에서 스폰 가능한 운석들 필터링
        var availableAsteroids = GetAsteroidsForZone(distancePercent, resourceManager.GetPlayerLevel());
        if (availableAsteroids.Count == 0)
        {
            // 스폰 가능한 운석이 없으면 로그 출력 후 패스
            Debug.Log($"구간 {distancePercent:F1}%에서 스폰 가능한 운석이 없음");
            return;
        }

        // 3단계: 가중치 기반 운석 선택
        var selectedData = GetWeightedRandomAsteroid(availableAsteroids);

        // 4단계: 운석 생성 및 설정
        GameObject asteroidObj = GetPooledAsteroid();
        if (asteroidObj == null) return;

        var asteroid = asteroidObj.GetComponent<Asteroid>();
        asteroid.Initialize(selectedData);

        // 5단계: 물리 설정 (질량 적용)
        var rigidbody = asteroidObj.GetComponent<Rigidbody>();
        if (rigidbody != null)
        {
            rigidbody.mass = selectedData.mass;
            rigidbody.linearDamping = selectedData.drag;
        }

        //6단계: 스프라이트 적용
        if (selectedData.asteroidSprite)
        {
            var renderer = asteroid.GetComponent<SpriteRenderer>();
            renderer.sprite = selectedData.asteroidSprite;
        }

        asteroidObj.transform.position = spawnPosition;
        asteroidObj.SetActive(true);

        activeAsteroids.Add(asteroidObj);

        //Debug.Log($"운석 스폰: {selectedData.asteroidName} (거리: {distancePercent:F1}%, 질량: {selectedData.mass})");
    }

    List<AsteroidData> GetAsteroidsForZone(float distancePercent, int playerLevel)
    {
        var availableAsteroids = new List<AsteroidData>();

        foreach (var asteroidData in asteroidDataList)
        {
            if (asteroidData == null) continue;


            // 구간 체크
            if (asteroidData.CanSpawnAtDistance(distancePercent))
            {
                availableAsteroids.Add(asteroidData);
            }
        }

        return availableAsteroids;
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

        if (totalWeight <= 0f) return availableAsteroids[0];

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

    public GameObject GetPooledAsteroid()
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

#if UNITY_EDITOR
    public void AutoLoadAsteroidData()
    {
        asteroidDataList.Clear();

        // GameData/Asteroid 폴더에서 모든 AsteroidData 찾기
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:AsteroidData", new[] { "Assets/GameData/Asteroid" });

        foreach (string guid in guids)
        {
            string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            AsteroidData asteroidData = UnityEditor.AssetDatabase.LoadAssetAtPath<AsteroidData>(assetPath);

            if (asteroidData != null)
            {
                asteroidDataList.Add(asteroidData);
            }
        }

        // 레벨 순으로 정렬
        asteroidDataList.Sort((a, b) => a.requiredLevel.CompareTo(b.requiredLevel));

        UnityEditor.EditorUtility.SetDirty(this);

        Debug.Log($"자동 로드 완료: {asteroidDataList.Count}개의 운석 데이터를 발견했습니다.");
    }

    public void CreateNewAsteroid()
    {
        // 새 AsteroidData 생성
        var newAsteroid = ScriptableObject.CreateInstance<AsteroidData>();

        // 기본값 설정
        newAsteroid.asteroidName = "새로운 운석";
        newAsteroid.asteroidType = AsteroidType.BasicAsteroid;
        newAsteroid.description = "새로 생성된 운석입니다.";
        newAsteroid.requiredLevel = 1;
        newAsteroid.miningTime = 3f;
        newAsteroid.energyCost = 10;
        newAsteroid.spawnWeight = 1f;
        newAsteroid.asteroidColor = Color.white;
        newAsteroid.mass = 20f;
        newAsteroid.drag = 10f;

        // 기본 드랍 설정 (강화석)
        var defaultDrop = new ResourceDrop
        {
            resourceType = ResourceType.UpgradeStone,
            dropChance = 0.8f,
            minAmount = 1,
            maxAmount = 2
        };
        newAsteroid.resourceDrops.Add(defaultDrop);

        // 폴더 확인 및 생성
        string folderPath = "Assets/GameData/Asteroid";
        if (!UnityEditor.AssetDatabase.IsValidFolder(folderPath))
        {
            if (!UnityEditor.AssetDatabase.IsValidFolder("Assets/GameData"))
            {
                UnityEditor.AssetDatabase.CreateFolder("Assets", "GameData");
            }
            UnityEditor.AssetDatabase.CreateFolder("Assets/GameData", "Asteroid");
        }

        // 고유한 파일명 생성
        string fileName = "NewAsteroid";
        string assetPath = $"{folderPath}/{fileName}.asset";
        int counter = 1;

        while (UnityEditor.AssetDatabase.LoadAssetAtPath(assetPath, typeof(AsteroidData)) != null)
        {
            assetPath = $"{folderPath}/{fileName}_{counter}.asset";
            counter++;
        }

        // 에셋 생성 및 저장
        UnityEditor.AssetDatabase.CreateAsset(newAsteroid, assetPath);
        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.AssetDatabase.Refresh();

        // 즉시 선택하여 Inspector에서 수정 가능하게 함
        UnityEditor.Selection.activeObject = newAsteroid;
        UnityEditor.EditorGUIUtility.PingObject(newAsteroid);

        Debug.Log($"새 운석 생성 완료: {assetPath}");

        // 자동으로 리스트에 추가
        AutoLoadAsteroidData();
    }
#endif
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(AsteroidPoolManager))]
public class AsteroidPoolManagerEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(20);

        AsteroidPoolManager manager = (AsteroidPoolManager)target;

        GUILayout.BeginVertical("box");
        GUILayout.Label("🎯 스폰 구간 시각화 (중심 → 외곽)", UnityEditor.EditorStyles.boldLabel);

        GUILayout.Label($"0% ~ {manager.safeZone}%: ❌ 스폰 안함 (위험지대)");
        GUILayout.Label($"{manager.safeZone}% ~ 30%: 🔴 전설 운석 구간 (중심부 - 매우 희귀, 무거움)");
        GUILayout.Label($"30% ~ 70%: 🟡 희귀 운석 구간 (중간부)");
        GUILayout.Label($"70% ~ 100%: 🟢 일반 운석 구간 (외곽부 - 흔함, 가벼움)");

        GUILayout.EndVertical();

        GUILayout.Space(10);

        GUILayout.BeginVertical("box");
        GUILayout.Label("운석 데이터 관리", UnityEditor.EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("GameData/Asteroid\n자동 로드", GUILayout.Height(50)))
        {
            manager.AutoLoadAsteroidData();
        }

        if (GUILayout.Button("새 운석 생성", GUILayout.Height(50)))
        {
            manager.CreateNewAsteroid();
        }

        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        // 현재 상태 표시
        GUILayout.Label("현재 상태:", UnityEditor.EditorStyles.boldLabel);

        if (manager.asteroidDataList != null && manager.asteroidDataList.Count > 0)
        {
            GUILayout.Label($"✅ 로드된 운석: {manager.asteroidDataList.Count}개",
                           new GUIStyle(GUI.skin.label) { normal = { textColor = Color.green } });

            // 운석 목록 표시
            GUILayout.Space(5);
            GUILayout.Label("운석 목록:", UnityEditor.EditorStyles.boldLabel);

            for (int i = 0; i < manager.asteroidDataList.Count && i < 10; i++) // 최대 10개까지만 표시
            {
                var asteroid = manager.asteroidDataList[i];
                if (asteroid != null)
                {
                    GUILayout.BeginHorizontal();

                    // 레벨 표시
                    GUILayout.Label($"Lv.{asteroid.requiredLevel}", GUILayout.Width(40));

                    // 이름과 타입
                    GUILayout.Label($"{asteroid.asteroidName} ({asteroid.asteroidType})");

                    // 가중치
                    GUILayout.Label($"가중치: {asteroid.spawnWeight:F1}", GUILayout.Width(80));

                    GUILayout.Label($"질량: {asteroid.mass:F1}", GUILayout.Width(80));

                    GUILayout.EndHorizontal();
                }
            }

            if (manager.asteroidDataList.Count > 10)
            {
                GUILayout.Label($"... 외 {manager.asteroidDataList.Count - 10}개 더");
            }
        }
        else
        {
            GUILayout.Label("⚠️ 로드된 운석이 없습니다.",
                           new GUIStyle(GUI.skin.label) { normal = { textColor = Color.yellow } });
            GUILayout.Label("'GameData/Asteroid 자동 로드' 버튼을 클릭하세요.");
        }

        GUILayout.EndVertical();

        // 폴더 정보
        GUILayout.Space(10);
        GUILayout.BeginVertical("box");
        GUILayout.Label("📁 폴더 정보", UnityEditor.EditorStyles.boldLabel);
        GUILayout.Label("운석 데이터 위치: Assets/GameData/Asteroid/");
        GUILayout.Label("새 운석은 자동으로 위 폴더에 생성됩니다.");
        GUILayout.EndVertical();
    }
}
#endif