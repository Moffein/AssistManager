using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using static AssistManager.AssistManager;

namespace AssistManager.VanillaTweaks
{
    public class Soulbound : VanillaTweakBase<Soulbound>
    {
        public override string ConfigOptionName => "Soulbound Catalyst";

        public override string ConfigDescriptionString => "Add assist support for this item.";

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
            int itemCount = attackerInventory.GetItemCount(RoR2Content.Items.Talisman);
            if (itemCount > 0)
            {
                attackerInventory.DeductActiveEquipmentCooldown(2f + itemCount * 2f);
            }
        }
    }
}
