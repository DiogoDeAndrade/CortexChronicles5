using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager instance;

    [SerializeField] private Camera         mainCamera;
    [SerializeField] private float          selectionRadius = 32.0f;
    [SerializeField] private SpriteRenderer selectionBox;
    [SerializeField] public  Bounds         limits;

    List<Character> characters;
    float           leftClickTime;
    float           rightClickTime;
    Vector2         clickPos;
    bool            boxSelect;
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
        RunCommands();

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
            leftClickTime = Time.time;
            clickPos = mainCamera.ScreenPointToRay(Input.mousePosition).origin.xy();
            boxSelect = false;
            selectionBox.gameObject.SetActive(false);
            selectionBox.transform.position = clickPos;
        }
        else if (Input.GetMouseButton(0))
        {
            Vector2 currentPos = mainCamera.ScreenPointToRay(Input.mousePosition).origin.xy();

            if (Vector2.Distance(clickPos, currentPos) > 2.0f)
            {
                boxSelect = true;
                selectionBox.gameObject.SetActive(true);
            }

            selectionBox.size = new Vector2(currentPos.x - clickPos.x, clickPos.y - currentPos.y);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (boxSelect)
            {
                // Left click, select characters
                bool isControlPressed = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
                if (!isControlPressed)
                {
                    ClearSelection();
                }

                Vector2 currentPos = mainCamera.ScreenPointToRay(Input.mousePosition).origin.xy();

                Vector2 start = Vector2.zero;
                Vector2 end = Vector2.zero;
                if (currentPos.x < clickPos.x) { start.x = currentPos.x; end.x = clickPos.x; }
                else { start.x = clickPos.x; end.x = currentPos.x; }
                if (currentPos.y < clickPos.y) { start.y = currentPos.y; end.y = clickPos.y; }
                else { start.y = clickPos.y; end.y = currentPos.y; }

                start.x -= selectionRadius;
                start.y -= selectionRadius;
                end.x += selectionRadius;
                end.y += selectionRadius;

                foreach (var character in characters)
                {
                    if (selectedCharacters.IndexOf(character) == -1)
                    {
                        var pos = character.transform.position.xy() + Vector2.up * selectionRadius;
                        if ((pos.x >= start.x) && (pos.x <= end.x) && (pos.y >= start.y) && (pos.y <= end.y))
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
            if ((Time.time - leftClickTime) < 0.25f)
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
            leftClickTime = 0;
            selectionBox.gameObject.SetActive(false);
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

    void RunCommands()
    {
        if (selectedCharacters.Count == 0) return;

        if (Input.GetMouseButtonDown(1))
        {
            rightClickTime = Time.time;
        }
        else if (Input.GetMouseButton(1))
        {
            if ((Time.time - rightClickTime) < 0.25f)
            {
                Vector3 avgPos = Vector3.zero;
                foreach (var c in selectedCharacters)
                {
                    avgPos += c.transform.position;
                }
                avgPos /= selectedCharacters.Count;

                Vector2 targetPos = mainCamera.ScreenPointToRay(Input.mousePosition).origin.xy();
                Vector2 delta = targetPos - avgPos.xy();

                foreach (var c in selectedCharacters)
                {
                    c.ForceMove(delta);
                }
            }
        }
    }
}
