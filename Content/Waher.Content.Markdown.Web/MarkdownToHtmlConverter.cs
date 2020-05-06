using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Waher.Content;
using Waher.Content.Markdown;
using Waher.Content.Emoji;
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
		private static IEmojiSource emojiSource = null;
		private static string bareJid = string.Empty;
		private static string rootFolder = string.Empty;
		private static HtmlSettings htmlSettings = new HtmlSettings();

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
					MarkdownCodec.ContentType
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
		/// Root folder used for web content.
		/// </summary>
		public static string RootFolder
		{
			get { return rootFolder; }
			set { rootFolder = value; }
		}

		/// <summary>
		/// Emoji source to use when converting Markdown documents to HTML.
		/// </summary>
		public static IEmojiSource EmojiSource
		{
			get { return emojiSource; }
			set { emojiSource = value; }
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

			if (!(Session is null) && Session.TryGetVariable("Request", out Variable v))
			{
				Request = v.ValueObject as HttpRequest;

				if (!(Request is null))
				{
					string Url = Request.Header.ResourcePart;

					if (!Session.TryGetVariable(" PageVariables ", out v) ||
						!(v.ValueObject is Variables PageVariables))
					{
						Session[" LastPage "] = Url;
						Session[" PageVariables "] = new Variables();
					}
					else if (!Session.TryGetVariable(" LastPage ", out v) ||
						!(v.ValueObject is string LastPageUrl) ||
						LastPageUrl != Url)
					{
						Session[" LastPage "] = Url;
						PageVariables.Clear();
					}

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

			MarkdownSettings Settings = new MarkdownSettings(emojiSource, true, Session)
			{
				RootFolder = rootFolder,
				HtmlSettings = htmlSettings
			};

			if (!string.IsNullOrEmpty(bareJid))
			{
				Settings.HttpxProxy = "/HttpxProxy/%URL%";
				Settings.LocalHttpxResourcePath = "httpx://" + bareJid + "/";
			}

			MarkdownDocument Doc = new MarkdownDocument(Markdown, Settings, FromFileName, ResourceName, URL, typeof(HttpException));

			if (Doc.TryGetMetaData("UserVariable", out KeyValuePair<string, bool>[] MetaValues))
			{
				bool Authorized = true;

				if (!Doc.TryGetMetaData("Login", out KeyValuePair<string, bool>[] Login))
					Login = null;

				if (!Doc.TryGetMetaData("Privilege", out KeyValuePair<string, bool>[] Privilege))
					Privilege = null;

				foreach (KeyValuePair<string, bool> P in MetaValues)
				{
					if (Session is null)
					{
						Authorized = false;
						break;
					}

					if (!Session.TryGetVariable(P.Key, out v))
					{
						Uri LoginUrl = null;
						string LoginFileName = null;
						string FromFolder = Path.GetDirectoryName(FromFileName);

						if (!(Login is null))
						{
							foreach (KeyValuePair<string, bool> P2 in Login)
							{
								LoginFileName = Path.Combine(FromFolder, P2.Key.Replace('/', Path.DirectorySeparatorChar));
								LoginUrl = new Uri(new Uri(URL), P2.Key.Replace(Path.DirectorySeparatorChar, '/'));

								if (File.Exists(LoginFileName))
									break;
								else
									LoginFileName = null;
							}
						}

						if (!(LoginFileName is null))
						{
							string LoginMarkdown = File.ReadAllText(LoginFileName);
							MarkdownDocument LoginDoc = new MarkdownDocument(LoginMarkdown, Settings, LoginFileName, LoginUrl.AbsolutePath,
								LoginUrl.ToString(), typeof(HttpException));

							if (!Session.TryGetVariable(P.Key, out v))
							{
								Authorized = false;
								break;
							}
						}
						else
						{
							Authorized = false;
							break;
						}
					}

					if (!(Privilege is null))
					{
						if (!(v.ValueObject is IUser User))
						{
							Authorized = false;
							break;
						}

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
					if (!(Login is null))
					{
						foreach (KeyValuePair<string, bool> P in Login)
						{
							StringBuilder Location = new StringBuilder(P.Key);
							if (P.Key.IndexOf('?') >= 0)
								Location.Append('&');
							else
								Location.Append('?');

							Location.Append("from=");

							if (!(Request is null))
								Location.Append(System.Net.WebUtility.UrlEncode(Request.Header.GetURL(true, true)));
							else
								Location.Append(System.Net.WebUtility.UrlEncode(URL));

							throw new TemporaryRedirectException(Location.ToString());
						}
					}

					throw new ForbiddenException("Access denied.");
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

			if (!(Session is null) && Session.TryGetVariable("Response", out v))
			{
				if (v.ValueObject is HttpResponse Response)
				{
					if (Response.ResponseSent)
						return Doc.IsDynamic;

					Response.SetHeader("X-Content-Type-Options", "nosniff");

					if (!this.CopyHttpHeader("Cache-Control", Doc, Response))
					{
						if (Doc.IsDynamic)
							Response.SetHeader("Cache-Control", "max-age=0, no-cache, no-store");
						else
							Response.SetHeader("Cache-Control", "no-transform,public,max-age=86400,s-maxage=86400");
					}

					this.CopyHttpHeader("Access-Control-Allow-Origin", Doc, Response);
					this.CopyHttpHeader("Content-Security-Policy", Doc, Response);
					this.CopyHttpHeader("Public-Key-Pins", Doc, Response);
					this.CopyHttpHeader("Strict-Transport-Security", Doc, Response);
					this.CopyHttpHeader("Sunset", Doc, Response);
					this.CopyHttpHeader("Vary", Doc, Response);
				}
			}

			string HTML = Doc.GenerateHTML();
			byte[] Data = Utf8WithBOM.GetBytes(HTML);
			To.Write(Data, 0, Data.Length);

			return Doc.IsDynamic;
		}

		private bool CopyHttpHeader(string Name, MarkdownDocument Doc, HttpResponse Response)
		{
			if (Doc.TryGetMetaData(Name, out KeyValuePair<string, bool>[] Value))
			{
				foreach (KeyValuePair<string, bool> P in Value)
				{
					Response.SetHeader(Name, P.Key);
					break;
				}

				return true;
			}
			else
				return false;
		}

		internal static readonly Encoding Utf8WithBOM = new UTF8Encoding(true);

		/// <summary>
		/// HTML settings for automatically converted content.
		/// </summary>
		public static HtmlSettings HtmlSettings
		{
			get => htmlSettings;
			set => htmlSettings = value;
		}
	}
}
