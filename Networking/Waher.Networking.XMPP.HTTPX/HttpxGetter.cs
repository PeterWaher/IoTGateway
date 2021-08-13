using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Networking.HTTP;
using Waher.Runtime.Inventory;
using Waher.Runtime.Temporary;

namespace Waher.Networking.XMPP.HTTPX
{
	/// <summary>
	/// Content Getter, retrieving content using the HTTPX URI Scheme.
	/// 
	/// For the getter to work, the application needs to register a <see cref="HttpxProxy"/> object instance
	/// as a Model Parameter named HTTPX.
	/// </summary>
	public class HttpxGetter : IContentGetter
	{
		private static HttpxProxy proxy = null;

		/// <summary>
		/// Content Getter, retrieving content using the HTTPX URI Scheme.
		/// 
		/// For the getter to work, the application needs to register a <see cref="HttpxProxy"/> object instance
		/// as a Model Parameter named HTTPX.
		/// </summary>
		public HttpxGetter()
		{
		}

		/// <summary>
		/// Supported URI schemes.
		/// </summary>
		public string[] UriSchemes => new string[] { "httpx" };

		/// <summary>
		/// If the getter is able to get a resource, given its URI.
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Grade">How well the getter would be able to get a resource given the indicated URI.</param>
		/// <returns>If the getter can get a resource with the indicated URI.</returns>
		public bool CanGet(Uri Uri, out Grade Grade)
		{
			switch (Uri.Scheme)
			{
				case "httpx":
					Grade = Grade.Ok;
					return true;

				default:
					Grade = Grade.NotAtAll;
					return false;
			}
		}

		/// <summary>
		/// Gets a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Decoded object.</returns>
		public Task<object> GetAsync(Uri Uri, params KeyValuePair<string, string>[] Headers)
		{
			return this.GetAsync(Uri, 60000, Headers);
		}

		/// <summary>
		/// Gets a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds. (Default=60000)</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <exception cref="InvalidOperationException">No <see cref="HttpxProxy"/> set in the HTTPX <see cref="Types"/> module parameter.</exception>
		/// <exception cref="ArgumentException">If the <paramref name="Uri"/> parameter is invalid.</exception>
		/// <exception cref="ArgumentException">If the object response be decoded.</exception>
		/// <exception cref="ConflictException">If an approved presence subscription with the remote entity does not exist.</exception>
		/// <exception cref="ServiceUnavailableException">If the remote entity is not online.</exception>
		/// <exception cref="TimeoutException">If the request times out.</exception>
		/// <exception cref="OutOfMemoryException">If resource too large to decode.</exception>
		/// <exception cref="IOException">If unable to read from temporary file.</exception>
		/// <returns>Decoded object.</returns>
		public async Task<object> GetAsync(Uri Uri, int TimeoutMs, params KeyValuePair<string, string>[] Headers)
		{
			KeyValuePair<string, TemporaryStream> Rec = await this.GetTempStreamAsync(Uri, TimeoutMs, Headers);
			string ContentType = Rec.Key;
			TemporaryStream File = Rec.Value;

			try
			{
				if (File is null)
					return null;

				File.Position = 0;

				if (File.Length > int.MaxValue)
					throw new OutOfMemoryException("Resource too large.");

				int Len = (int)File.Length;
				byte[] Bin = new byte[Len];
				if (await File.ReadAsync(Bin, 0, Len) != Len)
					throw new IOException("Unable to read from file.");

				return InternetContent.Decode(ContentType, Bin, Uri);
			}
			finally
			{
				File?.Dispose();
			}
		}

		/// <summary>
		/// Gets a (possibly big) resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <exception cref="InvalidOperationException">No <see cref="HttpxProxy"/> set in the HTTPX <see cref="Types"/> module parameter.</exception>
		/// <exception cref="ArgumentException">If the <paramref name="Uri"/> parameter is invalid.</exception>
		/// <exception cref="ConflictException">If an approved presence subscription with the remote entity does not exist.</exception>
		/// <exception cref="ServiceUnavailableException">If the remote entity is not online.</exception>
		/// <exception cref="TimeoutException">If the request times out.</exception>
		/// <returns>Content-Type, together with a Temporary file, if resource has been downloaded, or null if resource is data-less.</returns>
		public Task<KeyValuePair<string, TemporaryStream>> GetTempStreamAsync(Uri Uri, params KeyValuePair<string, string>[] Headers)
		{
			return this.GetTempStreamAsync(Uri, 60000, Headers);
		}

		/// <summary>
		/// Gets a (possibly big) resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds. (Default=60000)</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <exception cref="InvalidOperationException">No <see cref="HttpxProxy"/> set in the HTTPX <see cref="Types"/> module parameter.</exception>
		/// <exception cref="ArgumentException">If the <paramref name="Uri"/> parameter is invalid.</exception>
		/// <exception cref="ConflictException">If an approved presence subscription with the remote entity does not exist.</exception>
		/// <exception cref="ServiceUnavailableException">If the remote entity is not online.</exception>
		/// <exception cref="TimeoutException">If the request times out.</exception>
		/// <returns>Content-Type, together with a Temporary file, if resource has been downloaded, or null if resource is data-less.</returns>
		public async Task<KeyValuePair<string, TemporaryStream>> GetTempStreamAsync(Uri Uri, int TimeoutMs, params KeyValuePair<string, string>[] Headers)
		{
			if (proxy is null)
			{
				if (!Types.TryGetModuleParameter("HTTPX", out object Obj) ||
					!(Obj is HttpxProxy Proxy))
				{
					throw new InvalidOperationException("A HTTPX Proxy object has not been registered.");
				}

				if (Proxy.DefaultXmppClient.Disposed || Proxy.ServerlessMessaging.Disposed)
					throw new InvalidOperationException("Service is being shut down.");

				proxy = Proxy;
			}

			GetClientResponse Rec = await proxy.GetClientAsync(Uri);
			List<HttpField> Headers2 = new List<HttpField>();
			bool HasHost = false;

			foreach (KeyValuePair<string, string> Header in Headers)
			{
				switch (Header.Key.ToLower())
				{
					case "host":
						Headers2.Add(new HttpField("Host", Rec.BareJid));
						HasHost = true;
						break;

					case "cookie":
					case "set-cookie":
						// Do not forward cookies.
						break;

					default:
						Headers2.Add(new HttpField(Header.Key, Header.Value));
						break;
				}
			}

			if (!HasHost)
				Headers2.Add(new HttpField("Host", Uri.Authority));

			State State = null;
			Timer Timer = null;

			try
			{
				State = new State();
				Timer = new Timer((P) =>
				{
					State.Done.TrySetResult(false);
				}, null, TimeoutMs, Timeout.Infinite);

				Rec.HttpxClient.Request(Rec.FullJid, "GET", Rec.LocalUrl, async (sender, e) =>
				{
					if (e.Ok)
					{
						State.HttpResponse = e.HttpResponse;
						State.StatusCode = e.StatusCode;
						State.StatusMessage = e.StatusMessage;

						if (e.HasData)
						{
							State.File = new TemporaryStream();

							if (!(e.Data is null))
							{
								await State.File.WriteAsync(e.Data, 0, e.Data.Length);
								State.Done.TrySetResult(true);
							}
						}
						else
							State.Done.TrySetResult(true);
					}
					else
						State.Done.TrySetException(e.StanzaError ?? new Exception("Unable to get resource."));

				}, async (sender, e) =>
				{
					await (State.File?.WriteAsync(e.Data, 0, e.Data.Length) ?? Task.CompletedTask);
					if (e.Last)
						State.Done?.TrySetResult(true);

				}, State, Headers2.ToArray());

				if (!await State.Done.Task)
					throw new TimeoutException("Request timed out.");

				Timer.Dispose();
				Timer = null;

				if (State.StatusCode >= 200 && State.StatusCode < 300)
				{
					TemporaryStream Result = State.File;
					State.File = null;

					return new KeyValuePair<string, TemporaryStream>(State.HttpResponse?.ContentType, Result);
				}
				else
				{
					string ContentType = string.Empty;
					byte[] Data;

					if (State.File is null)
						Data = null;
					else
					{
						int Len = (int)State.File.Length;

						ContentType = State.HttpResponse.ContentType;
						State.File.Position = 0;
						Data = new byte[Len];
						await State.File.ReadAsync(Data, 0, Len);
					}

					throw GetExceptionObject(State.StatusCode, State.StatusMessage, 
						State.HttpResponse, Data, ContentType);
				}
			}
			finally
			{
				State.File?.Dispose();
				State.File = null;

				State.HttpResponse?.Dispose();
				State.HttpResponse = null;

				Timer?.Dispose();
				Timer = null;
			}
		}

		internal static Exception GetExceptionObject(int StatusCode, string StatusMessage,
			HttpResponse Response, byte[] Data, string ContentType)
		{
			switch (StatusCode)
			{
				case MovedPermanentlyException.Code: throw new MovedPermanentlyException(Response.GetFirstHeader("Location"), Data, ContentType);
				case FoundException.Code: throw new FoundException(Response.GetFirstHeader("Location"), Data, ContentType);
				case SeeOtherException.Code: throw new SeeOtherException(Response.GetFirstHeader("Location"), Data, ContentType);
				case NotModifiedException.Code: throw new NotModifiedException();
				case UseProxyException.Code: throw new UseProxyException(Response.GetFirstHeader("Location"), Data, ContentType);
				case TemporaryRedirectException.Code: throw new TemporaryRedirectException(Response.GetFirstHeader("Location"), Data, ContentType);
				case BadRequestException.Code: throw new BadRequestException(Data, ContentType);
				case ForbiddenException.Code: throw new ForbiddenException(Data, ContentType);
				case NotFoundException.Code: throw new NotFoundException(Data, ContentType);
				case MethodNotAllowedException.Code: throw new MethodNotAllowedException(GetMethods(Response.GetFirstHeader("Allow")), Data, ContentType);
				case RequestTimeoutException.Code: throw new RequestTimeoutException(Data, ContentType);
				case ConflictException.Code: throw new ConflictException(Data, ContentType);
				case GoneException.Code: throw new GoneException(Data, ContentType);
				case PreconditionFailedException.Code: throw new PreconditionFailedException(Data, ContentType);
				case UnsupportedMediaTypeException.Code: throw new UnsupportedMediaTypeException(Data, ContentType);
				case RangeNotSatisfiableException.Code: throw new RangeNotSatisfiableException(Data, ContentType);
				case MisdirectedRequestException.Code: throw new MisdirectedRequestException(Data, ContentType);
				case UnprocessableEntityException.Code: throw new UnprocessableEntityException(Data, ContentType);
				case LockedException.Code: throw new LockedException(Data, ContentType);
				case FailedDependencyException.Code: throw new FailedDependencyException(Data, ContentType);
				case UpgradeRequiredException.Code: throw new UpgradeRequiredException(Response.GetFirstHeader("Upgrade"), Data, ContentType);
				case TooManyRequestsException.Code: throw new TooManyRequestsException(Data, ContentType);
				case PreconditionRequiredException.Code: throw new PreconditionRequiredException(Data, ContentType);
				case UnavailableForLegalReasonsException.Code: throw new UnavailableForLegalReasonsException(Data, ContentType);
				case InternalServerErrorException.Code: throw new InternalServerErrorException(Data, ContentType);
				case HTTP.NotImplementedException.Code: throw new HTTP.NotImplementedException(Data, ContentType);
				case BadGatewayException.Code: throw new BadGatewayException(Data, ContentType);
				case ServiceUnavailableException.Code: throw new ServiceUnavailableException(Data, ContentType);
				case GatewayTimeoutException.Code: throw new GatewayTimeoutException(Data, ContentType);
				case VariantAlsoNegotiatesException.Code: throw new VariantAlsoNegotiatesException(Data, ContentType);
				case InsufficientStorageException.Code: throw new InsufficientStorageException(Data, ContentType);
				case LoopDetectedException.Code: throw new LoopDetectedException(Data, ContentType);
				case NotExtendedException.Code: throw new NotExtendedException(Data, ContentType);
				case NetworkAuthenticationRequiredException.Code: throw new NetworkAuthenticationRequiredException(Data, ContentType);
				default: throw new HttpException(StatusCode, StatusMessage, Data, ContentType);
			}
		}

		private static string[] GetMethods(string Allow)
		{
			if (string.IsNullOrEmpty(Allow))
				return new string[0];

			string[] Result = Allow.Split(',');
			int i, c = Result.Length;

			for (i = 0; i < c; i++)
				Result[i] = Result[i].Trim();

			return Result;
		}

		private class State
		{
			public HttpResponse HttpResponse = null;
			public TemporaryStream File = null;
			public TaskCompletionSource<bool> Done = new TaskCompletionSource<bool>();
			public string StatusMessage = string.Empty;
			public int StatusCode = 0;
		}

	}
}
