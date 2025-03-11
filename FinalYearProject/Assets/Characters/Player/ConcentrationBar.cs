using UnityEngine;
using UnityEngine.UI;

public class ConcentrationBar : MonoBehaviour
{
    public Slider concentrationSlider;
    public Image fillImage;

    public Color relaxedColor = Color.green;
    public Color moderateColor = new Color(1f, 0.64f, 0f); // Orange
    public Color highConcentrationColor = Color.red;

    private float concentrationValue = 0f;
    public float sliderTransitionSpeed = 3f; // Speed for smooth transition

    private float displayedSliderValue = 0f;
    private bool isHighConcentration = false;
    private float sustainedHighConcentrationDuration = 2f;
    private float highConcentrationTimer = 0f;

    void Update()
    {
        // Smoothly update the slider value
        UpdateSliderValue();

        // Manage sustained high concentration logic
        HandleHighConcentrationTimer();
    }

    public void UpdateConcentration(float betaAlphaRatio)
    {
        concentrationValue = Mathf.Clamp01(betaAlphaRatio);
    }

    private void UpdateSliderValue()
    {
        displayedSliderValue = Mathf.Lerp(displayedSliderValue, concentrationValue, Time.deltaTime * sliderTransitionSpeed);
        concentrationSlider.value = displayedSliderValue;

        UpdateFillColor(displayedSliderValue);
    }

    private void UpdateFillColor(float value)
    {
        if (value < 0.5f)
        {
            fillImage.color = Color.Lerp(relaxedColor, moderateColor, value * 2f);
            isHighConcentration = false;
            highConcentrationTimer = 0f;
        }
        else if (value < 0.95f)
        {
            fillImage.color = Color.Lerp(moderateColor, highConcentrationColor, (value - 0.5f) * 2f);
            isHighConcentration = false;
            highConcentrationTimer = 0f;
        }
        else
        {
            fillImage.color = highConcentrationColor;
            isHighConcentration = true;
        }
    }

    private void HandleHighConcentrationTimer()
    {
        if (isHighConcentration)
        {
            highConcentrationTimer += Time.deltaTime;

            if (highConcentrationTimer >= sustainedHighConcentrationDuration)
            {
                Debug.Log("Sustained high concentration!");
                highConcentrationTimer = 0f;
            }
        }
        else
        {
            highConcentrationTimer = 0f;
        }
    }
}
