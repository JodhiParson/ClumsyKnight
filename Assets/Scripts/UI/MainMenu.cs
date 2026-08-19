using UnityEngine;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;

public class MainMenu : MonoBehaviour
{
    public GameObject optionsMenuUI;
    public GameObject armoryUI;
    public GameObject HUD;

    private List<GameObject> otherMenus;
    public PlayerControls playerControls;

    void Start()
    {
        otherMenus = new List<GameObject> { optionsMenuUI, armoryUI }; // add any future menus here

        optionsMenuUI.SetActive(false);
        armoryUI.SetActive(false);
        HUD.SetActive(true);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Close any "other" menu first (shop, inventory, etc.)
            if (CloseAnyOpenMenu())
            {
                Resume();
                Debug.Log("Closed other menu, resuming game");
                return;
            }

            if (optionsMenuUI.activeSelf)
            {
                optionsMenuUI.SetActive(false);
                Resume();
                Debug.Log("Resuming game");
            }
            else
            {
                optionsMenuUI.SetActive(true);
                Pause();
                Debug.Log("Pausing game");
            }
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            OpenArmory();
            Pause();
        }
    }


    bool CloseAnyOpenMenu()
    {
        bool closedSomething = false;
        foreach (GameObject menu in otherMenus)
        {
            if (menu != null && menu.activeSelf)
            {
                menu.SetActive(false);
                closedSomething = true;
            }
        }
        return closedSomething;
    }


    public void OpenOptionsMenu()
    {
        optionsMenuUI.SetActive(true);
    }
    public void OpenArmory()
    {
        armoryUI.SetActive(true);
    }

    public void CloseMenu()
    {
        optionsMenuUI.SetActive(false);
        armoryUI.SetActive(false);
    }


    public void SetWindowedMode()
    {
        Screen.fullScreenMode = FullScreenMode.Windowed;
    }

    public void SetFullscreenMode()
    {
        Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void Resume()
    {
        Time.timeScale = 1f;
    }

    public void Pause()
    {
        Time.timeScale = 0f;
    }
}