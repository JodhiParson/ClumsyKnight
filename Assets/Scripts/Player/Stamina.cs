using UnityEngine;
using UnityEngine.UI;

public class Stamina : MonoBehaviour
{
    public int maxStamina = 100;
    public float currentStamina;
    public Slider staminaBar;

    [Header("Visuals")]
    public Image fillImage;
    public Color normalColor = Color.green;
    public Color exhaustedColor = Color.red;

    [Header("Regen Delay")]
    [SerializeField] private float regenDelay = 1f; // seconds to wait after last drain
    private float timeSinceLastDrain;

    private bool isExhausted;
    public bool IsExhausted => isExhausted;

    void Start()
    {
        currentStamina = maxStamina;
        staminaBar.maxValue = maxStamina;
        staminaBar.value = currentStamina;

        if (fillImage == null && staminaBar.fillRect != null)
        {
            fillImage = staminaBar.fillRect.GetComponent<Image>();
        }

        UpdateVisuals();
    }

    void Update()
    {
        timeSinceLastDrain += Time.deltaTime;
    }

    public void Drain(float amount)
    {
        currentStamina = Mathf.Clamp(currentStamina - amount, 0, maxStamina);
        staminaBar.value = currentStamina;
        timeSinceLastDrain = 0f; // reset the delay timer every time stamina is used

        if (currentStamina <= 0f && !isExhausted)
        {
            isExhausted = true;
            UpdateVisuals();
        }
    }

    public void Regen(float amount)
    {
        if (timeSinceLastDrain < regenDelay) return; // still on cooldown, do nothing

        currentStamina = Mathf.Clamp(currentStamina + amount, 0, maxStamina);
        staminaBar.value = currentStamina;

        if (isExhausted && currentStamina >= maxStamina * 0.5f)
        {
            isExhausted = false;
            UpdateVisuals();
        }
    }

    private void UpdateVisuals()
    {
        if (fillImage != null)
        {
            fillImage.color = isExhausted ? exhaustedColor : normalColor;
        }
    }
}