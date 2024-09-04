using AssistManager.VanillaTweaks;
using BepInEx;
using BepInEx.Configuration;
using R2API.Utils;
using System;
using System.Linq;
using System.Reflection;

namespace AssistManager
{
    [BepInDependency(R2API.R2API.PluginGUID)]
    [BepInDependency(R2API.DamageAPI.PluginGUID)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    [BepInPlugin("com.Moffein.AssistManager", "AssistManager", "1.1.0")]
    public class AssistManagerPlugin : BaseUnityPlugin
    {
        private void Awake()
        {
            AssistManager.Init();
            AddToAssembly();
        }

        private void AddToAssembly()
        {
            var fixTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(VanillaTweakBase)));

            foreach (var tweakType in fixTypes)
            {
                VanillaTweakBase tweak = (VanillaTweakBase)Activator.CreateInstance(tweakType);
                tweak.Init(Config);
            }
        }
    }
}