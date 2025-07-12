using UnityEngine;
using UnityEngine.EventSystems; // ���콺 �̺�Ʈ�� ����ϱ� ���� �ʿ��մϴ�.
using TMPro; // TextMeshPro�� ����ϱ� ���� �ʿ��մϴ�.

public class ButtonTextColorChanger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // �ν����Ϳ��� �ؽ�Ʈ ������Ʈ�� ������ ����
    public TextMeshProUGUI buttonText;

    // ���콺�� ��ư ���� �ö���� �� ȣ��˴ϴ�.
    public void OnPointerEnter(PointerEventData eventData)
    {
        // �ؽ�Ʈ ������ ��������� �����մϴ�.
        buttonText.color = Color.yellow;
    }

    // ���콺�� ��ư���� ����� �� ȣ��˴ϴ�.
    public void OnPointerExit(PointerEventData eventData)
    {
        // �ؽ�Ʈ ������ ������� �ǵ����ϴ�.
        buttonText.color = Color.white;
    }
}