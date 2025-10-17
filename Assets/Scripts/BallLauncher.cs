using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BallLauncher : MonoBehaviour
{
    [Header("References")]
    public GameObject ballPrefab;
    public Transform spawnPoint;
    public LineRenderer trajectoryLine;
    
    [Header("Launch Settings")]
    public float currentSpeed = 10f;
    public float currentAngle = 45f;
    
    [Header("UI References")]
    public TMP_InputField speedInputField;
    public TMP_InputField angleInputField;
    public Button startButton;
    public Button restartButton;

    private GameObject currentBall;
    private Rigidbody currentBallRigidbody;
    private bool isBallLaunched = false;

    void Start()
    {
        // Проверяем и настраиваем Line Renderer
        SetupTrajectoryLine();
        
        // Находим UI элементы
        FindUIElements();
        
        // Устанавливаем начальные значения в UI
        speedInputField.text = currentSpeed.ToString();
        angleInputField.text = currentAngle.ToString();
        
        // Подписываемся на события
        startButton.onClick.AddListener(OnStartButtonClick);
        restartButton.onClick.AddListener(OnRestartButtonClick);
        speedInputField.onValueChanged.AddListener(OnSpeedInputChanged);
        angleInputField.onValueChanged.AddListener(OnAngleInputChanged);
        
        // Создаем начальный шар
        SpawnNewBall();
        
        // Принудительно обновляем траекторию
        ForceUpdateTrajectory();
    }

    void SetupTrajectoryLine()
    {
        if (trajectoryLine == null)
        {
            return;
        }
        
        // Принудительная настройка Line Renderer
        trajectoryLine.positionCount = 50;
        trajectoryLine.startWidth = 0.05f;
        trajectoryLine.endWidth = 0.05f;
        trajectoryLine.useWorldSpace = true;
        
        // Создаем материал если его нет
        if (trajectoryLine.material == null)
        {
            Material newMaterial = new Material(Shader.Find("Unlit/Color"));
            newMaterial.color = Color.red;
            trajectoryLine.material = newMaterial;
        }
        
        trajectoryLine.startColor = Color.red;
        trajectoryLine.endColor = Color.red;
        
        trajectoryLine.enabled = true;
    }

    void FindUIElements()
    {
        if (speedInputField == null)
        {
            GameObject speedInput = GameObject.Find("Speed_Input");
            if (speedInput != null) speedInputField = speedInput.GetComponent<TMP_InputField>();
        }
        
        if (angleInputField == null)
        {
            GameObject angleInput = GameObject.Find("Angle_Input");
            if (angleInput != null) angleInputField = angleInput.GetComponent<TMP_InputField>();
        }
        
    }

    void Update()
    {
        // Постоянно обновляем траекторию, пока шар не запущен
        if (!isBallLaunched)
        {
            UpdateTrajectoryPreview();
        }
    }

    void SpawnNewBall()
    {
        // Удаляем старый шар
        if (currentBall != null)
        {
            Destroy(currentBall);
        }

        // Создаем новый шар
        currentBall = Instantiate(ballPrefab, spawnPoint.position, Quaternion.identity);
        currentBallRigidbody = currentBall.GetComponent<Rigidbody>();
        
        if (currentBallRigidbody == null)
        {
            currentBallRigidbody = currentBall.AddComponent<Rigidbody>();
        }
        
        currentBallRigidbody.isKinematic = true;
        isBallLaunched = false;
        
        // Включаем и обновляем траекторию
        trajectoryLine.enabled = true;
    }

    void ForceUpdateTrajectory()
    {
        // Принудительное обновление траектории
        trajectoryLine.enabled = true;
        UpdateTrajectoryPreview();
    }

    void UpdateTrajectoryPreview()
    {
        if (trajectoryLine == null || !trajectoryLine.enabled) return;
        
        // Рассчитываем направление полета
        float angleInRadians = currentAngle * Mathf.Deg2Rad;
        Vector3 startVelocity = new Vector3(
            Mathf.Cos(angleInRadians) * currentSpeed, 
            Mathf.Sin(angleInRadians) * currentSpeed, 
            0
        );

        // Настройка точек траектории
        int pointsCount = 50;
        trajectoryLine.positionCount = pointsCount;

        Vector3 startPosition = spawnPoint.position; // Используем позицию спавна

        // Рассчитываем позиции для каждой точки
        for (int i = 0; i < pointsCount; i++)
        {
            float simulationTime = i * 0.1f;
            
            // Формула движения: S = V₀t + (at²)/2
            Vector3 displacement = startVelocity * simulationTime + 
                                  Physics.gravity * simulationTime * simulationTime * 0.5f;
            
            Vector3 pointPosition = startPosition + displacement;
            trajectoryLine.SetPosition(i, pointPosition);
        }
    }

    public void OnSpeedInputChanged(string newValue)
    {
        if (float.TryParse(newValue, out float result) && result > 0)
        {
            currentSpeed = result;
        }
        else
        {
            speedInputField.text = currentSpeed.ToString();
        }
    }

    public void OnAngleInputChanged(string newValue)
    {
        if (float.TryParse(newValue, out float result))
        {
            currentAngle = Mathf.Clamp(result, 0, 90);
            angleInputField.text = currentAngle.ToString();
        }
        else
        {
            angleInputField.text = currentAngle.ToString();
        }
    }

    void OnStartButtonClick()
    {
        if (isBallLaunched || currentBallRigidbody == null) return;

        // Обновляем значения из полей ввода
        OnSpeedInputChanged(speedInputField.text);
        OnAngleInputChanged(angleInputField.text);

        // Запускаем шар
        LaunchCurrentBall();
    }

    void LaunchCurrentBall()
    {
        currentBallRigidbody.isKinematic = false;
        float angleInRadians = currentAngle * Mathf.Deg2Rad;
        Vector3 launchDirection = new Vector3(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians), 0);
        currentBallRigidbody.AddForce(launchDirection * currentSpeed, ForceMode.VelocityChange);

        // Отключаем траекторию после запуска
        trajectoryLine.enabled = false;
        isBallLaunched = true;
    }

    void OnRestartButtonClick()
    {
        SpawnNewBall();
        ForceUpdateTrajectory();
    }
}
