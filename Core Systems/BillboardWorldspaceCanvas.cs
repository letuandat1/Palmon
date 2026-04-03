using UnityEngine;

public class BillboardWorldspaceCanvas : MonoBehaviour
{
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = CursorController.Instance.GetMainCamera();
    }

    private void LateUpdate()
    {
        if (mainCamera == null) return;

        // Make the canvas face the camera
        Vector3 directionToCamera = transform.position - mainCamera.transform.position;
        directionToCamera.y = 0; // Keep only horizontal rotation
        if (directionToCamera.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToCamera);
            transform.rotation = targetRotation;
        }
    }
}