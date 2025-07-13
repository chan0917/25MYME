using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    private Camera cam;

    [Header("�� ����")]
    public float defaultFOV = 60f;

    // �� ���� ����
    private bool isZooming = false;
    private float startFOV;
    private float targetFOV;
    private float zoomDuration;
    private float zoomTimer;

    void Start()
    {
        cam = GetComponent<Camera>();
        cam.fieldOfView = defaultFOV;
    }

    void Update()
    {
        if (isZooming)
        {
            zoomTimer += Time.deltaTime;
            float progress = zoomTimer / zoomDuration;

            // �ε巯�� ����
            cam.fieldOfView = Mathf.Lerp(startFOV, targetFOV, progress);

            // �� �Ϸ� üũ
            if (progress >= 1f)
            {
                cam.fieldOfView = targetFOV;
                isZooming = false;
            }
        }
    }

    // �� ���� �Լ� (FOV ����)
    public void ZoomTo(float fov)
    {
        ZoomTo(fov, 1f); // �⺻ 1��
    }

    // �� ���� �Լ� (FOV + �ð�)
    public void ZoomTo(float fov, float duration)
    {
        if (isZooming) return; // �̹� �� ���̸� ����

        startFOV = cam.fieldOfView;
        targetFOV = fov;
        zoomDuration = duration;
        zoomTimer = 0f;
        isZooming = true;
    }

    // �⺻ FOV�� ����
    public void ZoomReset()
    {
        ZoomTo(defaultFOV, 1f);
    }

    public void ZoomReset(float duration)
    {
        ZoomTo(defaultFOV, duration);
    }
}