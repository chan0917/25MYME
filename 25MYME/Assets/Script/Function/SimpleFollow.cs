using UnityEngine;

public class SimpleFollow : MonoBehaviour
{
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;

    public Transform target;
    public float speed = 2f;

    [Header("Rotation Speed")]
    public Vector3 rotationSpeed = new Vector3(0, 90, 0);
    void Update()
    {
        if (target != null)
        {
            float rotationInput = 0f;

            if (Input.GetKey(leftKey))
                rotationInput = -1f;
            else if (Input.GetKey(rightKey))
                rotationInput = 1f;


            Quaternion goalRot = Quaternion.Euler(transform.rotation.x, transform.rotation.y, transform.rotation.z + rotationInput);

            transform.position = Vector3.Lerp(transform.position, target.position, speed * 3 * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, goalRot, speed * Time.deltaTime);
        }
    }
}
