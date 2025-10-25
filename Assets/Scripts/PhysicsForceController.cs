using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PhysicsForceController : MonoBehaviour
{
    [Header("Основные настройки")]
    public PushCube pushCubeSystem; // ГЛАВНОЕ - привязать систему куба!
    
    [Header("UI элементы")]
    public Slider forceSlider;
    public TMP_InputField forceInput;
    public Button applyButton;
    
    private float currentForce = 0f;
    
    void Start()
    {
        InitializeUI();
    }
    
    private void InitializeUI()
    {
        // Настройка слайдера
        forceSlider.minValue = 0f;
        forceSlider.maxValue = 100f;
        forceSlider.value = 0f;
        forceSlider.onValueChanged.AddListener(OnSliderChanged);
        
        // Настройка поля ввода
        forceInput.text = "0";
        forceInput.onEndEdit.AddListener(OnInputChanged);
        
        // Настройка кнопки
        applyButton.onClick.AddListener(ApplyForce);
    }
    
    private void OnSliderChanged(float value)
    {
        currentForce = value;
        forceInput.text = value.ToString("F1");
    }
    
    private void OnInputChanged(string input)
    {
        if (float.TryParse(input, out float force))
        {
            currentForce = Mathf.Clamp(force, 0, 100f);
            forceSlider.value = currentForce;
        }
        else
        {
            forceInput.text = currentForce.ToString("F1");
        }
    }
    
    public void ApplyForce()
    {
        if (pushCubeSystem != null && currentForce > 0)
        {
            pushCubeSystem.SetAppliedForce(currentForce);
            Debug.Log($"Приложена сила: {currentForce:F1}N");
        }
    }
}