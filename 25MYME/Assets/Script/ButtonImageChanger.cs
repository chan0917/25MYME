using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonImageChanger : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Sprite normalImage;      // 기본 이미지
    public Sprite pressedImage;     // 눌렸을 때 이미지

    private Image buttonImage;

    void Start()
    {
        buttonImage = GetComponent<Image>();

        // 기본 이미지가 설정되지 않았다면 현재 이미지를 사용
        if (normalImage == null)
            normalImage = buttonImage.sprite;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // 버튼을 누르는 순간
        buttonImage.sprite = pressedImage;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // 버튼을 떼는 순간
        buttonImage.sprite = normalImage;
    }
}