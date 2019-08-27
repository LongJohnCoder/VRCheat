using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using UnityEngine.Analytics;
using VRC.Core;
using VRCheat.Utils;

namespace VRCheat.Patching
{
    public static class Patches
    {
        private static Assembly HarmonyAssembly;
        private static readonly Type type = typeof(Patches);

        private static string _fakeDeviceId;
        private static string FakeDeviceId
        {
            get
            {
                if(_fakeDeviceId == null)
                {
                    Console.WriteLine("Generating fake HWID.");
                    _fakeDeviceId = MiscUtils.CalculateHash<SHA1>(Guid.NewGuid().ToString());
                    Console.WriteLine("HWID Generated ({0}).", _fakeDeviceId);
                }

                return _fakeDeviceId;
            }
        }

        public static void Initialize()
        {
            if (HarmonyAssembly == null)
            {
                Console.WriteLine("Loading 0Harmony.dll...");
                HarmonyAssembly = Assembly.Load(Resources._0Harmony);
                Console.WriteLine("0Harmony.dll loaded.");
            }
        }

        static HarmonyMethod GetPatch(string name)
            => new HarmonyMethod(typeof(Patches).GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic));
        
        public static void ApplyPatches()
        {
            try
            {
                HarmonyInstance Harmony = HarmonyInstance.Create(string.Empty);
                Console.WriteLine("Applying patches...");

                HarmonyMethod voidPatch = GetPatch("VoidPatch");

                Harmony.Patch(typeof(UnityEngine.Analytics.Analytics).GetMethod("CustomEvent", new Type[] { typeof(string), typeof(IDictionary<string, object>) }), GetPatch("CustomEventPrefix"));
                //Harmony.Patch(typeof(ApiAnalyticEvent.EventInfo).GetMethod("Save"), voidPatch);//GetPatch("AnalyticEventSendPrefix"));
                Harmony.Patch(typeof(AmplitudeSDKWrapper.AmplitudeWrapper).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).First(m => m.Name == "LogEvent" && m.GetParameters().Length == 4), voidPatch);
                Harmony.Patch(typeof(API).GetProperty("DeviceID").GetGetMethod(), GetPatch("DeviceIdPrefix"));

                Console.WriteLine("Patches applied.");
            }
            catch (Exception e)
            {
                Console.WriteLine("Error applying patches: {0}", e);
            }
        }

        static bool VoidPatch() => false;

        static bool DeviceIdPrefix(ref string __result)
        {
            __result = FakeDeviceId;
            return false;
        }

        static bool CustomEventPrefix(ref AnalyticsResult __result)
        {
            __result = AnalyticsResult.Ok;
            return false;
        }
    }
}
