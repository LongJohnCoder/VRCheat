using System;
using UnityEngine;

namespace VRCheat.Commands
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class CommandAttribute : Attribute
    {
        public readonly KeyCode KeyCode;
        public readonly bool Hold;

        public CommandAttribute(KeyCode KeyCode, bool Hold = false)
        {
            this.KeyCode = KeyCode;
            this.Hold = Hold;
        }
    }
}
