using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class Pill : MonoBehaviour
{
    public enum SideEffect { None, Death, Random };

    [SerializeField] private SpriteRenderer leftSide;
    [SerializeField] private SpriteRenderer rightSide;
    [SerializeField] private bool           sourceIsAny = false;
    [SerializeField, ShowIf("sourceIsSpecific")] 
    private Emotion        sourceEmotion;
    [SerializeField] private Emotion        destEmotion;
    [SerializeField] private SideEffect     sideEffect;

    bool sourceIsSpecific => !sourceIsAny;

    void Start()
    {
        SetColors();
    }

    // Update is called once per frame
    void Update()
    {
        var hit = Physics2D.OverlapCircle(transform.position.xy(), 16.0f, CharacterManager.instance.characterLayer);
        if (hit != null)
        {
            var character = hit.GetComponent<Character>();
            if (character)
            {
                if ((sourceIsAny) || (character.activeEmotion == sourceEmotion))
                {
                    character.SetEmotion(destEmotion);
                }
                else
                {
                    switch (sideEffect)
                    {
                        case SideEffect.None:
                            break;
                        case SideEffect.Death:
                            character.Die();
                            break;
                        case SideEffect.Random:
                            Emotion newEmotion;
                            while (true)
                            {
                                newEmotion = (Emotion)Random.Range(0, 5);
                                if (newEmotion == destEmotion) continue;
                                if (newEmotion == character.activeEmotion) continue;
                                break;
                            }
                            character.SetEmotion(newEmotion);
                            break;
                        default:
                            break;
                    }
                }

                Destroy(gameObject);
            }
        }
    }

    private void OnValidate()
    {
        if (CharacterManager.instance == null)
        {
            CharacterManager.instance = FindObjectOfType<CharacterManager>();
        }
        SetColors();
    }

    private void SetColors()
    {
        if (leftSide)
        {
            if (sourceIsAny) leftSide.color = CharacterManager.instance.GetColor(destEmotion);
            else leftSide.color = CharacterManager.instance.GetColor(sourceEmotion);
        }
        if (rightSide)
        {
            rightSide.color = CharacterManager.instance.GetColor(destEmotion);
        }
    }
}
