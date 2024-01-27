using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.TextCore.Text;

[CreateAssetMenu(menuName = "Cortex/Rule")]
public class Rule : ScriptableObject
{
    public enum Type { ConversionRule };
    public enum Comparison { Less, LessOrEqual, Equal, NotEqual, Greater, GreaterOrEqual };

    [SerializeField] public Type type = Type.ConversionRule;

    [SerializeField, ShowIf("isConversionRule")]
    private Emotion[]   sourceEmotions;
    [SerializeField, ShowIf("isConversionRule")]
    private Emotion[]   neighbourEmotions;
    [SerializeField, ShowIf("isConversionRule")]
    private float       radiusScale = 1.0f;
    [SerializeField, ShowIf("isConversionRule")]
    private Comparison  comparison = Comparison.GreaterOrEqual;
    [SerializeField, ShowIf("isConversionRule")]
    private int         count = 1;
    [SerializeField, ShowIf("isConversionRule")]
    private Emotion     targetEmotion;
    [SerializeField, ShowIf("isConversionRule")]
    private bool        afterTime = false;
    [SerializeField, ShowIf("needTime")]
    private float       time = 2.0f;

    bool isConversionRule => type == Type.ConversionRule;
    bool needTime => isConversionRule && afterTime;

    public bool CanTrigger(Character character, Dictionary<Character, float> distances)
    {
        if (isConversionRule) return CanTriggerConversion(character, distances);

        return false;
    }

    bool CanTriggerConversion(Character character, Dictionary<Character, float> distances)
    { 
        if (!character.activeEmotion.IsContained(sourceEmotions)) return false;

        float r = CharacterManager.instance.neighborRadius * radiusScale;

        int c = 0;
        foreach (var d in distances)
        {
            if (d.Value < r)
            {
                if (d.Key.activeEmotion.IsContained(neighbourEmotions))
                {
                    c++;
                }
            }
        }

        switch (comparison)
        {
            case Comparison.Less:
                return c < count;
            case Comparison.LessOrEqual:
                return c <= count;
            case Comparison.Equal:
                return c == count;
            case Comparison.NotEqual:
                return c != count;
            case Comparison.Greater:
                return c > count;
            case Comparison.GreaterOrEqual:
                return c >= count;
            default:
                break;
        }

        return false;
    }

    public bool Run(Character character)
    {
        switch (type)
        {
            case Type.ConversionRule:
                return RunConversion(character);
            default:
                break;
        }

        return false;
    }

    bool RunConversion(Character character)
    {       
        if (needTime)
        {
            float t = character.GetTimeOfRule(this);
            if ((Time.time - t) > time)
            {
                return character.SetEmotion(targetEmotion);
            }
            return false;
        }

        return character.SetEmotion(targetEmotion);
    }
}
