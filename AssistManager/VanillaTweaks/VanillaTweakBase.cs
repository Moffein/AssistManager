using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace AssistManager.VanillaTweaks
{
    public abstract class VanillaTweakBase<T> : VanillaTweakBase where T : VanillaTweakBase<T>
    {
        //This, which you will see on all the -base classes, will allow both you and other modders to enter through any class with this to access internal fields/properties/etc as if they were a member inheriting this -Base too from this class.
        public static T Instance { get; private set; }

        public VanillaTweakBase()
        {
            if (Instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting VanillaTweakBase was instantiated twice");
            Instance = this as T;
        }
    }

    public abstract class VanillaTweakBase
    {
        public abstract string ConfigOptionName { get; }

        public abstract string ConfigDescriptionString { get; }

        public ConfigEntry<bool> Enabled { get; private set; }

        protected virtual void ReadConfig(ConfigFile config)
        {
            Enabled = config.Bind<bool>("Vanilla Tweaks", ConfigOptionName, true, ConfigDescriptionString);
        }

        public bool changesActive { get; private set; }

        internal void Init(ConfigFile config)
        {
            changesActive = false;
            ReadConfig(config);
            SetEnabled(Enabled.Value);
        }

        public void SetEnabled(bool enabled)
        {
            if (enabled)
            {
                if (changesActive) return;
                ApplyChanges();
                changesActive = true;
            }
            else
            {
                if (!changesActive) return;
                RemoveChanges();
                changesActive = false;
            }
        }

        protected virtual void ApplyChanges()
        {
        }

        protected virtual void RemoveChanges()
        {
        }
    }
}
