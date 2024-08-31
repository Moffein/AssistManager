using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static AssistManager.AssistManager;

namespace AssistManager.VanillaTweaks
{
    public class Harpoon : VanillaTweakBase<Harpoon>
    {
        public override string ConfigOptionName => "Hunters Harpoon";

        public override string ConfigDescriptionString => "Add assist support to this item.";

        protected override void ApplyChanges()
        {
            AssistManager.HandleAssistInventoryActions += OnKillEffect;
        }

        protected override void RemoveChanges()
        {
            AssistManager.HandleAssistInventoryActions -= OnKillEffect;
        }

        private void OnKillEffect(Assist assist, Inventory attackerInventory, CharacterBody killerBody, DamageInfo damageInfo)
        {
            if (assist.attackerBody == killerBody) return;

            int itemCount = attackerInventory.GetItemCount(DLC1Content.Items.MoveSpeedOnKill);
            if (itemCount <= 0) return;

            CharacterBody attackerBody = assist.attackerBody;
            attackerBody.ClearTimedBuffs(DLC1Content.Buffs.KillMoveSpeed);

            int itemStack = itemCount - 1;
            int totalBuffs = 5;
            float duration = 1f + (float)itemStack * 0.5f;

            for (int l = 0; l < totalBuffs; l++)
            {
                attackerBody.AddTimedBuff(DLC1Content.Buffs.KillMoveSpeed, duration * (float)(l + 1) / (float)totalBuffs);
            }
            EffectData effectData = new EffectData();
            effectData.origin = attackerBody.corePosition;
            CharacterMotor characterMotor = attackerBody.characterMotor;
            bool flag = false;
            if (characterMotor)
            {
                Vector3 moveDirection = characterMotor.moveDirection;
                if (moveDirection != Vector3.zero)
                {
                    effectData.rotation = Util.QuaternionSafeLookRotation(moveDirection);
                    flag = true;
                }
            }
            if (!flag)
            {
                effectData.rotation = attackerBody.transform.rotation;
            }
            EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/MoveSpeedOnKillActivate"), effectData, true);
        }
    }
}
