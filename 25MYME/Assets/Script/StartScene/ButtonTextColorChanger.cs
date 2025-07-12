using UnityEngine;
using UnityEngine.EventSystems; // 마우스 이벤트를 사용하기 위해 필요합니다.
using TMPro; // TextMeshPro를 사용하기 위해 필요합니다.

public class ButtonTextColorChanger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // 인스펙터에서 텍스트 컴포넌트를 연결할 변수
    public TextMeshProUGUI buttonText;

    // 마우스가 버튼 위로 올라왔을 때 호출됩니다.
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 텍스트 색상을 노란색으로 변경합니다.
        buttonText.color = Color.yellow;
    }

    // 마우스가 버튼에서 벗어났을 때 호출됩니다.
    public void OnPointerExit(PointerEventData eventData)
    {
        // 텍스트 색상을 흰색으로 되돌립니다.
        buttonText.color = Color.white;
    }
}