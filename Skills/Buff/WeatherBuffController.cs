using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherBuffController : BuffController
{
    [SerializeField] private Battle_Element_Type[] weatherElementTypes = new Battle_Element_Type[Skill.MAXIMUM_ELEMENTAL_SKILL_TYPES];

    void Awake()
    {
        Init();
        this.RegisterSkillOwner(null, weatherElementTypes);
    }

    void OnValidate()
    {
        if (weatherElementTypes.Length != Skill.MAXIMUM_ELEMENTAL_SKILL_TYPES)
        {
            System.Array.Resize(ref weatherElementTypes, Skill.MAXIMUM_ELEMENTAL_SKILL_TYPES);
        }
    }
}
