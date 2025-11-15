using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarouselController : MonoBehaviour
{
    [Header("Physics References")]
    public Rigidbody carouselRigidbody;
    public List<Transform> massPoints; // Список точек крепления грузов
    public List<GameObject> massPrefabs; // Префабы грузов

    [Header("Level Settings")]
    public int currentLevel = 1;
    private List<GameObject> currentMasses = new List<GameObject>(); // Активные грузы

    [Header("Control Settings")]
    public float moveSpeed = 2.0f;
    private int selectedMassIndex = 0;

    [Header("Initial Spin")]
    public float initialSpinForce = 50f; // Начальный "толчок"
    // private bool isSpinning = false;

    [Header("UI References")]
    public TMP_Text angularVelocityText;
    public TMP_Text momentOfInertiaText;
    public TMP_Text angularMomentumText;
    // Добавь другие UI-элементы по необходимости

    void Start()
    {
        carouselRigidbody = GetComponent<Rigidbody>();
        // Вычисляем центр масс вручную в начале, чтобы он был точно по центру
        // UpdateCenterOfMass();
        // Загружаем начальный уровень
        LoadLevel(currentLevel);

        // Раскручиваем карусель в начале уровня
        StartSpinning();
    }

    void Update()
    {
        // Выбор объекта (уже правильно)
        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            selectedMassIndex--;
            if (selectedMassIndex < 0) selectedMassIndex = currentMasses.Count - 1;
        }
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            selectedMassIndex = (selectedMassIndex + 1) % currentMasses.Count;
        }

        // Перемещение выбранного объекта - ИСПРАВЛЕНО
        if (currentMasses.Count > 0)
        {
            GameObject selectedMass = currentMasses[selectedMassIndex];
            Transform massPoint = selectedMass.transform.parent;

            // ЗАМЕНА: Используем новую систему ввода вместо Input.GetAxis
            float moveInput = 0f;
            if (Keyboard.current.aKey.isPressed) moveInput = -1f;
            if (Keyboard.current.dKey.isPressed) moveInput = 1f;

            // Двигаем точку по радиусу (по локальной оси Z точки крепления)
            massPoint.Translate(0, 0, moveInput * moveSpeed * Time.deltaTime);

            // Ограничиваем радиус, чтобы груз не улетел
            Vector3 localPos = massPoint.localPosition;
            localPos.z = Mathf.Clamp(localPos.z, 1f, 5f);
            massPoint.localPosition = localPos;
        }

        // Пересчитываем физику при ЛЮБОМ движении груза
        if (Keyboard.current.aKey.isPressed || Keyboard.current.dKey.isPressed)
        {
            UpdateInertiaTensor();
        }
    }
    void LoadLevel(int level)
    {
        // Уничтожаем старые грузы
        foreach (GameObject mass in currentMasses)
        {
            if (mass != null) Destroy(mass);
        }
        currentMasses.Clear();

        // В зависимости от уровня, создаем грузы в разных позициях
        switch (level)
        {
            case 1:
                // Создаем 2 груза симметрично
                SpawnMassAtPoint(0);
                SpawnMassAtPoint(1);
                break;
            case 2:
                // Создаем 4 груза равномерно
                for (int i = 0; i < 4; i++) SpawnMassAtPoint(i);
                break;
            case 3:
                // Создаем асимметрично: 2 слева, 1 справа
                SpawnMassAtPoint(0); // Предположим, это слева
                SpawnMassAtPoint(1); // Слева
                SpawnMassAtPoint(3); // Справа
                break;
        }
        // После расстановки грузов ПЕРЕСЧИТЫВАЕМ тензор инерции!
        UpdateInertiaTensor();
    }

    void SpawnMassAtPoint(int pointIndex)
    {
        if (pointIndex < massPoints.Count && massPrefabs.Count > 0)
        {
            GameObject newMass = Instantiate(massPrefabs[0], massPoints[pointIndex].position, massPoints[pointIndex].rotation, massPoints[pointIndex]);
            currentMasses.Add(newMass);
        }
    }

    void UpdateInertiaTensor()
    {
        // 1. Отключаем автоматический расчет тензора
        carouselRigidbody.automaticCenterOfMass = false;
        carouselRigidbody.automaticInertiaTensor = false;

        // 2. Рассчитываем новый центр масс (в локальных координатах карусели)
        Vector3 centerOfMass = Vector3.zero;
        float totalMass = carouselRigidbody.mass;

        // Учитываем массу платформы (ее "точка" - это центр)
        centerOfMass += carouselRigidbody.mass * Vector3.zero;

        // Учитываем массы всех грузов
        foreach (GameObject mass in currentMasses)
        {
            // Предположим, у каждого груза есть скрипт MassObject с полем massValue
            MassObject massComp = mass.GetComponent<MassObject>();
            if (massComp != null)
            {
                // Преобразуем позицию груза в ЛОКАЛЬНУЮ позицию относительно Carousel
                Vector3 localPos = transform.InverseTransformPoint(mass.transform.position);
                centerOfMass += massComp.massValue * localPos;
                totalMass += massComp.massValue;
            }
        }
        centerOfMass /= totalMass;
        carouselRigidbody.centerOfMass = centerOfMass;

        // 3. Рассчитываем тензор инерции (упрощенно, для точечных масс)
        // I = sum( m_i * (r_i^2 * E - r_i * r_i) ) - общая формула, но мы упростим до диагонального
        float Ixx = 0f;
        float Iyy = 0f;
        float Izz = 0f;

        // Момент инерции платформы (возьмем как у диска)
        float platformI = carouselRigidbody.mass * 5f; // Эмпирическая константа

        Ixx += platformI;
        Iyy += platformI;
        Izz += platformI;

        foreach (GameObject mass in currentMasses)
        {
            MassObject massComp = mass.GetComponent<MassObject>();
            if (massComp != null)
            {
                // Позиция груза относительно НОВОГО центра масс
                Vector3 r = transform.InverseTransformPoint(mass.transform.position) - centerOfMass;
                float m = massComp.massValue;

                // Формулы из задания (для точечных масс)
                Ixx += m * (r.y * r.y + r.z * r.z);
                Iyy += m * (r.x * r.x + r.z * r.z);
                Izz += m * (r.x * r.x + r.y * r.y);
            }
        }

        // 4. Создаем и применяем тензор инерции (в главных осях, как Vector3)
        // Inertia Tensor в Unity представлен как диагональная матрица [Ixx, Iyy, Izz]
        carouselRigidbody.inertiaTensor = new Vector3(Ixx, Iyy, Izz);
        // Важно: также нужно задать rotation тензора. Для простоты - единичная.
        carouselRigidbody.inertiaTensorRotation = Quaternion.identity;
    }

    void OnDrawGizmos()
    {
        if (carouselRigidbody == null) return;

        // Рисуем центр масс
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(carouselRigidbody.worldCenterOfMass, 0.2f);

        // Рисуем вектор угловой скорости
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(carouselRigidbody.worldCenterOfMass, carouselRigidbody.angularVelocity);
    }

    void FixedUpdate()
    {
        UpdateUI();
    }

    void UpdateUI()
    {
        if (angularVelocityText != null)
            angularVelocityText.text = "Angular Velocity: " + carouselRigidbody.angularVelocity.ToString("F2");

        if (momentOfInertiaText != null)
            momentOfInertiaText.text = "Inertia Tensor: " + carouselRigidbody.inertiaTensor.ToString("F2");

        if (angularMomentumText != null)
        {
            Vector3 inertiaTensor = carouselRigidbody.inertiaTensor;
            Vector3 angularVelocity = carouselRigidbody.angularVelocity;
            
            // Поэлементное умножение (Ixx * ωx, Iyy * ωy, Izz * ωz)
            Vector3 angularMomentum = new Vector3(
                inertiaTensor.x * angularVelocity.x,
                inertiaTensor.y * angularVelocity.y,
                inertiaTensor.z * angularVelocity.z
            );
            
            angularMomentumText.text = "Angular Momentum: " + angularMomentum.ToString("F2");
        }
    }
    
    void StartSpinning()
    {
        // Прикладываем начальный вращательный импульс
        carouselRigidbody.AddTorque(0, initialSpinForce, 0, ForceMode.Impulse);
        // isSpinning = true;
    }
}
