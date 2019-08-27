using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace VRCheat.Commands
{
    public class CommandHandler : MonoBehaviour
    {
        private GameObject self = new GameObject();
        private List<CommandsBase> bases = new List<CommandsBase>();
        
        public T AddBase<T>() where T : CommandsBase, new()
        {
            CommandsBase cBase = Activator.CreateInstance<T>();
            List<Command> commands = new List<Command>();

            foreach(MethodInfo method in cBase.GetType().GetMethods())
            {

                if (method.GetCustomAttributes(false).FirstOrDefault(a => a is CommandAttribute) is CommandAttribute attribute)
                    commands.Add(new Command(cBase, method, attribute.KeyCode, attribute.Hold));
            }

            cBase.Commands = commands.ToArray();
            bases.Add(cBase);

            return cBase as T;
        }

        public void Awake()
        {
            DontDestroyOnLoad(this);
        }

        public void Update()
        {
            foreach (CommandsBase cBase in bases)
                if (cBase.Precondition())
                    foreach (Command command in cBase.Commands)
                        if ((command.Hold && Input.GetKey(command.KeyCode)) || (!command.Hold && Input.GetKeyDown(command.KeyCode)))
                            command.Execute();
        }
    }
}
