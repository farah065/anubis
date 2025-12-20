using UnityEngine;

public class Billboard : MonoBehaviour
{

    private Camera mainCamera;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 lookAtPos = mainCamera.transform.position;
        lookAtPos = new Vector3(-lookAtPos.x, -lookAtPos.y, -lookAtPos.z);
        transform.LookAt(lookAtPos);
       
    }
}
