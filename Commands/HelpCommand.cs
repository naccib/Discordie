using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Discordie.Commands
{
    /// <summary>
    /// A <see cref="Command"/> that is specialized in printing help to users.
    /// Normally, you would have only one of these per bot.
    /// </summary>
    public class HelpCommand : Command
    {
        private Dictionary<string, string> HelpDictionary = new Dictionary<string, string>();

        /// <summary>
        /// The format that the returned string from <see cref="GetHelpString(string)"/> will follow.
        /// </summary>
        public static string LookupErrorMessageFormat { get; set; } = "Could not find command with identifier {0}.";

        /// <summary>
        /// Creates a new <see cref="HelpCommand"/> from a JSON file.
        /// </summary>
        /// <param name="jsonFilePath">The path to a JSON file.</param>
        public static HelpCommand FromFile(string jsonFilePath)
        {
            if (String.IsNullOrEmpty(jsonFilePath))
                throw new ArgumentNullException("jsonFilePath");

            if (!System.IO.File.Exists(jsonFilePath))
                throw new System.IO.FileNotFoundException("The given jsonFile does not exists.");

            try
            {
                string content = System.IO.File.ReadAllText(jsonFilePath);
                return new HelpCommand("help", content);
            }
            catch(System.IO.IOException ex)
            {
                throw new System.IO.IOException("Could not read the jsonFile.", ex);
            }
        }

        /// <summary>
        /// Creates a new <see cref="HelpCommand"/>.
        /// </summary>
        public HelpCommand() : base("help")
        {
            Do(x => ExecuteHelp(x));
        }

        /// <summary>
        /// Creates a new <see cref="HelpCommand"/> with a given help dictionary.
        /// </summary>
        /// <param name="helpDictionary">The dictionary that will be looked up.</param>
        public HelpCommand(IDictionary<string, string> helpDictionary) : base("help")
        {
            if (helpDictionary == null)
                throw new ArgumentNullException("helpDictionary");

            HelpDictionary = (Dictionary<string, string>)HelpDictionary.Concat(helpDictionary);

            Do(x => ExecuteHelp(x));
        }

        /// <summary>
        /// Creates a new <see cref="HelpCommand"/> with a given identifier.
        /// </summary>
        /// <param name="identifier">The command identifier.</param>
        public HelpCommand(string identifier) : base(identifier)
        {
            Do(x => ExecuteHelp(x));
        }

        /// <summary>
        /// Creates a new <see cref="HelpCommand"/> with a given identifier and a lookup dictionary.
        /// </summary>
        /// <param name="identifier">The command identifier.</param>
        /// <param name="helpDictionary">The dictionary that will be looked up.</param>
        public HelpCommand(string identifier, IDictionary<string, string> helpDictionary) : base(identifier)
        {
            if (helpDictionary == null)
                throw new ArgumentNullException("helpDictionary");

            HelpDictionary = (Dictionary<string, string>)HelpDictionary.Concat(helpDictionary);
            Do(x => ExecuteHelp(x));
        }

        /// <summary>
        /// Creates a new <see cref="HelpCommand"/> from a identifier and a valid JSON string.
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        /// <param name="json">The JSON string containing the key value pairs.</param>
        public HelpCommand(string identifier, string json) : base(identifier)
        {
            try
            {
                JObject parsed = JObject.Parse(json);

                IDictionary<string, JToken> pairs = (JObject) parsed["commands"];
                HelpDictionary = pairs.ToDictionary(x => x.Key, x => (string)x.Value);                
            }
            catch(JsonException e)
            {
                throw new JsonException("Could not desserialize the JSON string.", e);
            }
            catch(Exception e)
            {
                throw new Exception("Unhandled exception.", e);
            }

            Do(x => ExecuteHelp(x));
        }
        
        /// <summary>
        /// Adds all <see cref="KeyValuePair{string, string}"/>s to the current command dictionary.
        /// </summary>
        /// <param name="pairs">The pairs to be added.</param>
        public HelpCommand Add(params KeyValuePair<string, string>[] pairs)
        {
            foreach (var kv in pairs)
                HelpDictionary.Add(kv.Key, kv.Value);

            return this;
        }

        /// <summary>
        /// Concatenates a given <see cref="IDictionary{TKey, TValue}"/> to the current command dictionary.
        /// </summary>
        public HelpCommand Add(IEnumerable<KeyValuePair<string, string>> dictionary)
        {
            if (dictionary == null)
                throw new ArgumentNullException("dictionary");

            HelpDictionary = (Dictionary<string, string>)HelpDictionary.Concat(dictionary);

            return this;
        }

        /// <summary>
        /// Get the help string of a given <see cref="Command"/> identifier.
        /// </summary>
        /// <param name="commandIdentifier">The <see cref="Command"/> identifier.</param>
        /// <returns>Returns the set <see cref="LookupErrorMessageFormat"/> if <paramref name="commandIdentifier"/> does not exists, else returns the help string.</returns>
        public string GetHelpString(string commandIdentifier)
        {
            if (String.IsNullOrEmpty(commandIdentifier))
                throw new ArgumentNullException("commandIdentifier");

            if (!HelpDictionary.ContainsKey(commandIdentifier))
                return String.Format(LookupErrorMessageFormat, commandIdentifier);
            else
                return HelpDictionary[commandIdentifier];
        }

        /// <summary>
        /// Gets the full help string.
        /// </summary>
        /// <returns>The full help string.</returns>
        public string GetHelpString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"Help for **{HelpDictionary.Count}** commands:");

            foreach (var kv in HelpDictionary)
                sb.AppendLine($"**{kv.Key}** → ```rb\n{kv.Value}```");

            return sb.ToString();
        }

        /// <summary>
        /// Gets a help string and sends to the user.
        /// </summary>
        internal void ExecuteHelp(CommandInfo _cinfo)
        {
            if (_cinfo.Arguments.RawArguments.Count() == 0) // has no args
                _cinfo.ReplyToUser(GetHelpString());
            else
                _cinfo.ReplyToUser(GetHelpString(_cinfo.Arguments.RawArguments.ElementAtOrDefault(0)));
        }
    }
}
