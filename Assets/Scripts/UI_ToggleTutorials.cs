using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class UI_ToggleTutorials : MonoBehaviour
{
    TextMeshProUGUI buttonText;

    void Start()
    {
        buttonText = GetComponentInChildren<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.isTutorialEnabled)
            buttonText.text = "Tutorials: On";
        else
            buttonText.text = "Tutorials: Off";
    }

    public void ToggleTutorials()
    {
        GameManager.isTutorialEnabled = !GameManager.isTutorialEnabled;

        EventSystem.current.SetSelectedGameObject(null);
    }
}
