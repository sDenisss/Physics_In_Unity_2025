using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BilliardBallController : MonoBehaviour
{
    public Transform spawnPoint;
    private string cueBallTag = "CueBall";

    public void RespawnCueBall()
    {
        // Всегда ищем биток по тегу, а не храним ссылку
        GameObject cueBall = GameObject.FindWithTag(cueBallTag);
        
        if (cueBall != null)
        {
            // Перемещаем существующий биток
            cueBall.transform.position = spawnPoint.position;
            
            Rigidbody rb = cueBall.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
        else
        {
            Debug.LogError("CueBall not found with tag: " + cueBallTag);
        }
    }
}