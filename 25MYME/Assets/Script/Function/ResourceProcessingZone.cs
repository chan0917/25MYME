using UnityEngine;

#region 단순한 자원 처리 구역
public class ResourceProcessingZone : MonoBehaviour
{
    [Header("참조")]
    public GrappleArmSystem grappleSystem;   // 집게팔 시스템 참조
    public ResourceManager resourceManager;  // 자원 매니저 참조

    [Header("설정")]
    public Vector3 zoneSize = new Vector3(8f, 3f, 6f); // 처리 구역 크기
    public float processDelay = 0.5f;        // 처리 지연 시간

    [Header("현재 상태")]
    public bool isSpaceshipInside = false;
    public bool isProcessing = false;
    public int totalProcessed = 0;

    [Header("비주얼 & 사운드")]
    public ParticleSystem processEffect;     // 처리 이펙트
    public AudioSource processSound;         // 처리 사운드

    [Header("UI 표시")]
    public UpgradeUI UpgradeUI;
    public TextMesh statusText;              // 상태 텍스트 (선택사항)

    void Start()
    {
        if (grappleSystem == null)
            grappleSystem = FindAnyObjectByType<GrappleArmSystem>();

        if (resourceManager == null)
            resourceManager = FindAnyObjectByType<ResourceManager>();

        Debug.Log("자원 처리 구역 초기화 완료");
    }

    void Update()
    {
        CheckForSpaceship();
        UpdateUI();
    }

    void CheckForSpaceship()
    {
        bool spaceshipDetected = IsSpaceshipInside();

        if (spaceshipDetected && !isSpaceshipInside)
        {
            UpgradeUI.OpenUI();
            // 우주선 진입
            isSpaceshipInside = true;
            grappleSystem.BlockGrap = true;
            Debug.Log("우주선이 자원 처리 구역에 진입했습니다!");

            // 운석을 들고 있다면 즉시 처리
            if (grappleSystem != null && grappleSystem.HasCarriedAsteroid() && !isProcessing)
            {
                StartCoroutine(ProcessResource());
            }
        }
        else if (!spaceshipDetected && isSpaceshipInside)
        {
            UpgradeUI.CloseUI();
            // 우주선 나감
            isSpaceshipInside = false;
            grappleSystem.BlockGrap = false;
            Debug.Log("우주선이 자원 처리 구역을 떠났습니다!");
        }
    }

    bool IsSpaceshipInside()
    {
        if (grappleSystem == null) return false;

        Vector3 spaceshipPos = grappleSystem.transform.position;
        Vector3 localPos = transform.InverseTransformPoint(spaceshipPos);
        Vector3 halfSize = zoneSize * 0.5f;

        return Mathf.Abs(localPos.x) <= halfSize.x &&
               Mathf.Abs(localPos.y) <= halfSize.y &&
               Mathf.Abs(localPos.z) <= halfSize.z;
    }

    System.Collections.IEnumerator ProcessResource()
    {
        if (!grappleSystem.HasCarriedAsteroid() || isProcessing) yield break;

        isProcessing = true;
        Debug.Log("자원 처리 시작!");

        // 처리 지연
        yield return new WaitForSeconds(processDelay);

        // 운석 데이터 가져오기
        var carriedAsteroid = grappleSystem.carriedAsteroid;

        if (carriedAsteroid?.asteroidData != null && resourceManager != null)
        {
            Debug.Log($"처리 중: {carriedAsteroid.asteroidData.asteroidName}");

            // 1. 플레이어 인벤토리에 자원 추가
            var droppedResources = carriedAsteroid.asteroidData.GetDroppedResources();
            foreach (var drop in droppedResources)
            {
                resourceManager.AddResource(drop.resourceType, drop.minAmount);
                Debug.Log($"자원 획득: {drop.resourceType} x{drop.minAmount}");
            }

            // 2. 집게팔에서 운석 제거 (pool에 자동 반환)
            grappleSystem.UnloadAsteroid();

            // 3. 처리 완료
            totalProcessed++;
            Debug.Log($"자원 처리 완료! (총 {totalProcessed}회)");
            UpgradeUI.OnUpdateResource();

            // 이펙트 재생
            // if (processEffect != null) processEffect.Play();
            // if (processSound != null) processSound.Play();
        }
        else
        {
            Debug.Log("자원 처리 실패: 데이터가 없습니다.");
        }

        isProcessing = false;
    }

    void UpdateUI()
    {
        if (statusText != null)
        {
            if (isSpaceshipInside)
            {
                if (grappleSystem != null && grappleSystem.HasCarriedAsteroid())
                {
                    if (isProcessing)
                    {
                        statusText.text = "처리 중...";
                        statusText.color = Color.yellow;
                    }
                    else
                    {
                        statusText.text = "자원 처리 완료";
                        statusText.color = Color.green;
                    }
                }
                else
                {
                    statusText.text = "운석 없음";
                    statusText.color = Color.white;
                }
            }
            else
            {
                statusText.text = $"자원 처리 구역\n처리: {totalProcessed}회";
                statusText.color = Color.gray;
            }
        }
    }

    #region 디버그 기즈모
    void OnDrawGizmos()
    {
        // 처리 구역 박스 표시
        Gizmos.color = isSpaceshipInside ? Color.green : Color.cyan;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, zoneSize);

        // 중심 표시
        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.identity;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
    #endregion
}
#endregion

#region Custom Editor
#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(ResourceProcessingZone))]
public class ResourceProcessingZoneEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ResourceProcessingZone zone = (ResourceProcessingZone)target;

        if (Application.isPlaying)
        {
            GUILayout.Space(20);
            GUILayout.BeginVertical("box");
            GUILayout.Label("🏭 자원 처리 구역 상태", UnityEditor.EditorStyles.boldLabel);

            // 진입 상태
            if (zone.isSpaceshipInside)
            {
                GUILayout.Label("🚀 우주선 진입 중",
                               new GUIStyle(GUI.skin.label) { normal = { textColor = Color.green } });
            }
            else
            {
                GUILayout.Label("⭕ 우주선 없음",
                               new GUIStyle(GUI.skin.label) { normal = { textColor = Color.gray } });
            }

            // 처리 상태
            if (zone.isProcessing)
            {
                GUILayout.Label("🔄 자원 처리 중...",
                               new GUIStyle(GUI.skin.label) { normal = { textColor = Color.yellow } });
            }

            // 통계
            GUILayout.Space(5);
            GUILayout.Label($"총 처리: {zone.totalProcessed}회");

            GUILayout.EndVertical();
        }
        else
        {
            GUILayout.Space(20);
            GUILayout.BeginVertical("box");
            GUILayout.Label("ℹ️ 정보", UnityEditor.EditorStyles.boldLabel);
            GUILayout.Label($"구역 크기: {zone.zoneSize}");
            GUILayout.Label("우주선이 들어오면 자동으로 운석을 처리합니다.");
            GUILayout.EndVertical();
        }
    }
}
#endif
#endregion