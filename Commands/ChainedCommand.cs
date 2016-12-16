using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discordie.Commands
{
    /// <summary>
    /// A custom <see cref="Command"/> that will handle processing and displaying of a result.
    /// </summary>
    /// <typeparam name="T">The <see cref="ChainedCommand{T}"/> result type.</typeparam>
    public class ChainedCommand <T> : Command
    {
        internal Func<CommandInfo, CommandResult<T>> ProcessFuncition { get; set; }

        private Action<CommandInfo, CommandResult<T>> AnswerAction { get; set; }

        /// <summary>
        /// Creates a new <see cref="ChainedCommand"/> witch is triggered with the given identifier.
        /// </summary>
        /// <param name="identifier">The <see cref="Command"/> identifier.</param>
        public ChainedCommand(string identifier) : base(identifier)
        {
        }

        /// <summary>
        /// Sets the <see cref="Func{CommandInfo, CommandResult}"/> that will handle the processing of the current <see cref="ChainedCommand{T}"/>.
        /// </summary>
        /// <param name="processFunction">The function.</param>
        public ChainedCommand<T> Process(Func<CommandInfo, CommandResult<T>> processFunction)
        {
            ProcessFuncition = processFunction;

            return this;
        }

        /// <summary>
        /// Sets the <see cref="Action{CommandInfo, CommandResult"/> that will handle the result of the command.
        /// </summary>
        /// <param name="onComplete"></param>
        /// <returns></returns>
        public ChainedCommand<T> Completed(Action<CommandInfo, CommandResult<T>> onComplete)
        {
            AnswerAction = onComplete;

            return this;
        }

        internal override void Invoke(CommandInfo _cinfo)
        {
            Do(x =>
            {
                var result = ProcessFuncition(x);

                if(result.Failed && result.FailMessage != null)
                {
                    x.Complain(result.FailMessage);
                    return;
                }

                if (AnswerAction != null)
                    AnswerAction(x, result);
                else
                    _cinfo.SendToChannel(result.Result);
            });

            base.Invoke(_cinfo);
        }
    }
    
    /// <summary>
    /// The result of a <see cref="ChainedCommand{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="CommandResult{T}"/></typeparam>
    public class CommandResult <T>
    {
        public T Result { get; private set; }
        public bool Failed { get; private set; }
        public string FailMessage { get; private set; }

        public CommandResult(T result, bool fail = false, string failMessage = null)
        {
            Result = result;
            Failed = fail;
            FailMessage = failMessage;
        }

        public static implicit operator CommandResult<T>(T t)
        {
            return new CommandResult<T>(t);
        }
    }
}
