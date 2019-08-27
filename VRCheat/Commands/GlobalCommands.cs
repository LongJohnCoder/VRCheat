using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using UnityEngine;
using VRC;
using VRC.Core;
using VRCheat.Components;
using VRCheat.Utils;

namespace VRCheat.Commands
{
    public class GlobalCommands : CommandsBase
    {
        private readonly ILogger oLogger = Debug.unityLogger;
        private readonly ILogger hLogger = new ConsoleLogger();

        public override bool Precondition()
            => Event.current.control;

        [Command(KeyCode.P)]
        public void ToggleDebugLogging()
        {
            FieldInfo loggerField = typeof(Debug).GetField("s_Logger", BindingFlags.Static | BindingFlags.NonPublic);

            bool isHooked = loggerField.GetValue(null) == hLogger;
            loggerField.SetValue(null, isHooked ? oLogger : hLogger);
            Console.WriteLine(string.Format("Debug logger {0}hooked.", isHooked ? "un" : string.Empty));
        }

        [Command(KeyCode.F)]
        public void ToggleFlyMode()
            => FlyMode.Enabled = !FlyMode.Enabled;

        [Command(KeyCode.O)]
        public void ToggleBoneEsp()
            => BoneEsp.BonesEnabled = !BoneEsp.BonesEnabled;

        [Command(KeyCode.L)]
        public void ToggleNameEsp()
            => BoneEsp.NamesEnabled = !BoneEsp.NamesEnabled;

        [Command(KeyCode.G)]
        public void TeleportTo()
        {
            Console.Clear();
            Console.WriteLine(string.Join(", ", PlayerManager.GetAllPlayers().Select(p => p.GetAPIUser().displayName).ToArray()));

            Player target = PlayerUtils.FindPlayer(ConsoleUtils.AskInput("Teleport to: "));
            PlayerUtils.TeleportTo(target);

            Console.WriteLine("Teleporting to: {0}", target.GetAPIUser().displayName);
        }

        [Command(KeyCode.M)]
        public void TeleportObjects()
            => new Thread(() =>
            {
                ObjectInternal[] pickUps = UnityEngine.Object.FindObjectsOfType<ObjectInternal>();

                foreach (ObjectInternal pu in pickUps)
                {
                    pu.RequestOwnership();

                    Thread.Sleep(200);

                    if (typeof(ObjectInternal).GetFields(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault(f => f.FieldType == typeof(VRCSDK2.VRC_Pickup)).GetValue(pu) != null)
                        pu.transform.position = PlayerManager.GetCurrentPlayer().transform.position;
                }
            }).Start();

        [Command(KeyCode.K)]
        public void SaveCurrentAvatar()
            => AvatarUtils.SaveAvatar(PlayerManager.GetCurrentPlayer().GetApiAvatar(), ConsoleUtils.AskInput("Enter avatar's name: ") ?? "Nameless");

        [Command(KeyCode.Delete)]
        public void DeleteCurrentAvatar()
            => AvatarUtils.DeleteAvatar(PlayerManager.GetCurrentPlayer().GetApiAvatar());

        [Command(KeyCode.I)]
        public void SetAvatarImage()
        {
            ApiAvatar currentAvatar = PlayerManager.GetCurrentPlayer().GetApiAvatar();

            if (currentAvatar == null)
                Console.WriteLine("Error getting current avatar!");
            else if (currentAvatar.authorId != APIUser.CurrentUser.id)
                Console.WriteLine("You must be the avatar's owner!");
            else
            {
                string imageUrl = ConsoleUtils.AskInput("Enter image URL: ");

                if (imageUrl == null)
                    Console.WriteLine("Invalid image URL!");
                else
                {
                    string extension = Path.GetExtension(new Uri(imageUrl).GetLeftPart(UriPartial.Path));

                    if (string.IsNullOrEmpty(extension) || (extension != ".jpeg" && extension != ".jpg" && extension != ".png"))
                    {
                        Console.WriteLine("File must be either a .jpg or .png (URL must end with file extension)");
                        return;
                    }

                    string tempFile = Path.Combine(Path.GetTempPath(), string.Concat(Path.GetRandomFileName(), '.', extension));
                    try
                    {
                        ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => true;
                        using (WebClient wc = new WebClient())
                        {
                            Console.WriteLine("Downloading image...");
                            wc.DownloadFile(imageUrl, tempFile);

                            void OnFileOpSuccess(ApiFile apiFile, string message)
                            {
                                currentAvatar.imageUrl = apiFile.GetFileURL();
                                currentAvatar.Save(s => Console.WriteLine("Avatar image updated!"), e => Console.WriteLine("Error upading image: {0}. {1}.", e.Error, e.Text));

                                File.Delete(tempFile);
                            }

                            void OnFileOpError(ApiFile apiFile, string error)
                            {
                                Console.WriteLine("Error uploading image: {0}", error);
                                File.Delete(tempFile);
                            }

                            Console.WriteLine("Uploading image...");
                            ApiFileHelper.UploadFileAsync(tempFile, null, Guid.NewGuid().ToString(), OnFileOpSuccess
                            , OnFileOpError, (a, b, c, d) => { }, _ => false);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error uploading image: {0}", e.Message);
                        File.Delete(tempFile);
                    }

                }
            }

            
        }

        [Command(KeyCode.Mouse2)]
        public void SaveAvatarId()
        {
            ApiAvatar avatar = new ApiAvatar()
            {
                id = ConsoleUtils.AskInput("Enter avatar ID: ")
            };

            avatar.Fetch(success =>
            {
                AvatarUtils.SaveAvatar(success.Model as ApiAvatar, ConsoleUtils.AskInput("Enter avatar's name: "));
            }, error => Console.WriteLine("Error saving avatar: {0}", error.Error));
        }
    }
}
