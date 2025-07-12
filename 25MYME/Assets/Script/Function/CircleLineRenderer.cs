using UnityEngine;

public class CircleLineRenderer : MonoBehaviour
{
    [Header("원 설정")]
    public GrappleArmSystem armSystem;
    public float radius = 5f;
    public int segments = 50; // 원의 부드러움 (많을수록 부드러움)
    public Material lineMaterial; // 선 재질

    [Header("움직임 설정")]
    public bool followObject = true; // 오브젝트를 따라 움직일지 여부
    public bool useLocalSpace = true; // 로컬 좌표 사용 여부
    public bool updateEveryFrame = false; // 매 프레임 업데이트 (성능 주의)

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
        // 위치나 크기가 변경되었을 때만 업데이트 (성능 최적화)
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
                CreateCircle(); // 크기가 변경되면 재생성
            }

            lastPosition = transform.position;
            lastRadius = radius;
        }
    }

    void CreateCircle()
    {
        // LineRenderer 컴포넌트 추가
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
            lineRenderer = gameObject.AddComponent<LineRenderer>();

        // LineRenderer 설정
        lineRenderer.material = lineMaterial;
        //lineRenderer.color = Color.red;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.positionCount = segments + 1;
        lineRenderer.useWorldSpace = !useLocalSpace; // 중요: 로컬/월드 공간 설정
        lineRenderer.loop = true; // 원형으로 연결

        // 원의 점들 계산
        Vector3[] positions = new Vector3[segments + 1];
        for (int i = 0; i <= segments; i++)
        {
            float angle = 2f * Mathf.PI * i / segments;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;

            if (useLocalSpace)
            {
                // 로컬 좌표 사용 - 오브젝트와 함께 자동으로 움직임
                positions[i] = new Vector3(x, 0, z);
            }
            else
            {
                // 월드 좌표 사용 - 수동으로 위치 계산
                positions[i] = transform.position + new Vector3(x, 0, z);
            }
        }

        lineRenderer.SetPositions(positions);
    }

    void UpdateCirclePosition()
    {
        if (!useLocalSpace && lineRenderer != null)
        {
            // 월드 좌표를 사용할 때만 수동으로 업데이트
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

    // 런타임에 원 크기 변경
    public void SetRadius(float newRadius)
    {
        radius = newRadius;
        CreateCircle();
    }

    // 로컬/월드 좌표 변경
    public void SetUseLocalSpace(bool useLocal)
    {
        useLocalSpace = useLocal;
        CreateCircle();
    }
}