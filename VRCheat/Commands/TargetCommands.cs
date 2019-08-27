using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using VRC;
using VRC.Core;
using VRCheat.Utils;
using VRCSDK2;

namespace VRCheat.Commands
{
    public class TargetCommands : CommandsBase
    {
        private MethodInfo quickMenuInstanceMethod;
        private FieldInfo selectedUserField;

        private QuickMenu menu;
        private APIUser selectedUser;
        private VRC.Player selectedPlayer;
        private VRCPlayer selectedVrcPlayer;

        public TargetCommands()
        {
            try
            {
                quickMenuInstanceMethod = typeof(QuickMenu).GetProperties().First(p => p.PropertyType == typeof(QuickMenu)).GetGetMethod();
                selectedUserField = typeof(QuickMenu).GetFields(BindingFlags.NonPublic | BindingFlags.Instance).First(f => f.FieldType == typeof(APIUser));
            }
            catch
            {
                Console.WriteLine("Error reflecting in SelectedPlayerCommands.");
            }
        }

        public override bool Precondition()
        {
            menu = (QuickMenu)quickMenuInstanceMethod.Invoke(null, null);
            selectedUser = (APIUser)selectedUserField.GetValue(menu);

            if (selectedUser != null)
            {
                selectedPlayer = PlayerManager.GetPlayer(selectedUser.id);
                selectedVrcPlayer = selectedPlayer.vrcPlayer;
                return true;
            }

            return false;
        }

        //

        [Command(KeyCode.Mouse2)]
        public void SaveAvatar()
            => AvatarUtils.SaveAvatar(selectedVrcPlayer, ConsoleUtils.AskInput("Enter avatar's name: "));

        [Command(KeyCode.T)]
        public void TeleportToPlayer()
            => PlayerUtils.TeleportTo(selectedPlayer);
    }
}
