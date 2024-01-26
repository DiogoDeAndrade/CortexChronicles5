using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField] private Emotion    emotion = Emotion.Neutral;
    [SerializeField] private Transform  leftPupil;
    [SerializeField] private Transform  rightPupil;
    [SerializeField] private Vector2    radius = new Vector2(4,4);
    [SerializeField] private Vector2    blinkIntervalMinMax = new Vector2(2, 4);
    [SerializeField] private Vector2    blinkTime = new Vector2(0.1f, 0.2f);
    [SerializeField] private GameObject selectionObject;

    Animator                        animator;
    int                             apEmotionId;
    float                           blinkTimerA = 0;
    float                           blinkTimerB = 0;
    SpriteRenderer                  spriteRendererLeftPupil;
    SpriteRenderer                  spriteRendererRightPupil;
    Dictionary<Character, float>    distances;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        apEmotionId = Animator.StringToHash("Emotion");

        blinkTimerA = blinkIntervalMinMax.Random();
        blinkTimerB = 0;

        spriteRendererLeftPupil = leftPupil.GetComponent<SpriteRenderer>();
        spriteRendererRightPupil = rightPupil.GetComponent<SpriteRenderer>();

        distances = new Dictionary<Character, float>();

        selectionObject.SetActive(false);
    }

    private void Start()
    {
        CharacterManager.instance.Add(this);
    }

    void OnDestroy()
    {
        CharacterManager.instance.Remove(this);
    }

    void Update()
    {
        animator.SetInteger(apEmotionId, (int)emotion);

        var lookAtPos = GetClosestCharacter()?.transform;
        
        if (lookAtPos)
        {
            var leftEyePos = leftPupil.parent.position;
            var rightEyePos = rightPupil.parent.position;
            if (emotion == Emotion.Sad)
            {
                leftPupil.position = leftEyePos + Vector3.down.Multiply(radius);
                rightPupil.position = rightEyePos + Vector3.down.Multiply(radius);
            }
            else
            {
                var r = radius;
                if (emotion == Emotion.Angry) r *= 0.5f;

                leftPupil.position = leftEyePos + (lookAtPos.position - leftEyePos).normalized.Multiply(r);
                rightPupil.position = rightEyePos + (lookAtPos.position - rightEyePos).normalized.Multiply(r);
            }
        }

        if (blinkTimerA > 0)
        {
            blinkTimerA -= Time.deltaTime;

            if (blinkTimerA <= 0)
            {
                blinkTimerB = blinkTime.Random();
            }
        }
        if (blinkTimerB > 0)
        {
            blinkTimerB -= Time.deltaTime;
            if (blinkTimerB <= 0)
            {
                blinkTimerA = blinkIntervalMinMax.Random();
            }
        }

        spriteRendererLeftPupil.enabled = spriteRendererRightPupil.enabled = (blinkTimerA > 0);
    }

    public void SetDistance(Character character, float distance)
    {
        distances[character] = distance;
    }

    public void RemoveDistance(Character c)
    {
        distances.Remove(c);
    }

    public Character GetClosestCharacter()
    {
        float       minDist = float.MaxValue;
        Character   closest = null;

        foreach (var c in distances)
        {
            if (c.Value < minDist)
            {
                closest = c.Key;
                minDist = c.Value;
            }
        }

        return closest;
    }

    public void Select(bool b)
    {
        selectionObject.SetActive(b);
    }
}
