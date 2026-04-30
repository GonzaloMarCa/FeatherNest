using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UICooldownManager : MonoBehaviour
{
    [Header("Referencias del Jugador")]
    [SerializeField] private PlayerMovement playerMovement;
    
    [Header("Elementos UI de Cooldown")]
    [SerializeField] private Slider attackCooldownSlider;
    [SerializeField] private Slider bombCooldownSlider;
    [SerializeField] private TextMeshProUGUI attackCooldownText;
    [SerializeField] private TextMeshProUGUI bombCooldownText;
    
    [Header("Opcional - Efectos Visuales")]
    [SerializeField] private Color cooldownActiveColor = Color.gray;
    [SerializeField] private Color cooldownReadyColor = Color.green;
    
    private Image attackFillImage;
    private Image bombFillImage;
    
    void Start()
    {
        // Buscar al jugador si no está asignado
        if (playerMovement == null)
        {
            playerMovement = FindObjectOfType<PlayerMovement>();
        }
        
        // Obtener las imágenes de relleno de los sliders
        if (attackCooldownSlider != null)
        {
            attackFillImage = attackCooldownSlider.fillRect.GetComponent<Image>();
        }
        
        if (bombCooldownSlider != null)
        {
            bombFillImage = bombCooldownSlider.fillRect.GetComponent<Image>();
        }
    }
    
    void Update()
    {
        UpdateAttackCooldown();
        UpdateBombCooldown();
    }
    
    void UpdateAttackCooldown()
    {
        if (playerMovement == null || attackCooldownSlider == null) return;
        
        // Obtener el progreso del cooldown (0 = en cooldown, 1 = listo)
        float progress = playerMovement.GetAttackCooldownProgress();
        attackCooldownSlider.value = progress;
        
        // Actualizar texto
        float remainingTime = playerMovement.GetAttackCooldownRemaining();
        if (attackCooldownText != null)
        {
            if (remainingTime > 0)
            {
                attackCooldownText.text = $"Ataque: {remainingTime:F1}s";
            }
            else
            {
                attackCooldownText.text = "Ataque: LISTO";
            }
        }
        
        // Cambiar color según estado
        if (attackFillImage != null)
        {
            attackFillImage.color = progress >= 0.99f ? cooldownReadyColor : cooldownActiveColor;
        }
    }
    
    void UpdateBombCooldown()
    {
        if (playerMovement == null || bombCooldownSlider == null) return;
        
        // Obtener el progreso del cooldown
        float progress = playerMovement.GetBombCooldownProgress();
        bombCooldownSlider.value = progress;
        
        // Actualizar texto
        float remainingTime = playerMovement.GetBombCooldownRemaining();
        if (bombCooldownText != null)
        {
            if (remainingTime > 0)
            {
                bombCooldownText.text = $"Bomba: {remainingTime:F1}s";
            }
            else
            {
                bombCooldownText.text = "Bomba: LISTA";
            }
        }
        
        // Cambiar color según estado
        if (bombFillImage != null)
        {
            bombFillImage.color = progress >= 0.99f ? cooldownReadyColor : cooldownActiveColor;
        }
    }
}