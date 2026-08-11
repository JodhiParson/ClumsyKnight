using UnityEngine;

public class AugmentUI : MonoBehaviour
{
    public static AugmentUI Instance { get; private set; }

    public GameObject augmentUIPanel;

    private void Awake()
    {
        Instance = this;
    }

    public void ToggleAugmentUI()
    {
        augmentUIPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void CloseAugmentUI()
    {
        augmentUIPanel.SetActive(false);
        Time.timeScale = 1f;
    }
}