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
    bool                            forceMove = false;
    Vector3                         commandTargetPos;
    float                           moveCooldown;
    float                           nextMoveCooldown;
    float                           speedFactor = 1.0f;

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
        commandTargetPos = transform.position.xy();
        moveCooldown = 2.0f;

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

        (var closest, var dist) = GetClosestCharacter();
        var lookAtPos = closest?.transform;
        
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
        if (forceMove)
        {
            targetPos = commandTargetPos;
        }
        else
        {
            if (!IsMoving())
            {
                moveCooldown -= Time.deltaTime;
                if (moveCooldown <= 0.0f)
                {
                    RandomMovement();
                }
            }
        }

        float distanceToTarget = Vector2.Distance(targetPos, transform.position);
        if (distanceToTarget > 1)
        {
            var nextPos = Vector2.MoveTowards(transform.position.xy(), targetPos, speedFactor * moveSpeed * Time.deltaTime);

            transform.position = new Vector3(nextPos.x, nextPos.y, 0.0f);

            moveAngle += bounceSpeed * Time.deltaTime;
            moveCooldown = nextMoveCooldown;
        }
        else
        {
            forceMove = false;

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

    public (Character, float) GetClosestCharacter()
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

        return (closest, minDist);
    }

    public (Character, float) GetClosestCharacter(Emotion[] emotions)
    {
        float minDist = float.MaxValue;
        Character closest = null;

        foreach (var c in distances)
        {
            if (c.Key.emotion.IsContained(emotions))
            {
                if (c.Value < minDist)
                {
                    closest = c.Key;
                    minDist = c.Value;
                }
            }
        }

        return (closest, minDist);
    }

    public void Select(bool b)
    {
        selectionObject.SetActive(b);
    }

    public bool IsMoving()
    {
        if (Vector2.Distance(targetPos, transform.position.xy()) > 1) return true;

        return false;
    }

    public Vector2 GetDeltaMovement()
    {
        return targetPos - transform.position.xy();
    }

    public void ForceMove(Vector2 delta)
    {
        commandTargetPos = targetPos = transform.position.xy() + delta;
        forceMove = true;
        speedFactor = 1.0f;
        nextMoveCooldown = 2.0f;
    }

    public float GetSpeedFactor() => speedFactor;

    void RandomMovement()
    {
        Vector2 target = transform.position.xy();

        switch (emotion)
        {
            case Emotion.Happy:
                {
                    // Happy checks if any Happy near him is moving, if it is, it also moves towards that position, relative to
                    // him (but slower)
                    var allHappy = GetAllCharacters(new[] { Emotion.Happy }, 150.0f);
                    bool foundMoving = false;
                    foreach (var c in allHappy)
                    {
                        if (c.IsMoving())
                        {
                            Vector2 delta = c.GetDeltaMovement();
                            target = target + delta;
                            foundMoving = true;
                            speedFactor = c.GetSpeedFactor() * 0.8f;
                            nextMoveCooldown = 2.0f;
                            break;
                        }
                    }
                    if (!foundMoving)
                    {
                        // Nobody was moving, let's move and (hopefully) ake everybody with us!
                        target = new Vector2(Random.Range(-500.0f, 500.0f), Random.Range(-275.0f, 275.0f));
                        speedFactor = 0.6f;
                        nextMoveCooldown = 2.0f;
                    }
                }
                break;
            case Emotion.Sad:
                {
                    // Sad tries to get away from closest person, at half-speed
                    (var closest, var distance) = GetClosestCharacter();
                    if (closest)
                    {
                        if (distance < 200.0f)
                        {
                            target = target + (target - closest.transform.position.xy()).normalized * 40.0f;
                            speedFactor = 0.5f;
                            nextMoveCooldown = 2.0f;
                        }
                    }
                }
                break;
            case Emotion.Angry:
                {
                    // Angry finds the closest Happy, Neutral or Serene and moves towards them
                    (var closest, var distance) = GetClosestCharacter(new [] { Emotion.Happy, Emotion.Neutral, Emotion.Serene });
                    if ((closest) && (distance > 40.0f))
                    {
                        target = target + (closest.transform.position.xy() - target).normalized * 40.0f;
                        speedFactor = 0.75f;
                        nextMoveCooldown = 2.0f;
                    }
                }
                break;
            case Emotion.Serene:
                // Serene doesn't move, just stays in the same place
                break;
            case Emotion.Neutral:
                {
                    // Neutral moves around the place where he is after the last movement
                    target = commandTargetPos.xy() + Random.insideUnitCircle * 60.0f;
                    speedFactor = 1.0f;
                    nextMoveCooldown = 4.0f;
                }
                break;
            default:
                break;
        }

        // Clamp to inside of play area

        targetPos = target;
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

    List<Character> GetAllCharacters(Emotion[] emotions, float maxRadius)
    {
        List<Character> ret = new List<Character>();

        foreach (var c in distances)
        {
            if (c.Value < maxRadius)
            {
                if (c.Key.emotion.IsContained(emotions))
                {
                    ret.Add(c.Key);
                }
            }
        }

        return ret;
    }
}
