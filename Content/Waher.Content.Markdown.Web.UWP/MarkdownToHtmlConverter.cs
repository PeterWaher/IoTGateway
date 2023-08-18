using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Waher.Content.Emoji;
using Waher.Networking.HTTP;
using Waher.Networking.HTTP.ScriptExtensions;
using Waher.Runtime.Inventory;
using Waher.Script;
using Waher.Security;
using System.Threading.Tasks;

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
		public string[] FromContentTypes => new string[] { MarkdownCodec.ContentType };

		/// <summary>
		/// Converts content to these content types. 
		/// </summary>
		public virtual string[] ToContentTypes
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
		public virtual Grade ConversionGrade => Grade.Excellent;

		/// <summary>
		/// Bare JID used, if the HTTPX URI scheme is supported.
		/// </summary>
		public static string BareJID
		{
			get => bareJid;
			set => bareJid = value;
		}

		/// <summary>
		/// Root folder used for web content.
		/// </summary>
		public static string RootFolder
		{
			get => rootFolder;
			set => rootFolder = value;
		}

		/// <summary>
		/// Emoji source to use when converting Markdown documents to HTML.
		/// </summary>
		public static IEmojiSource EmojiSource
		{
			get => emojiSource;
			set => emojiSource = value;
		}

		/// <summary>
		/// Performs the actual conversion.
		/// </summary>
		/// <param name="State">State of the current conversion.</param>
		/// <returns>If the result is dynamic (true), or only depends on the source (false).</returns>
		public async Task<bool> ConvertAsync(ConversionState State)
		{
			if (State is null)
				return true;

			HttpRequest Request = null;
			string Markdown;
			bool b;

			using (StreamReader rd = new StreamReader(State.From))
			{
				Markdown = rd.ReadToEnd();
			}

			if (!(State.Session is null) && State.Session.TryGetVariable("Request", out Variable v))
			{
				Request = v.ValueObject as HttpRequest;

				if (!(Request is null))
				{
					Page.GetPageVariables(State.Session, Request.Header.ResourcePart);

					int i = Markdown.IndexOf("\r\n\r\n");
					if (i < 0)
						i = Markdown.IndexOf("\n\n");

					if (i > 0)
					{
						HttpRequestHeader RequestHeader = Request.Header;
						Variables Variables = State.Session;
						string Header = Markdown.Substring(0, i);
						string Parameter;

						foreach (string Row in Header.Split(CommonTypes.CRLF, StringSplitOptions.RemoveEmptyEntries))
						{
							if (!Row.StartsWith("Parameter:", StringComparison.OrdinalIgnoreCase))
								continue;

							Parameter = Row.Substring(10).Trim();
							if (RequestHeader.TryGetQueryParameter(Parameter, out string Value))
							{
								Value = System.Net.WebUtility.UrlDecode(Value);
								if (CommonTypes.TryParse(Value, out double d))
									Variables[Parameter] = d;
								else if (bool.TryParse(Value, out b))
									Variables[Parameter] = b;
								else
									Variables[Parameter] = Value;
							}
							else
								Variables[Parameter] = string.Empty;
						}
					}
				}
			}

			MarkdownSettings Settings = new MarkdownSettings(emojiSource, true, State.Session)
			{
				RootFolder = rootFolder,
				HtmlSettings = htmlSettings,
				ResourceMap = Request.Server
			};

			if (!string.IsNullOrEmpty(bareJid))
			{
				Settings.HttpxProxy = "/HttpxProxy/%URL%";
				Settings.LocalHttpxResourcePath = "httpx://" + bareJid + "/";
			}

			MarkdownDocument Doc = await MarkdownDocument.CreateAsync(Markdown, Settings, State.FromFileName, State.LocalResourceName, State.URL, typeof(HttpException));

			if (Doc.TryGetMetaData("UserVariable", out KeyValuePair<string, bool>[] MetaValues))
			{
				object User = null;
				IUser User2 = null;
				bool Authorized = true;
				string MissingPrivilege = null;

				if (!Doc.TryGetMetaData("Login", out KeyValuePair<string, bool>[] Login))
					Login = null;

				if (!Doc.TryGetMetaData("Privilege", out KeyValuePair<string, bool>[] Privilege))
					Privilege = null;

				foreach (KeyValuePair<string, bool> P in MetaValues)
				{
					if (State.Session is null)
					{
						Authorized = false;
						break;
					}

					if (State.Session.TryGetVariable(P.Key, out v) && !(v.ValueObject is null))
						User = v.ValueObject;
					else
					{
						Uri LoginUrl = null;
						string LoginFileName = null;
						string FromFolder = Path.GetDirectoryName(State.FromFileName);

						if (!(Login is null))
						{
							foreach (KeyValuePair<string, bool> P2 in Login)
							{
								LoginFileName = Path.Combine(FromFolder, P2.Key.Replace('/', Path.DirectorySeparatorChar));
								LoginUrl = new Uri(new Uri(State.URL), P2.Key.Replace(Path.DirectorySeparatorChar, '/'));

								if (File.Exists(LoginFileName))
									break;
								else
									LoginFileName = null;
							}
						}

						if (!(LoginFileName is null))
						{
							string LoginMarkdown = await Resources.ReadAllTextAsync(LoginFileName);
							MarkdownDocument LoginDoc = await MarkdownDocument.CreateAsync(LoginMarkdown, Settings, LoginFileName, LoginUrl.AbsolutePath,
								LoginUrl.ToString(), typeof(HttpException));

							if (!State.Session.TryGetVariable(P.Key, out v))
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
						User2 = v.ValueObject as IUser;
						if (User2 is null)
						{
							Authorized = false;
							break;
						}

						foreach (KeyValuePair<string, bool> P2 in Privilege)
						{
							if (!User2.HasPrivilege(P2.Key))
							{
								MissingPrivilege = P2.Key;
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
								Location.Append(System.Net.WebUtility.UrlEncode(State.URL));

							throw new TemporaryRedirectException(Location.ToString());
						}
					}

					throw ForbiddenException.AccessDenied(State.FromFileName, User2?.UserName ?? User?.ToString() ?? string.Empty,
						MissingPrivilege);
				}

				if (User is null)
					throw ForbiddenException.AccessDenied(State.FromFileName, string.Empty, string.Empty);

				State.Session[" User "] = User;
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

			string s = await this.DoConversion(Doc);    // Result needs to be generated, so that IsDynamic property is properly evaluated. (Can depend on master file, which is loaded during generation.)

			if (!(State.Session is null) && State.Session.TryGetVariable("Response", out v))
			{
				if (v.ValueObject is HttpResponse Response)
				{
					if (Response.ResponseSent)
						return Doc.IsDynamic;

					if (!this.CopyHttpHeader("Cache-Control", Doc, Response))
					{
						if (Doc.IsDynamic)
							Response.SetHeader("Cache-Control", "max-age=0, no-cache, no-store");
						else
							Response.SetHeader("Cache-Control", "no-transform,public,max-age=60,s-maxage=60,stale-while-revalidate=604800");
					}

					this.CopyHttpHeader("Access-Control-Allow-Origin", Doc, Response);
					this.CopyHttpHeader("Content-Security-Policy", Doc, Response);
					this.CopyHttpHeader("Public-Key-Pins", Doc, Response);
					this.CopyHttpHeader("Strict-Transport-Security", Doc, Response);
					this.CopyHttpHeader("Sunset", Doc, Response);
					this.CopyHttpHeader("Vary", Doc, Response);
				}
			}

			byte[] Data = Utf8WithBOM.GetBytes(s);

			await State.To.WriteAsync(Data, 0, Data.Length);
			State.ToContentType += "; charset=utf-8";

			return Doc.IsDynamic;
		}

		/// <summary>
		/// Performs the actual conversion
		/// </summary>
		/// <param name="Doc">Markdown document prepared for conversion.</param>
		/// <returns>Conversion result.</returns>
		protected virtual Task<string> DoConversion(MarkdownDocument Doc)
		{
			return Doc.GenerateHTML();
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
