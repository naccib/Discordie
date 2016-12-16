﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord;
using System.Drawing;

namespace Discordie.Commands
{
    /// <summary>
    /// Holds all data about a possible <see cref="Command"/> that was generated by user input.
    /// </summary>
    public class CommandInfo
    {
        /// <summary>
        /// The <see cref="Discord.Channel"/> that the <see cref="Sender"/> sent the <see cref="CommandInfo"/> from.
        /// </summary>
        public Channel Channel { get; private set; }

        /// <summary>
        /// The <see cref="User"/> who sent the command.
        /// </summary>
        public User Sender { get; private set; }

        /// <summary>
        /// The <see cref="Discord.Server"/> that the <see cref="Sender"/> sent the <see cref="CommandInfo"/> from.
        /// </summary>
        public Server Server { get; private set; }

        /// <summary>
        /// The <see cref="Arguments"/> that are associeted with this <see cref="CommandInfo"/>.
        /// </summary>
        public Arguments Arguments { get; private set; }

        internal CommandInfo(MessageEventArgs args)
        {
            Channel = args.Channel;
            Sender = args.User;
            Server = args.Server;

            Arguments = new Arguments(args.Message.RawText);
        }

        /// <summary>
        /// Replies to the <see cref="Discord.Channel"/> that this <see cref="CommandInfo"/> was sent.
        /// </summary>
        /// <param name="messageText">The text to be replied.</param>
        public void ReplyToChannel(string messageText)
        {
            if (messageText == "")
                return;

            new Task(async () => await Channel.SendMessage(messageText))
                .Start();
        }

        /// <summary>
        /// Replies to the <see cref="User"/> that sent this <see cref="CommandInfo"/>.
        /// </summary>
        /// <param name="messageText"></param>
        public void ReplyToUser(string messageText)
        {
            new Task(async () => await Sender.SendMessage(messageText))
                .Start();
        }

        public void SendToChannel(object obj) => new Task(async () => await Channel.SendMessage(Convert.ToString(obj)));
        /// <summary>
        /// Complains to the current <see cref="Discord.Channel"/>.
        /// </summary>
        /// <param name="messageText">The complain text.</param>
        public void Complain(string messageText)
        {
            new Task(async () => await Channel.SendMessage($":exclamation: {messageText}"))
                .Start();
        }

        /// <summary>
        /// Informs something to the current <see cref="Discord.Channel"/>.
        /// </summary>
        /// <param name="messageText">The information text.</param>
        public void Inform(string messageText)
        {
            new Task(async () => await Channel.SendMessage($":information_source: {messageText}"))
                .Start();
        }

        /// <summary>
        /// Get a parameter as a number.
        /// </summary>
        /// <param name="paramName">The name of the parameter to be get as a number.</param>
        /// <returns>The number, if a conversion is possible.</returns>
        /// <exception cref="ArgumentException"></exception>
        public double GetAsNumber(string paramName)
        {
            if (!Arguments.HasPair(paramName))
                throw new ArgumentException($"The argument {paramName} does not exists.");

            try
            {
                return Double.Parse(Arguments.GetValue(paramName));
            }
            catch(Exception e)
            {
                throw new ArgumentException($"Could not convert {paramName} to a double.", e);
            }
        }

        public Tuple<bool, double> TryGetAsNumber(string paramName)
        {
            try
            {
                return new Tuple<bool, double>(true, GetAsNumber(paramName));
            }
            catch
            {
                return new Tuple<bool, double>(false, default(double));
            }
        }

        /// <summary>
        /// Sends all <see cref="Image"/>s to the current <see cref="Discord.Channel"/>.
        /// </summary>
        /// <param name="images">The <see cref="Image"/>s to be sent.</param>
        public void SendImages(IEnumerable<Image> images)
        {
            new Task(async () =>
            {
                foreach (var image in images)
                {
                    using (var newBitmap = new Bitmap(image))
                    {
                        ImageConverter converter = new ImageConverter();
                        byte[] data = (byte[])converter.ConvertTo(newBitmap, typeof(byte[]));

                        using (var newStream = new System.IO.MemoryStream(data))
                        {

                            await Channel.SendFile(new Guid().ToString() + ".png",
                                newStream);
                        }
                    }
                }
            }).Start();
            
        }
    }

    /// <summary>
    /// The result of the query of a parameter.
    /// </summary>
    public class ParameterResult <T>
    {
        /// <summary>
        /// Wether the query was succesfull or not.
        /// </summary>
        public bool Success { get; private set; }

        /// <summary>
        /// The value of the query.
        /// </summary>
        public T Value { get; private set; }
        
        internal ParameterResult(T t, bool ok)
        {
            Value = t;
            Success = ok;
        }

        public static implicit operator T(ParameterResult<T> pr) => pr.Value;

        public static implicit operator bool(ParameterResult<T> pr) => pr.Success;
    }
}
