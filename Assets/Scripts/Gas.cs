using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class Gas : MonoBehaviour
{
    [SerializeField] 
    private bool       deathGas;    
    [SerializeField, HideIf("deathGas")] 
    private Emotion destEmotion;

    ParticleSystem particleSystem;
    SpriteRenderer spriteRenderer;

    void Awake()
    {
        SetupColor();
    }

    void Update()
    {
        var hits = Physics2D.OverlapCapsuleAll(transform.position.xy(), new Vector2(64.0f, 16.0f), CapsuleDirection2D.Horizontal, 0.0f, CharacterManager.instance.characterLayer);

        if ((hits != null) && (hits.Length > 0))
        {
            foreach (var hit in hits)
            {
                var character = hit.GetComponent<Character>();
                if ((character) && (!character.isDead))
                {
                    if (deathGas)
                    {
                        character.Die();
                    }
                    else
                    {
                        character.SetEmotion(destEmotion);
                    }
                }
            }
        }
    }

    private void OnValidate()
    {
        SetupColor();
    }

    private void SetupColor()
    {
        particleSystem = GetComponent<ParticleSystem>();

        if (CharacterManager.instance == null)
        {
            CharacterManager.instance = FindObjectOfType<CharacterManager>();
        }

        Color c = (deathGas) ? Color.magenta : (CharacterManager.instance.GetColor(destEmotion));

        var mainProps = particleSystem.main;
        c.a = mainProps.startColor.color.a;
        mainProps.startColor = c;

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        spriteRenderer.color = c;
    }
}
