using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private static PlayerController instance;
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

    [Header("Trip Settings")]
    [SerializeField] private float tripDuration = 1f;
    [SerializeField] private float tripImpulseForce = 5f;
    [SerializeField] private float rollTripChance = 0.1f;
    [SerializeField] private float rollTripStaminaCost = 30f;

    [Header("Throw Weapon Settings")]
    [SerializeField] private float throwWeaponStaminaCost = 25f;
    [SerializeField] private GameObject weaponSpinPrefab;
    [SerializeField] private Transform weaponSpawnPoint;
    [SerializeField] private float weaponThrowForce = 15f;
    [SerializeField] private float weaponSpinTorque = 10f;
    [SerializeField] private float weaponThrowUpwardArc = 0.4f;

    // True while the player has their weapon equipped; flipped to false
    // the moment the weapon actually leaves the hand
    public bool weaponEquip = true;

    private bool isTripping = false;
    private float tripTimer = 0f;

    // Read-only state exposed for other scripts (e.g. ClumsyEvents)
    public bool IsMoving => movement.sqrMagnitude > 0f;
    public bool IsTripping => isTripping;

    private void Awake() {
        playerControls = new PlayerControls();
    }

    private void OnEnable() {
        playerControls.Enable();
    }

    private void OnDisable() {
        playerControls.Disable();
    }

    private void Start() 
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        rb = GetComponent<Rigidbody>();
        currentSpeed = walkSpeed;
    }

    void Update()
    {
        float x = playerControls.Player.Move.ReadValue<Vector2>().x;
        float z = playerControls.Player.Move.ReadValue<Vector2>().y;

        if (isAttacking || isRolling || isTripping)
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
        bool isSprinting = wantsToSprint && isMoving && !stamina.IsExhausted && !isAttacking && !isRolling && !isTripping;

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

        if (attackInput && !stamina.IsExhausted && !isRolling && !isTripping && weaponEquip)
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

            if (queuedNextAttack && weaponEquip)
            {
                StartAttack2();
            }
            else if (comboTimer <= 0f || !weaponEquip)
            {
                ResetCombo();
            }
        }

        // --- Roll input handling ---
        bool rollInput = playerControls.Player.Roll.triggered;

        if (rollCooldownTimer > 0f)
            rollCooldownTimer -= Time.deltaTime;

        if (rollInput && !isRolling && !isAttacking && !isTripping && rollCooldownTimer <= 0f && !stamina.IsExhausted)
        {
            rollCooldownTimer = rollCooldown;

            if (Random.value < rollTripChance)
            {
                stamina.Drain(rollTripStaminaCost);
                TryTrip();
            }
            else
            {
                StartRoll();
            }
        }

        if (isRolling)
        {
            rollTimer -= Time.deltaTime;
            if (rollTimer <= 0f)
            {
                EndRoll();
            }
        }

        // --- Trip countdown ---
        if (isTripping)
        {
            tripTimer -= Time.deltaTime;
            if (tripTimer <= 0f)
            {
                isTripping = false;
            }
        }
    }

    private void FixedUpdate() {
        if (isTripping) return;
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
        animator.ResetTrigger("CancelAttack");
        isAttacking = true;
        comboStep = 1;
        queuedNextAttack = false;
        movement = Vector3.zero;
        animator.SetTrigger("Attack1");
        stamina.Drain(attack1StaminaCost);
    }

    private void StartAttack2()
    {
        animator.ResetTrigger("CancelAttack");
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

    public bool TryTrip()
    {
        if (isTripping || isAttacking || isRolling) return false;

        isTripping = true;
        tripTimer = tripDuration;
        movement = Vector3.zero;
        rb.linearVelocity = Vector3.zero; // use rb.velocity instead if you're on Unity < 6
        rb.AddForce(Vector3.down * tripImpulseForce, ForceMode.Impulse);
        animator.SetTrigger("Trip");
        return true;
    }

    // Voluntary throw path — plays a dedicated ThrowWeapon clip.
    // The actual release happens later via OnWeaponReleaseAnimationEvent,
    // hooked to that clip's release frame.
    public bool TryThrowWeapon()
    {
        if (isTripping || isAttacking || isRolling || !weaponEquip || stamina.IsExhausted) return false;

        stamina.Drain(throwWeaponStaminaCost);
        animator.SetTrigger("ThrowWeapon");
        return true;
    }

    // Fumble path — cuts an in-progress Attack1/Attack2 clip short and
    // releases the weapon immediately, no separate throw clip involved.
    // Requires Attack1 -> Idle AND Attack2 -> Idle transitions using the
    // "CancelAttack" trigger with Has Exit Time unchecked on BOTH.
    public bool CancelAttackAndThrowWeapon()
    {
        if (isTripping || isRolling) return false;

        // Always interrupt the swing once this fires, even if the throw itself
        // can't happen — a fumble that can't launch anything should still cancel.
        isAttacking = false;
        ResetCombo();
        movement = Vector3.zero;
        animator.SetTrigger("CancelAttack");

        if (!weaponEquip || stamina.IsExhausted) return false;

        stamina.Drain(throwWeaponStaminaCost);
        ReleaseWeaponProjectile();
        return true;
    }

    // Hook this to an Animation Event on the ThrowWeapon clip, placed at the
    // exact frame the weapon should leave the player's hand.
    public void OnWeaponReleaseAnimationEvent()
    {
        ReleaseWeaponProjectile();
    }

    private void ReleaseWeaponProjectile()
    {
        weaponEquip = false;
        animator.SetBool("WeaponEquipped", weaponEquip);

        if (weaponSpinPrefab == null || weaponSpawnPoint == null) return;

        GameObject thrownWeapon = Instantiate(weaponSpinPrefab, weaponSpawnPoint.position, weaponSpawnPoint.rotation);

        if (thrownWeapon.TryGetComponent(out Rigidbody weaponRb))
        {
            // Prefabs are sometimes saved kinematic (e.g. for holding in-hand) —
            // force it into a simulated state so AddForce actually does something
            weaponRb.isKinematic = false;
            weaponRb.useGravity = true;

            Vector3 facing = new Vector3(Mathf.Sign(visualTransform.localScale.x), 0f, 0f);
            Vector3 throwDirection = (facing + Vector3.up * weaponThrowUpwardArc).normalized;

            weaponRb.AddForce(throwDirection * weaponThrowForce, ForceMode.Impulse);
            weaponRb.AddTorque(Vector3.forward * weaponSpinTorque, ForceMode.Impulse);
        }
    }

    public void EquipWeapon()
    {
        weaponEquip = true;
        animator.SetBool("WeaponEquipped", weaponEquip);
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
    public void ModifyMoveSpeed(float amount)
    {
        walkSpeed += amount;
        sprintSpeed += amount;
    }
}