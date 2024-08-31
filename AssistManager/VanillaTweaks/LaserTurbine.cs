using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using static AssistManager.AssistManager;

namespace AssistManager.VanillaTweaks
{
    public class LaserTurbine : VanillaTweakBase<LaserTurbine>
    {
        public override string ConfigOptionName => "Resonance Disk";

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
            int itemCount = attackerInventory.GetItemCount(RoR2Content.Items.LaserTurbine);
            if (itemCount > 0)
            {
                assist.attackerBody.AddTimedBuff(RoR2Content.Buffs.LaserTurbineKillCharge, EntityStates.LaserTurbine.RechargeState.killChargeDuration, EntityStates.LaserTurbine.RechargeState.killChargesRequired);
            }
        }
    }
}
