using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

[RequireComponent(typeof(PlayerInput))]
public class InputSystemHelper : MonoBehaviour
{
    void Start()
    {
        // Автоматическая настройка если нет Input Actions
        var playerInput = GetComponent<PlayerInput>();
        if (playerInput.actions == null)
        {
            Debug.Log("Creating default input actions...");
            CreateDefaultInputActions(playerInput);
        }
    }

    private void CreateDefaultInputActions(PlayerInput playerInput)
    {
        var asset = ScriptableObject.CreateInstance<InputActionAsset>();
        
        // Action Map
        var map = new InputActionMap("Player");
        
        // Move Action
        var moveAction = map.AddAction("Move", type: InputActionType.Value);
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
        
        // Look Action
        var lookAction = map.AddAction("Look", type: InputActionType.Value);
        lookAction.AddBinding("<Mouse>/delta");
        
        // Jump Action
        var jumpAction = map.AddAction("Jump", type: InputActionType.Button);
        jumpAction.AddBinding("<Keyboard>/space");
        
        asset.AddActionMap(map);
        playerInput.actions = asset;
    }
}