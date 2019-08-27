using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using VRC;
using VRC.Core;
using ExitGames.Client.Photon;

namespace VRCheat.Utils
{
    static class PlayerUtils
    {
        public static Type UserType;

        private static Assembly cSharp;
        private static Type photonPlayerType;
        private static MethodInfo getPlayerMethod;
        private static MethodInfo isModeratorMethod;
        private static MethodInfo currentUserMethod;
        private static MethodInfo getApiUserMethod;
        private static MethodInfo getPhotonPlayerMethod;
        private static MethodInfo getAvatarManagerMethod;
        private static MethodInfo getApiAvatarMethod;

        static PlayerUtils()
        {
            try
            {
                getPlayerMethod = typeof(VRCPlayer).GetProperties().First(p => p.GetGetMethod().Name == "get_player")?.GetGetMethod();
                photonPlayerType = typeof(Player).GetProperties().First(p => p.GetGetMethod().Name == "get_PhotonPlayer").PropertyType;
                //customPropertiesMethod = photonPlayerType.GetProperties().First(p => p.GetCustomAttributes(typeof(ObsoleteAttribute), false).Any(a => ((ObsoleteAttribute)a).Message.Contains("CustomProperties")))?.GetGetMethod();
                isModeratorMethod = typeof(Player).GetProperties().First(p => p.GetGetMethod().Name == "get_isModerator")?.GetGetMethod();
                cSharp = typeof(VRCApplicationSetup).Assembly;
                UserType = cSharp.GetTypes().First(t => t.BaseType == typeof(APIUser));
                currentUserMethod = UserType.GetProperties().First(p => p.PropertyType == UserType)?.GetGetMethod();
                getApiUserMethod = typeof(Player).GetProperties().First(p => p.PropertyType == typeof(APIUser))?.GetGetMethod();
                getPhotonPlayerMethod = typeof(Player).GetProperties().First(p => p.PropertyType == photonPlayerType)?.GetGetMethod();
                getAvatarManagerMethod = typeof(VRCPlayer).GetProperties().First(p => p.PropertyType == typeof(VRCAvatarManager)).GetGetMethod();
                getApiAvatarMethod = typeof(VRCAvatarManager).GetProperties().First(p => p.PropertyType == typeof(ApiAvatar)).GetGetMethod();
            }
            catch
            {
                Console.WriteLine("Error loading player types, fields, and methods.\n");
            }
        }

        private static object GetPhotonPlayer(this Player player) => getPhotonPlayerMethod.Invoke(player, null);
        public static object GetCurrentUser() => currentUserMethod.Invoke(null, null);

        public static VRCAvatarManager GetVRCAvatarManager(this VRCPlayer player)
            => getAvatarManagerMethod.Invoke(player, null) as VRCAvatarManager;

        public static ApiAvatar GetApiAvatar(this VRCAvatarManager avatarManager) => getApiAvatarMethod.Invoke(avatarManager, null) as ApiAvatar;
        public static ApiAvatar GetApiAvatar(this VRCPlayer player) => player.GetVRCAvatarManager().GetApiAvatar();
        public static ApiAvatar GetApiAvatar(this Player player) => player.vrcPlayer.GetApiAvatar();
        public static APIUser GetAPIUser(this Player player) => (APIUser)getApiUserMethod.Invoke(player, null);

        public static bool IsModerator(this Player player) => (bool)isModeratorMethod.Invoke(player, null);

        public static void TeleportTo(Player targetPlayer) => TeleportTo(targetPlayer.vrcPlayer);
        public static void TeleportTo(VRCPlayer targetPlayer) => Teleport(PlayerManager.GetCurrentPlayer().vrcPlayer, targetPlayer);

        public static void Teleport(Player player, Player targetPlayer) => Teleport(player.vrcPlayer, player.vrcPlayer);
        public static void Teleport(VRCPlayer player, VRCPlayer targetPlayer) => TeleportTransform(player.transform, targetPlayer.transform);

        public static Player FindPlayer(string search)
        {
            search = search.ToLower();
            return PlayerManager.GetAllPlayers().FirstOrDefault(p => p.GetAPIUser().displayName.ToLower().Contains(search));
        }

        public static void Follow(string user)
        {
            user = user.ToLower();
            APIUser.FetchUsers(user, users =>
            {
                Console.WriteLine(string.Join(", ", users.Select(u => string.Format("{0} ({1}, {2}, {3})", u.displayName, u.location, u.worldId, u.instanceId)).ToArray()));
                APIUser apiUser = users.Where(u => u.displayName.ToLower().Contains(user)).OrderBy(u => u.displayName.Length).FirstOrDefault();

                if (user != null)
                    Follow(apiUser);
                else
                    Console.WriteLine("User not found!");
            }, error => Console.WriteLine("Error fetching user: {0}", error));
        }

        public static void Follow(APIUser user)
        {
            if (user.location != "offline")
                if (user.location != "private")
                {
                    string[] parsedLocation = user.location.Split(':');

                    if (parsedLocation.Length > 1)
                    {
                        string worldId = user.location.Split(':')[0];
                        string worldInstance = user.location.Split(':')[1];

                        //ApiWorld.Fetch(worldId, onSuccess: w =>
                        //{
                        //    //Console.WriteLine("{0} is in {1} ({2})", user.displayName, w.name, ApiWorld.WorldInstance.GetAccessDetail(new ApiWorld.WorldInstance(worldInstance, 1).GetAccessType()).fullName);

                        //    Console.Write("Follow {0}? (y/n): ", user.displayName);
                        //    if (Console.ReadLine().ToLower() == "y")
                        //    {
                        //        Console.WriteLine("Following {0}", user.displayName);
                        //        MiscUtils.GetVRCFlowManagerInstance().EnterRoom(user.location, error => Console.WriteLine("Error following user: {0}", error));
                        //    }
                        //}, onFailure: error => Console.WriteLine("Error fetching world: {0}", error));
                    }
                    else
                        Console.WriteLine("Could not parse user location \"{0}\"", user.location);
                }
                else
                    Console.WriteLine("User is in a private room!");
            else
                Console.WriteLine("User is offline!");
        }

        private static void TeleportTransform(Transform from, Transform to)
        {
            from.position = to.position;
            from.rotation = to.rotation;
        }
    }
}
