using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using Waher.Content;
using Waher.Events;
using Waher.Networking.HTTP.HeaderFields;
using Waher.Runtime.Temporary;
using Waher.Script;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// Options on how to handle domain names provided in the Host header.
	/// </summary>
	public enum HostDomainOptions
	{
		/// <summary>
		/// Only provide access to files for specified domains.
		/// </summary>
		OnlySpecifiedDomains,

		/// <summary>
		/// All specified domains receive the same files.
		/// </summary>
		SameForAllDomains,

		/// <summary>
		/// If a subfolder exist for a specified host domain name, that subfolder will be used for the request.
		/// </summary>
		UseDomainSubfolders
	}

	/// <summary>
	/// Publishes a folder with all its files and subfolders through HTTP GET, with optional support for PUT and DELETE.
	/// If PUT and DELETE are allowed, users (if authenticated) can update the contents of the folder.
	/// </summary>
	public class HttpFolderResource : HttpAsynchronousResource, IHttpGetMethod, IHttpGetRangesMethod,
		IHttpPutMethod, IHttpPutRangesMethod, IHttpDeleteMethod, IHttpPostMethod
	{
		private const int BufferSize = 32768;

		private readonly static Dictionary<string, bool> protectedContentTypes = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
		private readonly Dictionary<string, CacheRec> cacheInfo = new Dictionary<string, CacheRec>();
		private readonly Dictionary<string, bool> definedDomains = new Dictionary<string, bool>(StringComparer.CurrentCultureIgnoreCase);
		private readonly Dictionary<string, string> folders = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
		private Dictionary<string, bool> allowTypeConversionFrom = null;
		private readonly HttpAuthenticationScheme[] authenticationSchemes;
		private readonly HostDomainOptions domainOptions;
		private readonly bool allowPut;
		private readonly bool allowDelete;
		private readonly bool anonymousGET;
		private readonly bool userSessions;
		private string folderPath;

		/// <summary>
		/// Publishes a folder with all its files and subfolders through HTTP GET, with optional support for PUT and DELETE.
		/// If PUT and DELETE are allowed, users (if authenticated) can update the contents of the folder.
		/// </summary>
		/// <param name="ResourceName">Name of resource.</param>
		/// <param name="FolderPath">Full path to folder to publish.</param>
		/// <param name="AllowPut">If the PUT method should be allowed.</param>
		/// <param name="AllowDelete">If the DELETE method should be allowed.</param>
		/// <param name="AnonymousGET">If Anonymous GET access is allowed.</param>
		/// <param name="UserSessions">If the resource uses user sessions.</param>
		/// <param name="AuthenticationSchemes">Any authentication schemes used to authenticate users before access is granted.</param>
		public HttpFolderResource(string ResourceName, string FolderPath, bool AllowPut, bool AllowDelete, bool AnonymousGET,
			bool UserSessions, params HttpAuthenticationScheme[] AuthenticationSchemes)
			: this(ResourceName, FolderPath, AllowPut, AllowDelete, AnonymousGET, UserSessions,
				  HostDomainOptions.SameForAllDomains, AuthenticationSchemes)
		{
		}

		/// <summary>
		/// Publishes a folder with all its files and subfolders through HTTP GET, with optional support for PUT and DELETE.
		/// If PUT and DELETE are allowed, users (if authenticated) can update the contents of the folder.
		/// </summary>
		/// <param name="ResourceName">Name of resource.</param>
		/// <param name="FolderPath">Full path to folder to publish.</param>
		/// <param name="AllowPut">If the PUT method should be allowed.</param>
		/// <param name="AllowDelete">If the DELETE method should be allowed.</param>
		/// <param name="AnonymousGET">If Anonymous GET access is allowed.</param>
		/// <param name="UserSessions">If the resource uses user sessions.</param>
		/// <param name="DomainOptions">Options on how to handle the Host header.</param>
		/// <param name="AuthenticationSchemes">Any authentication schemes used to authenticate users before access is granted.</param>
		public HttpFolderResource(string ResourceName, string FolderPath, bool AllowPut, bool AllowDelete, bool AnonymousGET,
			bool UserSessions, HostDomainOptions DomainOptions, params HttpAuthenticationScheme[] AuthenticationSchemes)
			: this(ResourceName, FolderPath, AllowPut, AllowDelete, AnonymousGET, UserSessions,
				  DomainOptions, new string[0], AuthenticationSchemes)
		{
		}

		/// <summary>
		/// Publishes a folder with all its files and subfolders through HTTP GET, with optional support for PUT and DELETE.
		/// If PUT and DELETE are allowed, users (if authenticated) can update the contents of the folder.
		/// </summary>
		/// <param name="ResourceName">Name of resource.</param>
		/// <param name="FolderPath">Full path to folder to publish.</param>
		/// <param name="AllowPut">If the PUT method should be allowed.</param>
		/// <param name="AllowDelete">If the DELETE method should be allowed.</param>
		/// <param name="AnonymousGET">If Anonymous GET access is allowed.</param>
		/// <param name="UserSessions">If the resource uses user sessions.</param>
		/// <param name="DomainOptions">Options on how to handle the Host header.</param>
		/// <param name="DomainNames">Pre-defined host names.</param>
		/// <param name="AuthenticationSchemes">Any authentication schemes used to authenticate users before access is granted.</param>
		public HttpFolderResource(string ResourceName, string FolderPath, bool AllowPut, bool AllowDelete, bool AnonymousGET,
			bool UserSessions, HostDomainOptions DomainOptions, string[] DomainNames, params HttpAuthenticationScheme[] AuthenticationSchemes)
			: base(ResourceName)
		{
			this.authenticationSchemes = AuthenticationSchemes;
			this.allowPut = AllowPut;
			this.allowDelete = AllowDelete;
			this.anonymousGET = AnonymousGET;
			this.userSessions = UserSessions;
			this.domainOptions = DomainOptions;

			foreach (string DomainName in DomainNames)
				this.definedDomains[DomainName] = true;

			this.FolderPath = FolderPath;

		}

		/// <summary>
		/// Protects a content type, so that it cannot be retrieved in raw format by external parties through any folder resources.
		/// </summary>
		/// <param name="ContentType">Content type to protect.</param>
		public static void ProtectContentType(string ContentType)
		{
			lock (protectedContentTypes)
			{
				protectedContentTypes[ContentType] = true;
			}
		}

		/// <summary>
		/// Folder path.
		/// </summary>
		public string FolderPath
		{
			get { return this.folderPath; }
			set
			{
				string s = value;

				int c = s.Length;
				if (c > 0 && (s[c - 1] == Path.DirectorySeparatorChar || s[c - 1] == '/' || s[c - 1] == '\\'))
					s = s.Substring(0, c - 1);

				this.folderPath = s;
			}
		}

		/// <summary>
		/// If the resource handles sub-paths.
		/// </summary>
		public override bool HandlesSubPaths => true;

		/// <summary>
		/// If the resource uses user sessions.
		/// </summary>
		public override bool UserSessions => this.userSessions;

		/// <summary>
		/// If the GET method is allowed.
		/// </summary>
		public bool AllowsGET => true;

		/// <summary>
		/// If the PUT method is allowed.
		/// </summary>
		public bool AllowsPUT => this.allowPut;

		/// <summary>
		/// If the DELETE method is allowed.
		/// </summary>
		public bool AllowsDELETE => this.allowDelete;

		/// <summary>
		/// If the POST method is allowed.
		/// </summary>
		public bool AllowsPOST => this.userSessions;

		/// <summary>
		/// Any authentication schemes used to authenticate users before access is granted to the corresponding resource.
		/// </summary>
		/// <param name="Request">Current request</param>
		public override HttpAuthenticationScheme[] GetAuthenticationSchemes(HttpRequest Request)
		{
			string s;

			if (this.anonymousGET && ((s = Request.Header.Method) == "GET" || s == "HEAD"))
				return null;
			else
				return this.authenticationSchemes;
		}

		/// <summary>
		/// Validates the request itself. This method is called prior to processing the request, to see if it is valid in the context of the resource 
		/// or not. If not, corresponding HTTP Exceptions should be thrown. Implementing validation checks in this method, instead of the corresponding
		/// execution method, allows the resource to respond correctly to requests using the "Expect: 100-continue" header.
		/// </summary>
		/// <param name="Request">Request to validate.</param>
		public override void Validate(HttpRequest Request)
		{
			base.Validate(Request);

			HttpRequestHeader Header = Request.Header;
			DateTimeOffset? Limit;

			if (Header.IfMatch is null && !(Header.IfUnmodifiedSince is null) && (Limit = Header.IfUnmodifiedSince.Timestamp).HasValue)
			{
				string FullPath = this.GetFullPath(Request, out bool Exists);
				if (Exists)
				{
					DateTime LastModified = File.GetLastWriteTime(FullPath);
					LastModified = LastModified.ToUniversalTime();

					if (GreaterOrEqual(LastModified, Limit.Value.ToUniversalTime()))
						throw new NotModifiedException();
				}
			}

			switch (Request.Header.Method)
			{
				case "PUT":
					if (!this.allowPut)
						throw new MethodNotAllowedException(this.AllowedMethods);

					break;

				case "DELETE":
					if (!this.allowDelete)
						throw new MethodNotAllowedException(this.AllowedMethods);
					break;
			}
		}

		private string GetFullPath(HttpRequest Request, out bool Exists)
		{
			string s = WebUtility.UrlDecode(Request.SubPath).Replace('/', Path.DirectorySeparatorChar);
			string s2;

			if (s.Contains("..") || s.Contains(doubleBackslash) || s.Contains(":"))
				throw new ForbiddenException("Path control characters not permitted.");

			if (this.domainOptions != HostDomainOptions.SameForAllDomains)
			{
				string Host = Request.Header.Host?.Value ?? string.Empty;
				string Folder;
				int i = Host.IndexOf(':');

				if (i > 0)
					Host = Host.Substring(0, i);

				if (this.domainOptions == HostDomainOptions.OnlySpecifiedDomains)
				{
					lock (this.definedDomains)
					{
						if (!this.definedDomains.ContainsKey(Host))
							throw new ForbiddenException("Access to this folder is not permitted on this domain.");
					}

					Folder = this.folderPath;
				}
				else
				{
					lock (this.folders)
					{
						if (!this.folders.TryGetValue(Host, out Folder))
						{
							Folder = this.folderPath + Path.DirectorySeparatorChar + Host;
							if (!Directory.Exists(Folder))
							{
								if (Host.StartsWith("www.", StringComparison.CurrentCultureIgnoreCase))
								{
									Folder = this.folderPath + Path.DirectorySeparatorChar + Host.Substring(4);
									if (!Directory.Exists(Folder))
										Folder = this.folderPath;
								}
								else
									Folder = this.folderPath;
							}

							this.folders[Host] = Folder;
						}
					}
				}

				if (Exists = File.Exists(s2 = Folder + s))
					return s2;
			}

			Exists = File.Exists(s2 = this.folderPath + s);
			return s2;
		}

		private readonly static string doubleBackslash = new string(Path.DirectorySeparatorChar, 2);

		private class CacheRec
		{
			public DateTime LastModified;
			public string ETag;
			public bool IsDynamic;
		}

		/// <summary>
		/// Executes the GET method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public async Task GET(HttpRequest Request, HttpResponse Response)
		{
			string FullPath = this.GetFullPath(Request, out bool Exists);
			if (Exists)
			{
				DateTime LastModified = File.GetLastWriteTime(FullPath).ToUniversalTime();
				CacheRec Rec;

				Rec = this.CheckCacheHeaders(FullPath, LastModified, Request);

				string ContentType = InternetContent.GetContentType(Path.GetExtension(FullPath));
				Stream f = CheckAcceptable(Request, Response, ref ContentType, out bool Dynamic, FullPath, Request.Header.Resource);
				Rec.IsDynamic = Dynamic;

				if (Response.ResponseSent)
					return;

				await SendResponse(f, FullPath, ContentType, Rec.IsDynamic, Rec.ETag, LastModified, Response, Request);
			}
			else
				await this.RaiseFileNotFound(FullPath, Request, Response);
		}

		private async Task RaiseFileNotFound(string FullPath, HttpRequest Request, HttpResponse Response)
		{
			NotFoundException ex = new NotFoundException("File not found: " + Request.SubPath);
			FileNotFoundEventHandler h = this.FileNotFound;

			if (!(h is null))
			{
				FileNotFoundEventArgs e = new FileNotFoundEventArgs(ex, FullPath, Request, Response);

				try
				{
					h(this, e);
				}
				catch (Exception ex2)
				{
					Log.Critical(ex2);
				}

				ex = e.Exception;
				if (ex is null)
					return;     // Sent asynchronously from event handler.
			}

			Log.Warning("File not found.", FullPath, Request.RemoteEndPoint, "FileNotFound");

			await Response.SendResponse(ex);
			Response.Dispose();
		}

		/// <summary>
		/// Event raised when a file was requested that could not be found. 
		/// </summary>
		public event FileNotFoundEventHandler FileNotFound = null;

		/// <summary>
		/// Sends a file-based response back to the client.
		/// </summary>
		/// <param name="FullPath">Full path of file.</param>
		/// <param name="ContentType">Content Type.</param>
		/// <param name="ETag">ETag of resource.</param>
		/// <param name="LastModified">When resource was last modified.</param>
		/// <param name="Response">HTTP response object.</param>
		public static Task SendResponse(string FullPath, string ContentType, string ETag, DateTime LastModified,
			HttpResponse Response)
		{
			return SendResponse(null, FullPath, ContentType, false, ETag, LastModified, Response, null);
		}

		/// <summary>
		/// Sends a file-based response back to the client.
		/// </summary>
		/// <param name="FullPath">Full path of file.</param>
		/// <param name="ContentType">Content Type.</param>
		/// <param name="ETag">ETag of resource.</param>
		/// <param name="LastModified">When resource was last modified.</param>
		/// <param name="Response">HTTP response object.</param>
		/// <param name="Request">HTTP request object.</param>
		public static Task SendResponse(string FullPath, string ContentType, string ETag, DateTime LastModified,
			HttpResponse Response, HttpRequest Request)
		{
			return SendResponse(null, FullPath, ContentType, false, ETag, LastModified, Response, Request);
		}

		private static async Task SendResponse(Stream f, string FullPath, string ContentType, bool IsDynamic, string ETag,
			DateTime LastModified, HttpResponse Response, HttpRequest Request)
		{
			ReadProgress Progress = new ReadProgress()
			{
				Response = Response,
				Request = Request,
				f = f ?? File.Open(FullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite),
				Next = null,
				Boundary = null,
				ContentType = null
			};
			Progress.BytesLeft = Progress.TotalLength = Progress.f.Length;
			Progress.BlockSize = (int)Math.Min(BufferSize, Progress.BytesLeft);
			Progress.Buffer = new byte[Progress.BlockSize];

			Response.ContentType = ContentType;
			Response.ContentLength = Progress.TotalLength;

			if (!IsDynamic)
			{
				Response.SetHeader("ETag", ETag);
				Response.SetHeader("Last-Modified", CommonTypes.EncodeRfc822(LastModified));
			}

			if (Response.OnlyHeader || Progress.TotalLength == 0)
			{
				await Response.SendResponse();
				await Progress.Dispose();
			}
			else
			{
				Task _ = Progress.BeginRead();
			}
		}

		private CacheRec CheckCacheHeaders(string FullPath, DateTime LastModified, HttpRequest Request)
		{
			string CacheKey = FullPath.ToLower();
			HttpRequestHeader Header = Request.Header;
			CacheRec Rec;
			DateTimeOffset? Limit;

			lock (this.cacheInfo)
			{
				if (this.cacheInfo.TryGetValue(CacheKey, out Rec))
				{
					if (Rec.LastModified != LastModified)
					{
						this.cacheInfo.Remove(CacheKey);
						Rec = null;
					}
				}
			}

			if (Rec is null)
			{
				Rec = new CacheRec()
				{
					LastModified = LastModified,
					IsDynamic = false
				};

				using (FileStream fs = File.Open(FullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				{
					Rec.ETag = this.ComputeETag(fs);
				}

				lock (this.cacheInfo)
				{
					this.cacheInfo[CacheKey] = Rec;
				}
			}

			if (!Rec.IsDynamic)
			{
				if (!(Header.IfNoneMatch is null))
				{
					if (Header.IfNoneMatch.Value == Rec.ETag)
						throw new NotModifiedException();
				}
				else if (!(Header.IfModifiedSince is null))
				{
					if ((Limit = Header.IfModifiedSince.Timestamp).HasValue &&
						LessOrEqual(LastModified, Limit.Value.ToUniversalTime()))
					{
						throw new NotModifiedException();
					}
				}
			}

			return Rec;
		}

		/// <summary>
		/// Computes <paramref name="LastModified"/>&lt;=<paramref name="Limit"/>. The normal &lt;= operator behaved strangely, and
		/// did not get the equality part of the operation correct, perhaps due to round-off errors. This method only compares
		/// properties own to second level, and assumes all time-zones to be GMT, and avoids comparing time zones.
		/// </summary>
		/// <param name="LastModified">DateTime value.</param>
		/// <param name="Limit">DateTimeOffset value.</param>
		/// <returns>If <paramref name="LastModified"/>&lt;=<paramref name="Limit"/>.</returns>
		public static bool LessOrEqual(DateTime LastModified, DateTimeOffset Limit)
		{
			int i;

			i = LastModified.Year - Limit.Year;
			if (i != 0)
				return i < 0;

			i = LastModified.Month - Limit.Month;
			if (i != 0)
				return i < 0;

			i = LastModified.Day - Limit.Day;
			if (i != 0)
				return i < 0;

			i = LastModified.Hour - Limit.Hour;
			if (i != 0)
				return i < 0;

			i = LastModified.Minute - Limit.Minute;
			if (i != 0)
				return i < 0;

			i = LastModified.Second - Limit.Second;
			return i <= 0;
		}

		/// <summary>
		/// Computes <paramref name="LastModified"/>&gt;=<paramref name="Limit"/>. The normal &gt;= operator behaved strangely, and
		/// did not get the equality part of the operation correct, perhaps due to round-off errors. This method only compares
		/// properties own to second level, and assumes all time-zones to be GMT, and avoids comparing time zones.
		/// </summary>
		/// <param name="LastModified">DateTime value.</param>
		/// <param name="Limit">DateTimeOffset value.</param>
		/// <returns>If <paramref name="LastModified"/>&gt;=<paramref name="Limit"/>.</returns>
		public static bool GreaterOrEqual(DateTime LastModified, DateTimeOffset Limit)
		{
			int i;

			i = LastModified.Year - Limit.Year;
			if (i != 0)
				return i > 0;

			i = LastModified.Month - Limit.Month;
			if (i != 0)
				return i > 0;

			i = LastModified.Day - Limit.Day;
			if (i != 0)
				return i > 0;

			i = LastModified.Hour - Limit.Hour;
			if (i != 0)
				return i > 0;

			i = LastModified.Minute - Limit.Minute;
			if (i != 0)
				return i > 0;

			i = LastModified.Second - Limit.Second;
			return i >= 0;
		}

		private Stream CheckAcceptable(HttpRequest Request, HttpResponse Response, ref string ContentType, out bool Dynamic,
			string FullPath, string ResourceName)
		{
			HttpRequestHeader Header = Request.Header;

			Dynamic = false;

			if (!(Header.Accept is null))
			{
				bool Acceptable = Header.Accept.IsAcceptable(ContentType, out double Quality, out AcceptanceLevel TypeAcceptance, null);

				if ((!Acceptable || TypeAcceptance == AcceptanceLevel.Wildcard) && (this.allowTypeConversionFrom is null ||
					(this.allowTypeConversionFrom.TryGetValue(ContentType, out bool Allowed) && Allowed)))
				{
					IContentConverter Converter = null;
					string NewContentType = null;

					foreach (AcceptRecord AcceptRecord in Header.Accept.Records)
					{
						NewContentType = AcceptRecord.Item;
						if (NewContentType.EndsWith("/*"))
						{
							NewContentType = null;
							continue;
						}

						if (InternetContent.CanConvert(ContentType, NewContentType, out Converter))
						{
							Acceptable = true;
							break;
						}
					}

					if (Converter is null)
					{
						IContentConverter[] Converters = InternetContent.GetConverters(ContentType);

						if (!(Converters is null))
						{
							string BestContentType = null;
							double BestQuality = 0;
							IContentConverter Best = null;
							bool Found;

							foreach (IContentConverter Converter2 in InternetContent.Converters)
							{
								Found = false;

								foreach (string FromContentType in Converter2.FromContentTypes)
								{
									if (ContentType == FromContentType)
									{
										Found = true;
										break;
									}
								}

								if (!Found)
									continue;

								foreach (string ToContentType in Converter2.ToContentTypes)
								{
									if (Header.Accept.IsAcceptable(ToContentType, out double Quality2) && Quality > BestQuality)
									{
										BestContentType = ToContentType;
										BestQuality = Quality;
										Best = Converter2;
									}
								}
							}

							if (!(Best is null) && (!Acceptable || BestQuality >= Quality))
							{
								Acceptable = true;
								Converter = Best;
								NewContentType = BestContentType;
							}
						}
					}

					if (Acceptable && !(Converter is null))
					{
						Stream f2 = null;
						Stream f = File.Open(FullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
						bool Ok = false;

						try
						{
							f2 = f.Length < HttpClientConnection.MaxInmemoryMessageSize ? (Stream)new MemoryStream() : new TemporaryFile();

							if (!(Request.Session is null))
							{
								Request.Session["Request"] = Request;
								Request.Session["Response"] = Response;
							}

							List<string> Alternatives = null;
							string[] Range = Converter.ToContentTypes;

							foreach (AcceptRecord AcceptRecord in Header.Accept.Records)
							{
								if (AcceptRecord.Item.EndsWith("/*") || AcceptRecord.Item == NewContentType)
									continue;

								if (Array.IndexOf<string>(Range, AcceptRecord.Item) >= 0)
								{
									if (Alternatives is null)
										Alternatives = new List<string>();

									Alternatives.Add(AcceptRecord.Item);
								}
							}

							if (Converter.Convert(ContentType, f, FullPath, ResourceName, Request.Header.GetURL(false, false),
								ref NewContentType, f2, Request.Session, Alternatives?.ToArray()))
							{
								Dynamic = true;
							}

							ContentType = NewContentType;
							Ok = true;
						}
						finally
						{
							if (f2 is null)
								f.Dispose();
							else if (!Ok)
							{
								f2.Dispose();
								f.Dispose();
							}
							else
							{
								f.Dispose();
								f = f2;
								f.Position = 0;
							}
						}

						return f;
					}
				}

				if (!Acceptable)
					throw new NotAcceptableException();
			}

			lock (protectedContentTypes)
			{
				if (protectedContentTypes.TryGetValue(ContentType, out bool Protected) && Protected)
					throw new ForbiddenException("Resource is protected.");
			}

			return null;
		}

		private class ReadProgress
		{
			public ByteRangeInterval Next;
			public HttpResponse Response;
			public HttpRequest Request;
			public Stream f;
			public string Boundary;
			public string ContentType;
			public long BytesLeft;
			public long TotalLength;
			public int BlockSize;
			public byte[] Buffer;

			public async Task BeginRead()
			{
				int NrRead;

				try
				{
					do
					{
						while (this.BytesLeft > 0)
						{
							NrRead = await this.f.ReadAsync(this.Buffer, 0, (int)Math.Min(this.BlockSize, this.BytesLeft));

							if (NrRead <= 0)
							{
								await this.Dispose();
								return;
							}
							else
							{
								await this.Response.Write(this.Buffer, 0, NrRead);
								this.BytesLeft -= NrRead;
							}
						}

						if (!(this.Next is null))
						{
							long First;

							if (this.Next.First.HasValue)
								First = this.Next.First.Value;
							else
								First = this.TotalLength - this.Next.Last.Value;

							this.f.Position = First;
							this.BytesLeft = this.Next.GetIntervalLength(this.TotalLength);

							await Response.WriteLine();
							await Response.WriteLine("--" + this.Boundary);
							await Response.WriteLine("Content-Type: " + this.ContentType);
							await Response.WriteLine("Content-Range: " + ContentByteRangeInterval.ContentRangeToString(First, First + this.BytesLeft - 1, this.TotalLength));
							await Response.WriteLine();

							this.Next = this.Next.Next;
						}
					}
					while (this.BytesLeft > 0);

					if (!string.IsNullOrEmpty(this.Boundary))
					{
						await Response.WriteLine();
						await Response.WriteLine("--" + this.Boundary + "--");
					}

					Variables Session;

					if (!(this.Request is null) &&
						this.Request.Header.Method == "GET" &&
						!((Session = this.Request.Session) is null) &&
						Session.ContainsVariable(" LastPost "))
					{
						Session.Remove(" LastPost ");
						Session.Remove(" LastPostResource ");
						Session.Remove(" LastPostReferer ");
					}

					await this.Dispose();
				}
				catch (Exception ex)
				{
					try
					{
						if (!this.Response.HeaderSent)
							await this.Response.SendResponse(ex);
						else
							await this.Response.Flush();

						this.Response.Dispose();
						this.Response = null;

						await this.Dispose();
					}
					catch (Exception)
					{
						// Ignore
					}
				}
			}

			public async Task Dispose()
			{
				if (!(this.f is null))
				{
					await this.f.FlushAsync();
					this.f.Dispose();
					this.f = null;
				}

				if (!(this.Response is null))
				{
					await this.Response.SendResponse();
					this.Response.Dispose();
					this.Response = null;
				}
			}
		}

		/// <summary>
		/// Executes the ranged GET method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <param name="FirstInterval">First byte range interval.</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public async Task GET(HttpRequest Request, HttpResponse Response, ByteRangeInterval FirstInterval)
		{
			string FullPath = this.GetFullPath(Request, out bool Exists);
			if (Exists)
			{
				HttpRequestHeader Header = Request.Header;
				DateTime LastModified = File.GetLastWriteTime(FullPath).ToUniversalTime();
				DateTimeOffset? Limit;
				CacheRec Rec;

				if (!(Header.IfRange is null) && (Limit = Header.IfRange.Timestamp).HasValue &&
					!LessOrEqual(LastModified, Limit.Value.ToUniversalTime()))
				{
					Response.StatusCode = 200;
					await this.GET(Request, Response);    // No ranged request.
					return;
				}

				Rec = this.CheckCacheHeaders(FullPath, LastModified, Request);

				string ContentType = InternetContent.GetContentType(Path.GetExtension(FullPath));
				Stream f = CheckAcceptable(Request, Response, ref ContentType, out bool Dynamic, FullPath, Request.Header.Resource);
				Rec.IsDynamic = Dynamic;

				if (Response.ResponseSent)
					return;

				ReadProgress Progress = new ReadProgress()
				{
					Response = Response,
					Request = Request,
					f = f ?? File.Open(FullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
				};

				ByteRangeInterval Interval = FirstInterval;
				Progress.TotalLength = Progress.f.Length;

				long i = 0;
				long j;
				long First;

				if (FirstInterval.First.HasValue)
					First = FirstInterval.First.Value;
				else
					First = Progress.TotalLength - FirstInterval.Last.Value;

				Progress.f.Position = First;
				Progress.BytesLeft = Interval.GetIntervalLength(Progress.TotalLength);
				Progress.Next = Interval.Next;

				while (!(Interval is null))
				{
					j = Interval.GetIntervalLength(Progress.TotalLength);
					if (j > i)
						i = j;

					Interval = Interval.Next;
				}

				Progress.BlockSize = (int)Math.Min(BufferSize, i);
				Progress.Buffer = new byte[Progress.BlockSize];

				if (FirstInterval.Next is null)
				{
					Progress.Boundary = null;
					Progress.ContentType = null;

					Response.ContentType = ContentType;
					Response.ContentLength = FirstInterval.GetIntervalLength(Progress.f.Length);
					Response.SetHeader("Content-Range", ContentByteRangeInterval.ContentRangeToString(First, First + Progress.BytesLeft - 1, Progress.TotalLength));
				}
				else
				{
					Progress.Boundary = Guid.NewGuid().ToString().Replace("-", string.Empty);
					Progress.ContentType = ContentType;

					Response.ContentType = "multipart/byteranges; boundary=" + Progress.Boundary;
					// chunked transfer encoding will be used
				}

				if (!Rec.IsDynamic)
				{
					Response.SetHeader("ETag", Rec.ETag);
					Response.SetHeader("Last-Modified", CommonTypes.EncodeRfc822(LastModified));
				}

				if (Response.OnlyHeader || Progress.BytesLeft == 0)
				{
					await Response.SendResponse();
					await Progress.Dispose();
				}
				else
				{
					if (!(FirstInterval.Next is null))
					{
						await Response.WriteLine();
						await Response.WriteLine("--" + Progress.Boundary);
						await Response.WriteLine("Content-Type: " + Progress.ContentType);
						await Response.WriteLine("Content-Range: " + ContentByteRangeInterval.ContentRangeToString(First, First + Progress.BytesLeft - 1, Progress.TotalLength));
						await Response.WriteLine();
					}

					Task _ = Progress.BeginRead();
				}
			}
			else
				await this.RaiseFileNotFound(FullPath, Request, Response);
		}

		/// <summary>
		/// Executes the PUT method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public async Task PUT(HttpRequest Request, HttpResponse Response)
		{
			string FullPath = this.GetFullPath(Request, out bool _);

			if (!Request.HasData)
				throw new BadRequestException("No data in PUT request.");

			string Folder = Path.GetDirectoryName(FullPath);
			if (!Directory.Exists(Folder))
				Directory.CreateDirectory(Folder);

			using (FileStream f = File.Create(FullPath))
			{
				Request.DataStream.CopyTo(f);
			}

			Response.StatusCode = 201;
			await Response.SendResponse();
			Response.Dispose();
		}

		/// <summary>
		/// Executes the ranged PUT method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <param name="Interval">Content byte range.</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public async Task PUT(HttpRequest Request, HttpResponse Response, ContentByteRangeInterval Interval)
		{
			string FullPath = this.GetFullPath(Request, out bool Exists);

			if (!Request.HasData)
				throw new BadRequestException("No data in PUT request.");

			if (!Exists)
			{
				string Folder = Path.GetDirectoryName(FullPath);
				if (!Directory.Exists(Folder))
					Directory.CreateDirectory(Folder);
			}

			using (FileStream f = Exists ? File.OpenWrite(FullPath) : File.Create(FullPath))
			{
				long l;

				if ((l = Interval.First - f.Length) > 0)
				{
					f.Position = f.Length;

					int BlockSize = (int)Math.Min(BufferSize, Interval.First - f.Length);
					byte[] Block = new byte[BlockSize];
					int i;

					while (l > 0)
					{
						i = (int)Math.Min(l, BlockSize);
						f.Write(Block, 0, i);
						l -= i;
					}
				}
				else
					f.Position = Interval.First;

				Request.DataStream.CopyTo(f);
			}

			Response.StatusCode = 201;
			await Response.SendResponse();
			Response.Dispose();
		}

		/// <summary>
		/// Executes the DELETE method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public async Task DELETE(HttpRequest Request, HttpResponse Response)
		{
			string FullPath = this.GetFullPath(Request, out bool Exists);

			if (Exists)
				File.Delete(FullPath);
			else if (Directory.Exists(FullPath))
				Directory.Delete(FullPath, true);
			else
				throw new NotFoundException("File not found: " + Request.SubPath);

			await Response.SendResponse();
			Response.Dispose();
		}

		/// <summary>
		/// Enables content conversion on files in this folder, and its subfolders. If no content types are specified,
		/// all types are seen fit to convert. Actual conversions are limited to classes implementing the
		/// <see cref="IContentConverter"/> interface that are found in the application.
		/// </summary>
		/// <param name="ContentTypes">Content types available for conversion. If the list is empty, any type of content is allowed to be converted.</param>
		public void AllowTypeConversion(params string[] ContentTypes)
		{
			if (ContentTypes.Length == 0)
				this.allowTypeConversionFrom = null;
			else
			{
				Dictionary<string, bool> List = new Dictionary<string, bool>();

				foreach (string s in ContentTypes)
					List[s] = true;

				this.allowTypeConversionFrom = List;
			}
		}

		/// <summary>
		/// Method called when a resource has been registered on a server.
		/// </summary>
		/// <param name="Server">Server</param>
		public override void AddReference(HttpServer Server)
		{
			base.AddReference(Server);

			Server.ETagSaltChanged += Server_ETagSaltChanged;
		}

		/// <summary>
		/// Method called when a resource has been unregistered from a server.
		/// </summary>
		/// <param name="Server">Server</param>
		public override bool RemoveReference(HttpServer Server)
		{
			Server.ETagSaltChanged -= Server_ETagSaltChanged;

			return base.RemoveReference(Server);
		}

		private void Server_ETagSaltChanged(object sender, EventArgs e)
		{
			lock (this.cacheInfo)
			{
				this.cacheInfo.Clear();
			}
		}

		/// <summary>
		/// Executes the POST method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public async Task POST(HttpRequest Request, HttpResponse Response)
		{
			Variables Session = Request.Session;
			if (Session is null)
				throw new MethodNotAllowedException(this.AllowedMethods);

			string Referer = Request.Header.Referer?.Value;

			Session[" LastPost "] = Request.DecodeData();
			Session[" LastPostResource "] = Request.SubPath;
			Session[" LastPostReferer "] = Referer;

			if (!string.IsNullOrEmpty(Referer) &&
				Uri.TryCreate(Referer, UriKind.RelativeOrAbsolute, out Uri RefererUri) &&
				string.Compare(Request.SubPath, RefererUri.AbsolutePath, true) == 0)
			{
				throw new SeeOtherException(Referer);  // PRG pattern.
			}
			else
				await this.GET(Request, Response);
		}
	}
}
