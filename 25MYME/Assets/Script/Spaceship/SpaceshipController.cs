using UnityEngine;

public class SpaceshipController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float thrustForce = 15f;          // 추진력
    public float maxSpeed = 20f;             // 최대 속도
    public float rotationSpeed = 200f;       // 회전 속도 (Y축)
    public float drag = 0.98f;               // 공기 저항 (0~1)
    public float heightOffset = 0.5f;        // 바닥에서의 높이

    [Header("Input Settings")]
    public KeyCode thrustKey = KeyCode.W;
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode backKey = KeyCode.S;

    [Header("Visual Effects")]
    public GameObject thrustEffect;          // 추진 이펙트
    public AudioSource thrustSound;          // 추진 사운드

    [Header("Collision Settings")]
    public float bounceForce = 10f;          // 충돌 반발력
    public float collisionDamping = 0.7f;    // 충돌 시 속도 감소
    public float velocityBounceMultiplier = 0.5f; // 속도에 비례한 반발력 배수
    public float maxVelocityBounceMultiplier = 3f; // 최대 속도 배수 제한
    public LayerMask obstacleLayer = -1;     // 장애물 레이어

    [Header("Debug")]
    public KeyCode DebugBoost = KeyCode.R;
    public float boostForce = 20f;

    private Rigidbody rb;
    private bool isThrusting = false;
    private bool isReverseThrusting = false;
    private float fixedY;                    // 고정된 Y 위치
    private Vector3 lastVelocity;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Rigidbody 설정
        rb.useGravity = false;         // 중력 비활성화
        rb.linearDamping = 0f;                  // 기본 드래그 비활성화
        rb.angularDamping = 5f;           // 회전 드래그

        // Y축 회전만 허용
        rb.freezeRotation = true;

        // 시작 높이 설정 (고정값)
        fixedY = transform.position.y;

        // 추진 이펙트 비활성화
        if (thrustEffect != null)
            thrustEffect.SetActive(false);
    }

    void Update()
    {
        HandleInput();
        UpdateVisualEffects();
    }

    void FixedUpdate()
    {
        // 충돌 감지를 위해 이전 속도 저장 (물리 업데이트 전)
        lastVelocity = rb.linearVelocity;

        HandleMovement();
        HandleRotation();
        ApplyDrag();
        LimitSpeed();
        MaintainFixedHeight();
    }

    void HandleInput()
    {
        isThrusting = Input.GetKey(thrustKey);
        isReverseThrusting = Input.GetKey(backKey);

        if (Input.GetKeyDown(DebugBoost))
        {
            AddImpulse(transform.forward * boostForce);
        }
    }

    void HandleMovement()
    {
        // 3D에서는 forward 방향이 Z축
        Vector3 thrustDirection = transform.forward;

        if (isThrusting)
        {
            // 전진 추진 (Y축 제외)
            Vector3 force = new Vector3(thrustDirection.x, 0, thrustDirection.z) * thrustForce;
            rb.AddForce(force, ForceMode.Force);
        }
        else if (isReverseThrusting)
        {
            // 후진 추진 (전진력의 60% 정도)
            Vector3 force = new Vector3(-thrustDirection.x, 0, -thrustDirection.z) * thrustForce * 0.6f;
            rb.AddForce(force, ForceMode.Force);
        }
    }

    void HandleRotation()
    {
        float rotationInput = 0f;

        if (Input.GetKey(leftKey))
            rotationInput = -1f;
        else if (Input.GetKey(rightKey))
            rotationInput = 1f;

        // Y축 회전만 적용
        if (rotationInput != 0f)
        {
            float rotationAmount = rotationInput * rotationSpeed * Time.fixedDeltaTime;
            transform.Rotate(0, rotationAmount, 0, Space.Self);
        }
    }

    void ApplyDrag()
    {
        // XZ 평면에서만 드래그 적용
        Vector3 velocity = rb.linearVelocity;
        velocity.x *= drag;
        velocity.z *= drag;
        rb.linearVelocity = velocity;
    }

    void LimitSpeed()
    {
        // XZ 평면에서의 속도만 제한
        Vector3 velocity = rb.linearVelocity;
        Vector3 horizontalVelocity = new Vector3(velocity.x, 0, velocity.z);

        if (horizontalVelocity.magnitude > maxSpeed)
        {
            horizontalVelocity = horizontalVelocity.normalized * maxSpeed;
            rb.linearVelocity = new Vector3(horizontalVelocity.x, velocity.y, horizontalVelocity.z);
        }
    }

    void MaintainFixedHeight()
    {
        // Y 위치를 고정값으로 유지
        Vector3 pos = transform.position;
        pos.y = fixedY;
        transform.position = pos;

        // Y축 속도 제거
        Vector3 velocity = rb.linearVelocity;
        velocity.y = 0;
        rb.linearVelocity = velocity;
    }

    // 충돌 처리
    void OnCollisionEnter(Collision collision)
    {
        if (IsObstacle(collision.gameObject))
        {
            HandleObstacleCollision(collision);
        }
    }

    bool IsObstacle(GameObject obj)
    {
        return ((1 << obj.layer) & obstacleLayer) != 0;
    }

    void HandleObstacleCollision(Collision collision)
    {
        ContactPoint contact = collision.contacts[0];
        Vector3 collisionNormal = contact.normal;

        // Y축 제거 (수평 충돌만)
        collisionNormal.y = 0;
        collisionNormal.Normalize();

        // 충돌 전 속도 (감쇠 적용 전)
        Vector3 preCollisionVelocity = lastVelocity;
        preCollisionVelocity.y = 0;
        float preCollisionSpeed = preCollisionVelocity.magnitude;

        // 반발 벡터 계산
        Vector3 reflectedVelocity = Vector3.Reflect(preCollisionVelocity, collisionNormal);

        // 충돌 시 속도 감소 적용
        rb.linearVelocity = reflectedVelocity * collisionDamping;

        // 충돌 전 속도로 반발력 배수 계산 (더 정확함)
        float speedRatio = preCollisionSpeed / maxSpeed;
        float velocityMultiplier = 1f + (speedRatio * velocityBounceMultiplier);
        velocityMultiplier = Mathf.Clamp(velocityMultiplier, 1f, maxVelocityBounceMultiplier);

        // 추가 반발력 (벽에서 밀려나는 느낌) - 속도 배수 적용
        Vector3 bounceImpulse = collisionNormal * bounceForce * velocityMultiplier;
        rb.AddForce(bounceImpulse, ForceMode.Impulse);

        // 충돌 이펙트 (충돌 전 속도 기준)
        CreateCollisionEffect(contact.point, preCollisionSpeed, velocityMultiplier);
    }

    void CreateCollisionEffect(Vector3 position, float collisionSpeed = 0f, float bounceMultiplier = 1f)
    {
        // 디버깅 정보 출력
        Debug.Log($"Collision - Speed: {collisionSpeed:F1}/{maxSpeed:F1} ({(collisionSpeed / maxSpeed) * 100:F0}%), " +
                 $"Bounce Multiplier: {bounceMultiplier:F2}x, " +
                 $"Bounce Force: {bounceForce * bounceMultiplier:F1}");

        // 카메라 이펙트가 있다면 속도에 따른 강도로 호출
        // CameraEffects cameraEffects = Camera.main.GetComponent<CameraEffects>();
        // if (cameraEffects != null)
        // {
        //     float effectIntensity = collisionSpeed / maxSpeed * bounceMultiplier;
        //     cameraEffects.CollisionShake(effectIntensity);
        // }
    }

    void UpdateVisualEffects()
    {
        // 추진 이펙트 활성화/비활성화
        if (thrustEffect != null)
        {
            thrustEffect.SetActive(isThrusting);
        }

        // 추진 사운드 재생/정지
        if (thrustSound != null)
        {
            if (isThrusting && !thrustSound.isPlaying)
            {
                thrustSound.Play();
            }
            else if (!isThrusting && thrustSound.isPlaying)
            {
                thrustSound.Stop();
            }
        }
    }

    // 현재 속도 비율 반환 (UI나 다른 스크립트에서 사용)
    public float GetSpeedRatio()
    {
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        return horizontalVelocity.magnitude / maxSpeed;
    }

    // 현재 속도 벡터 반환 (XZ 평면만)
    public Vector3 GetHorizontalVelocity()
    {
        return new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
    }

    // 즉시 정지 (충돌 시 등에 사용)
    public void Stop()
    {
        rb.linearVelocity = new Vector3(0, 0, 0);
        rb.angularVelocity = Vector3.zero;
    }

    // 추진력 즉시 적용 (부스터 아이템 등에 사용)
    public void AddImpulse(Vector3 impulse)
    {
        // Y축 제외하고 적용
        Vector3 horizontalImpulse = new Vector3(impulse.x, 0, impulse.z);
        rb.AddForce(horizontalImpulse, ForceMode.Impulse);
    }

    // 높이 수동 설정
    public void SetFixedHeight(float newY)
    {
        fixedY = newY;
        Vector3 pos = transform.position;
        pos.y = fixedY;
        transform.position = pos;
    }

    public bool IsThrusting => isThrusting;
    public bool IsReverseThrusting => isReverseThrusting;
    public float ThrustForce => thrustForce;

    // 디버그용 기즈모
    void OnDrawGizmos()
    {
        // 우주선 위치와 방향
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.5f);

        // 전진 방향 표시
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward * 2f);

        // 속도 벡터 표시
        if (Application.isPlaying && rb != null)
        {
            Gizmos.color = Color.green;
            Vector3 velocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            Gizmos.DrawRay(transform.position, velocity);
        }
    }
}
