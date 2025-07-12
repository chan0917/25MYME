using UnityEngine;

public class CursorManager : MonoBehaviour
{
    [Header("Ŀ�� �ؽ�ó")]
    public Texture2D normalCursor; // �⺻ ���콺 Ŀ��
    public Texture2D aimCursor;    // ���� ���콺 Ŀ��

    [Header("Ŀ�� �ֽ��� (Ŭ�� ����)")]
    public Vector2 normalHotspot = Vector2.zero;     // �⺻ Ŀ���� Ŭ�� ����
    public Vector2 aimHotspot = new Vector2(16, 16); // ���� Ŀ���� Ŭ�� ���� (���� �߾�)

    [Header("� ���� ����")]
    public string meteoriteComponentName = "Meteorite"; // � ������Ʈ �̸�
    public LayerMask raycastLayers = -1; // ����ĳ��Ʈ�� ���̾� (��� ���̾�)

    [Header("���η����� ���� ����")]
    public Animator targetLineRenderer; // ������ ������ ���η�����



    private Camera mainCamera;
    private bool isAiming = false;


    void Start()
    {
        // ���� ī�޶� ã��
        mainCamera = Camera.main;
        if (mainCamera == null)
            mainCamera = FindObjectOfType<Camera>();

        // �⺻ Ŀ���� ����
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
        // ���콺 ��ġ���� ����ĳ��Ʈ
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        bool hitMeteorite = false;

        // ����ĳ��Ʈ�� ������Ʈ ����
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, raycastLayers))
        {
            // � ������Ʈ�� �ִ��� Ȯ��
            if (HasMeteoriteComponent(hit.collider.gameObject))
            {
                hitMeteorite = true;
            }
        }

        // Ŀ�� ���� ����
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
        // Ư�� ������Ʈ �̸����� Ȯ��
        Component component = obj.GetComponent(meteoriteComponentName);
        if (component != null)
            return true;

        
        if (obj.GetComponent<Asteroid>())
            return true;

        // �Ǵ� Ư�� ��ũ��Ʈ Ÿ������ Ȯ���ϴ� ��� (����)
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

    // ���� ���� �� ������ ���콺 �ٽ� ���̱�
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

    // ���� ���� �� ����
    void OnDestroy()
    {
        // �⺻ ������ Ŀ���� ����
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        Cursor.visible = true;
    }

    // �������� Ŀ�� �����ϴ� �Լ���
    public void ForceNormalCursor()
    {
        SetNormalCursor();
    }

    public void ForceAimCursor()
    {
        SetAimCursor();
    }

    // � ������Ʈ �̸� ����
    public void SetMeteoriteComponentName(string componentName)
    {
        meteoriteComponentName = componentName;
    }
}