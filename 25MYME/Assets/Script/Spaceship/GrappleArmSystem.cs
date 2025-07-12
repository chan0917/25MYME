using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region 집게팔 상태 및 이벤트
public enum GrappleState
{
    Ready,          // 준비 상태 (발사 가능)
    Launching,      // 발사 중
    Extending,      // 목표 위치로 이동 중
    AtTarget,       // 목표 위치 도달
    Grabbing,       // 그랩 시도 중
    GrabSuccess,    // 그랩 성공
    GrabFailed,     // 그랩 실패
    Retracting,     // 돌아오는 중 (빈 손)
    RetractingWithCargo, // 돌아오는 중 (운석 포함)
    Cooldown        // 쿨타임 대기
}

[System.Serializable]
public class GrappleSettings
{
    [Header("기본 설정")]
    public float maxRange = 15f;           // 최대 사거리
    public float baseSpeed = 10f;          // 기본 속도 (플레이어 스탯으로 덮어씀)
    public float grabRadius = 2f;          // 그랩 가능 반경
    public float additionalCooldown = 0.5f; // 추가 쿨타임 (초)

    [Header("무게 시스템")]
    public float maxCarryWeight = 10f;     // 최대 운반 가능 무게
    public float weightSpeedMultiplier = 0.5f; // 무게에 따른 속도 감소 배수

    [Header("물리 설정")]
    public LayerMask asteroidLayer = -1;   // 운석 레이어
    public LayerMask obstacleLayer = -1;   // 장애물 레이어
}

[System.Serializable]
public class GrappleEvents
{
    [Header("이벤트 콜백")]
    public UnityEngine.Events.UnityEvent OnLaunch;           // 발사 시
    public UnityEngine.Events.UnityEvent OnTargetReached;    // 목표 도달 시
    public UnityEngine.Events.UnityEvent OnGrabSuccess;      // 그랩 성공 시
    public UnityEngine.Events.UnityEvent OnGrabFailed;       // 그랩 실패 시
    public UnityEngine.Events.UnityEvent OnReturnStart;      // 돌아오기 시작
    public UnityEngine.Events.UnityEvent OnReturnComplete;   // 돌아오기 완료
    public UnityEngine.Events.UnityEvent OnReady;            // 준비 완료 시
}
#endregion

#region 운석 무게 데이터 (확장용)
[System.Serializable]
public class CarriedAsteroid
{
    public AsteroidData asteroidData;
    public float weight;
    public float speedReduction; // 이속 감소율 (0~1)

    public CarriedAsteroid(AsteroidData data)
    {
        asteroidData = data;
        weight = data.mass / 10;

        // 질량에 비례한 속도 감소 계산 또는 운석 데이터의 고정값 사용
        // 여기서는 질량 비례 방식 사용
        speedReduction = Mathf.Clamp01(weight / 10f); // 10kg = 100% 감소
    }
}
#endregion

#region 집게팔 메인 시스템
public class GrappleArmSystem : MonoBehaviour
{
    [Header("참조")]
    public Transform grappleArmTransform;      // 집게팔 오브젝트
    public Transform grappleOrigin;            // 발사 시작점
    public SpaceshipLevelManager levelManager; // 스탯 참조용
    public Camera playerCamera;                // 마우스 위치 계산용
    public AsteroidPoolManager asteroidPoolManager;

    [Header("설정")]
    public GrappleSettings settings;
    public GrappleEvents events;

    [Header("현재 상태")]
    public GrappleState currentState = GrappleState.Ready;
    public float currentCooldown = 0f;
    public CarriedAsteroid carriedAsteroid = null; // 단일 운석만 운반

    [Header("비주얼 & 사운드 (FX 공간)")]
    public ParticleSystem launchEffect;        // 발사 이펙트
    public ParticleSystem grabSuccessEffect;   // 그랩 성공 이펙트
    public ParticleSystem grabFailEffect;      // 그랩 실패 이펙트
    public ParticleSystem returnEffect;        // 돌아오기 이펙트
    public ParticleSystem dropEffect;          // 운석 놓기 이펙트

    public AudioSource launchSound;            // 발사 사운드
    public AudioSource grabSuccessSound;       // 그랩 성공 사운드
    public AudioSource grabFailSound;          // 그랩 실패 사운드
    public AudioSource returnSound;            // 돌아오기 사운드
    public AudioSource dropSound;              // 운석 놓기 사운드

    [Header("운석 스프라이트 표시")]
    public SpriteRenderer carriedAsteroidSprite; // 잡은 운석 스프라이트 표시용
    public Transform spriteAttachPoint;          // 스프라이트 부착 위치 (집게팔 끝)

    // 내부 변수
    private Vector3 targetPosition;
    private Vector3 startPosition;
    private Vector3 dynamicReturnPosition; // 동적 반환 위치 (우주선 현재 위치)
    private float currentSpeed;
    private float totalCooldownTime;
    private Coroutine grappleCoroutine;
    private List<Asteroid> nearbyAsteroids = new List<Asteroid>();

    void Start()
    {
        InitializeGrapple();
    }

    void Update()
    {
        HandleInput();
        UpdateCooldown();
        UpdateGrappleVisual();
    }

    void InitializeGrapple()
    {
        if (levelManager == null)
            levelManager = FindAnyObjectByType<SpaceshipLevelManager>();

        if (asteroidPoolManager == null)
            asteroidPoolManager = FindAnyObjectByType<AsteroidPoolManager>();   

        if (playerCamera == null)
            playerCamera = Camera.main;

        if (grappleOrigin == null)
            grappleOrigin = transform;

        currentState = GrappleState.Ready;
        UpdateSpeed();
    }

    void HandleInput()
    {
        if (Input.GetMouseButtonDown(0) && CanLaunch())
        {
            Vector3 mouseWorldPos = GetMouseWorldPosition();
            if (mouseWorldPos != Vector3.zero)
            {
                // 운석을 들고 있다면 제자리에 놓고 새 위치로 발사
                if (HasCarriedAsteroid())
                {
                    DropAsteroidAtCurrentPosition();
                }

                LaunchGrapple(mouseWorldPos);
            }
        }
    }

    void UpdateCooldown()
    {
        if (currentState == GrappleState.Cooldown)
        {
            currentCooldown -= Time.deltaTime;
            if (currentCooldown <= 0f)
            {
                SetState(GrappleState.Ready);
            }
        }
    }

    Vector3 GetMouseWorldPosition()
    {
        if (playerCamera == null) return Vector3.zero;

        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);

        // XZ 평면과의 교차점 계산 (Y = 0.5f 고정)
        float targetY = 0.5f;
        if (Mathf.Abs(ray.direction.y) > 0.001f)
        {
            float distance = (targetY - ray.origin.y) / ray.direction.y;
            if (distance > 0)
            {
                Vector3 hitPoint = ray.origin + ray.direction * distance;
                return hitPoint;
            }
        }

        return Vector3.zero;
    }

    public bool CanLaunch()
    {
        return currentState == GrappleState.Ready;
    }

    public bool HasCarriedAsteroid()
    {
        if (carriedAsteroid != null)
            return carriedAsteroid.asteroidData != null;
        else
            return carriedAsteroid != null;
    }

    public bool IsOverweight()
    {
        return HasCarriedAsteroid() && carriedAsteroid.weight >= settings.maxCarryWeight;
    }

    public float GetTotalCarriedWeight()
    {
        return HasCarriedAsteroid() ? carriedAsteroid.weight : 0f;
    }

    public float GetSpeedReduction()
    {
        return HasCarriedAsteroid() ? carriedAsteroid.speedReduction : 0f;
    }

    void UpdateSpeed()
    {
        if (levelManager != null)
        {
            currentSpeed = levelManager.GetStatValue(SpaceshipStatType.Speed);
        }
        else
        {
            currentSpeed = settings.baseSpeed;
        }
    }

    public void LaunchGrapple(Vector3 targetPos)
    {
        // 사거리 체크
        float distance = Vector3.Distance(grappleOrigin.position, targetPos);
        if (distance > settings.maxRange)
        {
            // 최대 사거리로 제한
            Vector3 direction = (targetPos - grappleOrigin.position).normalized;
            targetPos = grappleOrigin.position + direction * settings.maxRange;
        }

        targetPosition = targetPos;
        startPosition = grappleOrigin.position;

        UpdateSpeed();

        // 총 쿨타임 계산 (가는 시간 + 오는 시간 + 추가 쿨타임)
        float goTime = distance / currentSpeed;
        float returnTime = distance / GetReturnSpeed();
        totalCooldownTime = goTime + returnTime + settings.additionalCooldown;

        SetState(GrappleState.Launching);

        if (grappleCoroutine != null)
            StopCoroutine(grappleCoroutine);

        grappleCoroutine = StartCoroutine(GrappleSequence());
    }

    float GetReturnSpeed()
    {
        // 돌아올 때는 무게에 따른 속도 감소 적용
        float speedReduction = GetSpeedReduction();
        return currentSpeed * (1f - speedReduction * settings.weightSpeedMultiplier);
    }

    IEnumerator GrappleSequence()
    {
        // 1단계: 발사
        yield return StartCoroutine(LaunchPhase());

        // 2단계: 목표 지점 도달
        SetState(GrappleState.AtTarget);
        OnTargetReached();

        // 3단계: 그랩 시도
        yield return StartCoroutine(GrabPhase());

        // 4단계: 돌아오기
        yield return StartCoroutine(ReturnPhase());

        // 5단계: 쿨타임
        SetState(GrappleState.Cooldown);
        currentCooldown = settings.additionalCooldown;
    }

    IEnumerator LaunchPhase()
    {
        SetState(GrappleState.Extending);
        OnLaunch();

        float journeyLength = Vector3.Distance(startPosition, targetPosition);
        float journeyTime = journeyLength / currentSpeed;
        float elapsedTime = 0f;

        while (elapsedTime < journeyTime)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / journeyTime;

            if (grappleArmTransform != null)
            {
                grappleArmTransform.position = Vector3.Lerp(startPosition, targetPosition, progress);
            }

            yield return null;
        }

        if (grappleArmTransform != null)
        {
            grappleArmTransform.position = targetPosition;
        }
    }

    IEnumerator GrabPhase()
    {
        SetState(GrappleState.Grabbing);
        Debug.Log("=== 그랩 단계 시작 ===");

        // 0.2초 대기 (그랩 애니메이션 시간)
        yield return new WaitForSeconds(0.2f);

        // 근처 운석 검색
        Debug.Log($"목표 위치: {targetPosition}, 그랩 반경: {settings.grabRadius}");
        FindNearbyAsteroids();
        Debug.Log($"근처 운석 검색 완료: {nearbyAsteroids.Count}개 발견");

        if (nearbyAsteroids.Count > 0)
        {
            // 발견된 운석들 로그
            for (int i = 0; i < nearbyAsteroids.Count; i++)
            {
                var asteroid = nearbyAsteroids[i];
                float distance = Vector3.Distance(asteroid.transform.position, targetPosition);
                Debug.Log($"  운석 {i + 1}: {asteroid.asteroidData?.asteroidName ?? "Unknown"} " +
                         $"(거리: {distance:F2}, 수집됨: {asteroid.isCollected})");
            }

            // 가장 가까운 운석 선택
            Asteroid closestAsteroid = GetClosestAsteroid();
            Debug.Log($"가장 가까운 운석 선택: {closestAsteroid.asteroidData?.asteroidName ?? "Unknown"}");

            if (CanCarryAsteroid(closestAsteroid))
            {
                Debug.Log("그랩 조건 통과: 운석 획득 가능");
                GrabAsteroid(closestAsteroid);
                SetState(GrappleState.GrabSuccess);
                OnGrabSuccess();
            }
            else
            {
                Debug.Log("그랩 실패: 운석을 운반할 수 없음");
                if (HasCarriedAsteroid())
                {
                    Debug.Log("  실패 원인: 이미 운석을 들고 있음");
                }
                else if (closestAsteroid.asteroidData.mass / 10 > settings.maxCarryWeight)
                {
                    Debug.Log($"  실패 원인: 운석이 너무 무거움 ({closestAsteroid.asteroidData.mass / 10} > {settings.maxCarryWeight})");
                }
                else
                {
                    Debug.Log("  실패 원인: 알 수 없는 이유");
                }
                SetState(GrappleState.GrabFailed);
                OnGrabFailed();
            }
        }
        else
        {
            Debug.Log("그랩 실패: 근처에 운석이 없음");
            SetState(GrappleState.GrabFailed);
            OnGrabFailed();
        }

        Debug.Log("=== 그랩 단계 완료 ===");
        yield return new WaitForSeconds(0.3f); // 결과 표시 시간
    }

    IEnumerator ReturnPhase()
    {
        bool hasCarried = currentState == GrappleState.GrabSuccess;
        SetState(hasCarried ? GrappleState.RetractingWithCargo : GrappleState.Retracting);
        OnReturnStart();

        // 동적 반환 위치 업데이트 (우주선의 현재 위치)
        dynamicReturnPosition = grappleOrigin.position;

        float returnSpeed = GetReturnSpeed();
        float journeyLength = Vector3.Distance(targetPosition, dynamicReturnPosition);
        float journeyTime = journeyLength / returnSpeed;
        float elapsedTime = 0f;

        Vector3 startReturnPos = targetPosition;

        while (elapsedTime < journeyTime)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / journeyTime;

            // 실시간으로 우주선 위치 업데이트
            dynamicReturnPosition = grappleOrigin.position;

            if (grappleArmTransform != null)
            {
                grappleArmTransform.position = Vector3.Lerp(startReturnPos, dynamicReturnPosition, progress);
            }

            // 운석 스프라이트도 함께 이동
            UpdateCarriedAsteroidSpritePosition();

            yield return null;
        }

        if (grappleArmTransform != null)
        {
            grappleArmTransform.position = dynamicReturnPosition;
        }

        UpdateCarriedAsteroidSpritePosition();
        OnReturnComplete();
    }

    void FindNearbyAsteroids()
    {
        nearbyAsteroids.Clear();

        Collider[] colliders = Physics.OverlapSphere(targetPosition, settings.grabRadius, settings.asteroidLayer);

        foreach (var collider in colliders)
        {
            Asteroid asteroid = collider.GetComponent<Asteroid>();
            if (asteroid != null && !asteroid.isCollected)
            {
                nearbyAsteroids.Add(asteroid);
            }
        }
    }

    Asteroid GetClosestAsteroid()
    {
        Asteroid closest = null;
        float closestDistance = float.MaxValue;

        foreach (var asteroid in nearbyAsteroids)
        {
            float distance = Vector3.Distance(asteroid.transform.position, targetPosition);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = asteroid;
            }
        }

        return closest;
    }

    bool CanCarryAsteroid(Asteroid asteroid)
    {
        if (asteroid == null || asteroid.asteroidData == null) return false;

        // 이미 운석을 들고 있다면 불가능
        if (HasCarriedAsteroid()) return false;

        // 무게 체크
        return asteroid.asteroidData.mass / 10 <= settings.maxCarryWeight;
    }

    void GrabAsteroid(Asteroid asteroid)
    {
        if (asteroid == null) return;

        // 기존 운석이 있다면 제거 (안전 체크)
        if (HasCarriedAsteroid())
        {
            UnloadAsteroid();
        }

        // 새 운석을 운반 목록에 추가
        carriedAsteroid = new CarriedAsteroid(asteroid.asteroidData);

        // ⚠️ 주의: 실제 자원 드랍은 하지 않음! 우주 정거장에서만 획득
        // var resourceManager = FindObjectOfType<ResourceManager>();
        // if (resourceManager != null)
        // {
        //     asteroid.TryCollect(resourceManager);
        // }

        // 운석 오브젝트는 비활성화 (수집된 것처럼 보이게)
        asteroid.isCollected = true;
        var poolManager = FindAnyObjectByType<AsteroidPoolManager>();
        if (poolManager != null)
        {
            poolManager.ReturnAsteroidToPool(asteroid.gameObject);
        }
        else
        {
            asteroid.gameObject.SetActive(false);
        }

        // 운석 스프라이트 표시
        ShowCarriedAsteroidSprite();

        Debug.Log($"운석 그랩 성공: {asteroid.asteroidData.asteroidName} (무게: {carriedAsteroid.weight})");
        Debug.Log("⚠️ 자원 획득을 위해서는 우주 정거장으로 운반해야 합니다!");
    }

    public void DropAsteroidAtCurrentPosition()
    {
        if (!HasCarriedAsteroid()) return;

        Debug.Log($"운석 놓기: {carriedAsteroid.asteroidData.asteroidName}");

        // 플레이어 앞에 실제 광물 오브젝트 생성
        CreatePhysicalAsteroid();

        // 놓기 이펙트
        OnDrop();

        // 운석 제거
        UnloadAsteroid();
    }

    void CreatePhysicalAsteroid()
    {
        if (!HasCarriedAsteroid() || asteroidPoolManager == null) return;

        // 플레이어 앞 위치 계산
        Vector3 dropPosition = CalculateDropPosition();

        // 풀에서 운석 오브젝트 가져오기
        GameObject asteroidObj = asteroidPoolManager.GetPooledAsteroid();
        if (asteroidObj == null)
        {
            Debug.LogWarning("풀에서 운석 오브젝트를 가져올 수 없습니다!");
            return;
        }

        // 운석 설정
        var asteroid = asteroidObj.GetComponent<Asteroid>();
        if (asteroid != null)
        {
            asteroid.Initialize(carriedAsteroid.asteroidData);
            asteroid.isCollected = false; // 다시 수집 가능하게 설정
        }

        // 위치 설정 및 활성화
        asteroidObj.transform.position = dropPosition;
        asteroidObj.SetActive(true);

        // 물리 적용 (살짝 떨어뜨리기)
        var rigidbody = asteroidObj.GetComponent<Rigidbody>();
        if (rigidbody != null)
        {
            rigidbody.mass = carriedAsteroid.asteroidData.mass;
            rigidbody.linearVelocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;

            // 살짝 아래로 떨어뜨리기
            rigidbody.AddForce(Vector3.down * 2f, ForceMode.Impulse);
        }

        if (carriedAsteroid.asteroidData)
        {
            var renderer = asteroid.GetComponent<SpriteRenderer>();
            renderer.sprite = carriedAsteroid.asteroidData.asteroidSprite;
        }

        Debug.Log($"물리적 운석 생성: {carriedAsteroid.asteroidData.asteroidName} at {dropPosition}");
    }

    Vector3 CalculateDropPosition()
    {
        if (grappleOrigin == null) return transform.position;

        // 플레이어 앞 1.5미터 지점에 생성
        Vector3 forwardDirection = grappleOrigin.forward;
        Vector3 dropPosition = grappleOrigin.position + forwardDirection * 3f;

        // Y 위치를 적절하게 조정 (바닥에서 살짝 위)
        dropPosition.y = 0.5f; // 고정 높이 또는 지형에 맞게 조정

        return dropPosition;
    }

    public void UnloadAsteroid()
    {
        if (HasCarriedAsteroid())
        {
            Debug.Log($"운석 하역: {carriedAsteroid.asteroidData.asteroidName}");
            carriedAsteroid = null;
            HideCarriedAsteroidSprite();
        }
    }

    void SetState(GrappleState newState)
    {
        if (currentState != newState)
        {
            currentState = newState;
            Debug.Log($"집게팔 상태 변경: {newState}");
        }
    }

    void UpdateGrappleVisual()
    {
        // 집게팔 시각적 업데이트 (라인 렌더러 등)
        // 구현 시 LineRenderer로 케이블 표현 등

        // 운석 스프라이트 위치 업데이트
        UpdateCarriedAsteroidSpritePosition();
    }

    void ShowCarriedAsteroidSprite()
    {
        if (carriedAsteroidSprite != null && HasCarriedAsteroid())
        {
            carriedAsteroidSprite.gameObject.SetActive(true);
            carriedAsteroidSprite.sprite = carriedAsteroid.asteroidData.asteroidSprite;
            carriedAsteroidSprite.color = carriedAsteroid.asteroidData.asteroidColor;
            UpdateCarriedAsteroidSpritePosition();
        }
    }

    void HideCarriedAsteroidSprite()
    {
        if (carriedAsteroidSprite != null)
        {
            carriedAsteroidSprite.gameObject.SetActive(false);
        }
    }

    void UpdateCarriedAsteroidSpritePosition()
    {
        if (carriedAsteroidSprite != null && carriedAsteroidSprite.gameObject.activeSelf)
        {
            // 스프라이트를 집게팔 끝이나 부착점에 위치시킴
            if (spriteAttachPoint != null)
            {
                carriedAsteroidSprite.transform.position = spriteAttachPoint.position;
            }
            else if (grappleArmTransform != null)
            {
                carriedAsteroidSprite.transform.position = grappleArmTransform.position;
            }
        }
    }

    #region 이벤트 함수들
    void OnLaunch()
    {
        Debug.Log("집게팔 발사!");

        // 이펙트 재생
        // if (launchEffect != null) launchEffect.Play();
        // if (launchSound != null) launchSound.Play();

        events.OnLaunch?.Invoke();
    }

    void OnTargetReached()
    {
        Debug.Log("목표 지점 도달!");
        events.OnTargetReached?.Invoke();
    }

    void OnGrabSuccess()
    {
        Debug.Log("그랩 성공!");

        // 이펙트 재생
        // if (grabSuccessEffect != null) grabSuccessEffect.Play();
        // if (grabSuccessSound != null) grabSuccessSound.Play();

        events.OnGrabSuccess?.Invoke();
    }

    void OnGrabFailed()
    {
        Debug.Log("그랩 실패!");

        // 이펙트 재생
        // if (grabFailEffect != null) grabFailEffect.Play();
        // if (grabFailSound != null) grabFailSound.Play();

        events.OnGrabFailed?.Invoke();
    }

    void OnReturnStart()
    {
        Debug.Log("돌아오기 시작!");

        // 이펙트 재생
        // if (returnEffect != null) returnEffect.Play();
        // if (returnSound != null) returnSound.Play();

        events.OnReturnStart?.Invoke();
    }

    void OnReturnComplete()
    {
        Debug.Log("돌아오기 완료!");
        events.OnReturnComplete?.Invoke();
    }

    void OnDrop()
    {
        Debug.Log("운석 놓기!");

        // 이펙트 재생
        // if (dropEffect != null) dropEffect.Play();
        // if (dropSound != null) dropSound.Play();

        // 별도 이벤트 없음 (기존 이벤트들로 충분)
    }
    #endregion

    #region 공개 인터페이스
    public bool IsReady()
    {
        return currentState == GrappleState.Ready;
    }

    public bool IsBusy()
    {
        return currentState != GrappleState.Ready && currentState != GrappleState.Cooldown;
    }

    public float GetCooldownProgress()
    {
        if (currentState == GrappleState.Cooldown)
            return 1f - (currentCooldown / settings.additionalCooldown);
        return 1f;
    }

    public int GetCarriedAsteroidCount()
    {
        return HasCarriedAsteroid() ? 1 : 0;
    }

    public float GetWeightRatio()
    {
        return GetTotalCarriedWeight() / settings.maxCarryWeight;
    }

    public bool CanPickupMoreAsteroids()
    {
        return !HasCarriedAsteroid();
    }

    public string GetCarriedAsteroidName()
    {
        return HasCarriedAsteroid() ? carriedAsteroid.asteroidData.asteroidName : "없음";
    }
    #endregion

    #region 디버그 기즈모

    public bool OnGizmo = false;
    // 디버그용 기즈모
    void OnDrawGizmos()
    {
        if (!OnGizmo) { return; }

        if (grappleOrigin == null) return;

        // 최대 사거리 표시
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(grappleOrigin.position, settings.maxRange);

        // 그랩 반경 표시 (목표 지점)
        if (currentState != GrappleState.Ready)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(targetPosition, settings.grabRadius);
        }

        // 집게팔 위치와 연결선
        if (grappleArmTransform != null && currentState != GrappleState.Ready)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(grappleOrigin.position, grappleArmTransform.position);
            Gizmos.DrawWireSphere(grappleArmTransform.position, 0.2f);
        }
    }
    #endregion
}
#endregion

#region Custom Editor
#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(GrappleArmSystem))]
public class GrappleArmSystemEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GrappleArmSystem grapple = (GrappleArmSystem)target;

        if (Application.isPlaying)
        {
            GUILayout.Space(20);
            GUILayout.BeginVertical("box");
            GUILayout.Label("🦾 집게팔 현재 상태", UnityEditor.EditorStyles.boldLabel);

            // 상태 표시
            GUILayout.Label($"상태: {grapple.currentState}");

            if (grapple.currentState == GrappleState.Cooldown)
            {
                float progress = grapple.GetCooldownProgress();
                GUILayout.Label($"쿨타임: {progress * 100:F0}%");
            }

            // 운반 상태
            GUILayout.Space(5);
            GUILayout.Label("운반 상태:", UnityEditor.EditorStyles.boldLabel);

            if (grapple.HasCarriedAsteroid())
            {
                GUILayout.Label($"운석: {grapple.GetCarriedAsteroidName()}");
                GUILayout.Label($"무게: {grapple.GetTotalCarriedWeight():F1}/{grapple.settings.maxCarryWeight:F1} ({grapple.GetWeightRatio() * 100:F0}%)");
                GUILayout.Label($"속도 감소: {grapple.GetSpeedReduction() * 100:F0}%");
            }
            else
            {
                GUILayout.Label("운석: 없음");
            }

            // 상태 아이콘
            if (grapple.IsOverweight())
            {
                GUILayout.Label("⚠️ 과부하! 움직일 수 없음",
                               new GUIStyle(GUI.skin.label) { normal = { textColor = Color.red } });
            }
            else if (grapple.HasCarriedAsteroid())
            {
                GUILayout.Label("📦 운석 운반 중",
                               new GUIStyle(GUI.skin.label) { normal = { textColor = Color.yellow } });
            }
            else if (grapple.IsReady())
            {
                GUILayout.Label("✅ 발사 준비 완료",
                               new GUIStyle(GUI.skin.label) { normal = { textColor = Color.green } });
            }
            else if (grapple.IsBusy())
            {
                GUILayout.Label("🔄 작업 중...",
                               new GUIStyle(GUI.skin.label) { normal = { textColor = Color.yellow } });
            }

            GUILayout.Space(10);

            // 테스트 버튼들
            GUILayout.BeginHorizontal();

            GUI.enabled = grapple.HasCarriedAsteroid();
            if (GUILayout.Button("운석 놓기"))
            {
                grapple.DropAsteroidAtCurrentPosition();
            }

            if (GUILayout.Button("운석 하역"))
            {
                grapple.UnloadAsteroid();
            }
            GUI.enabled = true;

            GUI.enabled = grapple.IsReady();
            if (GUILayout.Button("테스트 발사"))
            {
                Vector3 testTarget = grapple.transform.position + Vector3.forward * 10f;
                grapple.LaunchGrapple(testTarget);
            }
            GUI.enabled = true;

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }
    }
}
#endif
#endregion