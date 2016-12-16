using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Discordie.Commands
{
    /// <summary>
    /// Holds all data that represents arguments to <see cref="Command"/>.
    /// There are three argument types: flags, pairs and raw arguments.
    /// </summary>
    public class Arguments
    {
        /// <summary>
        /// The prefix that all flags should have.
        /// </summary>
        public string FlagPrefix = "--";

        /// <summary>
        /// The prefix that all pairs should have.
        /// </summary>
        public string PairPrefix = "-";

        /// <summary>
        /// The identifier of the <see cref="Command"/>.
        /// </summary>
        public string Identifier
        {
            get;
            private set;
        }

        /// <summary>
        /// Get the pairs.
        /// </summary>
        public IDictionary<string, string> Pairs
        {
            get
            {
                return _pairs;
            }
        }

        /// <summary>
        /// Get the flags.
        /// </summary>
        public IEnumerable<string> Flags
        {
            get
            {
                return _flags;
            }
        }

        /// <summary>
        /// Get the raw arguments.
        /// </summary>
        public IEnumerable<string> RawArguments
        {
            get
            {
                return _rawArgs;
            }
        }

        /// <summary>
        /// Get the raw <see cref="string"/> of arguments.
        /// </summary>
        public string RawString { get; private set; }

        /// <summary>
        /// Get the argument <see cref="string"/>.
        /// </summary>
        public string ArgumentString { get; private set; }

        private Dictionary<string, string> _pairs = new Dictionary<string, string>();
        private List<string> _flags = new List<string>();
        private List<string> _rawArgs = new List<string>();

        /// <summary>
        /// Creates a new <see cref="Arguments"/>.
        /// </summary>
        /// <param name="rawMessageText">The raw message text.</param>
        internal Arguments(string rawMessageText)
        {
            Parse(rawMessageText);
        }

        /// <summary>
        /// Determines wether the current <see cref="CommandInfo"/> contains the given flag.
        /// </summary>
        /// <param name="flag">The flag to be checked.</param>
        public bool HasFlag(string flag)
        {
            return _flags.Contains(flag);
        }

        /// <summary>
        /// Determines wether the current <see cref="CommandInfo"/> contains the given pair key.
        /// </summary>
        /// <param name="pairKey">The pair key to be checked.</param>
        public bool HasPair(string pairKey)
        {
            return _pairs.ContainsKey(pairKey);
        }

        /// <summary>
        /// Gets the value of the given key.
        /// </summary>
        /// <param name="pairKey">The key.</param>
        /// <returns>The value if it exists, else <see cref="null"/></returns>.
        public string GetValue(string pairKey)
        {
            if (!HasPair(pairKey))
                return null;

            return _pairs[pairKey];
        }

        public string this[string key]
        {
            get { return _pairs[key]; }
        }

        /// <summary>
        /// Parses the text to <see cref="Flags"/>, <see cref="Pairs"/> and <see cref="RawArguments"/>.
        /// </summary>
        /// <param name="text">The message text.</param>
        protected void Parse(string text)
        {
            RawString = text.Remove(0, Command.CommandPrefix.Length);
            

            var parts = Regex.Matches(text, @"[\""].+?[\""]|[^ ]+")
                .Cast<Match>()
                .Select(m => m.Value)
                .ToList();

            Identifier = parts[0].Remove(0, Command.CommandPrefix.Length);
            parts.RemoveAt(0); // removes the identifier from the list.

            ArgumentString = RawString.Remove(0, Identifier.Length + 1);

            for (int i = 0; i < parts.Count; i++)
            {
                string part = parts[i];

                if(part.StartsWith(FlagPrefix))
                {
                    string flag = part.Remove(0, FlagPrefix.Length); // remove the flag prefix.
                    _flags.Add(flag);
                }
                else if(part.StartsWith(PairPrefix))
                {
                    string key = part.Remove(0, PairPrefix.Length); // remove the pair prefix.
                    string value = string.Empty;

                    if (parts.Last() != part)
                        value = parts[i + 1]; // save the next as the value and skip it.

                    _pairs.Add(key, value);
                    i++;
                }
                else // is a raw argument
                {
                    _rawArgs.Add(part);
                }
            }
        }
    }
}
