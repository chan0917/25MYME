using UnityEngine;

public class CursorManager : MonoBehaviour
{
    [Header("커서 텍스처")]
    public Texture2D normalCursor; // 기본 마우스 커서
    public Texture2D aimCursor;    // 조준 마우스 커서

    [Header("커서 핫스팟 (클릭 지점)")]
    public Vector2 normalHotspot = Vector2.zero;     // 기본 커서의 클릭 지점
    public Vector2 aimHotspot = new Vector2(16, 16); // 조준 커서의 클릭 지점 (보통 중앙)

    [Header("운석 감지 설정")]
    public string meteoriteComponentName = "Meteorite"; // 운석 컴포넌트 이름
    public LayerMask raycastLayers = -1; // 레이캐스트할 레이어 (모든 레이어)

    [Header("라인렌더러 투명도 설정")]
    public Animator targetLineRenderer; // 투명도를 변경할 라인렌더러



    private Camera mainCamera;
    private bool isAiming = false;


    void Start()
    {
        // 메인 카메라 찾기
        mainCamera = Camera.main;
        if (mainCamera == null)
            mainCamera = FindObjectOfType<Camera>();

        // 기본 커서로 설정
        SetNormalCursor();
    }

    void Update()
    {
        CheckMouseTarget();

        if (isAiming)
        {
            targetLineRenderer.Play("LineOn");
            //float alpha = targetLineRenderer.material.color.a;
            //if (alpha <= maxAlpha)
            //{
            //    alpha += Time.deltaTime * fadeSpeed;
            //    targetLineRenderer.materials[0].color = new Color(255, 255, 255, alpha);
            //}
        }
        else
        {
            targetLineRenderer.Play("LineOff");
            //float alpha = targetLineRenderer.material.color.a;
            //if (alpha >= 0)
            //{
            //    alpha -= Time.deltaTime * fadeSpeed;
            //    targetLineRenderer.materials[0].color = new Color(255, 255, 255, alpha);
            //}
        }
    }


    void CheckMouseTarget()
    {
        // 마우스 위치에서 레이캐스트
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        bool hitMeteorite = false;

        // 레이캐스트로 오브젝트 감지
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, raycastLayers))
        {
            // 운석 컴포넌트가 있는지 확인
            if (HasMeteoriteComponent(hit.collider.gameObject))
            {
                hitMeteorite = true;
            }
        }

        // 커서 상태 변경
        if (hitMeteorite && !isAiming)
        {
            SetAimCursor();
        }
        else if (!hitMeteorite && isAiming)
        {
            SetNormalCursor();
        }
    }



    bool HasMeteoriteComponent(GameObject obj)
    {
        // 특정 컴포넌트 이름으로 확인
        Component component = obj.GetComponent(meteoriteComponentName);
        if (component != null)
            return true;

        
        if (obj.GetComponent<Asteroid>())
            return true;

        // 또는 특정 스크립트 타입으로 확인하는 방법 (예시)
        // if (obj.GetComponent<MeteoriteScript>() != null)
        //     return true;

        return false;
    }

    void SetNormalCursor()
    {
        Cursor.SetCursor(normalCursor, normalHotspot, CursorMode.Auto);
        isAiming = false;
    }

    void SetAimCursor()
    {
        Cursor.SetCursor(aimCursor, aimHotspot, CursorMode.Auto);
        isAiming = true;
    }

    // 게임 종료 시 윈도우 마우스 다시 보이기
    //void OnApplicationFocus(bool hasFocus)
    //{
    //    if (hasFocus)
    //    {
    //        Cursor.visible = false;
    //    }
    //    else
    //    {
    //        Cursor.visible = true;
    //    }
    //}

    //void OnApplicationPause(bool pauseStatus)
    //{
    //    if (pauseStatus)
    //    {
    //        Cursor.visible = true;
    //    }
    //    else
    //    {
    //        Cursor.visible = false;
    //    }
    //}

    // 게임 종료 시 정리
    void OnDestroy()
    {
        // 기본 윈도우 커서로 복원
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        Cursor.visible = true;
    }

    // 수동으로 커서 변경하는 함수들
    public void ForceNormalCursor()
    {
        SetNormalCursor();
    }

    public void ForceAimCursor()
    {
        SetAimCursor();
    }

    // 운석 컴포넌트 이름 변경
    public void SetMeteoriteComponentName(string componentName)
    {
        meteoriteComponentName = componentName;
    }
}