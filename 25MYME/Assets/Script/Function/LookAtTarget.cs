using UnityEngine;

public class LookAtTarget : MonoBehaviour
{
    public Transform player;

    public Transform target;

    void Update()
    {
        transform.position = player.position;

        transform.LookAt(target);
    }
}
