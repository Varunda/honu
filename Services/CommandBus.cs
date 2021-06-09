using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using watchtower.Commands;

namespace watchtower.Services {

    public class CommandBus : ICommandBus {

        private readonly ILogger<CommandBus> _Logger;
        private readonly IServiceProvider _Services;

        private List<Type> _Commands;

        public CommandBus(ILogger<CommandBus> logger,
            IServiceProvider services) {

            _Logger = logger;
            _Services = services;

            _Commands = new List<Type>();
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes()) {
                if (type.GetCustomAttribute<CommandAttribute>() != null) {
                    _Commands.Add(type);
                }
            }
        }

        public async Task Execute(string command) {
            if (String.IsNullOrWhiteSpace(command)) {
                return;
            }

            string[] args = command.Trim().Split(" ");
            if (args.Length == 0) {
                _Logger.LogWarning($"Failed to split {command} into args");
                return;
            }

            if (args[0] == ".list") {
                _PrintMethods();
                return;
            }


            Type? cmdClass = _Commands.Find(iter => iter.Name.ToLower() == $"{args[0]}Command".ToLower() || iter.Name.ToLower() == args[0].ToLower());
            if (cmdClass == null) {
                _Logger.LogWarning($"{args[0]} is not a valid command");
                return;
            }

            if (args.Length == 1) {
                string classUsage = _PrintClassMethods(cmdClass);
                _Logger.LogWarning($"Missing operation\n{classUsage}");
                return;
            }

            MethodInfo? cmd = null;
            foreach (MethodInfo possibleMethod in cmdClass.GetMethods()) {
                if (possibleMethod.Name.ToLower() == args[1].ToLower()
                        && possibleMethod.IsStatic == false
                        && possibleMethod.IsVirtual == false
                        && possibleMethod.IsPublic == true) {

                    cmd = possibleMethod;
                }
            }

            if (cmd == null) {
                _Logger.LogError($"{args[1]} is not a valid method");
                return;
            }

            ParameterInfo[] parms = cmd.GetParameters();
            if (parms.Length != args.Length - 2) {
                _Logger.LogError($"Incorrect number of arguments passed to {cmd.Name}. Expected {parms.Length}, got {args.Length - 2}");
                return;
            }

            object[] methodParams = new object[parms.Length];
            for (int i = 2; i < args.Length; ++i) {
                Type paramType = parms[i - 2].ParameterType;
                try {
                    if (paramType == typeof(string)) {
                        methodParams[i - 2] = args[i];
                    } else if (paramType == typeof(int)) {
                        methodParams[i - 2] = int.Parse(args[i]);
                    }
                } catch (Exception ex) {
                    _Logger.LogError(ex, "Failed to convert {param} to {type}", args[i], paramType.Name);
                    return;
                }
            }

            try {
                object? clazz = Activator.CreateInstance(cmdClass, _Services);

                if (cmd.ReturnType == typeof(Task)) {
                    Task? task = (Task?)cmd.Invoke(clazz, methodParams);
                    if (task != null) {
                        await task;
                    }
                } else {
                    cmd.Invoke(clazz, methodParams);
                }
            } catch (Exception ex) {
                _Logger.LogError(ex, "Failed to execute '{command}'", command);
            }
        }

        private void _PrintMethods() {
            StringBuilder msg = new StringBuilder();
            msg.AppendLine("Commands available: ");

            foreach (Type cmdClass in _Commands) {
                msg.Append(_PrintClassMethods(cmdClass));
            }

            Console.WriteLine(msg);
        }

        private string _PrintClassMethods(Type clazz) {
            StringBuilder msg = new StringBuilder();
            msg.AppendLine("Commands available: ");
            foreach (MethodInfo method in clazz.GetMethods()) {
                if (method.IsStatic == true || method.IsPublic == false || method.IsVirtual == true || method.Name == "GetType") {
                    continue;
                }
                msg.AppendLine($"\t{clazz.Name} {method.Name} {String.Join(" ", method.GetParameters().Select(i => $"{i.Name}:{i.ParameterType.Name}"))}");
            }
            return msg.ToString();
        }
        

    }
}
