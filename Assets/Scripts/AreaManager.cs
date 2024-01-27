using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.SceneManagement;

public class AreaManager : MonoBehaviour
{
    public enum Goal { CountReachPosition };

    [SerializeField]
    private string      nextLevelScene;

    [SerializeField] 
    private Goal        goal;
    [SerializeField, ShowIf("needTarget")]
    private Transform   target;
    [SerializeField, ShowIf("needRadius")]
    private float       radius = 100.0f;
    [SerializeField, ShowIf("needCount")]
    private int         count = 1;
    [SerializeField, ShowIf("needEmotion")]
    private bool        checkEmotion;
    [SerializeField, ShowIf(EConditionOperator.And, "needEmotion", "checkEmotion")]
    private Emotion     emotion;

    [SerializeField]
    private GameObject  congratulations;

    bool _isPlaying = true;

    public static bool isPlaying => instance._isPlaying;

    bool needCount => goal == Goal.CountReachPosition;
    bool needTarget => goal == Goal.CountReachPosition;
    bool needRadius => goal == Goal.CountReachPosition;
    bool needEmotion => goal == Goal.CountReachPosition;

    static AreaManager instance;

    private void Awake()
    {
        if ((instance == null) || (instance == this))
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        _isPlaying = false;
    }

    void Start()
    {
        congratulations.SetActive(false);

        Fader.FadeIn(() => { _isPlaying = true; }, true);
    }

    // Update is called once per frame
    void Update()
    {
        if (!_isPlaying) return;

        if (IsSuccess())
        {
            _isPlaying = false;
            congratulations.SetActive(true);
        }
    }

    bool IsSuccess()
    { 
        switch (goal)
        {
            case Goal.CountReachPosition:
                return CountReachPosition();
            default:
                break;
        }

        return false;
    }

    bool CountReachPosition()
    {
        // Get all near the target position
        var hits = Physics2D.OverlapCircleAll(target.position, radius, CharacterManager.instance.characterLayer);
        int c = 0;
        foreach (var hit in hits)
        {
            var character = hit.GetComponent<Character>();
            if (character)
            {
                if (character.isDead) continue;
                if (checkEmotion)
                {
                    if (character.activeEmotion == emotion)
                    {
                        c++;
                    }
                }
                else
                {
                    c++;
                }
            }
        }

        return (c >= count);
    }

    public void NextLevel()
    {
        if (IsSuccess())
        {
            Fader.FadeOut(() => { SceneManager.LoadScene(nextLevelScene); });
        }
    }

    private void OnDrawGizmosSelected()
    {
        switch (goal)
        {
            case Goal.CountReachPosition:
                if (target)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireSphere(target.position, radius);
                }
                break;
            default:
                break;
        }
    }
}
