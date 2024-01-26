using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager instance;

    [SerializeField] private Camera mainCamera;
    [SerializeField] private float  selectionRadius = 32.0f;

    List<Character> characters;
    float           clickTime;
    List<Character> selectedCharacters = new List<Character>();

    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        characters = new List<Character>();
    }

    public void Add(Character character)
    {
        characters.Add(character);
    }

    public void Remove(Character character)
    {
        characters.Remove(character);

        foreach (var c in characters)
        {
            c.RemoveDistance(character);
        }
    }

    private void Update()
    {
        RunSelection();

        // Evaluate distances
        for (int i = 0; i < characters.Count; i++)
        {
            Character character1 = characters[i];
            var       pos1 = character1.transform.position;

            for (int j = i + 1; j < characters.Count; j++)
            {
                Character character2 = characters[j];

                float distance = Vector3.Distance(pos1, character2.transform.position);

                character1.SetDistance(character2, distance);
                character2.SetDistance(character1, distance);
            }
        }
    }

    void RunSelection()
    {
        if (Input.GetMouseButtonDown(0))
        {
            clickTime = Time.time;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if ((Time.time - clickTime) < 0.25f)
            {
                // Left click, select characters
                bool isControlPressed = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
                if (!isControlPressed)
                {
                    ClearSelection();
                }

                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

                foreach (var character in characters)
                {
                    if (selectedCharacters.IndexOf(character) == -1)
                    {
                        var pos = character.transform.position.xy() + Vector2.up * selectionRadius;
                        float distance = Vector2.Distance(pos, ray.origin.xy());
                        if (distance < selectionRadius)
                        {
                            selectedCharacters.Add(character);
                        }
                    }
                }

                foreach (var selectedCharacter in selectedCharacters)
                {
                    selectedCharacter.Select(true);
                }
            }
        }
    }

    void ClearSelection()
    {
        if (selectedCharacters == null) return;

        foreach (var selectedCharacter in selectedCharacters)
        {
            selectedCharacter.Select(false);
        }

        selectedCharacters.Clear();
    }
}
