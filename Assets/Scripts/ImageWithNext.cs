using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ImageWithNext : MonoBehaviour
{
    [SerializeField] string      nextScene = "TitleScreen";
    [SerializeField] CanvasGroup canvasGroup;

    void Start()
    {
        Fader.FadeIn(null, true);        
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Fader.FadeOut(() => { SceneManager.LoadScene(nextScene); });

            StartCoroutine(FadeOutSound());
        }
    }

    IEnumerator FadeOutSound()
    {
        AudioSource source = GetComponent<AudioSource>();
        if (source)
        {
            float originalVolume = source.volume;

            while (source.volume > 0.0f)
            {
                if (canvasGroup)
                {
                    source.volume = Mathf.Pow(1 - canvasGroup.alpha, 0.5f);
                }
                else
                {
                    source.volume = originalVolume * Mathf.Clamp01(source.volume - Time.deltaTime);
                }
                yield return null;
            }

            source.Stop();
        }
    }
}
