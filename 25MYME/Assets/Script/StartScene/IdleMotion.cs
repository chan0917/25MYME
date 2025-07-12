using UnityEngine;

public class IdleMotion : MonoBehaviour
{
    public float moveSpeed = 1f;   // 움직이는 속도 조절
    public float moveRange = 10f;  // 움직이는 범위 조절
    private RectTransform rectTransform;
    private float startY;
    private int direction = 1; // 1: 위로, -1: 아래로

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
            direction = -1; // 아래로 방향 전환
        }
        else if (newY < startY - moveRange)
        {
            direction = 1;  // 위로 방향 전환
        }

        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, newY);
    }
}