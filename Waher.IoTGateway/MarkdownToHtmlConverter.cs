using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Waher.Content;
using Waher.Content.Markdown;
using Waher.Content.Emoji;
using Waher.Content.Emoji.Emoji1;
using Waher.Networking.HTTP;
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
		/// <param name="ResourceName">Local resource name of file, if accessed from a web server.</param>
		/// <param name="ToContentType">Content type of the content to convert to.</param>
		/// <param name="To">Stream pointing to where binary representation of content is to be sent.</param>
		/// <param name="Session">Session states.</param>
		public void Convert(string FromContentType, Stream From, string FromFileName, string ResourceName, string ToContentType,
			Stream To, Variables Session)
		{
			StreamReader rd = new StreamReader(From);
			string Markdown = rd.ReadToEnd();
			rd.Dispose();

			MarkdownDocument Doc = new MarkdownDocument(Markdown, new MarkdownSettings(Emoji1_24x24, true, Session), FromFileName, ResourceName);
			Variable v;

			if (Session.TryGetVariable("Request", out v))
			{
				HttpRequest Request = v.ValueObject as HttpRequest;

				if (Request != null)
				{
					string Value;
					double d;
					bool b;

					foreach (string Parameter in Doc.Parameters)
					{
						if (Request.Header.TryGetQueryParameter(Parameter, out Value))
						{
							Value = System.Web.HttpUtility.UrlDecode(Value);
							if (double.TryParse(Value.Replace(".", System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator), out d))
								Session[Parameter] = d;
							else if (bool.TryParse(Value, out b))
								Session[Parameter] = b;
							else
								Session[Parameter] = Value;
						}
						else
							Session[Parameter] = string.Empty;
					}
				}
			}

			if (Session.TryGetVariable("Response", out v))
			{
				HttpResponse Response = v.ValueObject as HttpResponse;
				if (Response != null)
				{
					KeyValuePair<string, bool>[] Value;

					if (Doc.TryGetMetaData("Cache-Control", out Value))
					{
						foreach (KeyValuePair<string, bool> P in Value)
						{
							Response.SetHeader("Cache-Control", P.Key);
							break;
						}
					}
					else if (Doc.IsDynamic)
						Response.SetHeader("Cache-Control", "max-age=0, no-cache, no-store");
					else
						Response.SetHeader("Cache-Control", "no-transform,public,max-age=86400,s-maxage=86400");

					if (Doc.TryGetMetaData("Vary", out Value))
					{
						foreach (KeyValuePair<string, bool> P in Value)
						{
							Response.SetHeader("Vary", P.Key);
							break;
						}
					}
				}
			}

			string HTML = Doc.GenerateHTML();
			byte[] Data = Utf8WithBOM.GetBytes(HTML);
			To.Write(Data, 0, Data.Length);
		}

		internal static readonly Emoji1LocalFiles Emoji1_24x24 = new Emoji1LocalFiles(Emoji1SourceFileType.Svg, 24, 24, 
			"/Graphics/Emoji1/svg/%FILENAME%", File.Exists, File.ReadAllBytes);
		internal static readonly Encoding Utf8WithBOM = new UTF8Encoding(true);
	}
}
