using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Waher.Content;
using Waher.Content.Markdown;
using Waher.Content.Emoji;
using Waher.Content.Emoji.Emoji1;
using Waher.Networking.HTTP;
using Waher.Runtime.Inventory;
using Waher.Script;
using Waher.Security;

namespace Waher.Content.Markdown.Web
{
	/// <summary>
	/// Converts Markdown documents to HTML documents.
	/// </summary>
	public class MarkdownToHtmlConverter : IContentConverter
	{
		private static string bareJid = string.Empty;

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
		/// Bare JID used, if the HTTPX URI scheme is supported.
		/// </summary>
		public static string BareJID
		{
			get { return bareJid; }
			set { bareJid = value; }
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
		/// <returns>If the result is dynamic (true), or only depends on the source (false).</returns>
		public bool Convert(string FromContentType, Stream From, string FromFileName, string ResourceName, string URL, string ToContentType,
			Stream To, Variables Session)
		{
			HttpRequest Request = null;
			string Markdown;
			bool b;

			using (StreamReader rd = new StreamReader(From))
			{
				Markdown = rd.ReadToEnd();
			}

			if (Session.TryGetVariable("Request", out Variable v))
			{
				Request = v.ValueObject as HttpRequest;

				if (Request != null)
				{
					int i = Markdown.IndexOf("\r\n\r\n");
					if (i < 0)
						i = Markdown.IndexOf("\n\n");

					if (i > 0)
					{
						string Header = Markdown.Substring(0, i);
						string Parameter;
						
						foreach (string Row in Header.Split(CommonTypes.CRLF, StringSplitOptions.RemoveEmptyEntries))
						{
							if (!Row.StartsWith("Parameter:", StringComparison.OrdinalIgnoreCase))
								continue;

							Parameter = Row.Substring(10).Trim();
							if (Request.Header.TryGetQueryParameter(Parameter, out string Value))
							{
								Value = System.Net.WebUtility.UrlDecode(Value);
								if (double.TryParse(Value.Replace(".", System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator), out double d))
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

			if (!string.IsNullOrEmpty(bareJid))
			{
				Settings.HttpxProxy = "/HttpxProxy/%URL%";
				Settings.LocalHttpxResourcePath = "httpx://" + bareJid + "/";
			}

			MarkdownDocument Doc = new MarkdownDocument(Markdown, Settings, FromFileName, ResourceName, URL, typeof(HttpException));
			IUser User;

			if (Doc.TryGetMetaData("UserVariable", out KeyValuePair<string, bool>[] MetaValues))
			{
				bool Authorized = true;

				if (!Doc.TryGetMetaData("Login", out KeyValuePair<string, bool>[] Login))
					Login = null;

				if (!Doc.TryGetMetaData("Privilege", out KeyValuePair<string, bool>[] Privilege))
					Privilege = null;

				foreach (KeyValuePair<string, bool> P in MetaValues)
				{
					if (!Session.TryGetVariable(P.Key, out v))
					{
						Authorized = false;
						break;
					}

					User = v.ValueObject as IUser;
					if (User == null)
					{
						Authorized = false;
						break;
					}

					if (Privilege != null)
					{
						foreach (KeyValuePair<string, bool> P2 in Privilege)
						{
							if (!User.HasPrivilege(P2.Key))
							{
								Authorized = false;
								break;
							}
						}
					}

					if (!Authorized)
						break;
				}

				if (!Authorized)
				{
					if (Login != null)
					{
						foreach (KeyValuePair<string, bool> P in Login)
						{
							StringBuilder Location = new StringBuilder(P.Key);
							if (P.Key.IndexOf('?') >= 0)
								Location.Append('&');
							else
								Location.Append('?');

							Location.Append("from=");

							if (Request != null)
								Location.Append(System.Net.WebUtility.UrlEncode(Request.Header.GetURL(true, true)));
							else
								Location.Append(System.Net.WebUtility.UrlEncode(URL));

							throw new TemporaryRedirectException(Location.ToString());
						}
					}

					throw new ForbiddenException();
				}
			}

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
				if (v.ValueObject is HttpResponse Response)
				{
					if (Doc.TryGetMetaData("Cache-Control", out KeyValuePair<string, bool>[] Value))
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

			return Doc.IsDynamic;
		}

		internal static readonly Emoji1LocalFiles Emoji1_24x24 = new Emoji1LocalFiles(Emoji1SourceFileType.Svg, 24, 24,
			"/Graphics/Emoji1/svg/%FILENAME%", File.Exists, File.ReadAllBytes);
		internal static readonly Encoding Utf8WithBOM = new UTF8Encoding(true);
	}
}
