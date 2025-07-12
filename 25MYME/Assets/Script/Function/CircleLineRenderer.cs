using UnityEngine;

public class CircleLineRenderer : MonoBehaviour
{
    [Header("�� ����")]
    public GrappleArmSystem armSystem;
    public float radius = 5f;
    public int segments = 50; // ���� �ε巯�� (�������� �ε巯��)
    public Material lineMaterial; // �� ����

    [Header("������ ����")]
    public bool followObject = true; // ������Ʈ�� ���� �������� ����
    public bool useLocalSpace = true; // ���� ��ǥ ��� ����
    public bool updateEveryFrame = false; // �� ������ ������Ʈ (���� ����)

    private LineRenderer lineRenderer;
    private Vector3 lastPosition;
    private float lastRadius;

    void Start()
    {
        armSystem = FindAnyObjectByType<GrappleArmSystem>();
        radius = armSystem.settings.maxRange;

        CreateCircle();
        lastPosition = transform.position;
        lastRadius = radius;
    }

    void Update()
    {
        // ��ġ�� ũ�Ⱑ ����Ǿ��� ���� ������Ʈ (���� ����ȭ)
        if (updateEveryFrame ||
            Vector3.Distance(lastPosition, transform.position) > 0.01f ||
            Mathf.Abs(lastRadius - radius) > 0.01f)
        {
            if (!useLocalSpace && !updateEveryFrame)
            {
                UpdateCirclePosition();
            }

            if (Mathf.Abs(lastRadius - radius) > 0.01f)
            {
                CreateCircle(); // ũ�Ⱑ ����Ǹ� �����
            }

            lastPosition = transform.position;
            lastRadius = radius;
        }
    }

    void CreateCircle()
    {
        // LineRenderer ������Ʈ �߰�
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
            lineRenderer = gameObject.AddComponent<LineRenderer>();

        // LineRenderer ����
        lineRenderer.material = lineMaterial;
        //lineRenderer.color = Color.red;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.positionCount = segments + 1;
        lineRenderer.useWorldSpace = !useLocalSpace; // �߿�: ����/���� ���� ����
        lineRenderer.loop = true; // �������� ����

        // ���� ���� ���
        Vector3[] positions = new Vector3[segments + 1];
        for (int i = 0; i <= segments; i++)
        {
            float angle = 2f * Mathf.PI * i / segments;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;

            if (useLocalSpace)
            {
                // ���� ��ǥ ��� - ������Ʈ�� �Բ� �ڵ����� ������
                positions[i] = new Vector3(x, 0, z);
            }
            else
            {
                // ���� ��ǥ ��� - �������� ��ġ ���
                positions[i] = transform.position + new Vector3(x, 0, z);
            }
        }

        lineRenderer.SetPositions(positions);
    }

    void UpdateCirclePosition()
    {
        if (!useLocalSpace && lineRenderer != null)
        {
            // ���� ��ǥ�� ����� ���� �������� ������Ʈ
            Vector3[] positions = new Vector3[segments + 1];
            for (int i = 0; i <= segments; i++)
            {
                float angle = 2f * Mathf.PI * i / segments;
                float x = Mathf.Cos(angle) * radius;
                float z = Mathf.Sin(angle) * radius;
                positions[i] = transform.position + new Vector3(x, 0, z);
            }
            lineRenderer.SetPositions(positions);
        }
    }

    // ��Ÿ�ӿ� �� ũ�� ����
    public void SetRadius(float newRadius)
    {
        radius = newRadius;
        CreateCircle();
    }

    // ����/���� ��ǥ ����
    public void SetUseLocalSpace(bool useLocal)
    {
        useLocalSpace = useLocal;
        CreateCircle();
    }
}