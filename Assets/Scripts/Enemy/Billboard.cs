using UnityEngine;

public class Billboard : MonoBehaviour
{
    public enum Mode { FullFace, YAxisOnly }
    public Mode mode = Mode.YAxisOnly; // Y-axis is usually best for characters standing on ground

    Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void LateUpdate()
    {
        if (cam == null) return;

        if (mode == Mode.FullFace)
        {
            // faces camera exactly, on all axes (good for particles/icons)
            transform.rotation = cam.transform.rotation;
        }
        else
        {
            // only rotates around Y - keeps sprite upright, just turns to face camera left/right
            Vector3 dir = transform.position - cam.transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(dir);
            }
        }
    }
}