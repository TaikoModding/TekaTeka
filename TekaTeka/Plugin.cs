using BepInEx.Unity.IL2CPP.Utils;
using BepInEx.Unity.IL2CPP;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using BepInEx.Configuration;
using TekaTeka.Plugins;
using UnityEngine;
using System.Collections;

namespace TekaTeka
{
    [BepInPlugin(PluginGuid, ModName, ModVersion)]
    public class Plugin : BasePlugin
    {
        public const string PluginGuid = "RF.TekaTeka";
        public const string ModName = "TekaTeka";
        public const string ModVersion = "1.2.2";

        public static Plugin? Instance;
        private Harmony _harmony;
        internal static ManualLogSource? LogSource;
        private CustomSongLoader? _loader;

        public ConfigEntry<bool> ConfigEnabled;

        public Plugin()
        {
            _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
            LogSource = base.Log;
            ConfigEnabled = Config.Bind("General", "Enabled", true, "Enables the mod.");
            _loader = new CustomSongLoader();
        }

        public override void Load()
        {
            SetupHarmony();
        }

        public override bool Unload()
        {
            _harmony.UnpatchSelf();
            _loader = null;
            return base.Unload();
        }

        private void SetupHarmony()
        {
            if (ConfigEnabled.Value)
            {
                bool result = true;
                // If any PatchFile fails, result will become false
                result &= PatchFile(typeof(CustomSongLoader));
                _loader?.InitializeLoader();

                if (result)
                {
                    LogSource?.LogInfo($"Plugin {MyPluginInfo.PLUGIN_NAME} is loaded!");
                }
                else
                {
                    LogSource?.LogError($"Plugin {MyPluginInfo.PLUGIN_GUID} failed to load.");
                    // Unload this instance of Harmony
                    // I hope this works the way I think it does
                    _harmony.UnpatchSelf();
                }
            }
            else
            {
                LogSource?.LogInfo($"Plugin {MyPluginInfo.PLUGIN_NAME} is disabled.");
            }
        }

        private bool PatchFile(Type type)
        {
            if (_harmony == null)
            {
                _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
            }
            try
            {
                _harmony.PatchAll(type);
#if DEBUG
                LogSource?.LogInfo("File patched: " + type.FullName);
#endif
                return true;
            }
            catch (Exception e)
            {
                LogSource?.LogInfo("Failed to patch file: " + type.FullName);
                LogSource?.LogInfo(e.Message);
                return false;
            }
        }

        public static MonoBehaviour GetMonoBehaviour() => TaikoSingletonMonoBehaviour<CommonObjects>.Instance;
        public void StartCoroutine(IEnumerator enumerator)
        {
            GetMonoBehaviour().StartCoroutine(enumerator);
        }
    }
}
