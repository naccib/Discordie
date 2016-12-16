using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discordie.Commands;

namespace Discordie
{
    /// <summary>
    /// Represents a Discord bot
    /// </summary>
    public class DiscordBot
    {
        /// <summary>
        /// The <see cref="DiscordClient"/> of the current <see cref="DiscordBot"/>.
        /// </summary>
        public DiscordClient Client { get; internal set; }

        /// <summary>
        /// The <see cref="CommandParser"/> that will parse all the <see cref="Command"/>s of the current <see cref="DiscordBot"/>.
        /// </summary>
        public CommandParser Parser { get; internal set; }

        /// <summary>
        /// Creates a new <see cref="DiscordBot"/>.
        /// </summary>
        /// <param name="token">The token of the bot.</param>
        public DiscordBot(string token) : this(token, new Command[] { }, null)
        {

        }

        /// <summary>
        /// Creates a new <see cref="DiscordBot"/> running the given <see cref="Command"/>s.
        /// </summary>
        /// <param name="token">The token of the bot.</param>
        /// <param name="commands">The <see cref="Command"/>s of the bot.</param>
        public DiscordBot(string token, IEnumerable<Command> commands) : this(token, commands, null)
        {

        }

        /// <summary>
        /// Creates a new <see cref="DiscordBot"/> running the given <see cref="Command"/>s.
        /// </summary>
        /// <param name="token">The token of the bot.</param>
        /// <param name="commands">The <see cref="Command"/>s of the bot.</param>
        /// <param name="config">The <see cref="DiscordConfig"/> of the bot.</param>
        public DiscordBot(string token, IEnumerable<Command> commands, DiscordConfig config)
        {
            if (commands == null)
                throw new ArgumentNullException(nameof(commands));

            Parser = new CommandParser(commands);

            if (config != null)
                Client = new DiscordClient(config);
            else
                Client = new DiscordClient();

            Client.ExecuteAndWait(async () => {
                Client.MessageReceived += GotMessage;

                await Client.Connect(token, TokenType.Bot);
                
            });
        }

        private void GotMessage(object sender, MessageEventArgs e)
        {
            Parser.ParseInput(e);
        }

        /// <summary>
        /// Adds a new <see cref="Command"/> to the current <see cref="Parser"/>.
        /// </summary>
        /// <param name="cmd">The command to be added.</param>
        public void AddCommand(params Command[] cmd)
        {
            Parser.Add(cmd);
        }
    }
}
