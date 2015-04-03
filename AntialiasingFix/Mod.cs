using System.Collections.Generic;
using ColossalFramework.Plugins;
using ICities;
using UnityEngine;

namespace DynamicResolution
{

    public class Mod : IUserMod
    {
        public static bool IsModActive()
        {
            var pluginManager = PluginManager.instance;
            var plugins = Util.GetPrivate<Dictionary<string, PluginManager.PluginInfo>>(pluginManager, "m_Plugins");

            foreach (var item in plugins)
            {
                if (item.Value.name != "406629464")
                {
                    continue;
                }

                return item.Value.isEnabled;
            }

            return false;
        }


        public string Name
        {
            get
            {
                return "Dynamic resolution";
            }
        }

        public string Description
        {
            get { return "Allows you to upsample/ downsample from any resolution"; }
        }

    }

    public class ModLoad : LoadingExtensionBase
    {

        private CameraHook hook;

        public override void OnLevelLoaded(LoadMode mode)
        {
            if (mode != LoadMode.NewGame && mode != LoadMode.LoadGame)
            {
                return;
            }

            var cameraController = GameObject.FindObjectOfType<CameraController>();
            hook = cameraController.gameObject.AddComponent<CameraHook>();
        }

        public override void OnLevelUnloading()
        {
            GameObject.Destroy(hook);
        }
    }

}
