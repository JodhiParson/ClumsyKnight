using System.Collections;
using System.Collections.Generic;
// using System.Numerics;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float staminaDrainRate = 15f;
    [SerializeField] private float staminaRegenRate = 10f;

    private PlayerControls playerControls;
    private Rigidbody rb;
    private Vector3 movement;
    private float currentSpeed;
    public Stamina stamina;

    private void Awake() {
        playerControls = new PlayerControls();
    }

    private void OnEnable() {
        playerControls.Enable();
    }

    private void OnDisable() {
        playerControls.Disable();
    }

    private void Start() {
        rb = GetComponent<Rigidbody>();
        currentSpeed = walkSpeed;
    }

    void Update()
    {
        float x = playerControls.Player.Move.ReadValue<Vector2>().x;
        float z = playerControls.Player.Move.ReadValue<Vector2>().y;

        movement = new Vector3(x, 0, z).normalized;
        bool isMoving = movement.sqrMagnitude > 0f;

    bool wantsToSprint = playerControls.Player.Sprint.IsPressed();
    bool isSprinting = wantsToSprint && isMoving && !stamina.IsExhausted;

    currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

    if (isSprinting)
    {
        stamina.Drain(Time.deltaTime * staminaDrainRate);
    }
    else
    {
        stamina.Regen(Time.deltaTime * staminaRegenRate);
    }
}

    private void FixedUpdate() {
        rb.MovePosition(transform.position + movement * currentSpeed * Time.fixedDeltaTime);
    }
}