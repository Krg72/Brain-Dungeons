using UnityEngine;

public class ClickVisual : MonoBehaviour
{
    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        //Vector3 worldPos = cam.ScreenToWorldPoint(Input.mousePosition);
        //worldPos.z = 0f;
        //transform.position = worldPos;
    }
}
