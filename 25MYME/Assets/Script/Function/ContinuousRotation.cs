using UnityEngine;

public class ContinuousRotation : MonoBehaviour
{
    [Header("Rotation Speed")]
    public Vector3 rotationSpeed = new Vector3(0, 90, 0); // X, Y, Z 축 회전 속도 (도/초)

    void Update()
    {
        // 각 축별로 회전 적용
        transform.Rotate(rotationSpeed * Time.deltaTime);

        // 또는 한 축만 회전하고 싶다면:
        // transform.Rotate(0, rotationSpeed.y * Time.deltaTime, 0); // Y축만
    }
}
