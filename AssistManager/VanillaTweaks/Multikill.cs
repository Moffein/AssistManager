using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using static AssistManager.AssistManager;

namespace AssistManager.VanillaTweaks
{
    public class Multikill : VanillaTweakBase<Multikill>
    {
        public override string ConfigOptionName => "Multikill";

        public override string ConfigDescriptionString => "On-Multikill items are affected by assists.";

        protected override void ApplyChanges()
        {
            AssistManager.HandleAssistActions += AddMultikill;
        }

        protected override void RemoveChanges()
        {
            AssistManager.HandleAssistActions -= AddMultikill;
        }

        private void AddMultikill(Assist assist, CharacterBody killerBody, DamageInfo damageInfo)
        {
            if (assist.attackerBody != killerBody) assist.attackerBody.AddMultiKill(1);
        }
    }
}
