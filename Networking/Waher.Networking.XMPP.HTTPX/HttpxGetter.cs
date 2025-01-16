using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Networking.HTTP;
using Waher.Runtime.Inventory;
using Waher.Runtime.IO;
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
		/// httpx
		/// </summary>
		public const string HttpxUriScheme = "httpx";

		/// <summary>
		/// Supported URI schemes.
		/// </summary>
		public string[] UriSchemes => new string[] { HttpxUriScheme };

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
				case HttpxUriScheme:
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
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="RemoteCertificateValidator">Optional validator of remote certificates.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Decoded object.</returns>
		public Task<ContentResponse> GetAsync(Uri Uri, X509Certificate Certificate, RemoteCertificateEventHandler RemoteCertificateValidator,
			params KeyValuePair<string, string>[] Headers)
		{
			return this.GetAsync(Uri, Certificate, RemoteCertificateValidator, 60000, Headers);
		}

		/// <summary>
		/// Gets a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="RemoteCertificateValidator">Optional validator of remote certificates.</param>
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
		public async Task<ContentResponse> GetAsync(Uri Uri, X509Certificate Certificate,
			RemoteCertificateEventHandler RemoteCertificateValidator, int TimeoutMs, params KeyValuePair<string, string>[] Headers)
		{
			using ContentStreamResponse Rec = await this.GetTempStreamAsync(Uri, Certificate, RemoteCertificateValidator, TimeoutMs, Headers);

			if (Rec.HasError)
				return new ContentResponse(Rec.Error);

			Rec.Encoded.Position = 0;

			if (Rec.Encoded.Length > int.MaxValue)
				return new ContentResponse(new OutOfMemoryException("Resource too large."));

			byte[] Bin = await Rec.Encoded.ReadAllAsync();

			return await InternetContent.DecodeAsync(Rec.ContentType, Bin, Uri);
		}

		/// <summary>
		/// Gets a (possibly big) resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="RemoteCertificateValidator">Optional validator of remote certificates.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <exception cref="InvalidOperationException">No <see cref="HttpxProxy"/> set in the HTTPX <see cref="Types"/> module parameter.</exception>
		/// <exception cref="ArgumentException">If the <paramref name="Uri"/> parameter is invalid.</exception>
		/// <exception cref="ConflictException">If an approved presence subscription with the remote entity does not exist.</exception>
		/// <exception cref="ServiceUnavailableException">If the remote entity is not online.</exception>
		/// <exception cref="TimeoutException">If the request times out.</exception>
		/// <returns>Content-Type, together with a Temporary file, if resource has been downloaded, or null if resource is data-less.</returns>
		public Task<ContentStreamResponse> GetTempStreamAsync(Uri Uri, X509Certificate Certificate,
			RemoteCertificateEventHandler RemoteCertificateValidator, params KeyValuePair<string, string>[] Headers)
		{
			return this.GetTempStreamAsync(Uri, Certificate, RemoteCertificateValidator, 60000, Headers);
		}

		/// <summary>
		/// Gets a (possibly big) resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds. (Default=60000)</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="RemoteCertificateValidator">Optional validator of remote certificates.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <exception cref="InvalidOperationException">No <see cref="HttpxProxy"/> set in the HTTPX <see cref="Types"/> module parameter.</exception>
		/// <exception cref="ArgumentException">If the <paramref name="Uri"/> parameter is invalid.</exception>
		/// <exception cref="ConflictException">If an approved presence subscription with the remote entity does not exist.</exception>
		/// <exception cref="ServiceUnavailableException">If the remote entity is not online.</exception>
		/// <exception cref="TimeoutException">If the request times out.</exception>
		/// <returns>Content-Type, together with a Temporary file, if resource has been downloaded, or null if resource is data-less.</returns>
		public async Task<ContentStreamResponse> GetTempStreamAsync(Uri Uri, X509Certificate Certificate,
			RemoteCertificateEventHandler RemoteCertificateValidator, int TimeoutMs, params KeyValuePair<string, string>[] Headers)
		{
			HttpxClient HttpxClient;
			string BareJid = Uri.UserInfo + "@" + Uri.Authority;
			string FullJid;
			string LocalUrl;

			if (Types.TryGetModuleParameter("HTTPX", out object Obj) && Obj is HttpxProxy Proxy)
			{
				if (Proxy.DefaultXmppClient.Disposed || Proxy.ServerlessMessaging.Disposed)
					return new ContentStreamResponse(new InvalidOperationException("Service is being shut down."));

				if (string.Compare(BareJid, Proxy.DefaultXmppClient.BareJID, true) == 0 &&
					Proxy.DefaultXmppClient.TryGetExtension(out HttpxServer Server))
				{
					return await Server.GetLocalTempStreamAsync(Uri.PathAndQuery + Uri.Fragment);
				}
				else
				{
					GetClientResponse Rec = await Proxy.GetClientAsync(Uri);

					BareJid = Rec.BareJid;
					FullJid = Rec.FullJid;
					HttpxClient = Rec.HttpxClient;
					LocalUrl = Rec.LocalUrl;
				}
			}
			else if (Types.TryGetModuleParameter("XMPP", out Obj) && Obj is XmppClient XmppClient)
			{
				if (XmppClient.Disposed)
					return new ContentStreamResponse(new InvalidOperationException("Service is being shut down."));

				if (string.Compare(BareJid, XmppClient.BareJID, true) == 0 &&
					XmppClient.TryGetExtension(out HttpxServer Server))
				{
					return await Server.GetLocalTempStreamAsync(Uri.PathAndQuery + Uri.Fragment);
				}
				else
				{
					if (!XmppClient.TryGetExtension(out HttpxClient HttpxClient2))
						return new ContentStreamResponse(new InvalidOperationException("No HTTPX Extesion has been registered on the XMPP Client."));

					HttpxClient = HttpxClient2;

					if (string.IsNullOrEmpty(Uri.UserInfo))
						FullJid = BareJid = Uri.Authority;
					else
					{
						BareJid = Uri.UserInfo + "@" + Uri.Authority;

						RosterItem Item = XmppClient.GetRosterItem(BareJid);

						if (Item is null)
							return new ContentStreamResponse(new ConflictException("No approved presence subscription with " + BareJid + "."));
						else if (!Item.HasLastPresence || !Item.LastPresence.IsOnline)
							return new ContentStreamResponse(new ServiceUnavailableException(BareJid + " is not online."));
						else
							FullJid = Item.LastPresenceFullJid;
					}

					LocalUrl = Uri.PathAndQuery + Uri.Fragment;
				}
			}
			else
				return new ContentStreamResponse(new InvalidOperationException("An HTTPX Proxy or XMPP Client Module Parameter has not been registered."));

			List<HttpField> Headers2 = new List<HttpField>();
			bool HasHost = false;

			foreach (KeyValuePair<string, string> Header in Headers)
			{
				switch (Header.Key.ToLower())
				{
					case "host":
						Headers2.Add(new HttpField("Host", BareJid));
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

				// TODO: Transport public part of Client certificate, if provided.

				if (HttpxClient is null)
					return new ContentStreamResponse(new Exception("No HTTPX client available."));

				await HttpxClient.Request(FullJid, "GET", LocalUrl, async (Sender, e) =>
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

				}, async (Sender, e) =>
				{
					await (State.File?.WriteAsync(e.Data, 0, e.Data.Length) ?? Task.CompletedTask);
					if (e.Last)
						State.Done?.TrySetResult(true);

				}, State, Headers2.ToArray());

				if (!await State.Done.Task)
					return new ContentStreamResponse(new TimeoutException("Request timed out."));

				Timer.Dispose();
				Timer = null;

				if (State.StatusCode >= 200 && State.StatusCode < 300)
				{
					TemporaryStream Result = State.File;
					State.File = null;

					return new ContentStreamResponse(State.HttpResponse?.ContentType, Result);
				}
				else
				{
					string ContentType = string.Empty;
					byte[] Data;

					if (State.File is null)
						Data = null;
					else
					{
						ContentType = State.HttpResponse.ContentType;
						State.File.Position = 0;
						Data = await State.File.ReadAllAsync();
					}

					return new ContentStreamResponse(GetExceptionObject(State.StatusCode, State.StatusMessage,
						State.HttpResponse, Data, ContentType));
				}
			}
			finally
			{
				State.File?.Dispose();
				State.File = null;

				if (!(State.HttpResponse is null))
				{
					await State.HttpResponse.DisposeAsync();
					State.HttpResponse = null;
				}

				Timer?.Dispose();
				Timer = null;
			}
		}

		internal static Exception GetExceptionObject(int StatusCode, string StatusMessage,
			HttpResponse Response, byte[] Data, string ContentType)
		{
			return StatusCode switch
			{
				// Client Errors
				BadRequestException.Code => new BadRequestException(Data, ContentType),
				ConflictException.Code => new ConflictException(Data, ContentType),
				FailedDependencyException.Code => new FailedDependencyException(Data, ContentType),
				ForbiddenException.Code => new ForbiddenException(Data, ContentType),
				GoneException.Code => new GoneException(Data, ContentType),
				LockedException.Code => new LockedException(Data, ContentType),
				MethodNotAllowedException.Code => new MethodNotAllowedException(GetMethods(Response.GetFirstHeader("Allow")), Data, ContentType),
				MisdirectedRequestException.Code => new MisdirectedRequestException(Data, ContentType),
				NotAcceptableException.Code => new NotAcceptableException(Data, ContentType),
				NotFoundException.Code => new NotFoundException(Data, ContentType),
				PreconditionFailedException.Code => new PreconditionFailedException(Data, ContentType),
				PreconditionRequiredException.Code => new PreconditionRequiredException(Data, ContentType),
				RangeNotSatisfiableException.Code => new RangeNotSatisfiableException(Data, ContentType),
				RequestTimeoutException.Code => new RequestTimeoutException(Data, ContentType),
				TooManyRequestsException.Code => new TooManyRequestsException(Data, ContentType),
				UnauthorizedException.Code => new UnauthorizedException(Data, ContentType, Response.GetChallenges()),
				UnavailableForLegalReasonsException.Code => new UnavailableForLegalReasonsException(Data, ContentType),
				UnprocessableEntityException.Code => new UnprocessableEntityException(Data, ContentType),
				UnsupportedMediaTypeException.Code => new UnsupportedMediaTypeException(Data, ContentType),
				UpgradeRequiredException.Code => new UpgradeRequiredException(Response.GetFirstHeader("Upgrade"), Data, ContentType),
				// Redirections
				MovedPermanentlyException.Code => new MovedPermanentlyException(Response.GetFirstHeader("Location"), Data, ContentType),
				FoundException.Code => new FoundException(Response.GetFirstHeader("Location"), Data, ContentType),
				SeeOtherException.Code => new SeeOtherException(Response.GetFirstHeader("Location"), Data, ContentType),
				NotModifiedException.Code => new NotModifiedException(),
				UseProxyException.Code => new UseProxyException(Response.GetFirstHeader("Location"), Data, ContentType),
				TemporaryRedirectException.Code => new TemporaryRedirectException(Response.GetFirstHeader("Location"), Data, ContentType),
				PermanentRedirectException.Code => new PermanentRedirectException(Response.GetFirstHeader("Location"), Data, ContentType),
				// Server Errors
				BadGatewayException.Code => new BadGatewayException(Data, ContentType),
				GatewayTimeoutException.Code => new GatewayTimeoutException(Data, ContentType),
				InsufficientStorageException.Code => new InsufficientStorageException(Data, ContentType),
				InternalServerErrorException.Code => new InternalServerErrorException(Data, ContentType),
				LoopDetectedException.Code => new LoopDetectedException(Data, ContentType),
				NetworkAuthenticationRequiredException.Code => new NetworkAuthenticationRequiredException(Data, ContentType),
				NotExtendedException.Code => new NotExtendedException(Data, ContentType),
				HTTP.NotImplementedException.Code => new HTTP.NotImplementedException(Data, ContentType),
				ServiceUnavailableException.Code => new ServiceUnavailableException(Data, ContentType),
				VariantAlsoNegotiatesException.Code => new VariantAlsoNegotiatesException(Data, ContentType),
				_ => new HttpException(StatusCode, StatusMessage, Data, ContentType),
			};
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
