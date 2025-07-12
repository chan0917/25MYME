using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target; // ���� ���
    public float followSpeed = 5f;
    public float rotationSpeed = 3f;

    void FixedUpdate()
    {
        // ��ġ ����ȭ
        Vector3 targetPosition = target.position;
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);

        // ȸ�� ����ȭ
        transform.rotation = target.rotation;
    }
}
