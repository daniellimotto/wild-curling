using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpWheel : MonoBehaviour
{
    public enum PowerUpType { None, Jumbo, Mini, Freeze }

    public PowerUpType currentPowerUp = PowerUpType.None;
    public bool isSpinning = false;

    // Wheel configuration
    public Transform wheelTransform;
    public Transform indicatorTransform;

    // Spin settings
    public float spinDuration = 3f; // How long the wheel spins
    
    // Audio settings
    public AudioSource spinningAudio;
    public AudioSource powerUpSelectedAudio;

    // Materials for each sector
    public Material jumboMaterial;
    public Material miniMaterial;
    public Material freezeMaterial;
    public Material defaultMaterial; // Material to reset to

    // References to renderers (assign these in inspector)
    public Renderer jumboSectorRenderer;
    public Renderer miniSectorRenderer;
    public Renderer freezeSectorRenderer;

    // Debug
    public bool debugMode = true;

    void Start()
    {
        if (wheelTransform == null)
            wheelTransform = transform;
            
        // Initialize with no power-up
        ResetPowerUp();
        
        // Audio setup and verification
        SetupAudio();
    }

    void SetupAudio()
    {
        // Check spinning audio
        if (spinningAudio == null)
        {
            Debug.LogError("Spinning Audio Source is not assigned!");
            return;
        }
        
        if (spinningAudio.clip == null)
        {
            Debug.LogError("No audio clip assigned to Spinning Audio Source!");
            return;
        }
        
        // Check selection audio
        if (powerUpSelectedAudio == null)
        {
            Debug.LogError("Power Up Selected Audio Source is not assigned!");
            return;
        }
        
        if (powerUpSelectedAudio.clip == null)
        {
            Debug.LogError("No audio clip assigned to Power Up Selected Audio Source!");
            return;
        }
        
        // Enable audio sources
        spinningAudio.enabled = true;
        powerUpSelectedAudio.enabled = true;
        
        // Configure audio sources
        spinningAudio.playOnAwake = false;
        powerUpSelectedAudio.playOnAwake = false;
        
        Debug.Log("Audio setup completed successfully!");
    }

    // Call this method to start spinning the wheel
    public void SpinWheel()
    {
        if (!isSpinning)
        {
            StartCoroutine(SpinRoutine());
        }
    }

    IEnumerator SpinRoutine()
    {
        isSpinning = true;
        currentPowerUp = PowerUpType.None;

        // Play spinning sound with detailed error checking
        if (spinningAudio != null)
        {
            if (!spinningAudio.enabled)
            {
                Debug.LogWarning("Spinning audio was disabled! Enabling now...");
                spinningAudio.enabled = true;
            }
            
            if (spinningAudio.clip == null)
            {
                Debug.LogError("No audio clip assigned to spinning audio!");
            }
            else
            {
                try
                {
                    spinningAudio.Play();
                    Debug.Log("Successfully started playing spinning sound");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error playing spinning sound: {e.Message}");
                }
            }
        }
        else
        {
            Debug.LogError("Spinning Audio Source is missing!");
        }

        // Randomly determine which power-up we'll land on
        PowerUpType targetPowerUp = (PowerUpType)Random.Range(1, 4); // 1=Jumbo, 2=Mini, 3=Freeze
        
        // Calculate target angle based on the power-up
        float targetAngle = 0f;
        switch (targetPowerUp)
        {
            case PowerUpType.Jumbo:
                targetAngle = Random.Range(210f, 330f); // Somewhere in Jumbo range
                break;
            case PowerUpType.Mini:
                if (Random.value > 0.5f)
                    targetAngle = Random.Range(330f, 360f); // First Mini range
                else
                    targetAngle = Random.Range(0f, 90f);    // Second Mini range
                break;
            case PowerUpType.Freeze:
                targetAngle = Random.Range(90f, 210f); // Somewhere in Freeze range
                break;
        }
        
        // Add some full rotations
        int rotations = Random.Range(2, 5); // 2-4 full rotations
        targetAngle += rotations * 360f;
        
        // Start from current angle
        float startAngle = wheelTransform.eulerAngles.x;
        float elapsed = 0f;
        
        if (debugMode) Debug.Log("Starting wheel spin from " + startAngle + " to " + targetAngle);
        
        // Perform spin animation
        while (elapsed < spinDuration)
        {
            elapsed += Time.deltaTime;
            
            // Use easing function for natural slowdown
            float t = elapsed / spinDuration;
            float easedT = 1f - Mathf.Pow(1f - t, 3f); // Cubic ease out
            
            // Calculate current angle
            float currentAngle = Mathf.Lerp(startAngle, targetAngle, easedT);

            // Apply rotation
            wheelTransform.rotation = Quaternion.Euler(currentAngle, 0f, 90f);

            yield return null;
        }

        // Ensure final position is exactly at target angle
        wheelTransform.rotation = Quaternion.Euler(targetAngle, 0f, 90f);

        // Determine power-up from final angle
        float finalAngle = Mathf.Repeat(targetAngle, 360f);
        DeterminePowerUp(finalAngle);
        
        // Stop spinning sound with error checking
        if (spinningAudio != null)
        {
            if (spinningAudio.isPlaying)
            {
                try
                {
                    spinningAudio.Stop();
                    Debug.Log("Successfully stopped spinning sound");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error stopping spinning sound: {e.Message}");
                }
            }
        }
            
        // Play power-up selected sound with error checking
        if (powerUpSelectedAudio != null)
        {
            if (!powerUpSelectedAudio.enabled)
            {
                Debug.LogWarning("Power-up selected audio was disabled! Enabling now...");
                powerUpSelectedAudio.enabled = true;
            }
            
            if (powerUpSelectedAudio.clip == null)
            {
                Debug.LogError("No audio clip assigned to power-up selected audio!");
            }
            else
            {
                try
                {
                    powerUpSelectedAudio.Play();
                    Debug.Log("Successfully started playing selection sound");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error playing selection sound: {e.Message}");
                }
            }
        }
        else
        {
            Debug.LogError("Power Up Selected Audio Source is missing!");
        }
            
        isSpinning = false;
    }

    void DeterminePowerUp(float angle)
    {
        if (debugMode) Debug.Log("Final wheel angle: " + angle);
        
        // Normalize angle to 0-360 range
        angle = Mathf.Repeat(angle, 360f);
        
        // Determine power-up based on angle
        // Red (Jumbo) range: 210° to 330°
        if (angle >= 210f && angle < 330f)
        {
            currentPowerUp = PowerUpType.Jumbo;
            if (debugMode) Debug.Log("JUMBO power-up selected!");
            // Apply visual change
            ApplyPowerUpVisual(PowerUpType.Jumbo);
        }
        // Green (Mini) range: 330° to 360° and 0° to 90°
        else if ((angle >= 330f && angle <= 360f) || (angle >= 0f && angle < 90f))
        {
            currentPowerUp = PowerUpType.Mini;
            if (debugMode) Debug.Log("MINI power-up selected!");
            // Apply visual change
            ApplyPowerUpVisual(PowerUpType.Mini);
        }
        // Blue (Freeze) range: 90° to 210°
        else if (angle >= 90f && angle < 210f)
        {
            currentPowerUp = PowerUpType.Freeze;
            if (debugMode) Debug.Log("FREEZE power-up selected!");
            // Apply visual change
            ApplyPowerUpVisual(PowerUpType.Freeze);
        }
    }
    
    void ApplyPowerUpVisual(PowerUpType powerUpType)
    {
        // Reset all renderers to their original materials
        ResetAllSectorVisuals();
        
        // Apply highlight based on power-up type
        switch (powerUpType)
        {
            case PowerUpType.Jumbo:
                if (jumboSectorRenderer != null && jumboMaterial != null)
                {
                    // Make a copy of the material to avoid affecting all objects using this material
                    Material highlightMaterial = new Material(jumboMaterial);
                    highlightMaterial.EnableKeyword("_EMISSION");
                    highlightMaterial.SetColor("_EmissionColor", Color.red * 2.0f);
                    jumboSectorRenderer.material = highlightMaterial;
                    
                    if (debugMode) Debug.Log("Applied Jumbo visual highlight");
                }
                break;
                
            case PowerUpType.Mini:
                if (miniSectorRenderer != null && miniMaterial != null)
                {
                    Material highlightMaterial = new Material(miniMaterial);
                    highlightMaterial.EnableKeyword("_EMISSION");
                    highlightMaterial.SetColor("_EmissionColor", Color.green * 2.0f);
                    miniSectorRenderer.material = highlightMaterial;
                    
                    if (debugMode) Debug.Log("Applied Mini visual highlight");
                }
                break;
                
            case PowerUpType.Freeze:
                if (freezeSectorRenderer != null && freezeMaterial != null)
                {
                    Material highlightMaterial = new Material(freezeMaterial);
                    highlightMaterial.EnableKeyword("_EMISSION");
                    highlightMaterial.SetColor("_EmissionColor", Color.blue * 2.0f);
                    freezeSectorRenderer.material = highlightMaterial;
                    
                    if (debugMode) Debug.Log("Applied Freeze visual highlight");
                }
                break;
        }
    }
    
    void ResetAllSectorVisuals()
    {
        // Reset jumbo sector renderer
        if (jumboSectorRenderer != null && jumboMaterial != null)
            jumboSectorRenderer.material = jumboMaterial;
            
        // Reset mini sector renderer
        if (miniSectorRenderer != null && miniMaterial != null)
            miniSectorRenderer.material = miniMaterial;
            
        // Reset freeze sector renderer
        if (freezeSectorRenderer != null && freezeMaterial != null)
            freezeSectorRenderer.material = freezeMaterial;
    }

    public void ResetPowerUp()
    {
        currentPowerUp = PowerUpType.None;
        
        // Reset all sector visuals
        ResetAllSectorVisuals();
    }
    
    // This can be called from GameController when needed
    public PowerUpType GetCurrentPowerUp()
    {
        return currentPowerUp;
    }
}
