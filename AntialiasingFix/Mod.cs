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
          //  Debugger.Initialize();
          //  Debugger.Log("initialized");

            var cameraController = GameObject.FindObjectOfType<CameraController>();
            var camera = cameraController.gameObject.GetComponent<Camera>();
            camera.gameObject.AddComponent<CameraHook>();

            var fields = typeof(DefaultTool).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo field = null;
            foreach (var f in fields)
            {
                if (f.Name == "m_hoverInstance")
                {
                    field = f;
                    break;
                }
            }

            var defaultTool = GameObject.FindObjectOfType<DefaultTool>();
            var hoverInstance = (InstanceID)field.GetValue(defaultTool);

        }

    }

}
