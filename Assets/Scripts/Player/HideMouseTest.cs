using UnityEngine;

public class HideMouseTest : MonoBehaviour
{
    [Header("Cursor Settings")]
    public bool hideCursor = true;

    void Update()
    {
        Cursor.visible = !hideCursor;
        
        if (hideCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }
}