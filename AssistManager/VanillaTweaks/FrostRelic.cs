using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using static AssistManager.AssistManager;

namespace AssistManager.VanillaTweaks
{
    public class FrostRelic : VanillaTweakBase<FrostRelic>
    {
        public override string ConfigOptionName => "Frost Relic";

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

            int itemCount = attackerInventory.GetItemCount(RoR2Content.Items.Icicle);
            if (itemCount <= 0) return;

            RoR2.Items.IcicleBodyBehavior ib = assist.attackerBody.GetComponent<RoR2.Items.IcicleBodyBehavior>();
            if (ib && ib.icicleAura) ib.icicleAura.OnOwnerKillOther();
        }
    }
}
