using System;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using VRC.Core;

namespace VRCheat.Utils
{
    static class MiscUtils
    {
        private static MethodInfo flowManagerMethod;
        private static MethodInfo uiManagerMethod;
        private static MethodInfo appSetupMethod;
        private static MethodInfo currentAvatarMethod;

        static MiscUtils()
        {
            try
            {
                flowManagerMethod = typeof(VRCFlowManager).GetProperties().FirstOrDefault(p => p.PropertyType == typeof(VRCFlowManager))?.GetGetMethod();
                uiManagerMethod = typeof(VRCUiManager).GetProperties().FirstOrDefault(p => p.PropertyType == typeof(VRCUiManager))?.GetGetMethod();
                appSetupMethod = typeof(VRCApplicationSetup).GetProperties().FirstOrDefault(p => p.PropertyType == typeof(VRCApplicationSetup))?.GetGetMethod();
                currentAvatarMethod = typeof(VRCApplicationSetup).GetProperties().FirstOrDefault(p => p.PropertyType == typeof(ApiAvatar))?.GetGetMethod();
            }
            catch
            {
                System.Console.WriteLine("Error loading VRChat information fields.");
            }
        }

        public static string CalculateHash<T>(string input) where T : HashAlgorithm
        {
            byte[] hashBytes = CalculateHash<T>(Encoding.UTF8.GetBytes(input));

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
                sb.Append(hashBytes[i].ToString("x2"));

            return sb.ToString();
        }

        public static byte[] CalculateHash<T>(byte[] buffer) where T : HashAlgorithm
        {
            using (T hashAlgorithm = typeof(T).GetMethod("Create", new Type[] { }).Invoke(null, null) as T)
                return hashAlgorithm.ComputeHash(buffer);
        }

        public static VRCFlowManager GetVRCFlowManagerInstance()
        {
            return (VRCFlowManager)flowManagerMethod.Invoke(null, null);
        }

        public static VRCApplicationSetup GetVRCApplicationSetup()
        {
            return (VRCApplicationSetup)appSetupMethod.Invoke(null, null);
        }

        public static VRCUiManager GetVRCUiManager()
        {
            return (VRCUiManager)uiManagerMethod.Invoke(null, null);
        }
    }
}
