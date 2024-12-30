using UnityEngine;
using UnityEngine.UI;

public class MoveObjectToTrack3D : MonoBehaviour
{
    public Color objectColor = Color.white; 
    public GameObject crosshair;
    private RectTransform rectTransform; 
    private Canvas parentCanvas; 
    public float currentIntensity = 1.00f;
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        parentCanvas = GetComponentInParent<Canvas>();

    } 

    public void PlaceBallAtPosition(Vector2 position)
    {
        if (rectTransform == null)
        {
            Debug.LogError("No RectTransform found. Ensure this object is part of a Canvas.");
            return;
        }

        rectTransform.anchorMin = position;
        rectTransform.anchorMax = position;
        rectTransform.anchoredPosition = Vector2.zero;  // Align to left-middle
    }

    public void PlaceCrosshairAtPosition(Vector2 position)
    {
        RectTransform crosshairRect = crosshair.GetComponent<RectTransform>();

        crosshairRect.anchorMin = position;
        crosshairRect.anchorMax = position;
        crosshairRect.anchoredPosition = Vector2.zero;  // Align crosshair to the bottom
    }
}