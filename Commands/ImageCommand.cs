using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Linq;
using System.Drawing;

namespace Discordie.Commands
{
    /// <summary>
    /// A custom <see cref="Command"/> that makes working with images easier.
    /// </summary>
    public class ImageCommand : Command
    {
        private IEnumerable<string> ImageParameters;

        private Func<IEnumerable<Image>, CommandInfo, CommandResult<IEnumerable<Image>>> ProcessFunction;

        public ImageCommand(string identifier) : base(identifier)
        {
            ImageParameters = new string[] { };
        }

        /// <summary>
        /// Adds parameters to download images from.
        /// </summary>
        /// <param name="imageParams">The parameters.</param>
        public ImageCommand AddImageParams(params string[] imageParams)
        {
            ImageParameters = imageParams;

            Require(ImageParameters.ToArray());

            return this;
        }

        /// <summary>
        /// Sets the process function.
        /// </summary>
        public ImageCommand Process(Func<IEnumerable<Image>, CommandInfo, CommandResult<IEnumerable<Image>>> processFunction)
        {
            ProcessFunction = processFunction;

            return this;
        }

        /// <summary>
        /// This will download all <see cref="Image"/>s from a <see cref="CommandInfo"/> and return.
        /// </summary>
        private IEnumerable<Image> GetImages(CommandInfo _cinfo)
        {
            List<Image> result = new List<Image>();

            Action<string> downloadImage = x =>
            {
                Uri uriResult;
                bool isURL = Uri.TryCreate(x, UriKind.Absolute, out uriResult)
                    && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

                if (isURL)
                {
                    using (WebClient wc = new WebClient())
                    {
                        byte[] data = wc.DownloadData(x);

                        using (MemoryStream ms = new MemoryStream(data))
                            result.Add(new Bitmap(ms));
                    }
                }
            };

            foreach(string arg in _cinfo.Arguments.RawArguments)
            {
                downloadImage(arg);
            }

            foreach(var pairs in _cinfo.Arguments.Pairs
                .Where(x => ImageParameters.Contains(x.Key)))
            {
                downloadImage(pairs.Value);
            }

            return result;
        }

        internal override void Invoke(CommandInfo _cinfo)
        {
            Do(x =>
            {
                var images = GetImages(_cinfo);

                if (images.Count() == 0)
                    x.Complain("No images were found.\nMake sure you pass images as `![command] http://url.com/image.png`.");
                else
                {
                    try
                    {
                        var result = ProcessFunction(images, _cinfo);

                        if (result.Failed && result.FailMessage != null)
                            x.Complain(result.FailMessage);
                        else
                            x.SendImages(result.Result);
                    }
                    catch(Exception ex)
                    {
                        x.Complain($"Exception thrown: {ex.Message}");
                    }
                }
            });

            base.Invoke(_cinfo);
        }
    }
}
