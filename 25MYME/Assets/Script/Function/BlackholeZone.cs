using UnityEngine;
using System.Collections.Generic;

public class BlackholeZone : MonoBehaviour
{
    [Header("Blackhole Settings")]
    public float outerRadius = 20f;
    public float innerRadius = 3f;
    public float maxGravityForce = 50f;
    public float gravityPower = 2f;

    [Header("Affected Objects")]
    public LayerMask affectedLayers = -1;
    public bool affectPlanets = true;
    public bool affectDebris = true;
    public bool affectProjectiles = false;

    [Header("Object-Specific Settings")]
    public float planetGravityMultiplier = 1f;
    public float debrisGravityMultiplier = 0.8f;
    public float playerGravityMultiplier = 1.2f;

    [Header("Escape Mechanics")]
    public float escapeThreshold = 0.8f;
    public bool showEscapeZone = true;

    [Header("Effects")]
    public AnimationCurve gravityFalloff = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public float rotationSpeed = 30f;
    public float maxRotationForce = 10f;

    [Header("Visual Effects")]
    public ParticleSystem gravityParticles;
    public GameObject warningEffect;
    public float warningDistance = 15f;

    [Header("Audio")]
    public AudioSource blackholeSound;
    public float maxSoundVolume = 1f;

    private Transform center;
    private SpaceshipController playerController;
    private bool playerInZone = false;
    private List<AffectedObject> affectedObjects = new List<AffectedObject>();
    private float scanTimer = 0f;

    [System.Serializable]
    public class AffectedObject
    {
        public Rigidbody rigidbody;
        public Transform transform;
        public ObjectType objectType;
        public float gravityMultiplier;
        public float mass;

        public AffectedObject(Rigidbody rb, ObjectType type, float multiplier)
        {
            rigidbody = rb;
            transform = rb.transform;
            objectType = type;
            gravityMultiplier = multiplier;
            mass = rb.mass;
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

        if (warningEffect != null)
            warningEffect.SetActive(false);

        if (blackholeSound != null)
        {
            blackholeSound.volume = 0f;
            blackholeSound.Play();
        }

        ScanForObjects();
    }

    void Update()
    {
        scanTimer += Time.deltaTime;

        // 0.5�ʸ��� ���ο� ��ü ��ĵ
        if (scanTimer >= 0.5f)
        {
            ScanForObjects();
            scanTimer = 0f;
        }

        UpdateAllObjects();
        UpdatePlayerEffects();
    }

    void ScanForObjects()
    {
        // �ı��� ��ü ����
        affectedObjects.RemoveAll(obj => obj.rigidbody == null);

        // ���� �� ��� Rigidbody ã��
        Collider[] colliders = Physics.OverlapSphere(center.position, outerRadius, affectedLayers);

        foreach (Collider col in colliders)
        {
            Rigidbody rb = col.GetComponent<Rigidbody>();
            if (rb == null) continue;

            // �̹� ��Ͽ� �ִ��� Ȯ��
            if (affectedObjects.Exists(obj => obj.rigidbody == rb))
                continue;

            ObjectType objectType = GetObjectType(col.gameObject);

            if (ShouldAffectObjectType(objectType))
            {
                float gravityMultiplier = GetGravityMultiplier(objectType);
                affectedObjects.Add(new AffectedObject(rb, objectType, gravityMultiplier));

                //Debug.Log($"{objectType} '{col.name}' entered blackhole zone!");
            }
        }
    }

    void UpdateAllObjects()
    {
        for (int i = affectedObjects.Count - 1; i >= 0; i--)
        {
            AffectedObject obj = affectedObjects[i];

            if (obj.rigidbody == null)
            {
                affectedObjects.RemoveAt(i);
                continue;
            }

            Vector3 objPos = obj.transform.position;
            Vector3 centerPos = center.position;
            objPos.y = centerPos.y; // Y�� ����

            float distance = Vector3.Distance(objPos, centerPos);

            if (distance > outerRadius)
            {
                Debug.Log($"{obj.objectType} '{obj.transform.name}' escaped blackhole zone!");
                affectedObjects.RemoveAt(i);
                continue;
            }

            ApplyGravity(obj, objPos, centerPos, distance);

            // �߽ɺ� ���� üũ
            if (distance <= innerRadius)
            {
                HandleCenterReached(obj);
                affectedObjects.RemoveAt(i);
            }
        }
    }

    void ApplyGravity(AffectedObject obj, Vector3 objPos, Vector3 centerPos, float distance)
    {
        // �߷� ���� ���
        float distanceRatio = Mathf.Clamp01(distance / outerRadius);
        float gravityStrength = gravityFalloff.Evaluate(1f - distanceRatio);
        gravityStrength = Mathf.Pow(gravityStrength, gravityPower);

        float finalGravityForce = gravityStrength * maxGravityForce * obj.gravityMultiplier;

        // ������ ���� ���� (ū ��ü�� �� �������)
        float massEffect = Mathf.Clamp(1f / Mathf.Sqrt(obj.mass), 0.1f, 2f);
        finalGravityForce *= massEffect;

        // �߷� ����
        Vector3 directionToCenter = (centerPos - objPos).normalized;
        Vector3 gravityForce = directionToCenter * finalGravityForce;
        obj.rigidbody.AddForce(gravityForce, ForceMode.Force);

        ApplyRotationalForce(obj, objPos, centerPos, gravityStrength);
    }

    void ApplyRotationalForce(AffectedObject obj, Vector3 objPos, Vector3 centerPos, float gravityStrength)
    {
        Vector3 toCenter = centerPos - objPos;
        Vector3 tangentDirection = Vector3.Cross(toCenter.normalized, Vector3.up);

        float rotationStrength = (gravityStrength / maxGravityForce) * maxRotationForce;
        rotationStrength *= obj.gravityMultiplier;
        rotationStrength *= Mathf.Clamp(1f / obj.mass, 0.1f, 1f);

        Vector3 rotationForce = tangentDirection * rotationStrength * rotationSpeed;
        obj.rigidbody.AddForce(rotationForce, ForceMode.Force);
    }

    void UpdatePlayerEffects()
    {
        if (playerController == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerController = player.GetComponent<SpaceshipController>();
        }

        if (playerController == null) return;

        Vector3 playerPos = playerController.transform.position;
        Vector3 centerPos = center.position;
        playerPos.y = centerPos.y;

        float distance = Vector3.Distance(playerPos, centerPos);
        bool currentlyInZone = distance <= outerRadius;

        if (currentlyInZone)
        {
            UpdateVisualEffects(distance);
            UpdateAudioEffects(distance);
            CheckPlayerEscape(distance);
        }
        else if (playerInZone)
        {
            OnPlayerExitZone();
        }

        playerInZone = currentlyInZone;
    }

    void CheckPlayerEscape(float distance)
    {
        if (playerController == null) return;

        Vector3 playerThrust = Vector3.zero;
        if (playerController.IsThrusting)
        {
            playerThrust = playerController.transform.forward * playerController.ThrustForce;
        }

        float gravityAtPosition = GetGravityStrengthAtPosition(playerController.transform.position);
        gravityAtPosition *= playerGravityMultiplier;

        bool canEscape = playerThrust.magnitude > gravityAtPosition * escapeThreshold;

        if (!canEscape && (warningEffect == null || !warningEffect.activeInHierarchy))
        {
            ShowEscapeWarning(true);
        }
        else if (canEscape && warningEffect != null && warningEffect.activeInHierarchy)
        {
            ShowEscapeWarning(false);
        }
    }

    void UpdateVisualEffects(float distance)
    {
        if (gravityParticles != null)
        {
            var emission = gravityParticles.emission;
            float gravityStrength = GetGravityStrengthAtPosition(playerController.transform.position);
            float particleRate = Mathf.Lerp(10f, 100f, gravityStrength / maxGravityForce);
            emission.rateOverTime = particleRate;
        }

        if (distance <= warningDistance && (warningEffect == null || !warningEffect.activeInHierarchy))
        {
            if (warningEffect != null)
                warningEffect.SetActive(true);
        }
        else if (distance > warningDistance && warningEffect != null && warningEffect.activeInHierarchy)
        {
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
            blackholeSound.pitch = 1f + (distanceRatio * 0.3f);
        }
    }

    void ShowEscapeWarning(bool show)
    {
        if (warningEffect != null)
        {
            warningEffect.SetActive(show);
        }
    }

    void OnPlayerExitZone()
    {
        playerInZone = false;

        if (warningEffect != null)
            warningEffect.SetActive(false);

        if (blackholeSound != null)
            blackholeSound.volume = 0f;

        Debug.Log("Player escaped blackhole zone!");
    }

    void HandleCenterReached(AffectedObject obj)
    {
        Debug.Log($"{obj.objectType} '{obj.transform.name}' reached blackhole center!");

        switch (obj.objectType)
        {
            case ObjectType.Player:
                HandlePlayerDestruction(obj);
                break;

            case ObjectType.Planet:
                CreateDestructionEffect(obj.transform.position, 2f);
                if(obj.transform.GetComponent<Asteroid>() != null)
                {
                    obj.transform.GetComponent<Asteroid>().ReturnAsteroid();
                    affectedObjects.Remove(obj);
                }
                else
                {
                    Destroy(obj.rigidbody.gameObject);
                }
                break;

            case ObjectType.Debris:
            case ObjectType.Projectile:
            default:
                Destroy(obj.rigidbody.gameObject);
                break;
        }
    }

    void HandlePlayerDestruction(AffectedObject obj)
    {
        SpaceshipController ship = obj.rigidbody.GetComponent<SpaceshipController>();
        if (ship != null)
        {
            ship.Stop();
        }

        Debug.Log("GAME OVER - Player absorbed by blackhole!");
        // ���⿡ ���ӿ��� ���� �߰�
        // GameManager.Instance.GameOver();
    }

    void CreateDestructionEffect(Vector3 position, float intensity)
    {
        //Debug.Log($"Destruction effect at {position} with intensity {intensity}");
        // ���⿡ �ı� ��ƼŬ ����Ʈ �߰�
    }

    // ��ƿ��Ƽ �Լ���
    ObjectType GetObjectType(GameObject obj)
    {
        if (obj.CompareTag("Player")) return ObjectType.Player;
        if (obj.CompareTag("Planet")) return ObjectType.Planet;
        if (obj.CompareTag("Debris")) return ObjectType.Debris;
        if (obj.CompareTag("Projectile")) return ObjectType.Projectile;

        if (obj.GetComponent<SpaceshipController>() != null) return ObjectType.Player;

        string name = obj.name.ToLower();
        if (name.Contains("planet") || name.Contains("asteroid")) return ObjectType.Planet;
        if (name.Contains("debris") || name.Contains("fragment")) return ObjectType.Debris;

        return ObjectType.Other;
    }

    float GetGravityMultiplier(ObjectType objectType)
    {
        switch (objectType)
        {
            case ObjectType.Player: return playerGravityMultiplier;
            case ObjectType.Planet: return planetGravityMultiplier;
            case ObjectType.Debris: return debrisGravityMultiplier;
            default: return 1f;
        }
    }

    bool ShouldAffectObjectType(ObjectType objectType)
    {
        switch (objectType)
        {
            case ObjectType.Player: return true;
            case ObjectType.Planet: return affectPlanets;
            case ObjectType.Debris: return affectDebris;
            case ObjectType.Projectile: return affectProjectiles;
            default: return true;
        }
    }

    public float GetGravityStrengthAtPosition(Vector3 position)
    {
        float distance = Vector3.Distance(position, transform.position);
        if (distance > outerRadius) return 0f;

        float distanceRatio = Mathf.Clamp01(distance / outerRadius);
        float gravityStrength = gravityFalloff.Evaluate(1f - distanceRatio);
        return Mathf.Pow(gravityStrength, gravityPower) * maxGravityForce;
    }

    public bool CanEscapeFromPosition(Vector3 position, float thrustForce)
    {
        float gravityAtPosition = GetGravityStrengthAtPosition(position);
        return thrustForce > gravityAtPosition * escapeThreshold;
    }

    public int GetAffectedObjectCount()
    {
        return affectedObjects.Count;
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

        // Ż�� �Ұ��� ����
        if (showEscapeZone)
        {
            float noEscapeRadius = outerRadius * (1f - escapeThreshold);
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawSphere(transform.position, noEscapeRadius);
        }

        // ����޴� ��ü��
        if (Application.isPlaying && affectedObjects != null)
        {
            foreach (var obj in affectedObjects)
            {
                if (obj.rigidbody != null)
                {
                    switch (obj.objectType)
                    {
                        case ObjectType.Player: Gizmos.color = Color.green; break;
                        case ObjectType.Planet: Gizmos.color = Color.cyan; break;
                        case ObjectType.Debris: Gizmos.color = Color.gray; break;
                        case ObjectType.Projectile: Gizmos.color = Color.magenta; break;
                        default: Gizmos.color = Color.white; break;
                    }

                    Gizmos.DrawLine(obj.transform.position, transform.position);
                    Gizmos.DrawWireSphere(obj.transform.position, 0.5f);
                }
            }
        }
    }
}