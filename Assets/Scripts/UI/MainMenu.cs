using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenuUI;
    public GameObject optionsMenuUI;
    void Start()
    {
        mainMenuUI.SetActive(true);
        optionsMenuUI.SetActive(false);
        Pause();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
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

    void Pause()
    {
        Time.timeScale = 0f;
    }
}
