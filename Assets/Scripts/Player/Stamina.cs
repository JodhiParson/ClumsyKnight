using UnityEngine;
using UnityEngine.UI;

public class Stamina : MonoBehaviour
{
    public int maxStamina = 100;
    public float currentStamina;
    public Slider staminaBar;
    public Image fillImage;
    public Color normalColor = Color.white;
    public Color exhaustedColor = Color.red;

    public bool IsExhausted { get; private set; }

    void Start()
    {
        currentStamina = maxStamina;
        staminaBar.maxValue = maxStamina;
        staminaBar.value = currentStamina;
    }

    public void Drain(float amount)
    {
        currentStamina = Mathf.Clamp(currentStamina - amount, 0, maxStamina);
        staminaBar.value = currentStamina;

        if (currentStamina <= 0f)
        {
            IsExhausted = true;
            UpdateVisuals();
        }
    }

    public void Regen(float amount)
    {
        currentStamina = Mathf.Clamp(currentStamina + amount, 0, maxStamina);
        staminaBar.value = currentStamina;

        if (IsExhausted && currentStamina >= maxStamina * 0.5f)
        {
            IsExhausted = false;
            UpdateVisuals();
        }
    }
        private void UpdateVisuals()
    {
        if (fillImage != null)
        {
            fillImage.color = IsExhausted ? exhaustedColor : normalColor;
        }
    }
}