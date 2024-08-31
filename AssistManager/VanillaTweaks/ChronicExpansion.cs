using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using static AssistManager.AssistManager;
using UnityEngine;

namespace AssistManager.VanillaTweaks
{
    public class ChronicExpansion : VanillaTweakBase<ChronicExpansion>
    {
        public override string ConfigOptionName => "Chronic Expansion";

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
            if (attackerInventory.GetItemCount(DLC2Content.Items.IncreaseDamageOnMultiKill) > 0) assist.attackerBody.AddIncreasedDamageMultiKillTime();
        }
    }
}
