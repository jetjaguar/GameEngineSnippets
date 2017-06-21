using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Extended BuffController that controls buff part of weather effect
 * Author: Patrick Finegan, May 2017
 */
[System.Serializable]
[CreateAssetMenu(menuName = GameGlobals.ASSET_MENU_FOLDER + "/WeatherBuffConfig")]
public class WeatherBuffConfiguration : BuffConfiguration
{
    public ElementType[] WeatherElementTypes = new ElementType[Skill.MAXIMUM_SKILL_TYPES];

#if UNITY_EDITOR
    protected void OnValidate()
    {
        if (WeatherElementTypes.Length != Skill.MAXIMUM_SKILL_TYPES)
        {
            System.Array.Resize(ref WeatherElementTypes, Skill.MAXIMUM_SKILL_TYPES);
        }
    }
#endif
}
