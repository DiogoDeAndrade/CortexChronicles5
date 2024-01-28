using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class UI_ToggleSound : MonoBehaviour
{
    TextMeshProUGUI buttonText;

    void Start()
    {
        buttonText = GetComponentInChildren<TextMeshProUGUI>();
        SoundManager.ToggleSound(GameManager.isSoundEnabled);
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.isSoundEnabled)
            buttonText.text = "Sound: On";
        else
            buttonText.text = "Sound: Off";
    }

    public void ToggleSound()
    {
        GameManager.isSoundEnabled = !GameManager.isSoundEnabled;

        EventSystem.current.SetSelectedGameObject(null);

        SoundManager.ToggleSound(GameManager.isSoundEnabled);
    }
}
