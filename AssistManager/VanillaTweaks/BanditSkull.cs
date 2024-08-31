using MonoMod.Cil;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using static AssistManager.AssistManager;

namespace AssistManager.VanillaTweaks
{
    public class BanditSkull : VanillaTweakBase<BanditSkull>
    {
        public override string ConfigOptionName => "Bandit Desperado";

        public override string ConfigDescriptionString => "Add assist support for Desperado.";

        private GameObject skullEffect = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/Bandit2KillEffect");
        protected override void ApplyChanges()
        {
            base.ApplyChanges();
            On.RoR2.GlobalEventManager.ProcessHitEnemy += GlobalEventManager_ProcessHitEnemy;
            AssistManager.HandleAssistActions += HandleBanditAssists;
        }

        protected override void RemoveChanges()
        {
            base.RemoveChanges();
            On.RoR2.GlobalEventManager.ProcessHitEnemy -= GlobalEventManager_ProcessHitEnemy;
            AssistManager.HandleAssistActions -= HandleBanditAssists;
        }

        private void GlobalEventManager_ProcessHitEnemy(On.RoR2.GlobalEventManager.orig_ProcessHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            if (NetworkServer.active
                && AssistManager.instance
                && victim
                && !damageInfo.rejected && damageInfo.attacker && (damageInfo.damageType & DamageType.GiveSkullOnKill) != 0)
            {
                CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                CharacterBody victimBody = victim.GetComponent<CharacterBody>();
                AssistManager.instance.AddDirectAssist(new Assist(attackerBody, victimBody, AssistManager.GetDirectAssistDurationForAttacker(damageInfo.attacker), DamageType.GiveSkullOnKill));
            }
            orig(self, damageInfo, victim);
        }

        private void HandleBanditAssists(Assist assist, CharacterBody killerBody, DamageInfo damageInfo)
        {
            if (assist.damageType != null && (assist.damageType & DamageType.GiveSkullOnKill) != 0 && assist.moddedDamageTypes.Count == 0)
            {
                bool isSkull = (damageInfo.damageType & DamageType.GiveSkullOnKill) != 0;

                if (!isSkull)
                {
                    EffectManager.SpawnEffect(skullEffect, new EffectData
                    {
                        origin = damageInfo.position
                    }, true);
                }

                if (!(isSkull && killerBody == assist.attackerBody))
                {
                    assist.attackerBody.AddBuff(RoR2Content.Buffs.BanditSkull);
                }
            }
        }
    }
}
