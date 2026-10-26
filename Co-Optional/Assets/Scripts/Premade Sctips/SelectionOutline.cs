using UnityEngine;

/// <summary>
/// Simple outline effect. Learn how it works at https://youtube.com/shorts/FyEiPibJuRU?si=Ub_IZIuz2gzYIk9d
/// </summary>
public class SelectionOutline : MonoBehaviour
{
    private int m_defaultLayer;
    private string m_highlightLayerName = "Outline";
    private int m_highlightLayer;

    private void Awake()
    {
        m_defaultLayer = gameObject.layer;
        m_highlightLayer = LayerMask.NameToLayer(m_highlightLayerName);
    }

    public void ToggleOutline(bool isHighlighted)
    {
        int layer = isHighlighted ? m_highlightLayer : m_defaultLayer;
        SetLayerRecursively(transform, layer);
    }

    private void SetLayerRecursively(Transform current, int layer)
    {
        current.gameObject.layer = layer;

        foreach (Transform child in current)
        {
            SetLayerRecursively(child, layer);
        }
    }
}
