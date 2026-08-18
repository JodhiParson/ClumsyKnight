using UnityEngine;
using UnityEngine.SceneManagement; // needed for SceneManager.LoadScene later

public class dungeonEntrance : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene("DungeonScene");
        }
    }
}