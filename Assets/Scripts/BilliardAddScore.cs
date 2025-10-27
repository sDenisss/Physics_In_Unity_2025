using UnityEngine;
using TMPro;

public class BilliardAddScore : MonoBehaviour
{
    // Общий счет для всех лунок
    private static int count = 0;
    public TMP_Text countString;
    public string ballTag = "BilliardBall";
    public string cueBallTag = "CueBall";
    public int pointsPerBall = 1;
    public BilliardBallController billiardBallController;


    void Start()
    {
        // Проверяем что коллайдер настроен как триггер
        Collider collider = GetComponent<Collider>();
        if (collider != null && !collider.isTrigger)
        {
            collider.isTrigger = true;
        }
    }

    private void AddScore()
    {
        count += pointsPerBall;
        UpdateScoreText();
    }

    private void UpdateScoreText()
    {
        if (countString != null)
        {
            countString.text = $"Score: {count}";
        }
    }

    private void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag(cueBallTag))
        {
            // AddScore();
            billiardBallController.RespawnCueBall();
            // НЕ УНИЧТОЖАЕМ биток, просто перемещаем
        }
        else if (col.CompareTag(ballTag))
        {
            AddScore();
            Destroy(col.gameObject);
        }
    }
}