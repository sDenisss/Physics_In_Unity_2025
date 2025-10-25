using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PushCube : MonoBehaviour
{
    [Header("Physics Parameters")]
    public float mass = 1.5f;
    public float staticFrictionCoefficient = 0.3f;
    public float kineticFrictionCoefficient = 0.1f;
    public float appliedForce = 0f;
    
    [Header("References")]
    public Rigidbody objectRb;
    public Transform startPoint;
    public Transform finishPoint;
    public TMP_Text forceText;
    public TMP_Text statusText;
    public TMP_Text distanceText;

    [Header("UI Inputs")]
    public TMP_InputField frictionCoefInput;
    public TMP_InputField cubesMassInput;      
    public TMP_InputField playerForceInput;
    public Button applyParamsButton;
    public Button restartButton;

    [Header("Finish UI")]
    public GameObject finishPanel; // Панель победы
    public TMP_Text resultsText;   // Текст результатов
    
    // Расчетные величины
    private float gravity = 9.81f;
    private float normalForce;
    private float maxStaticFriction;
    private float kineticFriction;
    private bool isMoving = false;
    private Vector3 initialPosition;
    
    // Для отслеживания достижения финиша
    private bool reachedFinish = false;
    private float finishDistance = 4f;

    void Start()
    {
        initialPosition = objectRb.position;
        RecalculateForces();

        // Устанавливаем начальные значения в UI
        frictionCoefInput.text = staticFrictionCoefficient.ToString("F2");
        cubesMassInput.text = mass.ToString("F2");
        playerForceInput.text = appliedForce.ToString("F2");
        
        // Назначаем обработчики кнопок
        applyParamsButton.onClick.AddListener(ApplyParameters);
        restartButton.onClick.AddListener(ResetObject);
        
        // Скрываем панель победы
        if (finishPanel != null)
            finishPanel.SetActive(false);
        
        UpdateUI();
    }
    
    void FixedUpdate()
    {
        // ПРОВЕРКА ДВИЖЕНИЯ - ФИКСИРУЕМ БАГ
        if (!isMoving && appliedForce > maxStaticFriction)
        {
            StartMovement();
        }
        
        // Если объект движется, применяем силу трения
        if (isMoving)
        {
            ApplyKineticFriction();
            
            // Проверяем остановку только если сила меньше трения
            if (objectRb.linearVelocity.magnitude < 0.1f && appliedForce <= kineticFriction)
            {
                StopMovement();
            }
            
            // Проверяем достижение финиша
            CheckFinish();
        }
        
        UpdateUI();
    }
    
    public void ApplyParameters()
    {
        // Обновляем массу
        if (float.TryParse(cubesMassInput.text, out float newMass) && newMass > 0)
        {
            mass = newMass;
            objectRb.mass = mass;
        }
        
        // Обновляем коэффициенты трения
        if (float.TryParse(frictionCoefInput.text, out float newFriction) && newFriction >= 0)
        {
            staticFrictionCoefficient = newFriction;
            kineticFrictionCoefficient = newFriction * 0.7f; // Кинетическое всегда меньше
        }
        
        // Обновляем приложенную силу
        if (float.TryParse(playerForceInput.text, out float newForce) && newForce >= 0)
        {
            appliedForce = newForce;
        }
        
        RecalculateForces();
        Debug.Log($"Параметры обновлены: Масса={mass}, Трение={staticFrictionCoefficient}, Сила={appliedForce}");
    }
    
    public void SetAppliedForce(float force)
    {
        appliedForce = force;
        playerForceInput.text = force.ToString("F2");
        
        // СРАЗУ проверяем, можно ли начать движение
        if (!isMoving && appliedForce > maxStaticFriction)
        {
            StartMovement();
        }
    }
    
    public void SetMass(float newMass)
    {
        mass = newMass;
        objectRb.mass = mass;
        cubesMassInput.text = mass.ToString("F2");
        RecalculateForces();
    }
    
    public void SetStaticFriction(float friction)
    {
        staticFrictionCoefficient = friction;
        frictionCoefInput.text = friction.ToString("F2");
        RecalculateForces();
    }
    
    private void RecalculateForces()
    {
        normalForce = mass * gravity;
        maxStaticFriction = staticFrictionCoefficient * normalForce;
        kineticFriction = kineticFrictionCoefficient * normalForce;
    }
    
    private void StartMovement()
    {
        isMoving = true;
        reachedFinish = false;
        
        // Очищаем предыдущие силы
        objectRb.linearVelocity = Vector3.zero;
        
        // Прикладываем избыточную силу
        float netForce = appliedForce - kineticFriction;
        if (netForce > 0)
        {
            objectRb.AddForce(Vector3.right * netForce, ForceMode.Force);
            Debug.Log($"Начато движение! Сила: {netForce:F2}N");
        }
    }
    
    private void ApplyKineticFriction()
    {
        // Применяем силу трения только если есть движение
        if (objectRb.linearVelocity.magnitude > 0.01f)
        {
            Vector3 frictionForce = -objectRb.linearVelocity.normalized * kineticFriction;
            objectRb.AddForce(frictionForce, ForceMode.Force);
        }
    }
    
    public void StopMovement()
    {
        isMoving = false;
        objectRb.linearVelocity = Vector3.zero;
    }
    
    private void CheckFinish()
    {
        if (!reachedFinish)
        {
            float currentDistance = Vector3.Distance(objectRb.position, startPoint.position);
            if (currentDistance >= finishDistance)
            {
                reachedFinish = true;
                OnFinishReached();
            }
        }
    }
    
    public void OnFinishReached()
    {
        Debug.Log($"🎉 ФИНИШ! Объект прошел {finishDistance} метров!");
        
        // Показываем панель победы
        if (finishPanel != null)
        {
            finishPanel.SetActive(true);
            
            // Заполняем результаты
            float actualDistance = Vector3.Distance(startPoint.position, objectRb.position);
            float time = Time.timeSinceLevelLoad;
            
            resultsText.text = 
                $"🎉 ФИНИШ! 🎉\n\n" +
                $"Пройдено: {actualDistance:F2} м\n" +
                $"Масса: {mass:F1} кг\n" +
                $"Трение: {staticFrictionCoefficient:F2}\n" +
                $"Сила: {appliedForce:F1} Н\n" +
                $"Время: {time:F1} сек";
        }
        
        // Меняем статус
        if (statusText != null)
        {
            statusText.color = Color.green;
            statusText.text = "ФИНИШ ДОСТИГНУТ!";
        }
        
        StopMovement();
    }
    
    public void ResetObject()
    {
        // Сбрасываем физику
        objectRb.linearVelocity = Vector3.zero;
        objectRb.angularVelocity = Vector3.zero;
        objectRb.position = initialPosition;
        
        // Сбрасываем состояние
        isMoving = false;
        reachedFinish = false;
        
        // Скрываем панель победы
        if (finishPanel != null)
            finishPanel.SetActive(false);
        
        // Сбрасываем UI
        if (statusText != null)
        {
            statusText.color = Color.white;
        }
        
        UpdateUI();
        Debug.Log("Объект сброшен в начальное положение");
    }
    
    private void UpdateUI()
    {
        if (forceText != null)
            forceText.text = $"Сила: {appliedForce:F2} N\n" +
                           $"Статич. трение: {maxStaticFriction:F2} N\n" +
                           $"Кинет. трение: {kineticFriction:F2} N";
        
        if (statusText != null && !reachedFinish)
        {
            string status = isMoving ? "ДВИЖЕТСЯ" : "ПОКОИТСЯ";
            statusText.text = $"{status}\nСкорость: {objectRb.linearVelocity.magnitude:F2} m/s";
        }
        
        if (distanceText != null)
        {
            float distanceFromStart = Vector3.Distance(objectRb.position, startPoint.position);
            float distanceToFinish = Mathf.Max(0, finishDistance - distanceFromStart);
            
            distanceText.text = $"От старта: {distanceFromStart:F2} m\n" +
                              $"До финиша: {distanceToFinish:F2} m\n" +
                              $"Цель: {finishDistance} m";
        }
    }
}