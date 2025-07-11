using System.Collections.Generic;
using UnityEngine;

public class BlackholeZone : MonoBehaviour
{
    [Header("Blackhole Settings")]
    public float outerRadius = 20f;          // ����� �ܰ� ������
    public float innerRadius = 3f;           // ���� ������ (�̺�Ʈ ȣ������)
    public float maxGravityForce = 50f;      // �ִ� �߷� (�߽ɺ�)
    public float gravityPower = 2f;          // �߷� ���� ���� (�������� �ް��� ����)

    [Header("Affected Objects")]
    public LayerMask affectedLayers = -1;    // ����޴� ���̾�
    public bool affectPlanets = true;        // �༺�鿡�� ����
    public bool affectDebris = true;         // ���ع����� ����
    public bool affectProjectiles = false;   // �߻�ü���� ���� ����

    [Header("Object-Specific Settings")]
    public float planetGravityMultiplier = 1f;     // �༺ �߷� ���
    public float debrisGravityMultiplier = 0.8f;   // ���ع� �߷� ���
    public float playerGravityMultiplier = 1.2f;   // �÷��̾� �߷� ��� (�� ���ϰ�)

    [Header("Escape Mechanics")]
    public float escapeThreshold = 0.8f;     // Ż�� �Ӱ��� (0~1, �������� Ż�� �����)
    public bool showEscapeZone = true;       // Ż�� �Ұ� ���� �ð�ȭ

    [Header("Effects")]
    public AnimationCurve gravityFalloff = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public float rotationSpeed = 30f;        // ������ �� ȸ�� �ӵ�
    public float maxRotationForce = 10f;     // �ִ� ȸ����

    [Header("Visual Effects")]
    public ParticleSystem gravityParticles; // �߷� ��ƼŬ
    public GameObject warningEffect;         // ��� ����Ʈ
    public float warningDistance = 15f;      // ��� ǥ�� �Ÿ�

    [Header("Audio")]
    public AudioSource blackholeSound;       // ��Ȧ ����
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

        // ��� ����Ʈ ��Ȱ��ȭ
        if (warningEffect != null)
            warningEffect.SetActive(false);

        // ���� �ʱ�ȭ
        if (blackholeSound != null)
        {
            blackholeSound.volume = 0f;
            blackholeSound.Play();
        }


    }

    void Update()
    {
        // �÷��̾� ã��
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

        // Y�� ���� (2D ��鿡���� ���)
        playerPos.y = centerPos.y;

        float distance = Vector3.Distance(playerPos, centerPos);

        // ����� üũ
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

        // �Ÿ� ���� ��� (0 = �߽�, 1 = �ܰ�)
        float distanceRatio = Mathf.Clamp01(distance / outerRadius);

        // �߷� ���� ��� (�Ÿ� ���� ����� + Ŀ���� Ŀ��)
        float gravityStrength = gravityFalloff.Evaluate(1f - distanceRatio);
        gravityStrength = Mathf.Pow(gravityStrength, gravityPower);
        currentGravityStrength = gravityStrength * maxGravityForce;

        // �߽����� ���ϴ� ����
        Vector3 directionToCenter = (centerPos - playerPos).normalized;

        // �߷� ����
        Vector3 gravityForce = directionToCenter * currentGravityStrength;
        playerRb.AddForce(gravityForce, ForceMode.Force);

        // ȸ���� ���� (�ҿ뵹�� ȿ��)
        ApplyRotationalForce(playerRb, playerPos, centerPos, gravityStrength);

        // Ż�� ���ɼ� üũ
        CheckEscapePossibility(playerRb, gravityForce);
    }

    void ApplyRotationalForce(Rigidbody playerRb, Vector3 playerPos, Vector3 centerPos, float gravityStrength)
    {
        // �߽��� �������� �� ���� ���� ���
        Vector3 toCenter = centerPos - playerPos;
        Vector3 tangentDirection = Vector3.Cross(toCenter.normalized, Vector3.up);

        // ȸ���� ���� (�߽ɿ� �������� ����)
        float rotationStrength = (gravityStrength / maxGravityForce) * maxRotationForce;

        // ���� �ӵ��� ���� ������ �������� �̹� ȸ���ϰ� �ִ��� Ȯ��
        Vector3 currentVelocity = playerRb.linearVelocity;
        float alignmentWithRotation = Vector3.Dot(currentVelocity.normalized, tangentDirection);

        // �̹� ���� �������� ȸ���ϰ� �ִٸ� ���� ����
        if (alignmentWithRotation > 0.5f)
            rotationStrength *= 0.3f;

        Vector3 rotationForce = tangentDirection * rotationStrength * rotationSpeed;
        playerRb.AddForce(rotationForce, ForceMode.Force);
    }

    void CheckEscapePossibility(Rigidbody playerRb, Vector3 gravityForce)
    {
        // �÷��̾��� ������ ���
        Vector3 playerThrust = Vector3.zero;
        if (playerController.IsThrusting) // ������Ƽ ���
        {
            playerThrust = playerController.transform.forward * playerController.ThrustForce;
        }

        // �߷¿� �����ϴ� ������ ���� ���
        Vector3 antigravityComponent = Vector3.Project(playerThrust, -gravityForce.normalized);

        // Ż�� ���ɼ� �Ǵ�
        bool canEscape = antigravityComponent.magnitude > gravityForce.magnitude * escapeThreshold;

        // Ż�� �Ұ��� ���� ���� �� ���
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
        // ��ƼŬ ����Ʈ ������Ʈ
        if (gravityParticles != null)
        {
            var emission = gravityParticles.emission;
            float particleRate = Mathf.Lerp(10f, 100f, currentGravityStrength / maxGravityForce);
            emission.rateOverTime = particleRate;

            // ��ƼŬ �ӵ��� �߷� ������ ����
            var velocityModule = gravityParticles.velocityOverLifetime;
            velocityModule.enabled = true;
            velocityModule.space = ParticleSystemSimulationSpace.World;

            Vector3 playerPos = playerController.transform.position;
            Vector3 toCenter = (center.position - playerPos).normalized;
            velocityModule.radial = new ParticleSystem.MinMaxCurve(-currentGravityStrength * 0.1f);
        }

        // ��� �Ÿ� üũ
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

            // ��ġ�� �߷� ������ ���� ����
            float targetPitch = 1f + (distanceRatio * 0.3f);
            blackholeSound.pitch = Mathf.Lerp(blackholeSound.pitch, targetPitch, Time.deltaTime);
        }
    }

    void ShowEscapeWarning(bool show)
    {
        if (warningEffect != null)
        {
            warningEffect.SetActive(show);

            // ī�޶� ����Ʈ (DoTween ���� �� Ȱ��ȭ ����)
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

        // ���� ����Ʈ (DoTween ���� �� Ȱ��ȭ ����)
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

        // ��� ����Ʈ ����
        if (warningEffect != null)
            warningEffect.SetActive(false);

        if (blackholeSound != null)
            blackholeSound.volume = 0f;

        Debug.Log("Player escaped blackhole zone!");
    }

    // �߽ɺ� ���� �� (���ӿ��� ��)
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

        // ���ӿ��� ó�� �Ǵ� Ư�� �̺�Ʈ
        ship.Stop(); // ���ּ� ����

        // ������ ī�޶� ����Ʈ (DoTween ���� �� Ȱ��ȭ ����)
        // var cameraEffects = Camera.main?.GetComponent<CameraEffects>();
        // if (cameraEffects != null)
        // {
        //     cameraEffects.ExplosionEffect(transform.position, 0f, 1f);
        // }

        // ���⿡ ���ӿ��� ���� �߰�
        // GameManager.Instance.GameOver();
    }

    // ����׿� �����
    void OnDrawGizmos()
    {
        // �ܰ� �����
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, outerRadius);

        // ���� ������ (�̺�Ʈ ȣ������)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, innerRadius);

        // ��� �Ÿ�
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, warningDistance);

        // Ż�� �Ұ��� ���� ǥ��
        if (showEscapeZone)
        {
            float noEscapeRadius = outerRadius * (1f - escapeThreshold);
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawSphere(transform.position, noEscapeRadius);
        }

        // �߷� ���� �ð�ȭ (���� ���� ��)
        if (Application.isPlaying && playerInZone)
        {
            Gizmos.color = Color.green;
            float visualRadius = (currentGravityStrength / maxGravityForce) * outerRadius;
            Gizmos.DrawWireSphere(transform.position, visualRadius);
        }
    }

    // �ܺο��� �߷� ������ Ȯ���� �� �ִ� �Լ�
    public float GetGravityStrengthAtPosition(Vector3 position)
    {
        float distance = Vector3.Distance(position, transform.position);
        if (distance > outerRadius) return 0f;

        float distanceRatio = Mathf.Clamp01(distance / outerRadius);
        float gravityStrength = gravityFalloff.Evaluate(1f - distanceRatio);
        return Mathf.Pow(gravityStrength, gravityPower) * maxGravityForce;
    }

    // Ư�� ��ġ���� Ż�� �������� üũ
    public bool CanEscapeFromPosition(Vector3 position, float thrustForce)
    {
        float gravityAtPosition = GetGravityStrengthAtPosition(position);
        return thrustForce > gravityAtPosition * escapeThreshold;
    }
}