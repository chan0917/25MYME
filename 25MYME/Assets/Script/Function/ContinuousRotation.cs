using UnityEngine;

public class ContinuousRotation : MonoBehaviour
{
    [Header("Rotation Speed")]
    public Vector3 rotationSpeed = new Vector3(0, 90, 0); // X, Y, Z �� ȸ�� �ӵ� (��/��)

    void Update()
    {
        // �� �ະ�� ȸ�� ����
        transform.Rotate(rotationSpeed * Time.deltaTime);

        // �Ǵ� �� �ุ ȸ���ϰ� �ʹٸ�:
        // transform.Rotate(0, rotationSpeed.y * Time.deltaTime, 0); // Y�ุ
    }
}
