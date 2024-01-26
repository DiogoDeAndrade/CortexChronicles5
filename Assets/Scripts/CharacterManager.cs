using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager instance;

    List<Character> characters;

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
}
