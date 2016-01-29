using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using Waher.Content;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// Publishes a folder with all its files and subfolders through HTTP GET, with optional support for PUT and DELETE.
	/// If PUT and DELETE are allowed, users (if authenticated) can update the contents of the folder.
	/// </summary>
	public class HttpFolderResource : HttpAsynchronousResource, IHttpGetMethod, IHttpGetRangesMethod, IHttpPutMethod, IHttpPutRangesMethod, IHttpDeleteMethod
	{
		private const int BufferSize = 8192;

		private HttpAuthenticationScheme[] authenticationSchemes;
		private string folderPath;
		private bool allowPut;
		private bool allowDelete;
		private bool anonymousGET;

		/// <summary>
		/// Publishes an embedded resource through HTTP GET.
		/// </summary>
		/// <param name="ResourceName">Name of resource.</param>
		/// <param name="FolderPath">Full path to folder to publish.</param>
		/// <param name="AllowPut">If the PUT method should be allowed.</param>
		/// <param name="AllowDelete">If the DELETE method should be allowed.</param>
		/// <param name="AnonymousGET">If Anonymous GET access is allowed.</param>
		/// <param name="AuthenticationSchemes">Any authentication schemes used to authenticate users before access is granted.</param>
		public HttpFolderResource(string ResourceName, string FolderPath, bool AllowPut, bool AllowDelete, bool AnonymousGET,
			params HttpAuthenticationScheme[] AuthenticationSchemes)
			: base(ResourceName)
		{
			this.folderPath = FolderPath;
			this.authenticationSchemes = AuthenticationSchemes;
			this.allowPut = AllowPut;
			this.allowDelete = AllowDelete;
			this.anonymousGET = AnonymousGET;
		}

		/// <summary>
		/// If the resource handles sub-paths.
		/// </summary>
		public override bool HandlesSubPaths
		{
			get { return true; }
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
				throw new Forbidden();

			switch (Request.Header.Method)
			{
				case "PUT":
					if (!this.allowPut)
						throw new MethodNotAllowed(this.allowDelete ? new string[] { "GET", "DELETE" } : new string[] { "GET" });

					break;

				case "DELETE":
					if (!this.allowDelete)
						throw new MethodNotAllowed(this.allowPut ? new string[] { "GET", "PUT" } : new string[] { "GET" });
					break;
			}
		}

		/// <summary>
		/// Executes the GET method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public void GET(HttpRequest Request, HttpResponse Response)
		{
			string FullPath = this.folderPath + Request.SubPath;
			if (!File.Exists(FullPath))
				throw new NotFound();

			ReadProgress Progress = new ReadProgress();
			Progress.Response = Response;
			Progress.f = File.OpenRead(FullPath);
			Progress.BytesLeft = Progress.TotalLength = Progress.f.Length;
			Progress.BlockSize = (int)Math.Min(BufferSize, Progress.BytesLeft);
			Progress.Buffer = new byte[Progress.BlockSize];
			Progress.Next = null;
			Progress.Boundary = null;
			Progress.ContentType = null;

			Response.ContentType = InternetContent.GetContentType(Path.GetExtension(FullPath));
			Response.ContentLength = Progress.TotalLength;

			if (Response.OnlyHeader || Progress.TotalLength == 0)
			{
				Response.SendResponse();
				Progress.Dispose();
			}
			else
				Progress.BeginRead();
		}

		private class ReadProgress : IDisposable
		{
			public ByteRangeInterval Next;
			public HttpResponse Response;
			public FileStream f;
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
				catch (Exception)
				{
					this.Dispose();
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
			string FullPath = this.folderPath + Request.SubPath;
			if (!File.Exists(FullPath))
				throw new NotFound();

			ReadProgress Progress = new ReadProgress();
			Progress.Response = Response;
			Progress.f = File.OpenRead(FullPath);

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

				Response.ContentType = InternetContent.GetContentType(Path.GetExtension(FullPath));
				Response.ContentLength = FirstInterval.GetIntervalLength(Progress.f.Length);
				Response.SetHeader("Content-Range", ContentByteRangeInterval.ContentRangeToString(First, First + Progress.BytesLeft - 1, Progress.TotalLength));
			}
			else
			{
				Progress.Boundary = Guid.NewGuid().ToString().Replace("-", string.Empty);
				Progress.ContentType = InternetContent.GetContentType(Path.GetExtension(FullPath));

				Response.ContentType = "multipart/byteranges; boundary=" + Progress.Boundary;
				// chunked transfer encoding will be used
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
			string FullPath = this.folderPath + Request.SubPath;

			if (!Request.HasData)
				throw new BadRequest();

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
			string FullPath = this.folderPath + Request.SubPath;

			if (!Request.HasData)
				throw new BadRequest();

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
			string FullPath = this.folderPath + Request.SubPath;

			if (File.Exists(FullPath))
				File.Delete(FullPath);
			else if (Directory.Exists(FullPath))
				Directory.Delete(FullPath, true);
			else
				throw new NotFound();

			Response.SendResponse();
		}
	}
}
