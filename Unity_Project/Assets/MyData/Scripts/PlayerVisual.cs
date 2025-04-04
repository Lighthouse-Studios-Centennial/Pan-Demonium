using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    [SerializeField] private Renderer[] renderers;

    private Material material;

    private void Awake()
    {
        material = new(renderers[0].material);

        foreach (var renderer in renderers)
        {
            renderer.material = material;
        }
    }

    public void SetPlayerColor(Color color)
    {
        material.color = color;
    }
}
