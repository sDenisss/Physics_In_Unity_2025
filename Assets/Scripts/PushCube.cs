using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PushCube : MonoBehaviour
{
    [Header("Physics Parameters")]
    public float mass = 1.5f;
    public float staticFrictionCoefficient = 0.1f;
    public float kineticFrictionCoefficient = 0.3f;
    public float appliedForce = 0f;
    
    [Header("References")]
    public Rigidbody objectRb;
    public Transform startPoint;
    public Transform finishPoint;
    public TMP_Text forceText;
    public TMP_Text statusText;
    public TMP_Text distanceText;
    
    // Расчетные величины
    private float gravity = 9.81f;
    private float normalForce;
    private float maxStaticFriction;
    private float kineticFriction;
    private bool isMoving = false;
    private Vector3 initialPosition;
    
    // Тестовые данные
    private TestResult[] testResults;
    private int currentTest = 0;
    
    void Start()
    {
        normalForce = mass * gravity;
        maxStaticFriction = staticFrictionCoefficient * normalForce;
        kineticFriction = kineticFrictionCoefficient * normalForce;
        
        initialPosition = objectRb.position;
        testResults = new TestResult[3];
        
        UpdateUI();
    }
    
    void FixedUpdate()
    {
        // Применяем силу, если объект не двигается
        if (!isMoving && appliedForce > 0)
        {
            if (appliedForce > maxStaticFriction)
            {
                StartMovement();
            }
        }
        
        // Если объект движется, применяем силу трения
        if (isMoving)
        {
            ApplyKineticFriction();
            
            // Проверяем остановку
            if (objectRb.linearVelocity.magnitude < 0.01f && appliedForce <= kineticFriction)
            {
                StopMovement();
            }
        }
        
        UpdateUI();
    }
    
    public void SetAppliedForce(float force)
    {
        appliedForce = force;
    }
    
    public void SetMass(float newMass)
    {
        mass = newMass;
        objectRb.mass = mass;
        RecalculateForces();
    }
    
    public void SetStaticFriction(float friction)
    {
        staticFrictionCoefficient = friction;
        RecalculateForces();
    }
    
    public void SetKineticFriction(float friction)
    {
        kineticFrictionCoefficient = friction;
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
        // Прикладываем избыточную силу (приложенная сила - кинетическое трение)
        float netForce = appliedForce - kineticFriction;
        objectRb.AddForce(Vector3.right * netForce, ForceMode.Force);
    }
    
    private void ApplyKineticFriction()
    {
        // Применяем силу трения, противоположную движению
        if (objectRb.linearVelocity.magnitude > 0)
        {
            Vector3 frictionForce = -objectRb.linearVelocity.normalized * kineticFriction;
            objectRb.AddForce(frictionForce, ForceMode.Force);
        }
    }
    
    private void StopMovement()
    {
        isMoving = false;
        objectRb.linearVelocity = Vector3.zero;
    }
    
    public void ResetObject()
    {
        objectRb.linearVelocity = Vector3.zero;
        objectRb.angularVelocity = Vector3.zero;
        objectRb.position = initialPosition;
        isMoving = false;
        appliedForce = 0f;
    }
    
    private void UpdateUI()
    {
        if (forceText != null)
            forceText.text = $"Приложенная сила: {appliedForce:F2} N\n" +
                           $"Статическое трение: {maxStaticFriction:F2} N\n" +
                           $"Кинетическое трение: {kineticFriction:F2} N";
        
        if (statusText != null)
        {
            string status = isMoving ? "ДВИЖЕТСЯ" : "ПОКОИТСЯ";
            statusText.text = $"Статус: {status}\n" +
                            $"Скорость: {objectRb.linearVelocity.magnitude:F2} m/s";
        }
        
        if (distanceText != null)
        {
            float distance = Vector3.Distance(objectRb.position, finishPoint.position);
            distanceText.text = $"До финиша: {distance:F2} m";
        }
    }
    
    // Методы для тестирования
    public void RunTest1()
    {
        // Тест 1: Сброс силы при Fпр = 1/2 F к.тр
        currentTest = 0;
        StartCoroutine(PerformTest(kineticFriction * 0.5f));
    }
    
    public void RunTest2()
    {
        // Тест 2: Сброс силы при Fпр = F к.тр
        currentTest = 1;
        StartCoroutine(PerformTest(kineticFriction));
    }
    
    public void RunTest3()
    {
        // Тест 3: Сброс силы при Fпр = 2F к.тр
        currentTest = 2;
        StartCoroutine(PerformTest(kineticFriction * 2f));
    }
    
    private System.Collections.IEnumerator PerformTest(float testForce)
    {
        ResetObject();
        yield return new WaitForSeconds(1f);
        
        // Прикладываем силу
        appliedForce = testForce;
        
        if (testForce > maxStaticFriction)
        {
            // Ждем пока объект начнет движение
            yield return new WaitUntil(() => isMoving);
            yield return new WaitForSeconds(0.5f);
        }
        
        // Сбрасываем силу
        float distanceBeforeReset = Vector3.Distance(initialPosition, objectRb.position);
        appliedForce = 0f;
        
        // Ждем полной остановки
        yield return new WaitUntil(() => !isMoving);
        float totalDistance = Vector3.Distance(initialPosition, objectRb.position);
        
        // Сохраняем результаты
        testResults[currentTest] = new TestResult
        {
            testName = $"Тест {currentTest + 1}",
            appliedForce = testForce,
            distanceBeforeReset = distanceBeforeReset,
            totalDistance = totalDistance,
            additionalDistance = totalDistance - distanceBeforeReset
        };
        
        DisplayTestResults();
    }
    
    private void DisplayTestResults()
    {
        Debug.Log($"=== РЕЗУЛЬТАТЫ ТЕСТА {currentTest + 1} ===");
        Debug.Log($"Приложенная сила: {testResults[currentTest].appliedForce:F2} N");
        Debug.Log($"Дистанция до сброса: {testResults[currentTest].distanceBeforeReset:F2} m");
        Debug.Log($"Дополнительная дистанция: {testResults[currentTest].additionalDistance:F2} m");
        Debug.Log($"Общая дистанция: {testResults[currentTest].totalDistance:F2} m");
    }
    
    [System.Serializable]
    public struct TestResult
    {
        public string testName;
        public float appliedForce;
        public float distanceBeforeReset;
        public float totalDistance;
        public float additionalDistance;
    }
}