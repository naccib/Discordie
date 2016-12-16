using System;
using System.Collections.Generic;

namespace Discordie.Commands
{
    /// <summary>
    /// A class that represents everything you can do to abstract a Discord command.
    /// </summary>
    public class Command
    {
        /// <summary>
        /// The command prefix that all commands will follow.
        /// </summary>
        public static string CommandPrefix = "!";

        /// <summary>
        /// The identifier that triggers the current <see cref="Command"/>.
        /// </summary>
        public string Identifier { get; private set; }

        /// <summary>
        /// The action that is triggered once the current <see cref="Command"/> fires up.
        /// </summary>
        private Action<CommandInfo> Action { get; set; }

        /// <summary>
        /// The conditions that must be met so the current <see cref="Command"/> fires up.
        /// </summary>
        private Dictionary<Predicate<CommandInfo>, string> Conditions { get; set; }

        /// <summary>
        /// The parameters that are required for the <see cref="Command"/> be executed.
        /// </summary>
        private List<string> RequiredParams { get; set; }

        /// <summary>
        /// The default parameters of the current <see cref="Command"/>.
        /// </summary>
        private Dictionary<string, string> DefaultParams { get; set; }

        /// <summary>
        /// Creates a new <see cref="Command"/> that is triggered with the given identifier.
        /// </summary>
        /// <param name="identifier">The identifier that triggers the <see cref="Command"/>.</param>
        public Command(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
                throw new ArgumentException("identifier cannot be null or empty.");

            Identifier = identifier;

            Conditions = new Dictionary<Predicate<CommandInfo>, string>();
            RequiredParams = new List<string>();
            DefaultParams = new Dictionary<string, string>();
        }

        /// <summary>
        /// Invokes the current <see cref="Command"/> action if all conditions are met.
        /// </summary>
        internal virtual void Invoke(CommandInfo _cinfo)
        {
            bool runable = true;

            foreach (var pairs in Conditions)
                if (!pairs.Key(_cinfo))
                {
                    _cinfo.Complain(pairs.Value);
                    runable = false;
                }

            foreach (var param in RequiredParams)
                if(!_cinfo.Arguments.Pairs.ContainsKey(param))
                {
                    _cinfo.Complain($"Missing parameter: `{param}`.\nUse `... -{param} value ...` to fix this.");
                    runable = false;
                }

            foreach (var param in DefaultParams)
                if (!_cinfo.Arguments.Pairs.Contains(param))
                    _cinfo.Arguments.Pairs.Add(param.Key, param.Value);

            if (runable)
                Action(_cinfo);
        }

        /// <summary>
        /// Dynamically sets the current <see cref="Command"/> action.
        /// </summary>
        /// <param name="action">The action that handles the command.</param>
        public Command Do(Action<CommandInfo> action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            Action = action;

            return this;
        }

        /// <summary>
        /// Dynamically adds a condition to the current <see cref="CommandInfo"/>.
        /// </summary>
        /// <param name="condition">The condition that must be met.</param>
        public Command Require(Predicate<CommandInfo> condition, string errorMessage = "An error ocurred.")
        {
            if (condition == null)
                throw new ArgumentNullException("condition");

            Conditions.Add(condition, errorMessage);

            return this;
        }

        /// <summary>
        /// Dynamically adds required parameters to the current <see cref="Command"/>.
        /// </summary>
        /// <param name="parametersName">The parameters</param>
        public Command Require(params string[] parametersName)
        {
            if (parametersName == null)
                throw new ArgumentNullException("parameterName");

            RequiredParams.AddRange(parametersName);
            return this;
        }
        
        /// <summary>
        /// Sets the default value for a parameter.
        /// </summary>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <param name="defaultValue">The default value of the parameter.</param>
        public Command SetDefault(string parameterName, string defaultValue)
        {
            if (parameterName == null || defaultValue == null)
                throw new ArgumentNullException(nameof(parameterName));

            DefaultParams.Add(parameterName, defaultValue);

            return this;
        } 
    }
}
