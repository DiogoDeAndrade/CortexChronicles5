using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField] private Emotion    emotion = Emotion.Neutral;
    [SerializeField] private float      moveSpeed = 50.0f;
    [SerializeField] private float      bounceAmplitude = 5.0f;
    [SerializeField] private float      bounceSpeed = 5.0f;
    [SerializeField] private Transform  leftPupil;
    [SerializeField] private Transform  rightPupil;
    [SerializeField] private Vector2    radius = new Vector2(4,4);
    [SerializeField] private Vector2    blinkIntervalMinMax = new Vector2(2, 4);
    [SerializeField] private Vector2    blinkTime = new Vector2(0.1f, 0.2f);
    [SerializeField] private GameObject selectionObject;
    [SerializeField] private Transform  characterGfx;
    [SerializeField] private Ruleset    ruleset;

    Animator                        animator;
    int                             apEmotionId;
    float                           blinkTimerA = 0;
    float                           blinkTimerB = 0;
    SpriteRenderer                  spriteRendererLeftPupil;
    SpriteRenderer                  spriteRendererRightPupil;
    Dictionary<Character, float>    distances;
    Vector2                         targetPos;
    float                           moveAngle = 0.0f;
    Vector3                         characterGfxBaseLocalPos;

    class RuleState
    {
        public bool trigger;
        public float timeOfTrigger;
    };
    Dictionary<Rule, RuleState>     ruleState;

    public Emotion activeEmotion => emotion;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        apEmotionId = Animator.StringToHash("Emotion");

        blinkTimerA = blinkIntervalMinMax.Random();
        blinkTimerB = 0;

        spriteRendererLeftPupil = leftPupil.GetComponent<SpriteRenderer>();
        spriteRendererRightPupil = rightPupil.GetComponent<SpriteRenderer>();

        distances = new Dictionary<Character, float>();
        ruleState = new Dictionary<Rule, RuleState>();
        // Evaluate rules
        foreach (var rule in ruleset.rules)
        {
            ruleState[rule] = new RuleState { trigger = false, timeOfTrigger = 0.0f };
        }

        selectionObject.SetActive(false);

        targetPos = transform.position.xy();

        characterGfxBaseLocalPos = characterGfx.localPosition;
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
            float eyeZ = leftPupil.position.z;
            if (emotion == Emotion.Sad)
            {
                leftPupil.position = new Vector3(leftEyePos.x, leftEyePos.y - radius.y, eyeZ);
                rightPupil.position = new Vector3(rightEyePos.x, rightEyePos.y - radius.y, eyeZ); 
            }
            else
            {
                var r = radius;
                if (emotion == Emotion.Angry) r *= 0.5f;

                var leftDir = (lookAtPos.position - leftEyePos).normalized;
                var rightDir = (lookAtPos.position - leftEyePos).normalized;
                leftPupil.position = new Vector3(leftEyePos.x + leftDir.x * r.x, leftEyePos.y + leftDir.y * r.y, eyeZ);
                rightPupil.position = new Vector3(rightEyePos.x + rightDir.x * r.x, rightEyePos.y + rightDir.y * r.y, eyeZ);
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

        // Movement
        float distanceToTarget = Vector2.Distance(targetPos, transform.position);
        if (distanceToTarget > 1)
        {
            var nextPos = Vector2.MoveTowards(transform.position.xy(), targetPos, moveSpeed * Time.deltaTime);

            transform.position = new Vector3(nextPos.x, nextPos.y, 0.0f);

            moveAngle += bounceSpeed * Time.deltaTime;
        }
        else
        {
            float distFromCenter = moveAngle % 180.0f;
            if (distFromCenter > 5.0f)
            {
                moveAngle += bounceSpeed * Time.deltaTime;
            }
        }

        characterGfx.localPosition = characterGfxBaseLocalPos + Vector3.up * bounceAmplitude * Mathf.Abs(Mathf.Sin(moveAngle * Mathf.Deg2Rad));

        var currentPos = transform.position;
        currentPos = new Vector3(currentPos.x, currentPos.y, GetZ(currentPos.y));
        transform.position = currentPos;

        EvaluateRules();
    }

    float GetZ(float y)
    {
        return 0.05f * (y + 1000.0f);
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

    public void Move(Vector2 delta)
    {
        targetPos = transform.position.xy() + delta;
    }

    void EvaluateRules()
    {
        // Evaluate rules
        foreach (var rule in ruleset.rules)
        {
            bool trigger = rule.CanTrigger(this, distances);
            if (trigger)
            {
                if (!ruleState[rule].trigger)
                {
                    ruleState[rule].trigger = true;
                    ruleState[rule].timeOfTrigger = Time.time;
                }
                rule.Run(this);
            }
            else
            {
                ruleState[rule].trigger = false;
            }
        }
    }

    public bool SetEmotion(Emotion emotion)
    {
        this.emotion = emotion;
        
        return true;
    }

    public float GetTimeOfRule(Rule rule)
    {
        if (!ruleState[rule].trigger) return Time.time + 1.0f;
        return ruleState[rule].timeOfTrigger;
    }
}
