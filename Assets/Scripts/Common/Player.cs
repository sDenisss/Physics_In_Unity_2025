using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 10f;
    public float jumpForce = 10f;
    public float mouseSensitivity = 2f;
    public float maxVerticalAngle = 45f;

    [Header("References")]
    public Transform cameraTransform;
    private Rigidbody rb;
    private Vector2 lookRotation;
    private bool isGrounded;
    private Vector2 moveInput;
    private Vector2 lookInput;

    // Новые Input Actions
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        // Cursor.lockState = CursorLockMode.Locked;
        
        if (cameraTransform == null)
        {
            enabled = false;
        }

        // Инициализация Input System
        InitializeInputSystem();
    }

    private void InitializeInputSystem()
    {
        playerInput = GetComponent<PlayerInput>();
        if (playerInput == null)
        {
            playerInput = gameObject.AddComponent<PlayerInput>();
        }

        // Создаем Input Actions если их нет
        if (playerInput.actions == null)
        {
            CreateDefaultInputActions();
        }

        // Получаем ссылки на действия
        moveAction = playerInput.actions["Move"];
        lookAction = playerInput.actions["Look"];
        // jumpAction = playerInput.actions["Jump"];

        // Включаем действия
        moveAction.Enable();
        lookAction.Enable();
        // jumpAction.Enable();
    }

    private void CreateDefaultInputActions()
    {
        // Создаем новый Input Action Asset
        var asset = ScriptableObject.CreateInstance<InputActionAsset>();
        asset.name = "PlayerControls";

        // Создаем Action Map
        var playerMap = new InputActionMap("Player");

        // Action для движения (WASD/Стрелки)
        var moveAction = playerMap.AddAction("Move", type: InputActionType.Value);
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        // Action для взгляда (Мышь)
        var lookAction = playerMap.AddAction("Look", type: InputActionType.Value);
        lookAction.AddBinding("<Mouse>/delta");

        // Action для прыжка (Пробел)
        // var jumpAction = playerMap.AddAction("Jump", type: InputActionType.Button);
        // jumpAction.AddBinding("<Keyboard>/space");

        // Добавляем map в asset
        asset.AddActionMap(playerMap);
        
        // Устанавливаем asset в PlayerInput
        playerInput.actions = asset;
    }

    void Update()
    {
        HandleMovementInput();
        // HandleJumpInput();
        HandleMouseLook();
    }

    void FixedUpdate()
    {
        // Движение
        if (moveAction != null)
        {
            Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y);
            Vector3 moveVelocity = transform.TransformDirection(moveDirection) * moveSpeed;
            rb.linearVelocity = new Vector3(moveVelocity.x, rb.linearVelocity.y, moveVelocity.z);
        }
    }

    private void HandleMovementInput()
    {
        // Получаем ввод из новой Input System с проверкой на null
        if (moveAction != null)
        {
            moveInput = moveAction.ReadValue<Vector2>();
        }
    }

    private void HandleJumpInput()
    {
        if (jumpAction != null && jumpAction.triggered && isGrounded)
        {
            Jump();
        }
    }

    private void Jump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
        isGrounded = false;
    }

    private void HandleMouseLook()
    {
        // Получаем ввод мыши из новой системы
        if (lookAction != null)
        {
            lookInput = lookAction.ReadValue<Vector2>();
            
            lookRotation.x += lookInput.x * mouseSensitivity * Time.deltaTime;
            lookRotation.y += lookInput.y * mouseSensitivity * Time.deltaTime;
            lookRotation.y = Mathf.Clamp(lookRotation.y, -maxVerticalAngle, maxVerticalAngle);

            cameraTransform.localRotation = Quaternion.Euler(-lookRotation.y, 0, 0);
            transform.localRotation = Quaternion.Euler(0, lookRotation.x, 0);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.contacts.Length > 0 && collision.contacts[0].normal.y > 0.7f)
        {
            isGrounded = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 200, 20), $"Grounded: {isGrounded}");
        GUI.Label(new Rect(10, 30, 200, 20), $"Velocity: {rb.linearVelocity}");
        GUI.Label(new Rect(10, 50, 200, 20), $"Move Input: {moveInput}");
    }
}