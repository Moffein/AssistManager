using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using static AssistManager.AssistManager;

namespace AssistManager.VanillaTweaks
{
    public class TopazBrooch : VanillaTweakBase<TopazBrooch>
    {
        public override string ConfigOptionName => "Topaz Brooch";

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

            int itemCount = attackerInventory.GetItemCount(RoR2Content.Items.BarrierOnKill);
            if (itemCount > 0)
            {
                assist.attackerBody.healthComponent.AddBarrier(15f * itemCount);
            }
        }
    }
}
