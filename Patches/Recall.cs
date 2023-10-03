﻿using System.Collections.Generic;
using System.Linq;
using GoodestBoy;
using UnityEngine;

namespace GoodestBoy.Patches;

public class Recall : StatusEffect
{
    public float m_maxDistance = 200f; //the range in which the tame must be in in order to be summoned.
    public List<Character> m_companions = new();
    private readonly float[] _offSets = { -80f, 80f, -60f, 60f, -30f, 30f }; //dont change this.

    public override void Setup(Character character)
    {
        base.Setup(character);
        m_companions.Clear();
        var charactersInRange = new List<Character>();
        Character.GetCharactersInRange(character.transform.position, m_maxDistance, charactersInRange);
        var num = 0;
        //this is where you add your creatures to be selected in summoning process.
        //pet.name.Replace("(Clone)", "") == "Wolf" -change "Wolf" into your creatures prefab name.
        //you can add more by adding this: || pet.name.Replace("(Clone)", "") == "yourpetprefabname"
        foreach (var pet in charactersInRange.Where(pet => pet.IsTamed() && pet.name.Replace("(Clone)", "") == "BestestDog"))
        {
            num++;
            var component = pet.GetComponent<MonsterAI>();
            if ((bool)component)
            {
                m_companions.Add(pet);
            }
            //if you only want the creatures whos following you to be summoned then uncomment the line of codes below and comment or delete the above if clause.
            // if ((bool)component && (bool)component.GetFollowTarget() &&
            //     component.GetFollowTarget() == character.gameObject)
            // {
            //     m_companions.Add(pet);
            // }
        }
    }

    public override void UpdateStatusEffect(float dt)
    {
        base.UpdateStatusEffect(dt);
        var num = 0;
        foreach (var companion in m_companions)
        {
            var vector = Quaternion.AngleAxis(_offSets[num], Vector3.up) * companion.transform.forward * 0.6f;
            var lookDir = m_character.transform.rotation * Vector3.forward;
            companion.transform.position = m_character.transform.position + vector;
            companion.transform.rotation = m_character.transform.rotation;
            companion.SetLookDir(lookDir);
            num++;
        }
    }
}