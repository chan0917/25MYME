using System.Collections.Generic;
using UnityEngine;

public class BlackholeZone : MonoBehaviour
{
    [Header("Blackhole Settings")]
    public float outerRadius = 20f;          // 영향권 외곽 반지름
    public float innerRadius = 3f;           // 내부 반지름 (이벤트 호라이즌)
    public float maxGravityForce = 50f;      // 최대 중력 (중심부)
    public float gravityPower = 2f;          // 중력 증가 지수 (높을수록 급격히 증가)

    [Header("Affected Objects")]
    public LayerMask affectedLayers = -1;    // 영향받는 레이어
    public bool affectPlanets = true;        // 행성들에게 적용
    public bool affectDebris = true;         // 잔해물에게 적용
    public bool affectProjectiles = false;   // 발사체에게 적용 여부

    [Header("Object-Specific Settings")]
    public float planetGravityMultiplier = 1f;     // 행성 중력 배수
    public float debrisGravityMultiplier = 0.8f;   // 잔해물 중력 배수
    public float playerGravityMultiplier = 1.2f;   // 플레이어 중력 배수 (더 강하게)

    [Header("Escape Mechanics")]
    public float escapeThreshold = 0.8f;     // 탈출 임계점 (0~1, 낮을수록 탈출 어려움)
    public bool showEscapeZone = true;       // 탈출 불가 구역 시각화

    [Header("Effects")]
    public AnimationCurve gravityFalloff = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public float rotationSpeed = 30f;        // 빨려들어갈 때 회전 속도
    public float maxRotationForce = 10f;     // 최대 회전력

    [Header("Visual Effects")]
    public ParticleSystem gravityParticles; // 중력 파티클
    public GameObject warningEffect;         // 경고 이펙트
    public float warningDistance = 15f;      // 경고 표시 거리

    [Header("Audio")]
    public AudioSource blackholeSound;       // 블랙홀 사운드
    public float maxSoundVolume = 1f;

    private Transform center;
    private bool playerInZone = false;
    private float currentGravityStrength = 0f;
    public SpaceshipController playerController;

    private List<AffectedObject> affectedObjects = new List<AffectedObject>();

    [System.Serializable]
    public class AffectedObject
    {
        public Rigidbody rigidbody;
        public Transform transform;
        public ObjectType objectType;
        public float gravityMultiplier;
        public float mass;
        public bool canEscape;

        public AffectedObject(Rigidbody rb, ObjectType type, float multiplier)
        {
            rigidbody = rb;
            transform = rb.transform;
            objectType = type;
            gravityMultiplier = multiplier;
            mass = rb.mass;
            canEscape = true;
        }
    }

    public enum ObjectType
    {
        Player,
        Planet,
        Debris,
        Projectile,
        Other
    }

    void Start()
    {
        center = transform;

        // 경고 이펙트 비활성화
        if (warningEffect != null)
            warningEffect.SetActive(false);

        // 사운드 초기화
        if (blackholeSound != null)
        {
            blackholeSound.volume = 0f;
            blackholeSound.Play();
        }


    }

    void Update()
    {
        // 플레이어 찾기
        if (playerController == null)
        {
            playerController = FindAnyObjectByType<SpaceshipController>();
        }

        if (playerController != null)
        {
            UpdateBlackholeEffects();
        }
    }

    void UpdateBlackholeEffects()
    {
        Vector3 playerPos = playerController.transform.position;
        Vector3 centerPos = center.position;

        // Y축 무시 (2D 평면에서만 계산)
        playerPos.y = centerPos.y;

        float distance = Vector3.Distance(playerPos, centerPos);

        // 영향권 체크
        if (distance <= outerRadius)
        {
            if (!playerInZone)
            {
                OnPlayerEnterZone();
            }

            ApplyBlackholeForces(playerPos, centerPos, distance);
            UpdateVisualEffects(distance);
            UpdateAudioEffects(distance);
        }
        else
        {
            if (playerInZone)
            {
                OnPlayerExitZone();
            }
        }
    }

    void ApplyBlackholeForces(Vector3 playerPos, Vector3 centerPos, float distance)
    {
        Rigidbody playerRb = playerController.GetComponent<Rigidbody>();
        if (playerRb == null) return;

        // 거리 비율 계산 (0 = 중심, 1 = 외곽)
        float distanceRatio = Mathf.Clamp01(distance / outerRadius);

        // 중력 강도 계산 (거리 제곱 역비례 + 커스텀 커브)
        float gravityStrength = gravityFalloff.Evaluate(1f - distanceRatio);
        gravityStrength = Mathf.Pow(gravityStrength, gravityPower);
        currentGravityStrength = gravityStrength * maxGravityForce;

        // 중심으로 향하는 방향
        Vector3 directionToCenter = (centerPos - playerPos).normalized;

        // 중력 적용
        Vector3 gravityForce = directionToCenter * currentGravityStrength;
        playerRb.AddForce(gravityForce, ForceMode.Force);

        // 회전력 적용 (소용돌이 효과)
        ApplyRotationalForce(playerRb, playerPos, centerPos, gravityStrength);

        // 탈출 가능성 체크
        CheckEscapePossibility(playerRb, gravityForce);
    }

    void ApplyRotationalForce(Rigidbody playerRb, Vector3 playerPos, Vector3 centerPos, float gravityStrength)
    {
        // 중심을 기준으로 한 접선 방향 계산
        Vector3 toCenter = centerPos - playerPos;
        Vector3 tangentDirection = Vector3.Cross(toCenter.normalized, Vector3.up);

        // 회전력 강도 (중심에 가까울수록 강함)
        float rotationStrength = (gravityStrength / maxGravityForce) * maxRotationForce;

        // 현재 속도와 접선 방향의 내적으로 이미 회전하고 있는지 확인
        Vector3 currentVelocity = playerRb.linearVelocity;
        float alignmentWithRotation = Vector3.Dot(currentVelocity.normalized, tangentDirection);

        // 이미 같은 방향으로 회전하고 있다면 힘을 줄임
        if (alignmentWithRotation > 0.5f)
            rotationStrength *= 0.3f;

        Vector3 rotationForce = tangentDirection * rotationStrength * rotationSpeed;
        playerRb.AddForce(rotationForce, ForceMode.Force);
    }

    void CheckEscapePossibility(Rigidbody playerRb, Vector3 gravityForce)
    {
        // 플레이어의 추진력 계산
        Vector3 playerThrust = Vector3.zero;
        if (playerController.IsThrusting) // 프로퍼티 사용
        {
            playerThrust = playerController.transform.forward * playerController.ThrustForce;
        }

        // 중력에 대항하는 추진력 성분 계산
        Vector3 antigravityComponent = Vector3.Project(playerThrust, -gravityForce.normalized);

        // 탈출 가능성 판단
        bool canEscape = antigravityComponent.magnitude > gravityForce.magnitude * escapeThreshold;

        // 탈출 불가능 구역 진입 시 경고
        if (!canEscape && !warningEffect.activeInHierarchy)
        {
            Debug.Log("Player entered event horizon!");
            ShowEscapeWarning(true);
        }
        else if (canEscape && warningEffect.activeInHierarchy)
        {
            ShowEscapeWarning(false);
        }
    }

    void UpdateVisualEffects(float distance)
    {
        // 파티클 이펙트 업데이트
        if (gravityParticles != null)
        {
            var emission = gravityParticles.emission;
            float particleRate = Mathf.Lerp(10f, 100f, currentGravityStrength / maxGravityForce);
            emission.rateOverTime = particleRate;

            // 파티클 속도를 중력 강도에 맞춤
            var velocityModule = gravityParticles.velocityOverLifetime;
            velocityModule.enabled = true;
            velocityModule.space = ParticleSystemSimulationSpace.World;

            Vector3 playerPos = playerController.transform.position;
            Vector3 toCenter = (center.position - playerPos).normalized;
            velocityModule.radial = new ParticleSystem.MinMaxCurve(-currentGravityStrength * 0.1f);
        }

        // 경고 거리 체크
        if (distance <= warningDistance && !warningEffect.activeInHierarchy)
        {
            if (warningEffect != null)
                warningEffect.SetActive(true);
        }
        else if (distance > warningDistance && warningEffect.activeInHierarchy)
        {
            if (warningEffect != null)
                warningEffect.SetActive(false);
        }
    }

    void UpdateAudioEffects(float distance)
    {
        if (blackholeSound != null)
        {
            float distanceRatio = 1f - Mathf.Clamp01(distance / outerRadius);
            float targetVolume = distanceRatio * maxSoundVolume;

            blackholeSound.volume = Mathf.Lerp(blackholeSound.volume, targetVolume, Time.deltaTime * 2f);

            // 피치도 중력 강도에 따라 변경
            float targetPitch = 1f + (distanceRatio * 0.3f);
            blackholeSound.pitch = Mathf.Lerp(blackholeSound.pitch, targetPitch, Time.deltaTime);
        }
    }

    void ShowEscapeWarning(bool show)
    {
        if (warningEffect != null)
        {
            warningEffect.SetActive(show);

            // 카메라 이펙트 (DoTween 정리 후 활성화 예정)
            // var cameraEffects = Camera.main?.GetComponent<CameraEffects>();
            // if (cameraEffects != null && show)
            // {
            //     cameraEffects.ColorPulse(Color.red, 0.3f, 1f);
            // }
        }
    }

    void OnPlayerEnterZone()
    {
        playerInZone = true;
        Debug.Log("Player entered blackhole zone!");

        // 진입 이펙트 (DoTween 정리 후 활성화 예정)
        // var cameraEffects = Camera.main?.GetComponent<CameraEffects>();
        // if (cameraEffects != null)
        // {
        //     cameraEffects.ColorPulse(Color.blue, 0.2f, 0.5f);
        // }
    }

    void OnPlayerExitZone()
    {
        playerInZone = false;
        currentGravityStrength = 0f;

        // 모든 이펙트 정리
        if (warningEffect != null)
            warningEffect.SetActive(false);

        if (blackholeSound != null)
            blackholeSound.volume = 0f;

        Debug.Log("Player escaped blackhole zone!");
    }

    // 중심부 도달 시 (게임오버 등)
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SpaceshipController ship = other.GetComponent<SpaceshipController>();
            if (ship != null)
            {
                float distance = Vector3.Distance(transform.position, other.transform.position);
                if (distance <= innerRadius)
                {
                    OnPlayerReachCenter(ship);
                }
            }
        }
    }

    void OnPlayerReachCenter(SpaceshipController ship)
    {
        Debug.Log("Player reached blackhole center!");

        // 게임오버 처리 또는 특수 이벤트
        ship.Stop(); // 우주선 정지

        // 강력한 카메라 이펙트 (DoTween 정리 후 활성화 예정)
        // var cameraEffects = Camera.main?.GetComponent<CameraEffects>();
        // if (cameraEffects != null)
        // {
        //     cameraEffects.ExplosionEffect(transform.position, 0f, 1f);
        // }

        // 여기에 게임오버 로직 추가
        // GameManager.Instance.GameOver();
    }

    // 디버그용 기즈모
    void OnDrawGizmos()
    {
        // 외곽 영향권
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, outerRadius);

        // 내부 반지름 (이벤트 호라이즌)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, innerRadius);

        // 경고 거리
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, warningDistance);

        // 탈출 불가능 구역 표시
        if (showEscapeZone)
        {
            float noEscapeRadius = outerRadius * (1f - escapeThreshold);
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawSphere(transform.position, noEscapeRadius);
        }

        // 중력 강도 시각화 (게임 실행 중)
        if (Application.isPlaying && playerInZone)
        {
            Gizmos.color = Color.green;
            float visualRadius = (currentGravityStrength / maxGravityForce) * outerRadius;
            Gizmos.DrawWireSphere(transform.position, visualRadius);
        }
    }

    // 외부에서 중력 강도를 확인할 수 있는 함수
    public float GetGravityStrengthAtPosition(Vector3 position)
    {
        float distance = Vector3.Distance(position, transform.position);
        if (distance > outerRadius) return 0f;

        float distanceRatio = Mathf.Clamp01(distance / outerRadius);
        float gravityStrength = gravityFalloff.Evaluate(1f - distanceRatio);
        return Mathf.Pow(gravityStrength, gravityPower) * maxGravityForce;
    }

    // 특정 위치에서 탈출 가능한지 체크
    public bool CanEscapeFromPosition(Vector3 position, float thrustForce)
    {
        float gravityAtPosition = GetGravityStrengthAtPosition(position);
        return thrustForce > gravityAtPosition * escapeThreshold;
    }
}