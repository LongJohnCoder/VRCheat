using System.Linq;
using System.Reflection;
using UnityEngine;
using VRC;

namespace VRCheat.Components
{
    public class FlyMode : MonoBehaviour
    {
        public static bool Enabled;
        private bool _enabled;

        private FieldInfo motionStateField;
        private LocomotionInputController inputController;
        private VRCMotionState motionState;
        private Vector3 originalGravity;

        public void Awake()
        {
            motionStateField = typeof(LocomotionInputController).GetFields(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault(f => f.FieldType == typeof(VRCMotionState));
            if (motionStateField == null)
                System.Console.WriteLine("Error loading VRCMotionState field.");

            DontDestroyOnLoad(this);
        }

        public void Update()
        {
            if (_enabled != Enabled)
            {
                _enabled = Enabled;
                if (Enabled)
                {
                    originalGravity = Physics.gravity;
                    Physics.gravity = Vector3.zero;
                }
                else
                    Physics.gravity = originalGravity;
            }
            
            bool isShift = Input.GetKey(KeyCode.LeftShift);
            int speed = isShift ? 8 : 4;
            if (Enabled)
            {
                VRCPlayer currentPlayer = PlayerManager.GetCurrentPlayer()?.vrcPlayer;

                if (currentPlayer != null)
                {
                    if (inputController == null)
                    {
                        inputController = currentPlayer.GetComponent<LocomotionInputController>();
                        motionState = (VRCMotionState)motionStateField.GetValue(inputController);
                    }

                    motionState.Reset();
                    Vector3 oldPos = currentPlayer.transform.position;

                    if (Input.GetKey(KeyCode.Q))
                        currentPlayer.transform.position = new Vector3(oldPos.x, oldPos.y - (speed * Time.deltaTime), oldPos.z);
                    else if (Input.GetKey(KeyCode.E))
                        currentPlayer.transform.position = new Vector3(oldPos.x, oldPos.y + (speed * Time.deltaTime), oldPos.z);
                }
            }

            inputController.strafeSpeed = Enabled ? speed : speed / 2;
            inputController.runSpeed = Enabled ? 8 : 4;
        }
    }
}
