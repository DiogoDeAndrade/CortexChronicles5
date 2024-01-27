using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fader : MonoBehaviour
{
    static Fader instance;

    CanvasGroup canvasGroup;
    
    float           targetAlpha = 0.0f;
    System.Action   callback = null;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        if ((instance == null) || (instance == this))
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Update()
    {
        float alpha = canvasGroup.alpha;
        if (targetAlpha < alpha)
        {
            alpha = Mathf.Clamp01(alpha - Time.deltaTime);
            if (alpha <= targetAlpha)
            {
                targetAlpha = alpha;
                if (callback != null) callback();
            }
        }
        else if (targetAlpha > alpha)
        {
            alpha = Mathf.Clamp01(alpha + Time.deltaTime);
            if (alpha >= targetAlpha)
            {
                targetAlpha = alpha;
                if (callback != null) callback();
            }
        }
        canvasGroup.alpha = alpha;
    }

    public static void FadeOut(System.Action callback)
    {
        instance.callback = callback;
        instance.targetAlpha = 1.0f;
    }

    public static void FadeIn(System.Action callback, bool forceBlack)
    {
        if (forceBlack)
        {
            instance.canvasGroup.alpha = 1.0f;
        }
        instance.callback = callback;
        instance.targetAlpha = 0.0f;
    }

    public static bool exists()
    {
        return (instance != null);
    }
}
