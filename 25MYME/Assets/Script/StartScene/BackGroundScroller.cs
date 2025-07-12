using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    // 인스펙터에서 속도를 조절할 수 있는 변수
    public float scrollSpeed = 50f;

    // 이동 방향을 결정하는 변수 (1: 오른쪽, -1: 왼쪽)
    private int moveDirection = 1;

    private RectTransform rectTransform;
    private RectTransform canvasRectTransform;
    private float screenWidth;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        // 부모 오브젝트인 Canvas를 찾아 RectTransform을 가져옵니다.
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            canvasRectTransform = canvas.GetComponent<RectTransform>();
            // 캔버스(화면)의 너비를 계산합니다.
            screenWidth = canvasRectTransform.rect.width;
        }
        else
        {
            Debug.LogError("배경 이미지가 Canvas 아래에 있지 않습니다. 스크립트를 비활성화합니다.");
            enabled = false;
        }
    }

    void Update()
    {
        // 현재 이미지 위치가 화면의 오른쪽 끝에 가까워지면 방향을 왼쪽으로 바꿉니다.
        if (rectTransform.anchoredPosition.x > screenWidth / 2f)
        {
            moveDirection = -1;
        }
        // 현재 이미지 위치가 화면의 왼쪽 끝에 가까워지면 방향을 오른쪽으로 바꿉니다.
        else if (rectTransform.anchoredPosition.x < -screenWidth / 2f)
        {
            moveDirection = 1;
        }

        // 현재 방향과 속도에 따라 배경을 이동시킵니다.
        rectTransform.anchoredPosition += new Vector2(scrollSpeed * moveDirection * Time.deltaTime, 0);
    }
}