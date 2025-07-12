using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonImageChanger : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Sprite normalImage;      // �⺻ �̹���
    public Sprite pressedImage;     // ������ �� �̹���

    private Image buttonImage;

    void Start()
    {
        buttonImage = GetComponent<Image>();

        // �⺻ �̹����� �������� �ʾҴٸ� ���� �̹����� ���
        if (normalImage == null)
            normalImage = buttonImage.sprite;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // ��ư�� ������ ����
        buttonImage.sprite = pressedImage;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // ��ư�� ���� ����
        buttonImage.sprite = normalImage;
    }
}