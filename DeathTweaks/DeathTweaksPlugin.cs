using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace DeathTweaks
{
    [BepInPlugin(GUID, Name, Version)]
    [BepInProcess("valheim.exe")]
    public sealed class DeathTweaksPlugin : BaseUnityPlugin
    {
        public const string Author = "SPladison";
        public const string Name = "DeathTweaks";
        public const string DisplayName = "Death Tweaks";
        public const string GUID = $"{Author}.{Name}";
        public const string Version = "1.0.0";

        public static DeathTweaksPlugin Instance { get; private set; }

        private static ConfigEntry<bool> saveEquippedItemsAfterDeath;
        private static ConfigEntry<bool> saveSkillsAfterDeath;

        private readonly Harmony harmony;

        public DeathTweaksPlugin()
        {
            harmony = new Harmony(GUID);
        }

        private void Awake()
        {
            InitConfig();

            harmony.PatchAll();
        }

        private void InitConfig()
        {
            saveEquippedItemsAfterDeath = Config.Bind<bool>("General", nameof(saveEquippedItemsAfterDeath), true, "Save equipped items after death");
            saveSkillsAfterDeath = Config.Bind<bool>("General", nameof(saveSkillsAfterDeath), true, "Save skills after death");
        }

        [HarmonyPatch(typeof(Skills), nameof(Skills.OnDeath))]
        private static class Skills_OnDeath_Patch
        {
            private static bool Prefix()
            {
                return !saveSkillsAfterDeath.Value;
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.CreateTombStone))]
        private static class Player_CreateTombStone_Patch
        {
            private static void Prefix(out bool __state)
            {
                if (!saveEquippedItemsAfterDeath.Value)
                {
                    __state = false;
                    return;
                }

                ref var globalKeys = ref ZoneSystem.instance.m_globalKeysEnums;
                __state = globalKeys.Contains(GlobalKeys.DeathKeepEquip);

                globalKeys.Add(GlobalKeys.DeathKeepEquip);
            }

            private static void Postfix(bool __state)
            {
                if (!saveEquippedItemsAfterDeath.Value || __state) return;

                ref var globalKeys = ref ZoneSystem.instance.m_globalKeysEnums;

                if (!globalKeys.Contains(GlobalKeys.DeathKeepEquip))
                {
                    ZLog.LogWarning($"'{nameof(GlobalKeys.DeathKeepEquip)}' key should be here...");
                    return;
                }

                globalKeys.Remove(GlobalKeys.DeathKeepEquip);
            }
        }
    }
}