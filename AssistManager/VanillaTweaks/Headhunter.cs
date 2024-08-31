using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using static AssistManager.AssistManager;

namespace AssistManager.VanillaTweaks
{
    public class Headhunter : VanillaTweakBase<Headhunter>
    {
        public override string ConfigOptionName => "Wake of Vultures";

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
            if (assist.attackerBody == killerBody || !assist.victimBody.isElite) return;

            int hhCount = attackerInventory.GetItemCount(RoR2Content.Items.HeadHunter);
            if (hhCount <= 0) return;

            float duration = 3f + 5f * hhCount;
            for (int l = 0; l < BuffCatalog.eliteBuffIndices.Length; l++)
            {
                BuffIndex buffIndex = BuffCatalog.eliteBuffIndices[l];
                if (assist.victimBody.HasBuff(buffIndex))
                {
                    assist.attackerBody.AddTimedBuff(buffIndex, duration);
                }
            }
        }
    }
}
