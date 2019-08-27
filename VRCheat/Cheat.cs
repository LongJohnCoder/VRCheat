using Harmony;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using VRC;
using VRC.Core;
using VRCheat.Commands;
using VRCheat.Components;
using VRCheat.Patching;
using VRCheat.Utils;
using VRLoader.Attributes;
using VRLoader.Modules;
using static System.Console;

namespace VRCheat
{
    [ModuleInfo("VRCheat", "2.0", "Harekuin")]
    public class Cheat : VRModule
    {
        public const int BUILD_NUMBER = 704;
        private static GameObject self;

        public void OnGUI()
        {
            int i = 0;
            PlayerManager.GetAllPlayers().Where(p => p.IsModerator()).ToList().ForEach(p => GUI.Label(new Rect(1, 50 + (i++ * 15), 300, 20), string.Format("{0}", p.GetAPIUser().displayName)));
        }

        public void Start()
        {
            if (self != null)
                return;
            
            GameObject newSelf = new GameObject();
            self = newSelf;
            self.AddComponent<FlyMode>();
            self.AddComponent<BoneEsp>();

            CommandHandler handler = self.AddComponent<CommandHandler>();
            handler.AddBase<TargetCommands>();
            handler.AddBase<GlobalCommands>();

            Patching.Patches.Initialize();
            Patching.Patches.ApplyPatches();

            Assembly.Load(Resources._LZ4);
            Assembly.Load(Resources._SevenZip);

            if (MiscUtils.GetVRCApplicationSetup().buildNumber != BUILD_NUMBER)
            {
                ForegroundColor = ConsoleColor.Red;
                WriteLine("WRONG GAME VERSION!\nDetected: {0}\nExpected: Build {1}\nThis VRChat version is not supported and VRCheat may not work!\n", VRCApplicationSetup.GetBuildVersionString(), BUILD_NUMBER);
                ForegroundColor = ConsoleColor.Gray;
            }

            WriteLine("VRCheat {0} for VRChat {1}", Assembly.GetExecutingAssembly().GetName().Version, VRCApplicationSetup.GetBuildVersionString());
            Title = string.Format("VRCheat by Harekuin (VRChat {0})", VRCApplicationSetup.GetBuildVersionString());
            DontDestroyOnLoad(this);
        }
    }
}
