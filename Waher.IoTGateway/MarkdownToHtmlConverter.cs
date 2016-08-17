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

namespace Waher.IoTGateway.Console
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
		/// <param name="URL">URL of resource, if accessed from a web server.</param>
		/// <param name="ToContentType">Content type of the content to convert to.</param>
		/// <param name="To">Stream pointing to where binary representation of content is to be sent.</param>
		/// <param name="Session">Session states.</param>
		public void Convert(string FromContentType, Stream From, string FromFileName, string ResourceName, string URL, string ToContentType,
			Stream To, Variables Session)
		{
			string Markdown;
			Variable v;
			bool b;

			using (StreamReader rd = new StreamReader(From))
			{
				Markdown = rd.ReadToEnd();
			}

			if (Session.TryGetVariable("Request", out v))
			{
				HttpRequest Request = v.ValueObject as HttpRequest;

				if (Request != null)
				{
					int i = Markdown.IndexOf("\r\n\r\n");
					if (i < 0)
						i = Markdown.IndexOf("\n\n");

					if (i > 0)
					{
						string Header = Markdown.Substring(0, i);
						string Parameter;
						string Value;
						double d;

						foreach (string Row in Header.Split(CommonTypes.CRLF, StringSplitOptions.RemoveEmptyEntries))
						{
							if (!Row.StartsWith("Parameter:", StringComparison.OrdinalIgnoreCase))
								continue;

							Parameter = Row.Substring(10).Trim();
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
			}

			MarkdownSettings Settings = new MarkdownSettings(Emoji1_24x24, true, Session);
			Settings.HttpxProxy = "/HttpxProxy/%URL%";
			Settings.LocalHttpxResourcePath = "httpx://" + Gateway.XmppClient.BareJID + "/";
			MarkdownDocument Doc = new MarkdownDocument(Markdown, Settings, FromFileName, ResourceName, URL);
			KeyValuePair<string, bool>[] MetaValues;

			if (Doc.TryGetMetaData("AudioControls", out MetaValues))
			{
				foreach (KeyValuePair<string, bool> P in MetaValues)
				{
					if (CommonTypes.TryParse(P.Key, out b))
						Settings.AudioControls = b;
				}
			}

			if (Doc.TryGetMetaData("AudioAutoplay", out MetaValues))
			{
				foreach (KeyValuePair<string, bool> P in MetaValues)
				{
					if (CommonTypes.TryParse(P.Key, out b))
						Settings.AudioAutoplay = b;
				}
			}

			if (Doc.TryGetMetaData("VideoControls", out MetaValues))
			{
				foreach (KeyValuePair<string, bool> P in MetaValues)
				{
					if (CommonTypes.TryParse(P.Key, out b))
						Settings.VideoControls = b;
				}
			}

			if (Doc.TryGetMetaData("VideoAutoplay", out MetaValues))
			{
				foreach (KeyValuePair<string, bool> P in MetaValues)
				{
					if (CommonTypes.TryParse(P.Key, out b))
						Settings.VideoAutoplay = b;
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
