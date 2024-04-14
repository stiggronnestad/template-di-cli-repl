using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Data;
using System.Reflection;
using Usignert.Di;

namespace Usignert.CommandLine
{
    /// <summary>
    /// CommandsHost
    /// Builder class that uses reflection to create commands and subcommands.
    /// The root command is built via the Build() method.
    /// </summary>
    [DiService(DiServiceAttribute.DiServiceType.Singleton)]
    public sealed class CommandsHost
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly List<Assembly> _assemblies = [];
        private RootCommand? _rootCommand;

        public CommandsHost(IServiceProvider services)
        {
            _serviceProvider = services;
        }

        public CommandsHost AddAssembly(Assembly assembly)
        {
            _assemblies.Add(assembly);
            return this;
        }

        public CommandsHost AddAssemblies(params Assembly[] assemblies)
        {
            _assemblies.AddRange(assemblies);
            return this;
        }

        public RootCommand Build(string description)
        {
            _rootCommand = new RootCommand(description);

            foreach (var assembly in _assemblies)
            {
                var commands = CreateCommands(assembly);

                foreach (var command in commands)
                {
                    _rootCommand.AddCommand(command);
                }
            }

            return _rootCommand;
        }

        public async Task<int> Parse(string command)
        {
            var args = CommandLineStringSplitter.Instance.Split(command);
            return await Parse(args.ToArray());
        }

        public async Task<int> Parse(string[] args)
        {
            if (_rootCommand == null)
                throw new Exception("Root command is null.");

            return await _rootCommand.InvokeAsync(args);
        }

        private Command[] CreateCommands(Assembly callingAssembly)
        {
            // Get all command attributes via reflection
            var commandAttributes = GetAllCommandAttributes(callingAssembly);
            var commands = new List<Command>();

            // Create a command for each command attribute
            foreach (var commandAttribute in commandAttributes)
            {
                var command = CreateCommand(commandAttribute.Key, commandAttribute.Value);
                commands.Add(command);
            }

            return [.. commands];
        }

        /// <summary>
        /// CreateCommand
        /// Create a command from a CommandAttribute and a Type.
        /// </summary>
        /// <param name="commandAttribute"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private Command CreateCommand(CommandAttribute commandAttribute, Type type)
        {
            //  Metadata for Command
            var commandName = commandAttribute.Name;
            var commandDescription = commandAttribute.Description;
            var commandInstance = ActivatorUtilities.CreateInstance(_serviceProvider, type) as ICommand;

            var command = new Command(commandName, commandDescription);

            if (commandInstance == null)
                throw new Exception("Command instance is null.");

            command.SetHandler((context) =>
            {
                commandInstance.Execute();
            });

            // Get all subcommands via reflection
            var subCommandTypes = GetAllSubCommandAttributes(type);

            foreach (var subCommandType in subCommandTypes)
            {
                var subCommandAttribute = subCommandType.Key;
                var subCommandMethod = subCommandType.Value;
                var subCommand = CreateCommand(subCommandAttribute, subCommandMethod);
                var parameters = subCommandMethod.GetParameters();

                // If there is no parameters, set the handler
                if (parameters.Length == 0)
                {
                    subCommand.SetHandler(() =>
                    {
                        subCommandMethod.Invoke(commandInstance, null);
                    });
                }
                else
                {
                    // Parse parameters and create delegates
                    var getValueDelegates = ParseParameters(subCommand, subCommandMethod, parameters);

                    // Set the handler using value delegates
                    subCommand.SetHandler((context) =>
                    {
                        var value = getValueDelegates[0](context);
                        var values = new List<object>();

                        for (int i = 0; i < getValueDelegates.Length; i++)
                        {
                            if (getValueDelegates[i] == null)
                                throw new Exception("Argument delegate is null.");

                            if (context == null)
                                throw new Exception("Context is null.");

                            // Get the value from the delegate in the context of the invocation
                            var delegateResult = getValueDelegates[i](context) ?? throw new Exception("Argument value is null.");

                            // Add the value to the list
                            values.Add(delegateResult);
                        }

                        // Invoke the subcommand method with the values
                        subCommandMethod.Invoke(commandInstance, [.. values]);
                    });
                }

                // Add the subcommand to the command
                command.AddCommand(subCommand);
            }

            // Return the command
            return command;
        }

        /// <summary>
        /// CreateCommand
        /// Create a command from a SubCommandAttribute and a MethodInfo.
        /// </summary>
        /// <param name="subCommandAttribute"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        private static Command CreateCommand(SubCommandAttribute subCommandAttribute, MethodInfo method)
        {
            var commandName = subCommandAttribute.Name;
            var commandDescription = subCommandAttribute.Description;
            var command = new Command(commandName, commandDescription);

            return command;
        }

        /// <summary>
        /// ParseParameters
        /// Parse the parameters of a subcommand method and create delegates for each parameter.
        /// </summary>
        /// <param name="subCommand"></param>
        /// <param name="subCommandMethod"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private static Func<InvocationContext, object?>[] ParseParameters(Command subCommand, MethodInfo subCommandMethod, ParameterInfo[] parameters)
        {
            var getValueDelegates = new List<Func<InvocationContext, object?>>();
            var argumentAttributes = GetAllArgumentAttributes(subCommandMethod);

            // Create an Argument for each parameter
            for (int i = 0; i < parameters.Length; i++)
            {
                var parameterName = parameters[i].Name;
                var parameterType = parameters[i].ParameterType;
                var description = string.Empty;

                if (argumentAttributes.Count > i)
                {
                    var argumentAttribute = argumentAttributes[parameters[i]];
                    description = argumentAttribute.Description;
                }

                if (parameters[i].HasDefaultValue)
                {
                    //
                    // Named Option
                    //

                    Type genericOption = typeof(Option<>);
                    Type constructedOption = genericOption.MakeGenericType(parameterType);

                    // Create an instance of the Option and type/null check
                    if (Activator.CreateInstance(constructedOption, $"--{parameterName}", description) is not Option option)
                        throw new Exception("Option instance is null.");

                    option.SetDefaultValue(parameters[i].DefaultValue);

                    // Create a delegate to get the value of the option in the context of the invocation
                    getValueDelegates.Add((context) => context.ParseResult.GetValueForOption(option));

                    // Add the option to the subcommand
                    subCommand.AddOption(option);
                }
                else
                {
                    //
                    // Positional Argument
                    //

                    Type genericArgument = typeof(Argument<>);
                    Type constructedArgument = genericArgument.MakeGenericType(parameterType);

                    // Create an instance of the Argument and type/null check
                    if (Activator.CreateInstance(constructedArgument, parameterName, description) is not Argument argument)
                        throw new Exception("Argument instance is null.");

                    // Create a delegate to get the value of the argument in the context of the invocation
                    getValueDelegates.Add((context) => context.ParseResult.GetValueForArgument(argument));

                    // Add the argument to the subcommand
                    subCommand.AddArgument(argument);
                }
            }

            // Return the delegates
            return [.. getValueDelegates];
        }

        /// <summary>
        /// GetAllArgumentAttributes
        /// Get all parameters with the ArgumentAttribute and return them as a dictionary.
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        private static Dictionary<ParameterInfo, ArgumentAttribute> GetAllArgumentAttributes(MethodInfo methodInfo)
        {
            var argumentAttributes = new Dictionary<ParameterInfo, ArgumentAttribute>();

            var parameters = methodInfo.GetParameters();

            foreach (var parameter in parameters)
            {
                var attribute = parameter.GetCustomAttribute<ArgumentAttribute>();

                if (attribute != null)
                {
                    argumentAttributes.Add(parameter, attribute);
                }
            }

            return argumentAttributes;
        }

        /// <summary>
        /// GetAllSubCommandAttributes
        /// Get all methods with the SubCommandAttribute and return them as a dictionary.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static Dictionary<SubCommandAttribute, MethodInfo> GetAllSubCommandAttributes(Type type)
        {
            var subCommands = new Dictionary<SubCommandAttribute, MethodInfo>();

            var methods = type.GetMethods()
                .Where(m => m.GetCustomAttribute<SubCommandAttribute>() != null);

            foreach (var method in methods)
            {
                var attribute = method.GetCustomAttribute<SubCommandAttribute>();

                if (attribute != null)
                {
                    subCommands.Add(attribute, method);
                }
            }

            return subCommands;
        }

        /// <summary>
        /// GetAllCommandAttributes
        /// Get all types with the CommandAttribute and return them as a dictionary.
        /// </summary>
        /// <returns></returns>
        private static Dictionary<CommandAttribute, Type> GetAllCommandAttributes(Assembly callingAssembly)
        {
            var commandTypes = new Dictionary<CommandAttribute, Type>();

            var types = callingAssembly.GetTypes()
                .Where(t => t.GetCustomAttribute<CommandAttribute>() != null);

            foreach (var type in types)
            {
                var attribute = type.GetCustomAttribute<CommandAttribute>();

                if (attribute != null)
                {
                    commandTypes.Add(attribute, type);
                }
            }

            return commandTypes;
        }
    }
}
