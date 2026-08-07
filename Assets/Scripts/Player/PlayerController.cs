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
    [SerializeField] private float comboWindow = 0.6f; // seconds after attack1 ends where attack2 can trigger
    [SerializeField] private float attack1StaminaCost = 15f;
    [SerializeField] private float attack2StaminaCost = 20f;

    private bool isAttacking = false;
    private int comboStep = 0;          // 0 = no combo in progress, 1 = attack1 just played
    private bool queuedNextAttack = false;
    private float comboTimer = 0f;

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

        // --- Attack / combo input handling ---
        bool attackInput = playerControls.Player.Attack.triggered;

        if (attackInput && !stamina.IsExhausted)
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
    }

    private void FixedUpdate() {
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

    // Called via Animation Event at the end of the Attack1 clip
    public void OnAttack1AnimationEnd()
    {
        isAttacking = false;
        comboTimer = comboWindow; // combo window starts counting down now
    }

    // Called via Animation Event at the end of the Attack2 clip
    public void OnAttack2AnimationEnd()
    {
        isAttacking = false;
        ResetCombo();
    }
}