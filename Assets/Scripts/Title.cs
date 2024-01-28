using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Title : MonoBehaviour
{
    [SerializeField] CanvasGroup    canvasGroup;
    [SerializeField] AudioClip      hoverSound;
    [SerializeField] AudioClip      selectSound;
    [SerializeField] AudioClip      titleMusic;
    [SerializeField] string         gameScreen = "Story01";

    void Start()
    {
        Fader.FadeIn(null, true);
        SoundManager.PlayMusic(titleMusic);
    }

    public void PlayHoverSound()
    {
        SoundManager.PlaySound(hoverSound);
    }

    public void PlaySelectSound()
    {
        SoundManager.PlaySound(selectSound);
    }

    public void StartGame()
    {
        Fader.FadeOut(() => { SceneManager.LoadScene(gameScreen); });
    }

    public void ExitGame()
    {
        SoundManager.StopMusic();
        Fader.FadeOut(() => { Application.Quit(); });
    }
}
