using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private bool tutorialEnable = true;
    [SerializeField] private bool soundEnable = true;

    static GameManager instance;

    public static bool isTutorialEnabled
    {
        get { return instance.tutorialEnable; }
        set { instance.tutorialEnable = value; }
    }
    public static bool isSoundEnabled
    {
        get { return instance.soundEnable; }
        set { instance.soundEnable = value; }
    }

    void Awake()
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

        DontDestroyOnLoad(gameObject);
    }
}
