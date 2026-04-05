using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        // Objenin her saniye kameraya doðru dönmesini saðlar
        transform.forward = mainCamera.transform.forward;
    }
}