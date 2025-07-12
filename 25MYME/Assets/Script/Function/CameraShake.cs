using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public float shakeDuration = 0.5f;
    public float shakeIntensity = 0.3f;

    private Vector3 originalPosition;
    private float shakeTimer;

    void Start()
    {
        originalPosition = transform.localPosition;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            TriggerShake();
        }

        if (shakeTimer > 0)
        {
            transform.localPosition = originalPosition + Random.insideUnitSphere * shakeIntensity;
            shakeTimer -= Time.deltaTime;
        }
        else
        {
            transform.localPosition = originalPosition;
        }
    }

    public void TriggerShake()
    {
        shakeTimer = shakeDuration;
    }

    public void TriggerShake(float duration, float power)
    {
        shakeDuration = duration;
        shakeIntensity = power;

        shakeTimer = shakeDuration;
    }
    
    public void TriggerBigShake()
    {
        shakeDuration = 0.5f;
        shakeIntensity = 0.3f;

        shakeTimer = shakeDuration;
    }

    public void TriggerMiddleShake()
    {
        shakeDuration = 0.3f;
        shakeIntensity = 0.2f;

        shakeTimer = shakeDuration;
    }

    public void TriggerSmallShake()
    {
        shakeDuration = 0.1f;
        shakeIntensity = 0.15f;

        shakeTimer = shakeDuration;
    }
}

