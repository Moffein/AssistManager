using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;

namespace AssistManager
{
    public class AssistManager : MonoBehaviour
    {
        public static float assistDuration = 3f;

        public static float directKillDurationAuthority = 0.25f;
        public static float directKillDurationNetworkPlayer = 0.5f;

        public static AssistManager instance { get; private set; }

        public delegate void HandleAssistInventory(Assist assist,Inventory attackerInventory, CharacterBody killerBody, DamageInfo damageInfo);
        public static HandleAssistInventory HandleAssistInventoryActions;


        public delegate void HandleAssist(Assist assist, CharacterBody killerBody, DamageInfo damageInfo);
        public static HandleAssist HandleAssistActions;

        public delegate void HandleAssistInventoryCompatible(CharacterBody attackerBody, CharacterBody victimBody, DamageType? assistDamageType, HashSet<R2API.DamageAPI.ModdedDamageType> assistModdedDamageTypes, Inventory attackerInventory, CharacterBody killerBody, DamageInfo damageInfo);
        public static HandleAssistInventoryCompatible HandleAssistInventoryCompatibleActions;


        public delegate void HandleAssistCompatible(CharacterBody attackerBody, CharacterBody victimBody, DamageType? assistDamageType, HashSet<R2API.DamageAPI.ModdedDamageType> assistModdedDamageTypes, CharacterBody killerBody, DamageInfo damageInfo);
        public static HandleAssistCompatible HandleAssistCompatibleActions;


        private List<Assist> pendingAssists = new List<Assist>();

        //Meant to be used with things like Bandit's Revolver, where you want its timer to run separately from normal assists.
        private List<Assist> pendingDirectAssists = new List<Assist>();

        public static float GetDirectAssistDurationForAttacker(GameObject attacker)
        {
            return IsLocalUser(attacker) ? directKillDurationAuthority : directKillDurationNetworkPlayer;
        }

        private static bool IsLocalUser(GameObject playerObject)
        {
            foreach (LocalUser user in LocalUserManager.readOnlyLocalUsersList)
            {
                if (playerObject == user.cachedBodyObject)
                {
                    return true;
                }
            }
            return false;
        }

        internal static void Init()
        {
            RoR2.Run.onRunStartGlobal += Run_onRunStartGlobal;
            On.RoR2.GlobalEventManager.OnCharacterDeath += GlobalEventManager_OnCharacterDeath;
            On.RoR2.GlobalEventManager.ProcessHitEnemy += GlobalEventManager_ProcessHitEnemy;
        }

        private static void GlobalEventManager_ProcessHitEnemy(On.RoR2.GlobalEventManager.orig_ProcessHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            bool validDamage = NetworkServer.active && !damageInfo.rejected;

            if (AssistManager.instance && validDamage && damageInfo.attacker && victim)
            {
                CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                CharacterBody victimBody = victim.GetComponent<CharacterBody>();

                if (attackerBody && victimBody)
                {
                    AssistManager.instance.AddAssist(new Assist(attackerBody, victimBody, AssistManager.assistDuration));
                }
            }

            orig(self, damageInfo, victim);
        }

        private static void GlobalEventManager_OnCharacterDeath(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, GlobalEventManager self, DamageReport damageReport)
        {
            orig(self, damageReport);

            if (NetworkServer.active && damageReport.attackerMaster)
            {
                CharacterBody attackerBody = damageReport.attackerBody;
                CharacterBody victimBody = damageReport.victimBody;

                if (attackerBody &&  victimBody && AssistManager.instance)
                {
                    AssistManager.instance.AddAssist(new Assist(attackerBody, victimBody, AssistManager.assistDuration));
                    AssistManager.instance.TriggerAssists(victimBody, attackerBody, damageReport.damageInfo);
                }
            }
        }

        private static void Run_onRunStartGlobal(RoR2.Run self)
        {
            if (!NetworkServer.active) return;
            instance= self.gameObject.GetComponent<AssistManager>();
            if (!instance)
            {
               instance = self.gameObject.AddComponent<AssistManager>();
            }
        }

        private void FixedUpdate()
        {
            if (pendingAssists.Count > 0) UpdateAssists(pendingAssists);
            if (pendingDirectAssists.Count > 0) UpdateAssists(pendingDirectAssists);
        }

        private void UpdateAssists(List<Assist> assists)
        {
            List<Assist> toRemove = new List<Assist>();
            foreach (Assist a in assists)
            {
                a.timer -= Time.fixedDeltaTime;
                if (a.timer <= 0 || !(a.attackerBody && a.attackerBody.healthComponent && a.attackerBody.healthComponent.alive))
                {
                    toRemove.Add(a);
                }
            }

            foreach (Assist a in toRemove)
            {
                assists.Remove(a);
            }
        }

        //Assists combine their information
        public void AddAssist(Assist newAssist)
        {
            //Check if this assist already exists.
            bool foundAssist = false;
            foreach (Assist a in pendingAssists)
            {
                if (a.attackerBody == newAssist.attackerBody && a.victimBody == newAssist.victimBody)
                {
                    foundAssist = true;
                    if (a.timer < newAssist.timer)
                    {
                        a.timer = newAssist.timer;
                    }

                    a.damageType = a.damageType | newAssist.damageType;
                    a.moddedDamageTypes.UnionWith(newAssist.moddedDamageTypes);
                    break;
                }
            }

            if (!foundAssist)
            {
                pendingAssists.Add(newAssist);
            }
        }

        //Direct assists only reset duration if there is an exact field match. They do not combine information.
        public void AddDirectAssist(Assist newAssist)
        {
            bool foundAssist = false;
            foreach (Assist a in pendingDirectAssists)
            {
                if (a.attackerBody == newAssist.attackerBody
                    && a.victimBody == newAssist.victimBody
                    && a.damageType == newAssist.damageType
                    && a.moddedDamageTypes.SetEquals(newAssist.moddedDamageTypes))
                {
                    foundAssist = true;
                    if (a.timer < newAssist.timer)
                    {
                        a.timer = newAssist.timer;
                    }
                    break;
                }
            }

            if (!foundAssist)
            {
                pendingDirectAssists.Add(newAssist);
            }
        }

        public void TriggerAssists(CharacterBody victimBody, CharacterBody killerBody, DamageInfo damageInfo)
        {
            if (!victimBody) return;
            if (pendingAssists.Count > 0)
            {
                List<Assist> toRemove = new List<Assist>();
                List<Assist> toRemovePending = new List<Assist>();
                foreach (Assist a in pendingAssists)
                {
                    if (a.victimBody == victimBody)
                    {
                        toRemove.Add(a);
                    }
                }
                foreach (Assist a in pendingDirectAssists)
                {
                    if (a.victimBody == victimBody)
                    {
                        toRemovePending.Add(a);
                    }
                }

                //We don't skip a.attacker == victimBody because there might be a previous attack that needs to be "assisted" on
                //Ex. Bandit Revolver Damage -> Item Proc Kill
                foreach (Assist a in toRemove)
                {
                    if (a.attackerBody && a.attackerBody.healthComponent && a.attackerBody.healthComponent.alive)
                    {
                        if (HandleAssistActions != null) HandleAssistActions.Invoke(a, killerBody, damageInfo);
                        if (HandleAssistCompatibleActions != null) HandleAssistCompatibleActions.Invoke(a.attackerBody,
                                                                   a.victimBody,
                                                                   a.damageType,
                                                                   a.moddedDamageTypes,
                                                                   killerBody, damageInfo);
                        Inventory attackerInventory = a.attackerBody.inventory;
                        if (attackerInventory)
                        {
                            if (HandleAssistInventoryActions != null) HandleAssistInventoryActions(a, attackerInventory, killerBody, damageInfo);
                            if (HandleAssistInventoryCompatibleActions != null) HandleAssistInventoryCompatibleActions(a.attackerBody,
                                                                   a.victimBody,
                                                                   a.damageType,
                                                                   a.moddedDamageTypes,
                                                                   attackerInventory,
                                                                   killerBody,
                                                                   damageInfo);
                        }
                    }
                    pendingAssists.Remove(a);
                }
                foreach (Assist a in toRemovePending)
                {

                    if (a.attackerBody && a.attackerBody.healthComponent && a.attackerBody.healthComponent.alive)
                    {
                        if (HandleAssistActions != null) HandleAssistActions.Invoke(a, killerBody, damageInfo);
                        if (HandleAssistCompatibleActions != null) HandleAssistCompatibleActions.Invoke(a.attackerBody,
                                                                   a.victimBody,
                                                                   a.damageType,
                                                                   a.moddedDamageTypes,
                                                                   killerBody, damageInfo);
                        Inventory attackerInventory = a.attackerBody.inventory;
                        if (attackerInventory)
                        {
                            if (HandleAssistInventoryActions != null) HandleAssistInventoryActions(a, attackerInventory, killerBody, damageInfo);
                            if (HandleAssistInventoryCompatibleActions != null) HandleAssistInventoryCompatibleActions(a.attackerBody,
                                                                   a.victimBody,
                                                                   a.damageType,
                                                                   a.moddedDamageTypes,
                                                                   attackerInventory,
                                                                   killerBody,
                                                                   damageInfo);
                        }
                    }
                    pendingDirectAssists.Remove(a);
                }
            }
        }

        public class Assist
        {
            public float timer;
            public CharacterBody attackerBody;
            public CharacterBody victimBody;

            [Tooltip("Use this if you want to save information about the hit that triggered the assist.")]
            public DamageType? damageType;

            //This needs to be a hashsset since it can't be combined like normal DamageTypes can.
            [Tooltip("Use this if you want to save information about the hit that triggered the assist.")]
            public HashSet<R2API.DamageAPI.ModdedDamageType> moddedDamageTypes;

            public Assist(CharacterBody attackerBody, CharacterBody victimBody, float timer)
            {
                this.attackerBody = attackerBody;
                this.victimBody = victimBody;
                this.timer = timer;
                this.damageType = null;
                this.moddedDamageTypes = new HashSet<R2API.DamageAPI.ModdedDamageType>();
            }

            public Assist(CharacterBody attackerBody, CharacterBody victimBody, float timer, DamageType damageType)
            {
                this.attackerBody = attackerBody;
                this.victimBody = victimBody;
                this.timer = timer;
                this.damageType = damageType;
                this.moddedDamageTypes = new HashSet<R2API.DamageAPI.ModdedDamageType>();
            }
        }
    }
}
