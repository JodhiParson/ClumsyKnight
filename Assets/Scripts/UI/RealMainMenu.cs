using UnityEngine;
using UnityEngine.SceneManagement;

public class RealMainMenu : MonoBehaviour
{
    public GameObject optionsMenu;
    
    public void Start()
    {
        optionsMenu.SetActive(false);
    }
    public void StartGame()
    {
        SceneManager.LoadScene("GameScene");
    }
    public void ToggleOptions()
    {
        optionsMenu.SetActive(!optionsMenu.activeSelf);
    }
        public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

}
