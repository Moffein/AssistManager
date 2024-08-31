using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using static AssistManager.AssistManager;

namespace AssistManager.VanillaTweaks
{
    public class Brainstalks : VanillaTweakBase<Brainstalks>
    {
        public override string ConfigOptionName => "Brainstalks";

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

            int bsCount = attackerInventory.GetItemCount(RoR2Content.Items.KillEliteFrenzy);
            if (bsCount > 0)
            {
                assist.attackerBody.AddTimedBuff(RoR2Content.Buffs.NoCooldowns, bsCount * 4f);
            }
        }
    }
}
