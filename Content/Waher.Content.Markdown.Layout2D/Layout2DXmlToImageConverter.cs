using SkiaSharp;
using System.Text;
using System.Threading.Tasks;
using Waher.Content.Images;
using Waher.Content.Xml.Text;
using Waher.Runtime.Inventory;
using Waher.Runtime.IO;
using Waher.Script;

namespace Waher.Content.Markdown.Layout2D
{
    /// <summary>
    /// Converts GraphViz documents to images.
    /// </summary>
    public class Layout2DXmlToImageConverter : IContentConverter
    {
        /// <summary>
        /// Converts GraphViz documents to images.
        /// </summary>
        public Layout2DXmlToImageConverter()
        {
        }

        /// <summary>
        /// Converts content from these content types.
        /// </summary>
        public string[] FromContentTypes => XmlCodec.XmlContentTypes;

        /// <summary>
        /// Converts content to these content types. 
        /// </summary>
        public virtual string[] ToContentTypes => ImageCodec.ImageContentTypes;

        /// <summary>
        /// How well the content is converted.
        /// </summary>
        public virtual Grade ConversionGrade => Grade.Barely;

		/// <summary>
		/// Performs the actual conversion.
		/// </summary>
		/// <param name="State">State of the current conversion.</param>
		/// <returns>If the result is dynamic (true), or only depends on the source (false).</returns>
		public async Task<bool> ConvertAsync(ConversionState State)
        {
            byte[] Bin = await State.From.ReadAllAsync();
            string Xml = Strings.GetString(Bin, Encoding.UTF8);
            string s = State.ToContentType;
            int i;

            i = s.IndexOf(';');
            if (i > 0)
                s = s.Substring(0, i);

            string Extension;
            SKEncodedImageFormat Format;
            int Quality = 100;

            switch (s.ToLower())
            {
                case ImageCodec.ContentTypePng:
                    Format = SKEncodedImageFormat.Png;
                    Extension = ImageCodec.FileExtensionPng;
                    break;

                case ImageCodec.ContentTypeBmp:
                    Format = SKEncodedImageFormat.Bmp;
                    Extension = ImageCodec.FileExtensionBmp;
                    break;

                case ImageCodec.ContentTypeGif:
                    Format = SKEncodedImageFormat.Gif;
                    Extension = ImageCodec.FileExtensionGif;
                    break;

                case ImageCodec.ContentTypeJpg:
                    Format = SKEncodedImageFormat.Jpeg;
                    Extension = ImageCodec.FileExtensionJpg;
                    Quality = 90;
                    break;

                case ImageCodec.ContentTypeWebP:
                    Format = SKEncodedImageFormat.Webp;
                    Extension = ImageCodec.FileExtensionWebP;
                    break;

                case ImageCodec.ContentTypeIcon:
                    Format = SKEncodedImageFormat.Ico;
                    Extension = ImageCodec.FileExtensionIcon;
                    break;

                case ImageCodec.ContentTypeTiff:
                case ImageCodec.ContentTypeWmf:
                case ImageCodec.ContentTypeEmf:
                case ImageCodec.ContentTypeSvg:
                default:
                    Format = XmlLayout.DefaultFormat;
                    Extension = XmlLayout.DefaultFileExtension;
                    State.ToContentType = XmlLayout.DefaultContentType;
                    break;
            }

            Variables Variables = new Variables();
            GraphInfo Graph = await XmlLayout.GetFileName("layout", Xml, Variables, Format, Quality, Extension);

            if (!Graph.Converted)
            {
                await State.To.WriteAsync(Bin, 0, Bin.Length);
                State.ToContentType = State.FromContentType;
                return true;
			}
			else if (Graph.Dynamic)
            {
                await State.To.WriteAsync(Graph.DynamicContent, 0, Graph.DynamicContent.Length);
                return true;
            }
            else
            {
                byte[] Data = await Runtime.IO.Files.ReadAllBytesAsync(Graph.FileName);

                await State.To.WriteAsync(Data, 0, Data.Length);

                return false;
            }
        }
    }
}