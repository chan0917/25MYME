using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    // �ν����Ϳ��� �ӵ��� ������ �� �ִ� ����
    public float scrollSpeed = 50f;

    // �̵� ������ �����ϴ� ���� (1: ������, -1: ����)
    private int moveDirection = 1;

    private RectTransform rectTransform;
    private RectTransform canvasRectTransform;
    private float screenWidth;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        // �θ� ������Ʈ�� Canvas�� ã�� RectTransform�� �����ɴϴ�.
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            canvasRectTransform = canvas.GetComponent<RectTransform>();
            // ĵ����(ȭ��)�� �ʺ� ����մϴ�.
            screenWidth = canvasRectTransform.rect.width;
        }
        else
        {
            Debug.LogError("��� �̹����� Canvas �Ʒ��� ���� �ʽ��ϴ�. ��ũ��Ʈ�� ��Ȱ��ȭ�մϴ�.");
            enabled = false;
        }
    }

    void Update()
    {
        // ���� �̹��� ��ġ�� ȭ���� ������ ���� ��������� ������ �������� �ٲߴϴ�.
        if (rectTransform.anchoredPosition.x > screenWidth / 2f)
        {
            moveDirection = -1;
        }
        // ���� �̹��� ��ġ�� ȭ���� ���� ���� ��������� ������ ���������� �ٲߴϴ�.
        else if (rectTransform.anchoredPosition.x < -screenWidth / 2f)
        {
            moveDirection = 1;
        }

        // ���� ����� �ӵ��� ���� ����� �̵���ŵ�ϴ�.
        rectTransform.anchoredPosition += new Vector2(scrollSpeed * moveDirection * Time.deltaTime, 0);
    }
}