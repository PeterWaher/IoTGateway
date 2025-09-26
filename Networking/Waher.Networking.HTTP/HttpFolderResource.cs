﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Net;
using Waher.Content;
using Waher.Events;
using Waher.Networking.HTTP.HeaderFields;
using Waher.Runtime.Temporary;
using Waher.Script;
using Waher.Runtime.IO;

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
	/// Publishes a folder with all its files and subfolders through HTTP GET, with optional support for PUT, PATCH and DELETE.
	/// If PUT, PATCH and DELETE are allowed, users (if authenticated) can update the contents of the folder.
	/// </summary>
	public class HttpFolderResource : HttpAsynchronousResource, IHttpGetMethod, IHttpGetRangesMethod,
		IHttpPutMethod, IHttpPutRangesMethod, IHttpPatchMethod, IHttpPatchRangesMethod, IHttpDeleteMethod, IHttpPostMethod
	{
		private const int BufferSize = 32768;

		private readonly static Dictionary<string, bool> protectedContentTypes = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
		private readonly Dictionary<string, CacheRec> cacheInfo = new Dictionary<string, CacheRec>();
		private readonly Dictionary<string, bool> definedDomains = new Dictionary<string, bool>(StringComparer.CurrentCultureIgnoreCase);
		private readonly Dictionary<string, string> folders = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
		private LinkedList<KeyValuePair<string, string>> defaultResponseHeaders = null;
		private Dictionary<string, bool> allowTypeConversionFrom = null;
		private readonly HttpAuthenticationScheme[] authenticationSchemes;
		private readonly HostDomainOptions domainOptions;
		private readonly bool allowPutPatch;
		private readonly bool allowDelete;
		private readonly bool anonymousGET;
		private readonly bool userSessions;
		private string folderPath;

		/// <summary>
		/// Publishes a folder with all its files and subfolders through HTTP GET, with optional support for PUT, PATCH and DELETE.
		/// If PUT, PATCH and DELETE are allowed, users (if authenticated) can update the contents of the folder.
		/// </summary>
		/// <param name="ResourceName">Name of resource.</param>
		/// <param name="FolderPath">Full path to folder to publish.</param>
		/// <param name="AllowPutPatch">If the PUT and PATCH methods should be allowed.</param>
		/// <param name="AllowDelete">If the DELETE method should be allowed.</param>
		/// <param name="AnonymousGET">If Anonymous GET access is allowed.</param>
		/// <param name="UserSessions">If the resource uses user sessions.</param>
		/// <param name="AuthenticationSchemes">Any authentication schemes used to authenticate users before access is granted.</param>
		public HttpFolderResource(string ResourceName, string FolderPath, bool AllowPutPatch, bool AllowDelete, bool AnonymousGET,
			bool UserSessions, params HttpAuthenticationScheme[] AuthenticationSchemes)
			: this(ResourceName, FolderPath, AllowPutPatch, AllowDelete, AnonymousGET, UserSessions,
				  HostDomainOptions.SameForAllDomains, AuthenticationSchemes)
		{
		}

		/// <summary>
		/// Publishes a folder with all its files and subfolders through HTTP GET, with optional support for PUT, PATCH and DELETE.
		/// If PUT, PATCH and DELETE are allowed, users (if authenticated) can update the contents of the folder.
		/// </summary>
		/// <param name="ResourceName">Name of resource.</param>
		/// <param name="FolderPath">Full path to folder to publish.</param>
		/// <param name="AllowPutPatch">If the PUT and PATCH methods should be allowed.</param>
		/// <param name="AllowDelete">If the DELETE method should be allowed.</param>
		/// <param name="AnonymousGET">If Anonymous GET access is allowed.</param>
		/// <param name="UserSessions">If the resource uses user sessions.</param>
		/// <param name="DomainOptions">Options on how to handle the Host header.</param>
		/// <param name="AuthenticationSchemes">Any authentication schemes used to authenticate users before access is granted.</param>
		public HttpFolderResource(string ResourceName, string FolderPath, bool AllowPutPatch, bool AllowDelete, bool AnonymousGET,
			bool UserSessions, HostDomainOptions DomainOptions, params HttpAuthenticationScheme[] AuthenticationSchemes)
			: this(ResourceName, FolderPath, AllowPutPatch, AllowDelete, AnonymousGET, UserSessions,
				  DomainOptions, Array.Empty<string>(), AuthenticationSchemes)
		{
		}

		/// <summary>
		/// Publishes a folder with all its files and subfolders through HTTP GET, with optional support for PUT, PATCH and DELETE.
		/// If PUT, PATCH and DELETE are allowed, users (if authenticated) can update the contents of the folder.
		/// </summary>
		/// <param name="ResourceName">Name of resource.</param>
		/// <param name="FolderPath">Full path to folder to publish.</param>
		/// <param name="AllowPutPatch">If the PUT and PATCH methods should be allowed.</param>
		/// <param name="AllowDelete">If the DELETE method should be allowed.</param>
		/// <param name="AnonymousGET">If Anonymous GET access is allowed.</param>
		/// <param name="UserSessions">If the resource uses user sessions.</param>
		/// <param name="DomainOptions">Options on how to handle the Host header.</param>
		/// <param name="DomainNames">Pre-defined host names.</param>
		/// <param name="AuthenticationSchemes">Any authentication schemes used to authenticate users before access is granted.</param>
		public HttpFolderResource(string ResourceName, string FolderPath, bool AllowPutPatch, bool AllowDelete, bool AnonymousGET,
			bool UserSessions, HostDomainOptions DomainOptions, string[] DomainNames, params HttpAuthenticationScheme[] AuthenticationSchemes)
			: base(ResourceName)
		{
			this.authenticationSchemes = AuthenticationSchemes;
			this.allowPutPatch = AllowPutPatch;
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
		/// Checks if a Content-Type is protected.
		/// </summary>
		/// <param name="ContentType">Content-Type to check.</param>
		/// <returns>if the content type is protected.</returns>
		public static bool IsProtected(string ContentType)
		{
			lock (protectedContentTypes)
			{
				return protectedContentTypes.TryGetValue(ContentType, out bool Protected) && Protected;
			}
		}

		/// <summary>
		/// Adds a default HTTP Response header that will be returned in responses for resources in the folder.
		/// </summary>
		/// <param name="Key">Header key.</param>
		/// <param name="Value">Header value.</param>
		public void AddDefaultResponseHeader(string Key, string Value)
		{
			if (this.defaultResponseHeaders is null)
				this.defaultResponseHeaders = new LinkedList<KeyValuePair<string, string>>();

			this.defaultResponseHeaders.AddLast(new KeyValuePair<string, string>(Key, Value));
		}

		/// <summary>
		/// Folder path.
		/// </summary>
		public string FolderPath
		{
			get => this.folderPath;
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
		public bool AllowsPUT => this.allowPutPatch;

		/// <summary>
		/// If the PATCH method is allowed.
		/// </summary>
		public bool AllowsPATCH => this.allowPutPatch;

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

			if (this.anonymousGET && ((s = Request.Header.Method) == "GET" || s == "HEAD" || s == "OPTIONS"))
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
				string FullPath = this.GetFullPath(Request, null, true, true, out bool Exists);
				if (Exists)
				{
					DateTime LastModified = File.GetLastWriteTimeUtc(FullPath);

					if (GreaterOrEqual(LastModified, Limit.Value.ToUniversalTime()))
						throw new NotModifiedException();
				}
			}

			switch (Request.Header.Method)
			{
				case "PUT":
				case "PATCH":
					if (!this.allowPutPatch)
						throw new MethodNotAllowedException(this.AllowedMethods, Request);

					break;

				case "DELETE":
					if (!this.allowDelete)
						throw new MethodNotAllowedException(this.AllowedMethods, Request);
					break;
			}
		}

		/// <summary>
		/// Checks a string to see if it contains invalid file characters.
		/// </summary>
		/// <param name="s">String to check.</param>
		/// <param name="PermitFolderSeparator">If folder separator characters are permitted.</param>
		/// <param name="PermitIpAddress">If IP Addresses are permitted.</param>
		/// <returns>If invalid characters are found in the string.</returns>
		public static bool ContainsInvalidFileCharacters(string s, bool PermitFolderSeparator, bool PermitIpAddress)
		{
			if (string.IsNullOrEmpty(s))
				return false;

			bool PrevPeriod = false;

			foreach (char ch in s)
			{
				switch (ch)
				{
					case '/':
					case '\\':
						if (!PermitFolderSeparator)
							return true;
						PrevPeriod = false;
						break;

					case ':':
						if (!PermitIpAddress || !IPAddress.TryParse(s, out _))
							return true;
						PrevPeriod = false;
						break;

					case '|':
					case '<':
					case '>':
					case '?':
					case '"':
					case '*':
					case '\x00':
					case '\x01':
					case '\x02':
					case '\x03':
					case '\x04':
					case '\x05':
					case '\x06':
					case '\x07':
					case '\x08':
					case '\x09':
					case '\x0a':
					case '\x0b':
					case '\x0c':
					case '\x0d':
					case '\x0e':
					case '\x0f':
					case '\x10':
					case '\x11':
					case '\x12':
					case '\x13':
					case '\x14':
					case '\x15':
					case '\x16':
					case '\x17':
					case '\x18':
					case '\x19':
					case '\x1a':
					case '\x1b':
					case '\x1c':
					case '\x1d':
					case '\x1e':
					case '\x1f':
						return true;

					case '.':
						if (PrevPeriod)
							return true;

						PrevPeriod = true;
						break;

					default:
						PrevPeriod = false;
						break;
				}
			}

			return false;
		}

		/// <summary>
		/// Gets the full path of a resource in the folder.
		/// </summary>
		/// <param name="Request">HTTP Request object.</param>
		/// <param name="AltSubPath">Alternative sub-path, if request subpath not available.</param>
		/// <param name="ForbiddenExceptions">If forbidden exceptions can be thrown, if irregularities are found.</param>
		/// <param name="MustExist">If file must exist.</param>
		/// <param name="Found">If file name is found.</param>
		/// <returns>Full path, if found.</returns>
		internal string GetFullPath(HttpRequest Request, string AltSubPath, bool ForbiddenExceptions, bool MustExist, out bool Found)
		{
			string SubPath = Request?.SubPath ?? AltSubPath;
			HttpRequestHeader Header = Request?.Header;
			string s = WebUtility.UrlDecode(SubPath).Replace('/', Path.DirectorySeparatorChar);
			string s2, s3;
			string ContentType;
			int i;

			if (ContainsInvalidFileCharacters(s, true, false))
			{
				if (ForbiddenExceptions)
					throw new ForbiddenException(Request, "Path control characters not permitted in resource name.");
				else
				{
					Found = false;
					return null;
				}
			}

			if (this.domainOptions != HostDomainOptions.SameForAllDomains)
			{
				string Host = Header?.Host?.Value;
				string Folder;

				if (Host is null)
					Host = string.Empty;
				else
				{
					Host = Host.RemovePortNumber();

					if (ContainsInvalidFileCharacters(Host, false, true))
					{
						if (ForbiddenExceptions)
							throw new ForbiddenException(Request, "Path control characters not permitted in Host header.");
						else
						{
							Found = false;
							return null;
						}
					}
				}

				if (this.domainOptions == HostDomainOptions.OnlySpecifiedDomains)
				{
					lock (this.definedDomains)
					{
						if (!this.definedDomains.ContainsKey(Host))
						{
							if (ForbiddenExceptions)
								throw new ForbiddenException(Request, "Access to this folder is not permitted on this domain.");
							else
							{
								Found = false;
								return null;
							}
						}
					}

					Folder = this.folderPath;
				}
				else
					Folder = this.GetRootHostFolder(Host);

				if (Folder.EndsWith(Path.DirectorySeparatorChar) && s.StartsWith(Path.DirectorySeparatorChar))
					s2 = Folder + s.Substring(1);
				else
					s2 = Folder + s;

				if (Found = File.Exists(s2))
					return s2;

				i = s2.LastIndexOf('.');
				if (i > 0 &&
					File.Exists(s3 = s2.Substring(0, i)) &&
					InternetContent.TryGetContentType(s2.Substring(i + 1), out ContentType) &&
					(Header?.Accept?.IsAcceptable(ContentType) ?? true))
				{
					if (!(Header is null))
						Header.Accept = new HttpFieldAccept("Accept", ContentType);

					Found = true;
					return s3;
				}
			}

			s2 = this.folderPath + s;
			Found = !MustExist || File.Exists(s2);
			if (Found)
				return s2;

			i = s2.LastIndexOf('.');
			if (i > 0 &&
				File.Exists(s3 = s2.Substring(0, i)) &&
				InternetContent.TryGetContentType(s2.Substring(i + 1), out ContentType) &&
				(Header?.Accept?.IsAcceptable(ContentType) ?? true))
			{
				if (!(Header is null))
					Header.Accept = new HttpFieldAccept("Accept", ContentType);

				Found = true;
				return s3;
			}

			return s2;
		}

		private string GetRootHostFolder(string Host)
		{
			lock (this.folders)
			{
				if (this.folders.TryGetValue(Host, out string Folder))
					return Folder;

				Folder = this.folderPath + Path.DirectorySeparatorChar + Host;
				if (Directory.Exists(Folder))
				{
					this.folders[Host] = Folder;
					return Folder;
				}

				if (Host.StartsWith("www.", StringComparison.CurrentCultureIgnoreCase))
				{
					Folder = this.folderPath + Path.DirectorySeparatorChar + Host.Substring(4);
					if (Directory.Exists(Folder))
					{
						this.folders[Host] = Folder;
						return Folder;
					}
				}

				int i = Host.IndexOf('.');
				if (i > 0)
				{
					Folder = this.folderPath + Path.DirectorySeparatorChar + Host.Substring(0, i);
					if (Directory.Exists(Folder))
					{
						this.folders[Host] = Folder;
						return Folder;
					}
				}

				Folder = this.folderPath;
				this.folders[Host] = Folder;
				return Folder;
			}
		}

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
			string FullPath = this.GetFullPath(Request, null, true, true, out bool Exists);
			if (Exists)
			{
				DateTime LastModified = File.GetLastWriteTimeUtc(FullPath);
				CacheRec Rec;

				Rec = this.CheckCacheHeaders(FullPath, LastModified, Request);
				if (Rec is null)
				{
					await Response.SendResponse(new NotModifiedException());
					return;
				}

				string ContentType = InternetContent.GetContentType(Path.GetExtension(FullPath));
				AcceptableResponse AcceptableResponse = await this.CheckAcceptable(Request, Response,
					ContentType, FullPath, Request.Header.Resource, LastModified);

				if (AcceptableResponse is null || Response.ResponseSent)
					return;

				Rec.IsDynamic = AcceptableResponse.Dynamic;

				await SendResponse(AcceptableResponse.Stream, FullPath, 
					AcceptableResponse.ContentType, Rec.IsDynamic, Rec.ETag, 
					AcceptableResponse.LastModified, AcceptableResponse.LastModifiedUpdated,
					Response, Request, this.defaultResponseHeaders);
			}
			else
				await this.RaiseFileNotFound(FullPath, Request, Response);
		}

		private async Task RaiseFileNotFound(string FullPath, HttpRequest Request, HttpResponse Response)
		{
			NotFoundException ex = new NotFoundException("File not found: " + Request.SubPath);
			FileNotFoundEventArgs e = new FileNotFoundEventArgs(ex, FullPath, Request, Response);
			await this.FileNotFound.Raise(this, e, false);

			ex = e.Exception;
			if (ex is null)
				return;     // Sent asynchronously from event handler.

			Log.Warning("File not found.", FullPath, Request.RemoteEndPoint, "FileNotFound");

			await Response.SendResponse(ex);
			await Response.DisposeAsync();
		}

		/// <summary>
		/// Event raised when a file was requested that could not be found. 
		/// </summary>
		public event EventHandlerAsync<FileNotFoundEventArgs> FileNotFound = null;

		/// <summary>
		/// Sends a file-based response back to the client.
		/// </summary>
		/// <param name="FullPath">Full path of file.</param>
		/// <param name="ContentType">Content Type.</param>
		/// <param name="ETag">ETag of resource.</param>
		/// <param name="LastModified">When resource was last modified.</param>
		/// <param name="LastModifiedUpdated">If <paramref name="LastModified"/> has
		/// been updated during the processing of the resource.</param>
		/// <param name="Response">HTTP response object.</param>
		public static Task SendResponse(string FullPath, string ContentType, string ETag, 
			DateTime LastModified, bool LastModifiedUpdated, HttpResponse Response)
		{
			return SendResponse(null, FullPath, ContentType, false, ETag, LastModified,
				LastModifiedUpdated, Response, null, null);
		}

		/// <summary>
		/// Sends a file-based response back to the client.
		/// </summary>
		/// <param name="FullPath">Full path of file.</param>
		/// <param name="ContentType">Content Type.</param>
		/// <param name="ETag">ETag of resource.</param>
		/// <param name="LastModified">When resource was last modified.</param>
		/// <param name="LastModifiedUpdated">If <paramref name="LastModified"/> has
		/// been updated during the processing of the resource.</param>
		/// <param name="Response">HTTP response object.</param>
		/// <param name="Request">HTTP request object.</param>
		public static Task SendResponse(string FullPath, string ContentType, string ETag, 
			DateTime LastModified, bool LastModifiedUpdated, HttpResponse Response, 
			HttpRequest Request)
		{
			return SendResponse(null, FullPath, ContentType, false, ETag, LastModified,
				LastModifiedUpdated, Response, Request, null);
		}

		/// <summary>
		/// Sets any default response headers registered on the file folder object, to a HTTP Response object.
		/// </summary>
		/// <param name="Response">HTTP Response.</param>
		public void SetDefaultResponseHeaders(HttpResponse Response)
		{
			SetDefaultResponseHeaders(Response, this.defaultResponseHeaders);
		}

		private static void SetDefaultResponseHeaders(HttpResponse Response, IEnumerable<KeyValuePair<string, string>> DefaultResponseHeaders)
		{
			if (!(DefaultResponseHeaders is null))
			{
				foreach (KeyValuePair<string, string> P in DefaultResponseHeaders)
					Response.SetHeader(P.Key, P.Value);
			}
		}

		private static async Task SendResponse(Stream f, string FullPath, string ContentType, 
			bool IsDynamic, string ETag, DateTime LastModified, bool LastModifiedUpdated,
			HttpResponse Response, HttpRequest Request, LinkedList<KeyValuePair<string, string>> DefaultResponseHeaders)
		{
			ReadProgress Progress = new ReadProgress()
			{
				Response = Response,
				Request = Request,
				f = f ?? File.Open(FullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite),
				Next = null,
				Boundary = null,
				ContentType = null,
				FullPath = FullPath,
				SetLastModified = LastModifiedUpdated ? (DateTime?)LastModified : null
			};
			Progress.BytesLeft = Progress.TotalLength = Progress.f.Length;
			Progress.BlockSize = (int)Math.Min(BufferSize, Progress.BytesLeft);
			Progress.Buffer = new byte[Progress.BlockSize];

			SetDefaultResponseHeaders(Response, DefaultResponseHeaders);

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
						return null;
				}
				else if (!(Header.IfModifiedSince is null))
				{
					if ((Limit = Header.IfModifiedSince.Timestamp).HasValue &&
						LessOrEqual(LastModified, Limit.Value.ToUniversalTime()))
					{
						return null;
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

		private class AcceptableResponse : ICodecProgress
		{
			public ICodecProgress Progress;
			public DateTime LastModified;
			public Stream Stream;
			public string ContentType;
			public bool Dynamic;
			public bool LastModifiedUpdated = false;

			public Task EarlyHint(string Resource, string Relation, params KeyValuePair<string, string>[] AdditionalParameters)
				=> this.Progress?.EarlyHint(Resource, Relation, AdditionalParameters) ?? Task.CompletedTask;

			public Task HeaderProcessed() => this.Progress?.HeaderProcessed() ?? Task.CompletedTask;
			public Task BodyProcessed() => this.Progress?.BodyProcessed() ?? Task.CompletedTask;

			public void DependencyTimestamp(DateTime Timestamp)
			{
				DateTime TP = Timestamp.ToUniversalTime();

				if (TP > this.LastModified)
				{
					this.LastModified = TP;
					this.LastModifiedUpdated = true;
				}

				this.Progress?.DependencyTimestamp(Timestamp);
			}
		}

		private async Task<AcceptableResponse> CheckAcceptable(HttpRequest Request, HttpResponse Response,
			string ContentType, string FullPath, string ResourceName, DateTime LastModified)
		{
			HttpRequestHeader Header = Request.Header;
			AcceptableResponse Result = new AcceptableResponse()
			{
				Progress = Response.Progress,
				LastModified = LastModified,
				ContentType = ContentType,
				Dynamic = false
			};

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

							foreach (IContentConverter Converter2 in Converters)
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
									else if (BestQuality == 0 && ToContentType == "*")
									{
										BestContentType = ContentType;
										BestQuality = double.Epsilon;
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

							List<string> Alternatives = null;
							string[] Range = Converter.ToContentTypes;
							bool All = Range.Length == 1 && Range[0] == "*";

							foreach (AcceptRecord AcceptRecord in Header.Accept.Records)
							{
								if (AcceptRecord.Item.EndsWith("/*") || AcceptRecord.Item == NewContentType)
									continue;

								if (All || Array.IndexOf(Range, AcceptRecord.Item) >= 0)
								{
									if (Alternatives is null)
										Alternatives = new List<string>();

									Alternatives.Add(AcceptRecord.Item);
								}
							}

							ConversionState State = new ConversionState(ContentType, f, FullPath, ResourceName,
								Request.Header.GetURL(false, false), NewContentType, f2, Request.Session, Result,
								Request.Server, Request.TryGetLocalResourceFileName, Alternatives?.ToArray());

							if (await Converter.ConvertAsync(State))
							{
								NewContentType = State.ToContentType;
								Result.Dynamic = true;
							}

							if (State.HasError)
							{
								await Response.SendResponse(State.Error);
								return null;
							}
							else
							{
								Result.ContentType = NewContentType;
								Ok = true;
							}
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

						Result.Stream = f;
						return Result;
					}
				}

				if (!Acceptable)
				{
					await Response.SendResponse(new NotAcceptableException());
					return null;
				}
			}

			bool Protected;

			lock (protectedContentTypes)
			{
				if (!protectedContentTypes.TryGetValue(ContentType, out Protected))
					Protected = false;
			}

			if (Protected)
			{
				await Response.SendResponse(new ForbiddenException(Request, "Resource is protected."));
				return null;
			}

			return Result;
		}

		private class ReadProgress
		{
			public ByteRangeInterval Next;
			public HttpResponse Response;
			public HttpRequest Request;
			public DateTime? SetLastModified;
			public Stream f;
			public string Boundary;
			public string ContentType;
			public string FullPath;
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
								await this.Response.Write(false, this.Buffer, 0, NrRead);
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

							await this.Response.WriteLine();
							await this.Response.WriteLine("--" + this.Boundary);
							await this.Response.WriteLine("Content-Type: " + this.ContentType);
							await this.Response.WriteLine("Content-Range: " + ContentByteRangeInterval.ContentRangeToString(First, First + this.BytesLeft - 1, this.TotalLength));
							await this.Response.WriteLine();

							this.Next = this.Next.Next;
						}
					}
					while (this.BytesLeft > 0);

					if (!string.IsNullOrEmpty(this.Boundary))
					{
						await this.Response.WriteLine();
						await this.Response.WriteLine("--" + this.Boundary + "--");
					}

					SessionVariables Session;

					if (!(this.Request is null) &&
						this.Request.Header.Method == "GET" &&
						!((Session = this.Request.Session) is null))
					{
						Session.LastPost = null;
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
							await this.Response.Flush(true);
					}
					catch (Exception)
					{
						// Ignore
					}

					try
					{
						await this.Response.DisposeAsync();
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
					await this.Response.DisposeAsync();
					this.Response = null;
				}

				if (this.SetLastModified.HasValue)
				{
					try
					{
						File.SetLastWriteTimeUtc(this.FullPath, this.SetLastModified.Value);
					}
					catch (Exception ex)
					{
						Log.Exception(ex);
					}
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
			string FullPath = this.GetFullPath(Request, null, true, true, out bool Exists);
			if (Exists)
			{
				HttpRequestHeader Header = Request.Header;
				DateTime LastModified = File.GetLastWriteTimeUtc(FullPath);
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
				if (Rec is null)
				{
					await Response.SendResponse(new NotModifiedException());
					return;
				}

				string ContentType = InternetContent.GetContentType(Path.GetExtension(FullPath));
				AcceptableResponse AcceptableResponse = await this.CheckAcceptable(Request, Response,
					ContentType, FullPath, Request.Header.Resource, LastModified);

				if (AcceptableResponse is null || Response.ResponseSent)
					return;

				LastModified = AcceptableResponse.LastModified;
				ContentType = AcceptableResponse.ContentType;
				Rec.IsDynamic = AcceptableResponse.Dynamic;

				ReadProgress Progress = new ReadProgress()
				{
					Response = Response,
					Request = Request,
					FullPath = FullPath,
					f = AcceptableResponse.Stream ?? File.Open(FullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite),
					SetLastModified = AcceptableResponse.LastModifiedUpdated ? (DateTime?)LastModified : null
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
		public Task PUT(HttpRequest Request, HttpResponse Response)
		{
			return this.PUTPATCH(Request, Response);
		}


		/// <summary>
		/// Executes the PATCH method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public Task PATCH(HttpRequest Request, HttpResponse Response)
		{
			return this.PUTPATCH(Request, Response);
		}

		/// <summary>
		/// Executes the PUT or PATCH method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public async Task PUTPATCH(HttpRequest Request, HttpResponse Response)
		{
			string FullPath = this.GetFullPath(Request, null, true, false, out bool _);

			if (!Request.HasData)
			{
				await Response.SendResponse(new BadRequestException("No data in " + Request.Header.Method + " request."));
				return;
			}

			string Folder = Path.GetDirectoryName(FullPath);
			if (!Directory.Exists(Folder))
				Directory.CreateDirectory(Folder);

			using (FileStream f = File.Create(FullPath))
			{
				Request.DataStream.CopyTo(f);
			}

			Response.StatusCode = 201;
			Response.StatusMessage = "Created";
			await Response.SendResponse();
			await Response.DisposeAsync();
		}

		/// <summary>
		/// Executes the ranged PUT method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <param name="Interval">Content byte range.</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public Task PUT(HttpRequest Request, HttpResponse Response, ContentByteRangeInterval Interval)
		{
			return this.PUTPATCH(Request, Response, Interval);
		}

		/// <summary>
		/// Executes the ranged PATCH method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <param name="Interval">Content byte range.</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public Task PATCH(HttpRequest Request, HttpResponse Response, ContentByteRangeInterval Interval)
		{
			return this.PUTPATCH(Request, Response, Interval);
		}

		/// <summary>
		/// Executes the ranged PUT or PATCH method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <param name="Interval">Content byte range.</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public async Task PUTPATCH(HttpRequest Request, HttpResponse Response, ContentByteRangeInterval Interval)
		{
			string FullPath = this.GetFullPath(Request, null, true, false, out bool Exists);

			if (!Request.HasData)
			{
				await Response.SendResponse(new BadRequestException("No data in " + Request.Header.Method + " request."));
				return;
			}

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
			Response.StatusMessage = "Created";
			await Response.SendResponse();
			await Response.DisposeAsync();
		}

		/// <summary>
		/// Executes the DELETE method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public async Task DELETE(HttpRequest Request, HttpResponse Response)
		{
			string FullPath = this.GetFullPath(Request, null, true, true, out bool Exists);

			if (Exists)
				File.Delete(FullPath);
			else if (Directory.Exists(FullPath))
				Directory.Delete(FullPath, true);
			else
			{
				await Response.SendResponse(new NotFoundException("File not found: " + Request.SubPath));
				return;
			}

			await Response.SendResponse();
			await Response.DisposeAsync();
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

			Server.ETagSaltChanged += this.Server_ETagSaltChanged;
		}

		/// <summary>
		/// Method called when a resource has been unregistered from a server.
		/// </summary>
		/// <param name="Server">Server</param>
		public override bool RemoveReference(HttpServer Server)
		{
			Server.ETagSaltChanged -= this.Server_ETagSaltChanged;

			return base.RemoveReference(Server);
		}

		private Task Server_ETagSaltChanged(object Sender, EventArgs e)
		{
			lock (this.cacheInfo)
			{
				this.cacheInfo.Clear();
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// Executes the POST method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public async Task POST(HttpRequest Request, HttpResponse Response)
		{
			SessionVariables Session = Request.Session;
			if (Session is null)
			{
				await Response.SendResponse(new ForbiddenException(Request, "Session required."));
				return;
			}

			string Referer = Request.Header.Referer?.Value;
			ContentResponse Content = await Request.DecodeDataAsync();
			if (Content.HasError)
			{
				await Response.SendResponse(Content.Error);
				return;
			}

			Session.LastPost = new PostedInformation()
			{
				DecodedContent = Expression.Encapsulate(Content.Decoded),
				Resource = Request.SubPath,
				Referer = Referer
			};

			if (!string.IsNullOrEmpty(Referer) &&
				Uri.TryCreate(Referer, UriKind.RelativeOrAbsolute, out Uri RefererUri) &&
				string.Compare(Request.SubPath, RefererUri.AbsolutePath, true) == 0)
			{
				await Response.SendResponse(new SeeOtherException(Referer));  // PRG pattern.
				return;
			}

			await this.GET(Request, Response);
		}
	}
}
