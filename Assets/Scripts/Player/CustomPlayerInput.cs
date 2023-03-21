using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CustomPlayerInput : MonoBehaviour
{
    // InputAction: https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/api/UnityEngine.InputSystem.InputAction.html

    [Header("Movement")]
    public Vector2 direction = Vector2.zero;
    public InputAction directionAction;

    [Header("A Button")]
    public bool aInput;
    public InputAction aAction;

    [Header("Y Attack")]
    public bool bInput;
    public InputAction bAction;


    private void OnEnable()
    {
        directionAction.Enable();
        aAction.Enable();
        bAction.Enable();
    }

    private void OnDisable()
    {
        directionAction.Disable();
        aAction.Disable();
        bAction.Disable();
    }

    private void Update()
    { 
        direction = directionAction.ReadValue<Vector2>();

        aAction.started += context => aInput = true;
        aAction.canceled += context => aInput = false;

        bAction.started += context => bInput = true;
        bAction.canceled += context => bInput = false;

    }

}
