using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord;

namespace Discordie.Commands
{
    /// <summary>
    /// The <see cref="CommandParser"/> should be used to handle messages input/output.
    /// </summary>
    public class CommandParser
    {
        private List<Command> _commandLibrary;

        /// <summary>
        /// Creates a new <see cref="CommandParser"/> using the given <see cref="Command"/>s.
        /// </summary>
        /// <param name="commands">The initial <see cref="Command"/>s.</param>
        public CommandParser(IEnumerable<Command> commands)
        {
            if (commands == null)
                throw new ArgumentNullException("commands");

            _commandLibrary = commands.ToList();
        }

        /// <summary>
        /// Adds a new <see cref="Command"/> to the command library.
        /// </summary>
        /// <param name="command">The <see cref="Command"/> to be added.</param>
        public CommandParser Add(params Command[] command)
        {
            if (command == null)
                throw new ArgumentNullException("command");

            _commandLibrary.AddRange(command);
            return this;
        }

        /// <summary>
        /// Parses the input and fires a command if the <see cref="MessageEventArgs"/> matches the identifier.
        /// </summary>
        public void ParseInput(MessageEventArgs args)
        {
            if (args == null)
                throw new ArgumentNullException("args");

            var commandInfo = new CommandInfo(args);

            foreach (var command in _commandLibrary)
                if (command.Identifier == commandInfo.Arguments.Identifier)
                    command.Invoke(commandInfo);
        }
    }
}
