using UnityEngine;

[ExecuteAlways] // Optional: to see it in edit mode too
public class CanvasFaceCamera : MonoBehaviour
{
    public Camera targetCamera;

    [Header("Axis Lock")]
    public bool faceX = true;
    public bool faceY = true;
    public bool faceZ = true;

    void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            if (targetCamera == null) return;
        }

        // Compute the full look rotation toward the camera
        Vector3 directionToCamera = transform.position - targetCamera.transform.position;

        if (directionToCamera == Vector3.zero) return;

        Quaternion lookRotation = Quaternion.LookRotation(directionToCamera.normalized, Vector3.up);

        // Extract Euler angles
        Vector3 desiredEuler = lookRotation.eulerAngles;
        Vector3 currentEuler = transform.rotation.eulerAngles;

        // Preserve axes not selected
        if (!faceX) desiredEuler.x = currentEuler.x;
        if (!faceY) desiredEuler.y = currentEuler.y;
        if (!faceZ) desiredEuler.z = currentEuler.z;

        // Apply back the new rotation
        transform.rotation = Quaternion.Euler(desiredEuler);

        //transform.rotation = Quaternion.Euler(new Vector3(90, 0, 0));
    }
}
