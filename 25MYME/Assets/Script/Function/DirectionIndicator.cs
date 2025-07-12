using UnityEngine;

public class CircleDirectionIndicator : MonoBehaviour
{
    [Header("Ÿ�� ����")]
    public Transform targetPosition; // ����ų ��ǥ ��ġ (�ǹ� ��)
    public Vector3 targetWorldPosition; // �Ǵ� ���� ��ǥ �Է�
    public bool useWorldPosition = false; // targetWorldPosition ��� ����

    [Header("�� ����")]
    public float circleRadius = 100f; // ���� ������ (UI ����)
    public Vector2 circleCenter = Vector2.zero; // ���� �߽� (ȭ�� �߾� ����)

    [Header("�÷��̾� ����")]
    public Transform playerTransform; // �÷��̾� Transform

    private RectTransform rectTransform;
    private Canvas canvas;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        // ȭ��ǥ ��Ŀ�� �߽����� ����
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0f); // �ǹ��� ȭ��ǥ ���� �κ����� ����
    }

    void Update()
    {
        UpdateDirectionIndicator();
    }

    void UpdateDirectionIndicator()
    {
        // Ÿ�� ��ġ ����
        Vector3 target = useWorldPosition ? targetWorldPosition : targetPosition.position;

        // �÷��̾�� Ÿ�ٱ����� ���� ���� ��� (���� ��ǥ��)
        Vector3 directionToTarget = target - playerTransform.position;

        // ž���̹Ƿ� Y�� ���� (���󿡼��� ���)
        directionToTarget.y = 0;
        directionToTarget.Normalize();

        // �÷��̾��� ȸ���� ����� ����� ���� ���
        // �÷��̾ ȸ���ص� ȭ��ǥ�� �������� ������ �����Ѿ� ��

        // �÷��̾��� ���� ȸ�� ���� (Y�� ����)
        float playerRotationY = playerTransform.eulerAngles.y;

        // �������� ���⿡�� �÷��̾��� ȸ���� �� ����� ���� ���
        Vector3 relativeDirection = Quaternion.Euler(0, -playerRotationY, 0) * directionToTarget;

        // UI ��ǥ��� ��ȯ
        Vector2 uiDirection = new Vector2(relativeDirection.x, relativeDirection.z);

        // �� ���� ��ġ ���
        Vector2 circlePosition = circleCenter + uiDirection * circleRadius;

        // ���� ǥ�ñ� ��ġ ������Ʈ
        rectTransform.anchoredPosition = circlePosition;

        // ���� ǥ�ñ� ȸ�� (ȭ��ǥ ���κ��� Ÿ���� ����Ű����)
        // ����� ���� �������� ���� ���
        float angle = Mathf.Atan2(uiDirection.x, uiDirection.y) * Mathf.Rad2Deg;
        rectTransform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    // Ÿ�� ��ġ ���� ����� �Լ���
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

    // ���� ������ ����
    public void SetCircleRadius(float newRadius)
    {
        circleRadius = newRadius;
    }

    // ���� �߽� ��ġ ����
    public void SetCircleCenter(Vector2 newCenter)
    {
        circleCenter = newCenter;
    }

    // ����׿� - �����Ϳ��� ����� ǥ��
    void OnDrawGizmos()
    {
        if (playerTransform != null)
        {
            Vector3 target = useWorldPosition ? targetWorldPosition :
                           (targetPosition != null ? targetPosition.position : Vector3.zero);

            // �÷��̾�� Ÿ�ٱ����� �� �׸���
            Gizmos.color = Color.red;
            Gizmos.DrawLine(playerTransform.position, target);

            // Ÿ�� ��ġ ǥ��
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(target, 0.5f);

            // �÷��̾� ��ġ ǥ��
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(playerTransform.position, 0.3f);

            // ���� ���� ǥ��
            Vector3 direction = target - playerTransform.position;
            direction.y = 0;
            Gizmos.color = Color.green;
            Gizmos.DrawRay(playerTransform.position, direction.normalized * 2f);
        }
    }

    // ����� ���� ���
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
            GUI.Label(new Rect(10, 30, 300, 20), $"Player Rotation: {playerRotation:F1}��");
            GUI.Label(new Rect(10, 50, 300, 20), $"Relative Direction: {relativeDirection}");
            GUI.Label(new Rect(10, 70, 300, 20), $"UI Position: {rectTransform.anchoredPosition}");
            GUI.Label(new Rect(10, 90, 300, 20), $"UI Rotation: {rectTransform.rotation.eulerAngles.z:F1}��");
        }
    }
}