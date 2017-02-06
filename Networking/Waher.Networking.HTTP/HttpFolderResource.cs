using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Waher.Content;
using Waher.Networking.HTTP.HeaderFields;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// Publishes a folder with all its files and subfolders through HTTP GET, with optional support for PUT and DELETE.
	/// If PUT and DELETE are allowed, users (if authenticated) can update the contents of the folder.
	/// </summary>
	public class HttpFolderResource : HttpAsynchronousResource, IHttpGetMethod, IHttpGetRangesMethod, IHttpPutMethod, IHttpPutRangesMethod, IHttpDeleteMethod
	{
		private const int BufferSize = 32768;

		private Dictionary<string, bool> allowTypeConversionFrom = null;
		private HttpAuthenticationScheme[] authenticationSchemes;
		private string folderPath;
		private bool allowPut;
		private bool allowDelete;
		private bool anonymousGET;
		private bool userSessions;

		/// <summary>
		/// Publishes an embedded resource through HTTP GET.
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
			: base(ResourceName)
		{
			this.authenticationSchemes = AuthenticationSchemes;
			this.allowPut = AllowPut;
			this.allowDelete = AllowDelete;
			this.anonymousGET = AnonymousGET;
			this.userSessions = UserSessions;

			this.FolderPath = FolderPath;

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
		public override bool HandlesSubPaths
		{
			get { return true; }
		}

		/// <summary>
		/// If the resource uses user sessions.
		/// </summary>
		public override bool UserSessions
		{
			get { return this.userSessions; }
		}

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

			if (Request.SubPath.Contains(".."))
				throw new ForbiddenException();

			HttpRequestHeader Header = Request.Header;
			DateTimeOffset? Limit;

			if (Header.IfMatch == null && Header.IfUnmodifiedSince != null && (Limit = Header.IfUnmodifiedSince.Timestamp).HasValue)
			{
				string FullPath = this.GetFullPath(Request);
				if (File.Exists(FullPath))
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
						throw new MethodNotAllowedException(this.allowDelete ? new string[] { "GET", "DELETE" } : new string[] { "GET" });

					break;

				case "DELETE":
					if (!this.allowDelete)
						throw new MethodNotAllowedException(this.allowPut ? new string[] { "GET", "PUT" } : new string[] { "GET" });
					break;
			}
		}

		private string GetFullPath(HttpRequest Request)
		{
			return this.folderPath + HttpUtility.UrlDecode(Request.SubPath).Replace('/', Path.DirectorySeparatorChar);
		}

		private Dictionary<string, CacheRec> cacheInfo = new Dictionary<string, CacheRec>();

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
		public void GET(HttpRequest Request, HttpResponse Response)
		{
			string FullPath = this.GetFullPath(Request);
			if (!File.Exists(FullPath))
				throw new NotFoundException();

			DateTime LastModified = File.GetLastWriteTime(FullPath).ToUniversalTime();
			CacheRec Rec;

			Rec = this.CheckCacheHeaders(FullPath, LastModified, Request);

			string ContentType = InternetContent.GetContentType(Path.GetExtension(FullPath));
			bool Dynamic;
			Stream f = CheckAcceptable(Request, Response, ref ContentType, out Dynamic, FullPath, Request.Header.Resource);
			Rec.IsDynamic = Dynamic;

			ReadProgress Progress = new ReadProgress();
			Progress.Response = Response;
			Progress.f = f != null ? f : File.OpenRead(FullPath);
			Progress.BytesLeft = Progress.TotalLength = Progress.f.Length;
			Progress.BlockSize = (int)Math.Min(BufferSize, Progress.BytesLeft);
			Progress.Buffer = new byte[Progress.BlockSize];
			Progress.Next = null;
			Progress.Boundary = null;
			Progress.ContentType = null;

			Response.ContentType = ContentType;
			Response.ContentLength = Progress.TotalLength;

			if (!Rec.IsDynamic)
			{
				Response.SetHeader("ETag", Rec.ETag);
				Response.SetHeader("Last-Modified", CommonTypes.EncodeRfc822(LastModified));
			}

			if (Response.OnlyHeader || Progress.TotalLength == 0)
			{
				Response.SendResponse();
				Progress.Dispose();
			}
			else
				Progress.BeginRead();
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

			if (Rec == null)
			{
				Rec = new CacheRec();
				Rec.LastModified = LastModified;
				Rec.IsDynamic = true;

				using (FileStream fs = File.OpenRead(FullPath))
				{
					Rec.ETag = Hashes.ComputeSHA1HashString(fs);
				}

				lock (this.cacheInfo)
				{
					this.cacheInfo[CacheKey] = Rec;
				}
			}

			if (!Rec.IsDynamic)
			{
				if (Request.Header.IfNoneMatch != null)
				{
					if (Request.Header.IfNoneMatch.Value == Rec.ETag)
						throw new NotModifiedException();
				}
				else if (Header.IfModifiedSince != null)
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
		internal static bool LessOrEqual(DateTime LastModified, DateTimeOffset Limit)
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
		internal static bool GreaterOrEqual(DateTime LastModified, DateTimeOffset Limit)
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

			if (Header.Accept != null)
			{
				ContentTypeAcceptance TypeAcceptance;
				double Quality;
				bool Acceptable;
				bool Allowed;

				Acceptable = Header.Accept.IsAcceptable(ContentType, out Quality, out TypeAcceptance, null);

				if ((!Acceptable || TypeAcceptance == ContentTypeAcceptance.Wildcard) && (this.allowTypeConversionFrom == null ||
					(this.allowTypeConversionFrom.TryGetValue(ContentType, out Allowed) && Allowed)))
				{
					IContentConverter Converter = null;
					string NewContentType = null;

					foreach (AcceptRecord AcceptRecord in Header.Accept.Records)
					{
						NewContentType = AcceptRecord.Item;
						if (NewContentType.EndsWith("/*"))
							continue;

						if (InternetContent.CanConvert(ContentType, NewContentType, out Converter))
						{
							Acceptable = true;
							break;
						}
					}

					if (Acceptable && Converter != null)
					{
						Stream f2 = null;
						Stream f = File.OpenRead(FullPath);
						bool Ok = false;

						try
						{
							f2 = f.Length < HttpClientConnection.MaxInmemoryMessageSize ? (Stream)new MemoryStream() : new TemporaryFile();

							Request.Session["Request"] = Request;
							Request.Session["Response"] = Response;

							if (Converter.Convert(ContentType, f, FullPath, ResourceName, Request.Header.GetURL(false, false),
								NewContentType, f2, Request.Session))
							{
								Dynamic = true;
							}

							ContentType = NewContentType;
							Ok = true;
						}
						finally
						{
							if (f2 == null)
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
								f2 = null;
								f.Position = 0;
							}
						}

						return f;
					}
				}

				if (!Acceptable)
					throw new NotAcceptableException();
			}

			return null;
		}

		private class ReadProgress : IDisposable
		{
			public ByteRangeInterval Next;
			public HttpResponse Response;
			public Stream f;
			public string Boundary;
			public string ContentType;
			public long BytesLeft;
			public long TotalLength;
			public int BlockSize;
			public byte[] Buffer;

			public void BeginRead()
			{
				this.f.BeginRead(this.Buffer, 0, (int)Math.Min(this.BlockSize, this.BytesLeft), this.DataRead, null);
			}

			public void DataRead(IAsyncResult ar)
			{
				try
				{
					int NrRead = this.f.EndRead(ar);
					if (NrRead <= 0)
						this.Dispose();
					else
					{
						this.Response.Write(this.Buffer, 0, NrRead);
						this.BytesLeft -= NrRead;

						if (this.BytesLeft <= 0)
							this.StartNext();
						else
							this.BeginRead();
					}
				}
				catch (Exception ex)
				{
					try
					{
						if (!this.Response.HeaderSent)
							this.Response.SendResponse(ex);
						else
							this.Response.Close();

						this.Response.Dispose();
						this.Response = null;

						this.Dispose();
					}
					catch (Exception)
					{
						// Ignore
					}
				}
			}

			private void StartNext()
			{
				if (this.Next == null)
				{
					if (!string.IsNullOrEmpty(this.Boundary))
					{
						Response.WriteLine();
						Response.WriteLine("--" + this.Boundary + "--");
					}

					this.Dispose();
				}
				else
				{
					long First;

					if (this.Next.First.HasValue)
						First = this.Next.First.Value;
					else
						First = this.TotalLength - this.Next.Last.Value;

					this.f.Position = First;
					this.BytesLeft = this.Next.GetIntervalLength(this.TotalLength);

					Response.WriteLine();
					Response.WriteLine("--" + this.Boundary);
					Response.WriteLine("Content-Type: " + this.ContentType);
					Response.WriteLine("Content-Range: " + ContentByteRangeInterval.ContentRangeToString(First, First + this.BytesLeft - 1, this.TotalLength));
					Response.WriteLine();

					this.Next = this.Next.Next;

					this.BeginRead();
				}
			}

			public void Dispose()
			{
				if (this.Response != null)
				{
					this.Response.SendResponse();
					this.Response = null;
				}

				if (this.f != null)
				{
					this.f.Dispose();
					this.f = null;
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
		public void GET(HttpRequest Request, HttpResponse Response, ByteRangeInterval FirstInterval)
		{
			string FullPath = this.GetFullPath(Request);
			if (!File.Exists(FullPath))
				throw new NotFoundException();

			HttpRequestHeader Header = Request.Header;
			DateTime LastModified = File.GetLastWriteTime(FullPath).ToUniversalTime();
			DateTimeOffset? Limit;
			CacheRec Rec;

			if (Header.IfRange != null && (Limit = Header.IfRange.Timestamp).HasValue &&
				!LessOrEqual(LastModified, Limit.Value.ToUniversalTime()))
			{
				Response.StatusCode = 200;
				this.GET(Request, Response);    // No ranged request.
				return;
			}

			Rec = this.CheckCacheHeaders(FullPath, LastModified, Request);

			string ContentType = InternetContent.GetContentType(Path.GetExtension(FullPath));
			bool Dynamic;
			Stream f = CheckAcceptable(Request, Response, ref ContentType, out Dynamic, FullPath, Request.Header.Resource);
			Rec.IsDynamic = Dynamic;

			ReadProgress Progress = new ReadProgress();
			Progress.Response = Response;
			Progress.f = f != null ? f : File.OpenRead(FullPath);

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

			while (Interval != null)
			{
				j = Interval.GetIntervalLength(Progress.TotalLength);
				if (j > i)
					i = j;

				Interval = Interval.Next;
			}

			Progress.BlockSize = (int)Math.Min(BufferSize, i);
			Progress.Buffer = new byte[Progress.BlockSize];

			if (FirstInterval.Next == null)
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
				Response.SendResponse();
				Progress.Dispose();
			}
			else
			{
				if (FirstInterval.Next != null)
				{
					Response.WriteLine();
					Response.WriteLine("--" + Progress.Boundary);
					Response.WriteLine("Content-Type: " + Progress.ContentType);
					Response.WriteLine("Content-Range: " + ContentByteRangeInterval.ContentRangeToString(First, First + Progress.BytesLeft - 1, Progress.TotalLength));
					Response.WriteLine();
				}

				Progress.BeginRead();
			}
		}

		/// <summary>
		/// Executes the PUT method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public void PUT(HttpRequest Request, HttpResponse Response)
		{
			string FullPath = this.GetFullPath(Request);

			if (!Request.HasData)
				throw new BadRequestException();

			string Folder = Path.GetDirectoryName(FullPath);
			if (!Directory.Exists(Folder))
				Directory.CreateDirectory(Folder);

			using (FileStream f = File.Create(FullPath))
			{
				Request.DataStream.CopyTo(f);
			}

			Response.SendResponse();
		}

		/// <summary>
		/// Executes the ranged PUT method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <param name="Interval">Content byte range.</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public void PUT(HttpRequest Request, HttpResponse Response, ContentByteRangeInterval Interval)
		{
			string FullPath = this.GetFullPath(Request);

			if (!Request.HasData)
				throw new BadRequestException();

			string Folder = Path.GetDirectoryName(FullPath);
			if (!Directory.Exists(Folder))
				Directory.CreateDirectory(Folder);

			using (FileStream f = File.Exists(FullPath) ? File.OpenWrite(FullPath) : File.Create(FullPath))
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

			Response.SendResponse();
		}

		/// <summary>
		/// Executes the DELETE method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public void DELETE(HttpRequest Request, HttpResponse Response)
		{
			string FullPath = this.GetFullPath(Request);

			if (File.Exists(FullPath))
				File.Delete(FullPath);
			else if (Directory.Exists(FullPath))
				Directory.Delete(FullPath, true);
			else
				throw new NotFoundException();

			Response.SendResponse();
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
	}
}
