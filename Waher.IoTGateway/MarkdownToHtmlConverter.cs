using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Waher.Content;
using Waher.Content.Markdown;
using Waher.Content.Emoji;
using Waher.Content.Emoji.Emoji1;
using Waher.Script;

namespace Waher.IoTGateway
{
	/// <summary>
	/// Converts Markdown documents to HTML documents.
	/// </summary>
	public class MarkdownToHtmlConverter : IContentConverter
	{
		/// <summary>
		/// Converts Markdown documents to HTML documents.
		/// </summary>
		public MarkdownToHtmlConverter()
		{
		}

		/// <summary>
		/// Converts content from these content types.
		/// </summary>
		public string[] FromContentTypes
		{
			get
			{
				return new string[] 
				{
					"text/markdown"
				};
			}
		}

		/// <summary>
		/// Converts content to these content types. 
		/// </summary>
		public string[] ToContentTypes
		{
			get
			{
				return new string[] 
				{
					"text/html",
					"application/xhtml+xml"
				};
			}
		}

		/// <summary>
		/// How well the content is converted.
		/// </summary>
		public Grade ConversionGrade
		{
			get { return Grade.Excellent; }
		}

		/// <summary>
		/// Performs the actual conversion.
		/// </summary>
		/// <param name="FromContentType">Content type of the content to convert from.</param>
		/// <param name="From">Stream pointing to binary representation of content.</param>
		/// <param name="FromFileName">If the content is coming from a file, this parameter contains the name of that file. 
		/// Otherwise, the parameter is the empty string.</param>
		/// <param name="ToContentType">Content type of the content to convert to.</param>
		/// <param name="To">Stream pointing to where binary representation of content is to be sent.</param>
		public void Convert(string FromContentType, Stream From, string FromFileName, string ToContentType, Stream To)
		{
			StreamReader rd = new StreamReader(From);
			string Markdown = rd.ReadToEnd();
			rd.Dispose();

			MarkdownDocument Doc = new MarkdownDocument(Markdown, settings);
			Doc.FileName = FromFileName;

			string HTML = Doc.GenerateHTML();
			byte[] Data = Utf8WithBOM.GetBytes(HTML);
			To.Write(Data, 0, Data.Length);
		}

		internal static readonly Emoji1LocalFiles Emoji1_24x24 = new Emoji1LocalFiles(Emoji1SourceFileType.Svg, 24, 24, "/Graphics/Emoji1/svg/%FILENAME%");
		internal static readonly MarkdownSettings settings = new MarkdownSettings(Emoji1_24x24, true);
		internal static readonly Encoding Utf8WithBOM = new UTF8Encoding(true);
	}
}
