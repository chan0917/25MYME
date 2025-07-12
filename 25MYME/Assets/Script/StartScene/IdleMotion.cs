using UnityEngine;

public class IdleMotion : MonoBehaviour
{
    public float moveSpeed = 1f;   // �����̴� �ӵ� ����
    public float moveRange = 10f;  // �����̴� ���� ����
    private RectTransform rectTransform;
    private float startY;
    private int direction = 1; // 1: ����, -1: �Ʒ���

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        startY = rectTransform.anchoredPosition.y;
    }

    void Update()
    {
        float newY = rectTransform.anchoredPosition.y + direction * moveSpeed * Time.deltaTime;

        if (newY > startY + moveRange)
        {
            direction = -1; // �Ʒ��� ���� ��ȯ
        }
        else if (newY < startY - moveRange)
        {
            direction = 1;  // ���� ���� ��ȯ
        }

        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, newY);
    }
}