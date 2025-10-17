using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishTrigger : MonoBehaviour
{
    public GameObject panelVictory;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Убедись, что у игрока тег "Player"
        {
            Debug.Log("Игра завершена.");
            OnWin();
        }
    }

    void OnWin()
    {
        panelVictory.SetActive(true);
    }
}
