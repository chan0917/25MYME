using UnityEngine;

public class CircleDirectionIndicator : MonoBehaviour
{
    [Header("타겟 설정")]
    public Transform targetPosition; // 가리킬 목표 위치 (건물 등)
    public Vector3 targetWorldPosition; // 또는 직접 좌표 입력
    public bool useWorldPosition = false; // targetWorldPosition 사용 여부

    [Header("원 설정")]
    public float circleRadius = 100f; // 원의 반지름 (UI 단위)
    public Vector2 circleCenter = Vector2.zero; // 원의 중심 (화면 중앙 기준)

    [Header("플레이어 설정")]
    public Transform playerTransform; // 플레이어 Transform

    private RectTransform rectTransform;
    private Canvas canvas;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        // 화살표 앵커를 중심으로 설정
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0f); // 피벗을 화살표 꼬리 부분으로 설정
    }

    void Update()
    {
        UpdateDirectionIndicator();
    }

    void UpdateDirectionIndicator()
    {
        // 타겟 위치 결정
        Vector3 target = useWorldPosition ? targetWorldPosition : targetPosition.position;

        // 플레이어에서 타겟까지의 방향 벡터 계산 (월드 좌표계)
        Vector3 directionToTarget = target - playerTransform.position;

        // 탑뷰이므로 Y축 제거 (평면상에서만 계산)
        directionToTarget.y = 0;
        directionToTarget.Normalize();

        // 플레이어의 회전을 고려한 상대적 방향 계산
        // 플레이어가 회전해도 화살표는 절대적인 방향을 가리켜야 함

        // 플레이어의 현재 회전 각도 (Y축 기준)
        float playerRotationY = playerTransform.eulerAngles.y;

        // 절대적인 방향에서 플레이어의 회전을 뺀 상대적 방향 계산
        Vector3 relativeDirection = Quaternion.Euler(0, -playerRotationY, 0) * directionToTarget;

        // UI 좌표계로 변환
        Vector2 uiDirection = new Vector2(relativeDirection.x, relativeDirection.z);

        // 원 위의 위치 계산
        Vector2 circlePosition = circleCenter + uiDirection * circleRadius;

        // 방향 표시기 위치 업데이트
        rectTransform.anchoredPosition = circlePosition;

        // 방향 표시기 회전 (화살표 끝부분이 타겟을 가리키도록)
        // 상대적 방향 기준으로 각도 계산
        float angle = Mathf.Atan2(uiDirection.x, uiDirection.y) * Mathf.Rad2Deg;
        rectTransform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    // 타겟 위치 동적 변경용 함수들
    public void SetTargetTransform(Transform newTarget)
    {
        targetPosition = newTarget;
        useWorldPosition = false;
    }

    public void SetTargetWorldPosition(Vector3 newPosition)
    {
        targetWorldPosition = newPosition;
        useWorldPosition = true;
    }

    // 원의 반지름 조절
    public void SetCircleRadius(float newRadius)
    {
        circleRadius = newRadius;
    }

    // 원의 중심 위치 조절
    public void SetCircleCenter(Vector2 newCenter)
    {
        circleCenter = newCenter;
    }

    // 디버그용 - 에디터에서 기즈모 표시
    void OnDrawGizmos()
    {
        if (playerTransform != null)
        {
            Vector3 target = useWorldPosition ? targetWorldPosition :
                           (targetPosition != null ? targetPosition.position : Vector3.zero);

            // 플레이어에서 타겟까지의 선 그리기
            Gizmos.color = Color.red;
            Gizmos.DrawLine(playerTransform.position, target);

            // 타겟 위치 표시
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(target, 0.5f);

            // 플레이어 위치 표시
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(playerTransform.position, 0.3f);

            // 방향 벡터 표시
            Vector3 direction = target - playerTransform.position;
            direction.y = 0;
            Gizmos.color = Color.green;
            Gizmos.DrawRay(playerTransform.position, direction.normalized * 2f);
        }
    }

    // 디버그 정보 출력
    void OnGUI()
    {
        if (Application.isPlaying && playerTransform != null)
        {
            Vector3 target = useWorldPosition ? targetWorldPosition : targetPosition.position;
            Vector3 direction = target - playerTransform.position;
            direction.y = 0;

            float playerRotation = playerTransform.eulerAngles.y;
            Vector3 relativeDirection = Quaternion.Euler(0, -playerRotation, 0) * direction.normalized;

            GUI.Label(new Rect(10, 10, 300, 20), $"World Direction: {direction.normalized}");
            GUI.Label(new Rect(10, 30, 300, 20), $"Player Rotation: {playerRotation:F1}°");
            GUI.Label(new Rect(10, 50, 300, 20), $"Relative Direction: {relativeDirection}");
            GUI.Label(new Rect(10, 70, 300, 20), $"UI Position: {rectTransform.anchoredPosition}");
            GUI.Label(new Rect(10, 90, 300, 20), $"UI Rotation: {rectTransform.rotation.eulerAngles.z:F1}°");
        }
    }
}