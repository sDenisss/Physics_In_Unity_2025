using UnityEngine;
using UnityEngine.UI;

public class PhysicsForceController : MonoBehaviour
{
    [Header("Physics Object")]
    public Rigidbody targetObject;
    public float maxForce = 100f;
    
    [Header("UI Controls")]
    public Slider forceSlider;
    public InputField forceInput;
    public Dropdown axisDropdown;
    public Button applyButton;
    public Text statusText;
    
    private float currentForce = 0f;
    private Vector3 forceDirection = Vector3.forward;
    
    void Start()
    {
        InitializeUI();
        UpdateStatus();
    }
    
    private void InitializeUI()
    {
        // Слайдер
        if (forceSlider != null)
        {
            forceSlider.minValue = 0f;
            forceSlider.maxValue = maxForce;
            forceSlider.value = 0f;
            forceSlider.onValueChanged.AddListener(OnSliderChanged);
        }
        
        // Поле ввода
        if (forceInput != null)
        {
            forceInput.text = "0";
            forceInput.onEndEdit.AddListener(OnInputChanged);
        }
        
        // Выбор оси
        if (axisDropdown != null)
        {
            axisDropdown.onValueChanged.AddListener(OnAxisChanged);
        }
        
        // Кнопка применения
        if (applyButton != null)
        {
            applyButton.onClick.AddListener(ApplyForce);
        }
    }
    
    private void OnSliderChanged(float value)
    {
        currentForce = value;
        if (forceInput != null)
            forceInput.text = value.ToString("F1");
        UpdateStatus();
    }
    
    private void OnInputChanged(string input)
    {
        if (float.TryParse(input, out float force))
        {
            currentForce = Mathf.Clamp(force, 0, maxForce);
            if (forceSlider != null)
                forceSlider.value = currentForce;
        }
        UpdateStatus();
    }
    
    private void OnAxisChanged(int index)
    {
        switch (index)
        {
            case 0: forceDirection = Vector3.forward; break;  // Z
            case 1: forceDirection = Vector3.right; break;    // X
            case 2: forceDirection = Vector3.up; break;       // Y
        }
        UpdateStatus();
    }
    
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
    
    private void UpdateStatus()
    {
        if (statusText != null)
        {
            string axisName = GetAxisName();
            statusText.text = $"Сила: {currentForce:F1} N\nОсь: {axisName}";
        }
    }
    
    private string GetAxisName()
    {
        if (forceDirection == Vector3.forward) return "Z (Вперед)";
        if (forceDirection == Vector3.right) return "X (Вправо)";
        if (forceDirection == Vector3.up) return "Y (Вверх)";
        return "Неизвестно";
    }
    
    // Метод для сброса силы
    public void ResetForce()
    {
        currentForce = 0f;
        if (forceSlider != null) forceSlider.value = 0f;
        if (forceInput != null) forceInput.text = "0";
        UpdateStatus();
    }
}