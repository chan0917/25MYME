using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    private Camera cam;

    [Header("줌 설정")]
    public float defaultFOV = 60f;

    // 줌 상태 관리
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

            // 부드러운 보간
            cam.fieldOfView = Mathf.Lerp(startFOV, targetFOV, progress);

            // 줌 완료 체크
            if (progress >= 1f)
            {
                cam.fieldOfView = targetFOV;
                isZooming = false;
            }
        }
    }

    // 줌 실행 함수 (FOV 값만)
    public void ZoomTo(float fov)
    {
        ZoomTo(fov, 1f); // 기본 1초
    }

    // 줌 실행 함수 (FOV + 시간)
    public void ZoomTo(float fov, float duration)
    {
        if (isZooming) return; // 이미 줌 중이면 무시

        startFOV = cam.fieldOfView;
        targetFOV = fov;
        zoomDuration = duration;
        zoomTimer = 0f;
        isZooming = true;
    }

    // 기본 FOV로 복귀
    public void ZoomReset()
    {
        ZoomTo(defaultFOV, 1f);
    }

    public void ZoomReset(float duration)
    {
        ZoomTo(defaultFOV, duration);
    }
}