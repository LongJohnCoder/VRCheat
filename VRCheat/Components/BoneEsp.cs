using System.Linq;
using System.Reflection;
using UnityEngine;
using VRC;
using VRC.Core;
using VRCheat.Utils;

namespace VRCheat.Components
{
    public class BoneEsp : MonoBehaviour
    {
        private Texture2D _tex;
        public static bool BonesEnabled = true;
        public static bool NamesEnabled = false;

        public void OnGUI()
        {
            if (_tex == null)
                _tex = new Texture2D(1, 1);

            Camera cam = VRCVrCamera.GetInstance().screenCamera;
            foreach (Player p in PlayerManager.GetAllPlayers())
            {
                ApiAvatar userAvatar = p.GetApiAvatar();
                APIUser user = p.GetAPIUser();

                if (user == null || user.id == APIUser.CurrentUser.id)
                    continue;

                if (BonesEnabled)
                {
                    GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                    bool drewName = false;

                    Animator animator = p.vrcPlayer?.GetVRCAvatarManager()?.currentAvatarObject?.GetComponent<Animator>();
                    if (animator != null)
                    {
                        for (int i = 0; i < 56; i++)
                        {
                            HumanBodyBones bone = (HumanBodyBones)i;
                            Transform boneTransform = animator.GetBoneTransform(bone);
                            if (boneTransform == null)
                                continue;

                            Vector3 bonePosition = boneTransform.position;
                            if (Vector3.Dot(cam.transform.forward, bonePosition - cam.transform.position) > 0)
                            {
                                Vector3 boneScreenPosition = cam.WorldToScreenPoint(bonePosition);
                                Graphics.DrawTexture(new Rect(boneScreenPosition.x, Screen.height - boneScreenPosition.y, 2, 2), _tex);
                                if (NamesEnabled && bone == HumanBodyBones.Head)
                                {
                                    bonePosition.y += 0.25f;
                                    boneScreenPosition = cam.WorldToScreenPoint(bonePosition);
                                    GUI.Label(new Rect(boneScreenPosition.x - 50, Screen.height - boneScreenPosition.y - 50, 100, 100), user.displayName, GUI.skin.label);
                                    drewName = true;
                                }
                            }
                        }
                    }

                    if (NamesEnabled && !drewName)
                    {
                        Vector3 playerPosition = p.vrcPlayer.transform.position;
                        if (Vector3.Dot(cam.transform.forward, playerPosition - cam.transform.position) > 0)
                        {
                            Vector3 playerSreenPosition = cam.WorldToScreenPoint(playerPosition);
                            GUI.Label(new Rect(playerSreenPosition.x - 50, Screen.height - playerSreenPosition.y - 50, 100, 100), user.displayName, GUI.skin.label);
                        }
                    }
                }
            }
        }
    }
}
