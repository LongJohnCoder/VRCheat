using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using VRC.Core;
using VRCheat.Decompression;

namespace VRCheat.Utils
{
    public static class AvatarUtils
    {
        public static void Init(this ApiAvatar avatar, string id, APIUser user, string name, string imageUrl, string assetUrl, string description, string releaseStatus, List<string> tags, string packageUrl = null)
        {
            avatar.id = id;
            avatar.authorName = user.displayName;
            avatar.authorId = user.id;
            avatar.name = name;
            avatar.assetUrl = assetUrl;
            avatar.imageUrl = imageUrl;
            avatar.description = description;
            avatar.releaseStatus = releaseStatus;
            avatar.tags = tags;
            avatar.unityPackageUrl = packageUrl;
        }

        public static void DeleteAvatar(ApiAvatar avatar)
            => avatar.Delete(s =>
            {
                Console.WriteLine("Avatar deleted.");
            }, e => Console.WriteLine("Error deleting avatar: {0}", e.Error));

        private static void DownloadVRCImage(string address, string outFileName, out string finalFilePath)
        {
            using (WebClient wc = new WebClient())
            {
                byte[] imageBytes = wc.DownloadData(address);
                finalFilePath = string.Concat(outFileName, '.', wc.ResponseHeaders[HttpResponseHeader.ContentType].Split('/')[1]);
                File.WriteAllBytes(finalFilePath, imageBytes);
            }
        }

        public static ApiAvatar SaveAvatar(VRCPlayer vrcPlayer, string name, string imageUrl = "") => SaveAvatar(vrcPlayer.GetApiAvatar(), name);
        public static ApiAvatar SaveAvatar(ApiAvatar avatar, string name)
        {
            Console.WriteLine("Saving avatar...");
            ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => true;
            using (WebClient wc = new WebClient())
            {
                string tempFile = string.Concat(Path.GetTempPath(), Path.GetRandomFileName(), ".vrca");
                try
                {
                    string friendlyName = string.Concat(MiscUtils.CalculateHash<MD5>(Guid.NewGuid().ToString()), Path.GetRandomFileName());
                    wc.DownloadFile(avatar.assetUrl, tempFile);
                    
                    Console.WriteLine("Decompressing...");
                    AssetBundle assetBundle = new AssetBundle(tempFile);
                    Console.WriteLine("Decompressed...");
                    
                    void OnError(string error)
                    {
                        Console.WriteLine("Error saving avatar: {0}", error);
                        File.Delete(tempFile);
                    }

                    ApiFile.Create(friendlyName, "application/x-avatar", ".vrca", s =>
                    {
                        string fileUrl = string.Format("https://api.vrchat.cloud/api/1/file/{0}/1/file", s.Model.id);

                        ApiAvatar newAvatar = new ApiAvatar();
                        newAvatar.Init(null, APIUser.CurrentUser, name, avatar.imageUrl, fileUrl, avatar.description, "private", null, null);
                        newAvatar.Save(success =>
                        {
                            Console.WriteLine("Changing blueprint id...");
                            assetBundle.SetAvatarId(success.Model.id);
                            Console.WriteLine("Recompressing...");
                            assetBundle.SaveTo(tempFile);
                            Console.WriteLine("Uploading file...");

                            ApiFileHelper.UploadFileAsync(tempFile, null, friendlyName, (assetFile, bt) =>
                            {
                                DownloadVRCImage(avatar.imageUrl, friendlyName, out string imagePath);
                                ApiFileHelper.UploadFileAsync(imagePath, null, friendlyName, (imageFile, msg) =>
                                {
                                    Console.WriteLine("Saving ApiAvatar...");
                                    newAvatar.imageUrl = imageFile.GetFileURL();
                                    newAvatar.assetUrl = assetFile.GetFileURL();
                                    newAvatar.Save(succ => { Console.WriteLine("Avatar saved!"); }, er => Console.WriteLine(er.Error));
                                }, (_, error) => OnError(error), (_, b, d, e) => { }, _ => false);

                            }
                            , (_, error) => OnError(error), (az, b, d, e) => { }, _ => false);
                        });
                    }, error => OnError(error.Error));

                }
                catch (Exception e)
                {
                    Console.WriteLine("Error saving avatar: {0}", e.Message);
                    File.Delete(tempFile);
                }
            }

            return avatar;
        }
    }
}
