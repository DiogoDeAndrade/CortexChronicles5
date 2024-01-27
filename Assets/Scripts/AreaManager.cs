using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;
using UnityEngine.EventSystems;

public class AreaManager : MonoBehaviour
{
    public enum Goal { CountReachPosition };

    [SerializeField]
    private string      nextLevelScene;
    [SerializeField, ResizableTextArea]
    private string[]    tutorial; 

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
    private GameObject      congratulations;
    [SerializeField]
    private CanvasGroup     tutorialObject;
    [SerializeField]
    private TextMeshProUGUI nextTutorialPage;

    bool            _isPlaying = true;
    float           tutorialTargetAlpha = 0.0f;
    TextMeshProUGUI tutorialText;
    int             tutorialIndex;

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
        tutorialObject.gameObject.SetActive(false);
        tutorialText = tutorialObject.GetComponentInChildren<TextMeshProUGUI>();

        Fader.FadeIn(() => 
        {
            if (GameManager.isTutorialEnabled)
            {
                tutorialObject.gameObject.SetActive(true);
                tutorialObject.alpha = 0.0f;
                tutorialTargetAlpha = 1.0f;
                tutorialIndex = 0;
                tutorialText.text = tutorial[0];

                if (tutorial.Length > 1)
                {
                    nextTutorialPage.text = "Next page...";
                }
                else
                {
                    nextTutorialPage.text = "Start!";
                }
            }
            else
            {
                _isPlaying = true;
            }
        }, true);
    }

    // Update is called once per frame
    void Update()
    {
        if (tutorialObject.gameObject.activeSelf)
        {
            if (tutorialTargetAlpha < tutorialObject.alpha)
            {
                tutorialObject.alpha = Mathf.Clamp01(tutorialObject.alpha - Time.deltaTime);
                if (tutorialObject.alpha <= tutorialTargetAlpha)
                {
                    tutorialObject.alpha = tutorialTargetAlpha;
                    tutorialObject.gameObject.SetActive(false);
                    // Start playing
                    _isPlaying = true;
                }
            }
            else if (tutorialTargetAlpha > tutorialObject.alpha)
            {
                tutorialObject.alpha = Mathf.Clamp01(tutorialObject.alpha + Time.deltaTime);
                if (tutorialObject.alpha >= tutorialTargetAlpha)
                {
                    tutorialObject.alpha = tutorialTargetAlpha;
                    // Wait for next page...                
                }
            }
        }

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

    public void NextTutorial()
    {
        tutorialIndex++;
        if (tutorialIndex >= tutorial.Length)
        {
            tutorialTargetAlpha = 0.0f;
        }
        else
        {
            tutorialText.text = tutorial[tutorialIndex];

            if (tutorial.Length > (tutorialIndex + 1))
            {
                nextTutorialPage.text = "Next page...";
            }
            else
            {
                nextTutorialPage.text = "Start!";
            }

            EventSystem.current.SetSelectedGameObject(null);
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
