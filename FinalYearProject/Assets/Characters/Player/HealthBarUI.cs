
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    public Slider healthSlider; // Drag your Slider component here in Unity
    public Image fillImage; // Drag the Fill image of the slider here (optional for color change)

    public Color fullHealthColor = Color.green;
    public Color lowHealthColor = Color.red;

    private float maxHealth = 100f; // Set max HP
    public Health currhealth;
    // Start is called before the first frame update
    void Start()
    {


    }
    private void Update()
    {
        UpdateHealthUI();
    }

    //public void Heal(float healAmount)
    //{
    //    currentHealth += healAmount;
    //    currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    //    UpdateHealthUI();
    //}
    private void UpdateHealthUI()
    {
        healthSlider.value = currhealth.health / maxHealth; // Normalize between 0 and 1

        // Change bar color based on health percentage
        fillImage.color = Color.Lerp(lowHealthColor, fullHealthColor, healthSlider.value);
    }

}


