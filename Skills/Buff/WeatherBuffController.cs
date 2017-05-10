using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherBuffController : BuffController
{
    [SerializeField] private ElementType[] weatherElementTypes = new ElementType[Skill.MAXIMUM_SKILL_TYPES];

    protected override void Awake()
    {
        base.Awake();
        BuffAffinity = weatherElementTypes;
    }

    void OnValidate()
    {
        if (weatherElementTypes.Length != Skill.MAXIMUM_SKILL_TYPES)
        {
            System.Array.Resize(ref weatherElementTypes, Skill.MAXIMUM_SKILL_TYPES);
        }
    }
}
