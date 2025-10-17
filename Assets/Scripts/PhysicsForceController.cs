using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PhysicsForceController : MonoBehaviour
{
    [Header("=== ОСНОВНЫЕ НАСТРОЙКИ ===")]
    [Tooltip("Объект который будем толкать (должен иметь Rigidbody)")]
    public Rigidbody targetObject;          // → Cube (1) или Cube (2)
    
    [Tooltip("Максимальная сила толчка")]
    public float maxForce = 100f;

    [Header("=== ЭЛЕМЕНТЫ UI ===")]
    [Tooltip("Ползунок для выбора силы")]
    public Slider forceSlider;              // → ForceSlider
    
    [Tooltip("Поле для точного ввода числа")]
    public TMP_InputField forceInput;           // → PlayerForce_Input
    
    // [Tooltip("Выбор оси толкания (X/Y/Z)")]
    // public TMP_Dropdown axisDropdown;           // → AxisDropdown
    
    [Tooltip("Кнопка для применения силы")]
    public Button applyButton;              // → Создать кнопку "Apply Force"
    
    [Tooltip("Текст для отображения статуса")]
    public TMP_Text statusText;                 // → Status

    [Header("=== ДОПОЛНИТЕЛЬНЫЕ ЭЛЕМЕНТЫ ===")]
    [Tooltip("Точка старта для расчета дистанции")]
    public Transform startPoint;            // → StartPoint
    
    [Tooltip("Точка финиша")]
    public Transform finishPoint;           // → FinishPoint

    private float currentForce = 0f;
    private Vector3 forceDirection = Vector3.forward;
    
    void Start()
    {
        InitializeUI();
        // UpdateStatus();
    }
    
    private void InitializeUI()
    {
        // === НАСТРОЙКА СЛАЙДЕРА ===
        if (forceSlider != null)
        {
            forceSlider.minValue = 0f;
            forceSlider.maxValue = maxForce;
            forceSlider.value = 0f;
            forceSlider.onValueChanged.AddListener(OnSliderChanged);
        }
        
        // === НАСТРОЙКА ПОЛЯ ВВОДА ===
        if (forceInput != null)
        {
            // forceInput.text = "0";
            // forceInput.placeholder.GetComponent<Text>().text = "Введите силу...";
            forceInput.onEndEdit.AddListener(OnInputChanged);
        }
        
        // === НАСТРОЙКА ВЫБОРА ОСИ ===
        // if (axisDropdown != null)
        // {
        //     // Очищаем старые选项
        //     axisDropdown.ClearOptions();
        //     // Добавляем новые选项
        //     axisDropdown.AddOptions(new System.Collections.Generic.List<string> 
        //     { 
        //         "Ось Z (Вперед)", 
        //         "Ось X (Вправо)", 
        //         "Ось Y (Вверх)" 
        //     });
        //     // axisDropdown.onValueChanged.AddListener(OnAxisChanged);
        // }

        // === НАСТРОЙКА КНОПКИ ===
        if (applyButton != null)
        {
            applyButton.onClick.AddListener(ApplyForce);
            // applyButton.GetComponentInChildren<TMP_Text>().text = "Толкнуть!";
        }
        
        // Обновляем текст статуса
        // UpdateStatus();
    }
    
    // === ОБРАБОТЧИК ИЗМЕНЕНИЯ СЛАЙДЕРА ===
    private void OnSliderChanged(float value)
    {
        currentForce = value;
        if (forceInput != null)
            forceInput.text = value.ToString("F1");
        // UpdateStatus();
    }
    
    // === ОБРАБОТЧИК ИЗМЕНЕНИЯ ПОЛЯ ВВОДА ===
    private void OnInputChanged(string input)
    {
        if (float.TryParse(input, out float force))
        {
            currentForce = Mathf.Clamp(force, 0, maxForce);
            if (forceSlider != null)
                forceSlider.value = currentForce;
        }
        else
        {
            // Если ввод некорректен, восстанавливаем предыдущее значение
            if (forceInput != null)
                forceInput.text = currentForce.ToString("F1");
        }
        // UpdateStatus();
    }
    
    // // === ОБРАБОТЧИК ИЗМЕНЕНИЯ ОСИ ===
    // private void OnAxisChanged(int index)
    // {
    //     switch (index)
    //     {
    //         case 0: forceDirection = Vector3.forward; break;  // Z - Вперед
    //         case 1: forceDirection = Vector3.right; break;    // X - Вправо
    //         case 2: forceDirection = Vector3.up; break;       // Y - Вверх
    //     }
    //     // UpdateStatus();
    // }
    
    // === ПРИМЕНЕНИЕ СИЛЫ ===
    public void ApplyForce()
    {
        if (targetObject != null && currentForce > 0)
        {
            // Останавливаем объект перед применением новой силы
            targetObject.linearVelocity = Vector3.zero;
            targetObject.angularVelocity = Vector3.zero;
            
            // Применяем силу строго по выбранной оси
            targetObject.AddForce(forceDirection * currentForce, ForceMode.Impulse);
            
            Debug.Log($"Приложена сила: {currentForce:F1}N по оси {forceDirection}");
        }
    }
    
    // // === ОБНОВЛЕНИЕ СТАТУСА ===
    // private void UpdateStatus()
    // {
    //     if (statusText != null)
    //     {
    //         string axisName = GetAxisName();
    //         statusText.text = $"Сила: {currentForce:F1} N\nОсь: {axisName}";
    //     }
        
    //     // Обновляем дистанцию если есть ссылки
    //     UpdateDistance();
    // }
    
    // // === ОБНОВЛЕНИЕ ДИСТАНЦИИ ===
    // private void UpdateDistance()
    // {
    //     if (distanceText != null && startPoint != null && targetObject != null)
    //     {
    //         float distance = Vector3.Distance(startPoint.position, targetObject.position);
    //         distanceText.text = $"Дистанция: {distance:F2} m";
    //     }
    // }
    
    // private string GetAxisName()
    // {
    //     if (forceDirection == Vector3.forward) return "Z (Вперед)";
    //     if (forceDirection == Vector3.right) return "X (Вправо)";
    //     if (forceDirection == Vector3.up) return "Y (Вверх)";
    //     return "Неизвестно";
    // }
    
    // === СБРОС СИЛЫ ===
    public void ResetForce()
    {
        currentForce = 0f;
        if (forceSlider != null) forceSlider.value = 0f;
        if (forceInput != null) forceInput.text = "0";
        // UpdateStatus();
    }
    
    void Update()
    {
        // Постоянно обновляем дистанцию
        // UpdateDistance();
    }
}