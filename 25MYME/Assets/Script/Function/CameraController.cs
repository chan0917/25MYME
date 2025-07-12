using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target; // 따라갈 대상
    public float followSpeed = 5f;
    public float rotationSpeed = 3f;

    void FixedUpdate()
    {
        // 위치 동기화
        Vector3 targetPosition = target.position;
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);

        // 회전 동기화
        transform.rotation = target.rotation;
    }
}
