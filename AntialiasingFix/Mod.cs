using System.Reflection;
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

        public override void OnLevelLoaded(LoadMode mode)
        {
            if (mode != LoadMode.NewGame && mode != LoadMode.LoadGame)
            {
                return;
            }

            var cameraController = GameObject.FindObjectOfType<CameraController>();
            var camera = cameraController.gameObject.GetComponent<Camera>();
            camera.gameObject.AddComponent<CameraHook>();
        }

    }

}
