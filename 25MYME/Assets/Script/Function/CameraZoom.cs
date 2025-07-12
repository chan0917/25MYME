using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    private Camera cam;
    public float zoomSpeed = 2f;
    public float minFOV = 20f;
    public float maxFOV = 80f;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        //float scroll = Input.GetAxis("Mouse ScrollWheel");
        //float targetFOV = cam.fieldOfView - scroll * zoomSpeed;
        //cam.fieldOfView = Mathf.Clamp(targetFOV, minFOV, maxFOV);
    }
}