using MonoMod.Cil;
using RoR2;
using RoR2.Orbs;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace AssistManager.VanillaTweaks
{
    public class Infusion : VanillaTweakBase<Infusion>
    {
        public override string ConfigOptionName => "Infusion";

        public override string ConfigDescriptionString => "Add assist support for this item.";

        protected override void ApplyChanges()
        {
            base.ApplyChanges();
            AssistManager.HandleAssistInventoryActions += OnKillEffect;
        }

        protected override void RemoveChanges()
        {
            base.RemoveChanges();
            AssistManager.HandleAssistInventoryActions -= OnKillEffect;
        }

        private void OnKillEffect(AssistManager.Assist assist, Inventory attackerInventory, CharacterBody killerBody, DamageInfo damageInfo)
        {
            //Let Vanilla handle effect if you are killer.
            if (assist.attackerBody == killerBody) return;

            int itemCount = attackerInventory.GetItemCount(RoR2Content.Items.Infusion);
            if (itemCount <= 0) return;

            int maxOrbs = itemCount * 100;
            if ((ulong)attackerInventory.infusionBonus < (ulong)((long)maxOrbs))
            {
                InfusionOrb infusionOrb = new InfusionOrb()
                {
                    origin = assist.victimBody.transform.position,
                    target = Util.FindBodyMainHurtBox(assist.attackerBody),
                    maxHpValue = itemCount
                };
                OrbManager.instance.AddOrb(infusionOrb);
            }
        }
    }
}
