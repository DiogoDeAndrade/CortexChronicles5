using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CycleColor : MonoBehaviour
{
    [SerializeField] private Color[] colors;
    [SerializeField] private float   cycleSpeed = 0.25f;

    int             index = 0;
    float           timer = 0;
    SpriteRenderer  spriteRenderer;

    void Start()
    {
        timer = cycleSpeed;

        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = colors[0];
    }

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            index = (index + 1) % colors.Length;

            timer = cycleSpeed;
            spriteRenderer.color = colors[index];
        }
    }
}
