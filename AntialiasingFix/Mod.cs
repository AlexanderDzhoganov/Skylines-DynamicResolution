using ICities;
using UnityEngine;

namespace DynamicResolution
{

    public class Mod : IUserMod
    {

        public string Name
        {
            get { return "Dynamic resolution"; }
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
