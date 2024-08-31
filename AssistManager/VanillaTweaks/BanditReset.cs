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
    public class BanditReset : VanillaTweakBase<BanditReset>
    {
        public override string ConfigOptionName => "Bandit Reset";

        public override string ConfigDescriptionString => "Add assist support for Lights Out.";

        private GameObject resetEffect = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/Bandit2ResetEffect");

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
                && !damageInfo.rejected && damageInfo.attacker && (damageInfo.damageType & DamageType.ResetCooldownsOnKill) != 0)
            {
                CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                CharacterBody victimBody = victim.GetComponent<CharacterBody>();
                AssistManager.instance.AddDirectAssist(new Assist(attackerBody, victimBody, AssistManager.GetDirectAssistDurationForAttacker(damageInfo.attacker), DamageType.ResetCooldownsOnKill));
            }
            orig(self, damageInfo, victim);
        }

        private void HandleBanditAssists(Assist assist, CharacterBody killerBody, DamageInfo damageInfo)
        {
            if (assist.attackerBody && assist.damageType != null && (assist.damageType & DamageType.ResetCooldownsOnKill) != 0 && assist.moddedDamageTypes.Count == 0)
            {
                bool isReset = (damageInfo.damageType & DamageType.ResetCooldownsOnKill) != 0;

                if (!isReset)
                {
                    EffectManager.SpawnEffect(resetEffect, new EffectData
                    {
                        origin = damageInfo.position
                    }, true);
                }

                if (!(isReset && killerBody == assist.attackerBody) && assist.attackerBody.skillLocator)
                {
                    assist.attackerBody.skillLocator.ResetSkills();
                }
            }
        }
    }
}
