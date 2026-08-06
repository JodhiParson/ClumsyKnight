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

    [SerializeField] private Transform visualTransform;
    [SerializeField] private Animator animator;
    [SerializeField] private float sprintAnimSpeedMultiplier = 1.6f;

    private bool isAttacking = false;

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

        if (isAttacking)
        {
            movement = Vector3.zero;
        }
        else
        {
            movement = new Vector3(x, 0, z).normalized;

            if (Mathf.Abs(x) > 0.01f)
            {
                Vector3 scale = visualTransform.localScale;
                scale.x = Mathf.Abs(scale.x) * Mathf.Sign(x);
                visualTransform.localScale = scale;
            }
        }

        bool isMoving = movement.sqrMagnitude > 0f;

        bool wantsToSprint = playerControls.Player.Sprint.IsPressed();
        bool isSprinting = wantsToSprint && isMoving && !stamina.IsExhausted && !isAttacking;

        currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

        if (isSprinting)
        {
            stamina.Drain(Time.deltaTime * staminaDrainRate);
        }
        else
        {
            stamina.Regen(Time.deltaTime * staminaRegenRate);
        }

        animator.SetBool("IsWalking", isMoving);
        animator.speed = isSprinting ? sprintAnimSpeedMultiplier : 1f;

        bool attackInput = playerControls.Player.Attack.triggered;

        if (attackInput && !isAttacking && !stamina.IsExhausted)
        {
            StartAttack();
        }
    }

    private void FixedUpdate() {
        if (isAttacking) return; // freeze physics movement entirely while attacking
        rb.MovePosition(transform.position + movement * currentSpeed * Time.fixedDeltaTime);
    }

    private void StartAttack()
    {
        isAttacking = true;
        movement = Vector3.zero;
        animator.SetTrigger("Attack");
        stamina.Drain(15f);
    }

    public void OnAttackAnimationEnd()
    {
        isAttacking = false;
        Debug.Log("Attack ended");
    }
}