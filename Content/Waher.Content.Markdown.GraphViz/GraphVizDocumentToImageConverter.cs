using System;
using System.IO;
using System.Threading.Tasks;
using Waher.Content.Images;
using Waher.Runtime.Inventory;
using Waher.Script;

namespace Waher.Content.Markdown.GraphViz
{
    /// <summary>
    /// Converts GraphViz documents to images.
    /// </summary>
    public class GraphVizDocumentToImageConverter : IContentConverter
    {
        /// <summary>
        /// Converts GraphViz documents to images.
        /// </summary>
        public GraphVizDocumentToImageConverter()
        {
        }

        /// <summary>
        /// Converts content from these content types.
        /// </summary>
        public string[] FromContentTypes => new string[] { GraphVizCodec.DefaultContentType };

        /// <summary>
        /// Converts content to these content types. 
        /// </summary>
        public virtual string[] ToContentTypes
        {
            get
            {
                return new string[]
                {
                    ImageCodec.ContentTypePng,
                    ImageCodec.ContentTypeSvg
                };
            }
        }

        /// <summary>
        /// How well the content is converted.
        /// </summary>
        public virtual Grade ConversionGrade => Grade.Excellent;

        /// <summary>
        /// Performs the actual conversion.
        /// </summary>
        /// <param name="State">State of the current conversion.</param>
        /// <returns>If the result is dynamic (true), or only depends on the source (false).</returns>
        public async Task<bool> ConvertAsync(ConversionState State)
        {
            string GraphDescription;

            using (StreamReader rd = new StreamReader(State.From))
            {
                GraphDescription = rd.ReadToEnd();
            }

            string s = State.ToContentType;
            int i;

            i = s.IndexOf(';');
            if (i > 0)
                s = s.Substring(0, i);

            bool Png = string.Compare(s, ImageCodec.ContentTypePng, true) == 0;
            bool Svg = string.Compare(s, ImageCodec.ContentTypeSvg, true) == 0;

            if (!(State.PossibleContentTypes is null))
            {
                foreach (string ContentType in State.PossibleContentTypes)
                {
                    s = ContentType;
                    i = s.IndexOf(';');
                    if (i > 0)
                        s = s.Substring(0, i);

                    Png |= string.Compare(s, ImageCodec.ContentTypePng, true) == 0;
                    Svg |= string.Compare(s, ImageCodec.ContentTypeSvg, true) == 0;
                }
            }

            string Language;

            if (string.IsNullOrEmpty(State.FromFileName))
                Language = "dot";
            else
            {
                Language = Path.GetExtension(State.FromFileName).ToLower();
                if (Language.StartsWith("."))
                    Language = Language.Substring(1);

                switch (Language)
                {
                    case "dot":
                    case "neato":
                    case "fdp":
                    case "sfdp":
                    case "twopi":
                    case "circo":
                        break;

                    default:
                        Language = "dot";
                        break;
                }
            }

            Variables Variables = new Variables();
            GraphInfo Graph;

            if (Svg)
            {
                Graph = await GraphViz.GetFileName(Language, GraphDescription, ResultType.Svg, true, Variables);
                State.ToContentType = ImageCodec.ContentTypeSvg;
            }
            else if (Png)
            {
                Graph = await GraphViz.GetFileName(Language, GraphDescription, ResultType.Png, true, Variables);
                State.ToContentType = ImageCodec.ContentTypePng;
            }
            else
            {
                State.Error = new Exception("Unable to convert document from " + State.FromContentType + " to " + State.ToContentType);
                return false;
            }

            byte[] Data = await Runtime.IO.Files.ReadAllBytesAsync(Graph.FileName);

            await State.To.WriteAsync(Data, 0, Data.Length);

            return false;
        }

    }
}