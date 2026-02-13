using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GoodestBoy.Patches
{
    public class Recall : StatusEffect
    {
        public float m_maxDistance = 200f;

        // Companions selected during Setup
        public List<Character> m_companions = new();

        // Max number of companions == offsets length
        private readonly float[] _offSets = { -80f, 80f, -60f, 60f, -30f, 30f };

        public override void Setup(Character character)
        {
            base.Setup(character);

            m_companions.Clear();

            if (character == null)
                return;

            var charactersInRange = new List<Character>();
            Character.GetCharactersInRange(
                character.transform.position,
                m_maxDistance,
                charactersInRange
            );

            foreach (var pet in charactersInRange)
            {
                // Cap companions to available offsets
                if (m_companions.Count >= _offSets.Length)
                    break;

                if (pet == null || !pet.IsTamed())
                    continue;

                if (pet.name.Replace("(Clone)", "") != "BestestDog")
                    continue;

                var ai = pet.GetComponent<MonsterAI>();
                if (!ai)
                    continue;

                // OPTIONAL: only recall dogs following the player
                // if (!ai.GetFollowTarget() || ai.GetFollowTarget() != character.gameObject)
                //     continue;

                m_companions.Add(pet);
            }
        }

        public override void UpdateStatusEffect(float dt)
        {
            base.UpdateStatusEffect(dt);

            if (m_character == null || m_companions == null || m_companions.Count == 0)
                return;

            int index = 0;

            // Iterate backwards so we can safely remove invalid entries
            for (int i = m_companions.Count - 1; i >= 0; i--)
            {
                var companion = m_companions[i];

                // Remove dead or despawned companions
                if (companion == null || companion.IsDead())
                {
                    m_companions.RemoveAt(i);
                    continue;
                }

                // Safety cap (should already be capped in Setup)
                if (index >= _offSets.Length)
                    break;

                Vector3 offsetDir =
                    Quaternion.AngleAxis(_offSets[index], Vector3.up) *
                    m_character.transform.forward;

                Vector3 targetPos =
                    m_character.transform.position + offsetDir * 0.6f;

                companion.transform.SetPositionAndRotation(
                    targetPos,
                    m_character.transform.rotation
                );

                companion.SetLookDir(m_character.transform.forward);

                index++;
            }
        }

        public override void OnDestroy()
        {
            m_companions.Clear();
            base.OnDestroy();
        }
    }
}
