using System.Collections;
using System.Collections.Generic;
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

    [Header("Combo Settings")]
    [SerializeField] private float comboWindow = 0.6f;
    [SerializeField] private float attack1StaminaCost = 15f;
    [SerializeField] private float attack2StaminaCost = 20f;

    private bool isAttacking = false;
    private int comboStep = 0;
    private bool queuedNextAttack = false;
    private float comboTimer = 0f;

    [Header("Roll Settings")]
    [SerializeField] private float rollSpeed = 12f;
    [SerializeField] private float rollDuration = 0.4f;
    [SerializeField] private float rollStaminaCost = 20f;
    [SerializeField] private float rollCooldown = 0.2f;

    private bool isRolling = false;
    private Vector3 rollDirection;
    private float rollTimer = 0f;
    private float rollCooldownTimer = 0f;

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

        if (isAttacking || isRolling)
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
        bool isSprinting = wantsToSprint && isMoving && !stamina.IsExhausted && !isAttacking && !isRolling;

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

        // --- Attack / combo input handling ---
        bool attackInput = playerControls.Player.Attack.triggered;

        if (attackInput && !stamina.IsExhausted && !isRolling)
        {
            if (!isAttacking && comboStep == 0)
            {
                StartAttack1();
            }
            else if (isAttacking && comboStep == 1)
            {
                // Buffer the click; it'll be consumed when attack1's window opens/animation ends
                queuedNextAttack = true;
            }
        }

        // Combo window countdown — only runs once attack1 has finished and we're waiting for a follow-up
        if (comboStep == 1 && !isAttacking)
        {
            comboTimer -= Time.deltaTime;

            if (queuedNextAttack)
            {
                StartAttack2();
            }
            else if (comboTimer <= 0f)
            {
                ResetCombo();
            }
        }

        // --- Roll input handling ---
        bool rollInput = playerControls.Player.Roll.triggered;

        if (rollCooldownTimer > 0f)
            rollCooldownTimer -= Time.deltaTime;

        if (rollInput && !isRolling && !isAttacking && rollCooldownTimer <= 0f && !stamina.IsExhausted)
        {
            StartRoll();
        }

        if (isRolling)
        {
            rollTimer -= Time.deltaTime;
            if (rollTimer <= 0f)
            {
                EndRoll();
            }
        }
    }

    private void FixedUpdate() {
        if (isRolling)
        {
            rb.MovePosition(transform.position + rollDirection * rollSpeed * Time.fixedDeltaTime);
            return;
        }
        if (isAttacking) return;
        rb.MovePosition(transform.position + movement * currentSpeed * Time.fixedDeltaTime);
    }

    private void StartAttack1()
    {
        isAttacking = true;
        comboStep = 1;
        queuedNextAttack = false;
        movement = Vector3.zero;
        animator.SetTrigger("Attack1");
        stamina.Drain(attack1StaminaCost);
    }

    private void StartAttack2()
    {
        isAttacking = true;
        queuedNextAttack = false;
        movement = Vector3.zero;
        animator.SetTrigger("Attack2");
        stamina.Drain(attack2StaminaCost);
    }

    private void ResetCombo()
    {
        comboStep = 0;
        comboTimer = 0f;
        queuedNextAttack = false;
    }

    private void StartRoll()
    {
        isRolling = true;
        rollTimer = rollDuration;
        rollCooldownTimer = rollCooldown;

        // Roll in whatever direction the player is currently moving;
        // fall back to facing direction if standing still
        Vector3 xDir = playerControls.Player.Move.ReadValue<Vector2>().x * Vector3.right;
        Vector3 zDir = playerControls.Player.Move.ReadValue<Vector2>().y * Vector3.forward;
        Vector3 inputDir = (xDir + zDir).normalized;

        rollDirection = inputDir.sqrMagnitude > 0.01f
            ? inputDir
            : new Vector3(Mathf.Sign(visualTransform.localScale.x), 0, 0);

        stamina.Drain(rollStaminaCost);
        animator.SetTrigger("Roll");
    }

    private void EndRoll()
    {
        isRolling = false;
    }

    public void OnAttack1AnimationEnd()
    {
        isAttacking = false;
        comboTimer = comboWindow; // combo window starts counting down now
    }

    public void OnAttack2AnimationEnd()
    {
        isAttacking = false;
        ResetCombo();
    }
}