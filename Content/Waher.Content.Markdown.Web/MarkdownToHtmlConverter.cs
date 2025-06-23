using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Waher.Content.Emoji;
using Waher.Content.Html;
using Waher.Content.Html.Elements;
using Waher.Content.Markdown.Rendering;
using Waher.Networking.HTTP;
using Waher.Networking.HTTP.ScriptExtensions;
using Waher.Runtime.Inventory;
using Waher.Runtime.IO;
using Waher.Script;
using Waher.Script.Graphs.Functions.Plots;
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
					HtmlCodec.DefaultContentType,
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

			if (State.Session is SessionVariables SessionVariables &&
				!(SessionVariables.CurrentRequest is null))
			{
				Request = SessionVariables.CurrentRequest;

				Page.GetPageVariables(SessionVariables, Request.Header.ResourcePart);

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

			MarkdownSettings Settings = new MarkdownSettings(emojiSource, true, State.Session)
			{
				RootFolder = rootFolder,
				ResourceMap = Request.Server,
				Progress = State.Progress
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
				bool Authorized = !(State.Session is null);
				string MissingPrivilege = null;
				bool LoggedIn = false;

				if (!Doc.TryGetMetaData("Login", out KeyValuePair<string, bool>[] Login))
					Login = null;

				if (!Doc.TryGetMetaData("Privilege", out KeyValuePair<string, bool>[] Privilege))
					Privilege = null;

				if (Authorized)
				{
					foreach (KeyValuePair<string, bool> P in MetaValues)
					{
						if (!State.Session.TryGetVariable(P.Key, out Variable v) ||
							v.ValueObject is null)
						{
							continue;
						}

						User = v.ValueObject;
						LoggedIn = true;

						if (!(Privilege is null))
						{
							User2 = User as IUser;
							if (User2 is null)
							{
								User = null;
								Authorized = false;
								break;
							}

							foreach (KeyValuePair<string, bool> P2 in Privilege)
							{
								if (!User2.HasPrivilege(P2.Key))
								{
									User = null;
									MissingPrivilege = P2.Key;
									Authorized = false;
									break;
								}
							}
						}

						if (!Authorized)
							break;
					}

					if (Authorized && User is null)
					{
						if (Login is null)
							Authorized = false;
						else
						{
							Uri LoginUrl = null;
							string LoginFileName = null;

							foreach (KeyValuePair<string, bool> P2 in Login)
							{
								if (Request.Server.TryGetFileName(P2.Key, false, out LoginFileName) &&
									File.Exists(LoginFileName))
								{
									LoginUrl = new Uri(new Uri(State.URL), P2.Key.Replace(Path.DirectorySeparatorChar, '/'));
									break;
								}
								else
									LoginFileName = null;
							}

							if (!(LoginFileName is null))
							{
								string LoginMarkdown = await Files.ReadAllTextAsync(LoginFileName);
								await MarkdownDocument.CreateAsync(LoginMarkdown, Settings, LoginFileName, LoginUrl.AbsolutePath,
									LoginUrl.ToString(), typeof(HttpException));
								
								if (!State.Session.ContainsVariable("UserVariable"))
									Authorized = false;
							}
							else
								Authorized = false;
						}
					}
				}

				if (!Authorized)
				{
					if (!(Login is null) && !LoggedIn)
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

							State.Error = new TemporaryRedirectException(Location.ToString());
							return false;
						}
					}

					State.Error = ForbiddenException.AccessDenied(State.FromFileName, User2?.UserName ?? User?.ToString() ?? string.Empty,
						MissingPrivilege);
					return false;
				}

				if (User is null)
				{
					State.Error = ForbiddenException.AccessDenied(State.FromFileName, string.Empty, string.Empty);
					return false;
				}

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
			HttpResponse Response = Request?.Response;

			if (!(Response is null))
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

			byte[] Data = Strings.Utf8WithBom.GetBytes(s);

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
			return Doc.GenerateHTML(htmlSettings);
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
