using UnityEngine;
using System.Collections.Generic;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenuUI;
    public GameObject optionsMenuUI;
    public GameObject shopMenuUI;

    private List<GameObject> otherMenus;

    void Start()
    {
        otherMenus = new List<GameObject> { shopMenuUI }; // add any future menus here

        mainMenuUI.SetActive(true);
        optionsMenuUI.SetActive(false);
        shopMenuUI.SetActive(false);
        Pause();
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

            // Otherwise handle the options menu
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
    }

    public void OpenShopMenu()
    {
        shopMenuUI.SetActive(true);
        Pause();
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

    public void StartGame()
    {
        mainMenuUI.SetActive(false);
        Resume();
    }

    public void OpenOptionsMenu()
    {
        optionsMenuUI.SetActive(true);
    }

    public void CloseMenu()
    {
        optionsMenuUI.SetActive(false);
        shopMenuUI.SetActive(false);
    }

    public void BackToMainMenu()
    {
        mainMenuUI.SetActive(true);
        optionsMenuUI.SetActive(false);
        Pause();
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