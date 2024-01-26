using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cortex/Ruleset")]
public class Ruleset : ScriptableObject
{
    [SerializeField] public Rule[] rules;
}
