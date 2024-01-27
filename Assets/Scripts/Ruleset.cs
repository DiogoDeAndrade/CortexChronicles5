using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cortex/Ruleset")]
public class Ruleset : ScriptableObject
{
    [SerializeField] private Ruleset[]  rulesets;
    [SerializeField] private Rule[]     rules;

    public void GetRules(List<Rule> r)
    {
        foreach (var ruleset in rulesets)
        {
            ruleset.GetRules(r);
        }

        foreach (var rule in rules)
        {
            r.Add(rule);
        }
    }
}
