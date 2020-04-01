using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Security.JWS;
using Waher.Security.PKCS;
using Waher.Security.PKCS.Decoders;

namespace Waher.Security.ACME
{
	/// <summary>
	/// Implements an ACME client for the generation of certificates using ACME-compliant certificate servers.
	/// </summary>
	public class AcmeClient : IDisposable
	{
		private const int KeySize = 4096;

		private readonly Uri directoryEndpoint;
		private HttpClient httpClient;
		private AcmeDirectory directory = null;
		private RsaSsaPkcsSha256 jws;
		private string nonce = null;
		private string jwkThumbprint = null;

		/// <summary>
		/// Implements an ACME client for the generation of certificates using ACME-compliant certificate servers.
		/// </summary>
		/// <param name="DirectoryEndpoint">HTTP endpoint for the ACME directory resource.</param>
		/// <param name="Parameters">RSA key parameters.</param>
		public AcmeClient(Uri DirectoryEndpoint, RSAParameters Parameters)
		{
			this.directoryEndpoint = DirectoryEndpoint;
			this.jws = new RsaSsaPkcsSha256(Parameters);

			try
			{
				this.httpClient = new HttpClient(new HttpClientHandler()
				{
					AllowAutoRedirect = true,
					AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
					CheckCertificateRevocationList = true,
					SslProtocols = SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12
				}, true);
			}
			catch (PlatformNotSupportedException)
			{
				this.httpClient = new HttpClient(new HttpClientHandler()
				{
					AllowAutoRedirect = true
				}, true);
			}

			Type T = typeof(AcmeClient);
			Version Version = T.GetTypeInfo().Assembly.GetName().Version;
			StringBuilder UserAgent = new StringBuilder();

			UserAgent.Append(T.Namespace);
			UserAgent.Append('/');
			UserAgent.Append(Version.Major.ToString());
			UserAgent.Append('.');
			UserAgent.Append(Version.Minor.ToString());
			UserAgent.Append('.');
			UserAgent.Append(Version.Build.ToString());

			this.httpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent.ToString());
			this.httpClient.DefaultRequestHeaders.Add("Accept", JwsAlgorithm.JwsContentType);
			this.httpClient.DefaultRequestHeaders.Add("Accept-Language", "en");
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			if (this.httpClient != null)
			{
				this.httpClient.Dispose();
				this.httpClient = null;
			}

			if (this.jws != null)
			{
				this.jws.Dispose();
				this.jws = null;
			}
		}

		/// <summary>
		/// Gets the ACME directory.
		/// </summary>
		/// <returns>Directory object.</returns>
		public async Task<AcmeDirectory> GetDirectory()
		{
			if (this.directory is null)
				this.directory = new AcmeDirectory(this, (await this.GET(this.directoryEndpoint)).Payload);

			return this.directory;
		}

		internal Task<AcmeResponse> POST_as_GET(Uri URL, Uri AccountLocation)
		{
			return this.POST(URL, AccountLocation, null);
		}

		internal async Task<AcmeResponse> GET(Uri URL)
		{
			HttpResponseMessage Response = await this.httpClient.GetAsync(URL);

			byte[] Bin = await Response.Content.ReadAsByteArrayAsync();
			string CharSet = Response.Content.Headers.ContentType?.CharSet;
			Encoding Encoding;

			if (string.IsNullOrEmpty(CharSet))
				Encoding = Encoding.UTF8;
			else
				Encoding = InternetContent.GetEncoding(CharSet);

			string JsonResponse = Encoding.GetString(Bin);

			if (!(JSON.Parse(JsonResponse) is IEnumerable<KeyValuePair<string, object>> Obj))
				throw new Exception("Unexpected response returned.");

			if (Response.Content.Headers.TryGetValues("Retry-After", out IEnumerable<string> _))
			{
				// TODO: Rate limit
			}

			if (Response.IsSuccessStatusCode)
			{
				return new AcmeResponse()
				{
					Payload = Obj,
					Location = URL,
					Json = JsonResponse,
					ResponseMessage = Response
				};
			}
			else
				throw CreateException(Obj, Response);
		}

		internal async Task<string> NextNonce()
		{
			if (!string.IsNullOrEmpty(this.nonce))
			{
				string s = this.nonce;
				this.nonce = null;
				return s;
			}

			if (this.directory is null)
				await this.GetDirectory();

			HttpRequestMessage Request = new HttpRequestMessage(HttpMethod.Head, this.directory.NewNonce);
			HttpResponseMessage Response = await this.httpClient.SendAsync(Request);
			Response.EnsureSuccessStatusCode();

			if (Response.Headers.TryGetValues("Replay-Nonce", out IEnumerable<string> Values))
			{
				foreach (string s in Values)
					return s;
			}

			throw new Exception("No nonce returned from server.");
		}

		/// <summary>
		/// Directory object.
		/// </summary>
		public Task<AcmeDirectory> Directory
		{
			get
			{
				if (this.directory is null)
					return this.GetDirectory();
				else
					return Task.FromResult<AcmeDirectory>(this.directory);
			}
		}

		internal class AcmeResponse
		{
			public IEnumerable<KeyValuePair<string, object>> Payload;
			public HttpResponseMessage ResponseMessage;
			public Uri Location;
			public string Json;
		}

		private async Task<HttpResponseMessage> HttpPost(Uri URL, Uri KeyID, string Accept, params KeyValuePair<string, object>[] Payload)
		{
			string HeaderString;
			string PayloadString;
			string Signature;

			if (KeyID is null)
			{
				this.jws.Sign(new KeyValuePair<string, object>[]
				{
					new KeyValuePair<string, object>("nonce", await this.NextNonce()),
					new KeyValuePair<string, object>("url", URL.ToString())
				}, Payload, out HeaderString, out PayloadString, out Signature);
			}
			else
			{
				this.jws.Sign(new KeyValuePair<string, object>[]
				{
					new KeyValuePair<string, object>("kid", KeyID.ToString()),
					new KeyValuePair<string, object>("nonce", await this.NextNonce()),
					new KeyValuePair<string, object>("url", URL.ToString())
				}, Payload, out HeaderString, out PayloadString, out Signature);
			}

			string Json = JSON.Encode(new KeyValuePair<string, object>[]
			{
				new KeyValuePair<string, object>("protected", HeaderString),
				new KeyValuePair<string, object>("payload", PayloadString),
				new KeyValuePair<string, object>("signature", Signature)
			}, null);

			HttpContent Content = new ByteArrayContent(System.Text.Encoding.ASCII.GetBytes(Json));
			Content.Headers.Add("Content-Type", JwsAlgorithm.JwsContentType);

			if (!string.IsNullOrEmpty(Accept))
				Content.Headers.TryAddWithoutValidation("Accept", Accept);

			HttpResponseMessage Response = await this.httpClient.PostAsync(URL, Content);

			this.GetNextNonce(Response);

			return Response;
		}

		internal async Task<AcmeResponse> POST(Uri URL, Uri KeyID, params KeyValuePair<string, object>[] Payload)
		{
			HttpResponseMessage Response = await this.HttpPost(URL, KeyID, null, Payload);
			byte[] Bin = await Response.Content.ReadAsByteArrayAsync();
			string CharSet = Response.Content.Headers.ContentType?.CharSet;
			Encoding Encoding;
			
			if (string.IsNullOrEmpty(CharSet))
				Encoding = Encoding.UTF8;
			else
				Encoding = InternetContent.GetEncoding(CharSet);

			AcmeResponse AcmeResponse = new AcmeResponse()
			{
				Json = Encoding.GetString(Bin),
				Location = URL,
				ResponseMessage = Response,
				Payload = null
			};

			if (Response.Headers.TryGetValues("Location", out IEnumerable<string> Values))
			{
				foreach (string s in Values)
				{
					AcmeResponse.Location = new Uri(s);
					break;
				}
			}

			if (string.IsNullOrEmpty(AcmeResponse.Json))
				AcmeResponse.Payload = null;
			else if ((AcmeResponse.Payload = JSON.Parse(AcmeResponse.Json) as IEnumerable<KeyValuePair<string, object>>) is null)
				throw new Exception("Unexpected response returned.");

			if (Response.IsSuccessStatusCode)
				return AcmeResponse;
			else
				throw CreateException(AcmeResponse.Payload, Response);
		}

		internal static AcmeException CreateException(IEnumerable<KeyValuePair<string, object>> Obj, HttpResponseMessage Response)
		{
			AcmeException[] Subproblems = null;
			string Type = null;
			string Detail = null;
			string instance = null;
			int? Status = null;

			foreach (KeyValuePair<string, object> P in Obj)
			{
				switch (P.Key)
				{
					case "type":
						Type = P.Value as string;
						break;

					case "detail":
						Detail = P.Value as string;
						break;

					case "instance":
						instance = P.Value as string;
						break;

					case "status":
						if (int.TryParse(P.Value as string, out int i))
							Status = i;
						break;

					case "subproblems":
						if (P.Value is Array A)
						{
							List<AcmeException> Subproblems2 = new List<AcmeException>();

							foreach (object Obj2 in A)
							{
								if (Obj2 is IEnumerable<KeyValuePair<string, object>> Obj3)
									Subproblems2.Add(CreateException(Obj3, Response));
							}

							Subproblems = Subproblems2.ToArray();
						}
						break;
				}
			}

			if (Type.StartsWith("urn:ietf:params:acme:error:"))
			{
				switch (Type.Substring(27))
				{
					case "accountDoesNotExist": return new AcmeAccountDoesNotExistException(Type, Detail, Status, Subproblems);
					case "badCSR": return new AcmeBadCsrException(Type, Detail, Status, Subproblems);
					case "badNonce": return new AcmeBadNonceException(Type, Detail, Status, Subproblems);
					case "badRevocationReason": return new AcmeBadRevocationReasonException(Type, Detail, Status, Subproblems);
					case "badSignatureAlgorithm": return new AcmeBadSignatureAlgorithmException(Type, Detail, Status, Subproblems);
					case "caa": return new AcmeCaaException(Type, Detail, Status, Subproblems);
					case "compound": return new AcmeCompoundException(Type, Detail, Status, Subproblems);
					case "connection": return new AcmeConnectionException(Type, Detail, Status, Subproblems);
					case "dns": return new AcmeDnsException(Type, Detail, Status, Subproblems);
					case "externalAccountRequired": return new AcmeExternalAccountRequiredException(Type, Detail, Status, Subproblems);
					case "incorrectResponse": return new AcmeIncorrectResponseException(Type, Detail, Status, Subproblems);
					case "invalidContact": return new AcmeInvalidContactException(Type, Detail, Status, Subproblems);
					case "malformed": return new AcmeMalformedException(Type, Detail, Status, Subproblems);
					case "rateLimited": return new AcmeRateLimitedException(Type, Detail, Status, Subproblems);
					case "rejectedIdentifier": return new AcmeRejectedIdentifierException(Type, Detail, Status, Subproblems);
					case "serverInternal": return new AcmeServerInternalException(Type, Detail, Status, Subproblems);
					case "tls": return new AcmeTlsException(Type, Detail, Status, Subproblems);
					case "unauthorized": return new AcmeUnauthorizedException(Type, Detail, Status, Subproblems);
					case "unsupportedContact": return new AcmeUnsupportedContactException(Type, Detail, Status, Subproblems);
					case "unsupportedIdentifier": return new AcmeUnsupportedIdentifierException(Type, Detail, Status, Subproblems);
					case "userActionRequired": return new AcmeUserActionRequiredException(Type, Detail, Status, Subproblems, new Uri(instance), GetLink(Response, "terms-of-service"));
					default: return new AcmeException(Type, Detail, Status, Subproblems);
				}
			}
			else
				return new AcmeException(Type, Detail, Status, Subproblems);
		}

		private static readonly Regex nextUrl = new Regex("^\\s*[<](?'URL'[^>]+)[>]\\s*;\\s*rel\\s*=\\s*['\"](?'Rel'.*)['\"]\\s*$", RegexOptions.Singleline | RegexOptions.Compiled);

		internal static Uri GetLink(HttpResponseMessage Response, string Rel)
		{
			if (Response.Headers.TryGetValues("Link", out IEnumerable<string> Values))
			{
				foreach (string s in Values)
				{
					Match M = nextUrl.Match(s);
					if (M.Success)
					{
						if (M.Groups["Rel"].Value == Rel)
							return new Uri(M.Groups["URL"].Value);
					}
				}
			}

			return null;
		}

		private void GetNextNonce(HttpResponseMessage Response)
		{
			if (Response.Headers.TryGetValues("Replay-Nonce", out IEnumerable<string> Values))
			{
				foreach (string s in Values)
				{
					this.nonce = s;
					return;
				}
			}
		}

		/// <summary>
		/// Creates an account on the ACME server.
		/// </summary>
		/// <param name="ContactURLs">URLs for contacting the account holder.</param>
		/// <param name="TermsOfServiceAgreed">If the terms of service have been accepted.</param>
		/// <returns>ACME account object.</returns>
		public async Task<AcmeAccount> CreateAccount(string[] ContactURLs, bool TermsOfServiceAgreed)
		{
			if (this.directory is null)
				await this.GetDirectory();

			AcmeResponse Response = await this.POST(this.directory.NewAccount, null,
				new KeyValuePair<string, object>("termsOfServiceAgreed", TermsOfServiceAgreed),
				new KeyValuePair<string, object>("contact", ContactURLs));

			AcmeAccount Account;

			if (Response.Payload is null)
			{
				Response = await this.POST(Response.Location, Response.Location);
				Account = new AcmeAccount(this, Response.Location, Response.Payload);

				bool ContactsDifferent = false;
				int i, c = ContactURLs.Length;

				if (c != Account.Contact.Length)
					ContactsDifferent = true;
				else
				{
					for (i = 0; i < c; i++)
					{
						if (ContactURLs[i] != Account.Contact[i])
						{
							ContactsDifferent = true;
							break;
						}
					}
				}

				if (ContactsDifferent)
					Account = await this.UpdateAccount(Account.Location, ContactURLs);
			}
			else
				Account = new AcmeAccount(this, Response.Location, Response.Payload);

			return Account;
		}

		/// <summary>
		/// Gets the account object from the ACME server.
		/// </summary>
		/// <returns>ACME account object.</returns>
		public async Task<AcmeAccount> GetAccount()
		{
			if (this.directory is null)
				await this.GetDirectory();

			AcmeResponse Response = await this.POST(this.directory.NewAccount, null,
				new KeyValuePair<string, object>("onlyReturnExisting", true));

			if (Response.Payload is null)
				Response = await this.POST(Response.Location, Response.Location);

			return new AcmeAccount(this, Response.Location, Response.Payload);
		}

		/// <summary>
		/// Updates an account.
		/// </summary>
		/// <param name="AccountLocation">Account location.</param>
		/// <param name="Contact">New contact information.</param>
		/// <returns>New account object.</returns>
		public async Task<AcmeAccount> UpdateAccount(Uri AccountLocation, string[] Contact)
		{
			if (this.directory is null)
				await this.GetDirectory();

			AcmeResponse Response = await this.POST(AccountLocation, AccountLocation,
				new KeyValuePair<string, object>("contact", Contact));

			return new AcmeAccount(this, Response.Location, Response.Payload);
		}

		/// <summary>
		/// Deactivates an account.
		/// </summary>
		/// <param name="AccountLocation">Account location.</param>
		/// <returns>New account object.</returns>
		public async Task<AcmeAccount> DeactivateAccount(Uri AccountLocation)
		{
			if (this.directory is null)
				await this.GetDirectory();

			AcmeResponse Response = await this.POST(AccountLocation, AccountLocation,
				new KeyValuePair<string, object>("status", "deactivated"));

			return new AcmeAccount(this, Response.Location, Response.Payload);
		}

		/// <summary>
		/// Generates a new key for the account. (Account keys are managed by the CSP.)
		/// </summary>
		/// <param name="AccountLocation">URL of the account resource.</param>
		public async Task<AcmeAccount> NewKey(Uri AccountLocation)
		{
			if (this.directory is null)
				await this.GetDirectory();
			RSA NewKey = RSA.Create();
			NewKey.KeySize = KeySize;

			if (NewKey.KeySize != KeySize)	// Happens when using library from traditioanl .NET FW
			{
				Type T = Runtime.Inventory.Types.GetType("System.Security.Cryptography.RSACryptoServiceProvider");
				if (T is null)
					throw new Exception("Unable to set RSA key size to anything but default (" + NewKey.KeySize.ToString() + " bits).");

				NewKey = Activator.CreateInstance(T, KeySize) as RSA;
			}

			RsaSsaPkcsSha256 Jws2 = new RsaSsaPkcsSha256(NewKey);

			try
			{
				Jws2.Sign(new KeyValuePair<string, object>[]
					{
						new KeyValuePair<string, object>("url", this.directory.KeyChange.ToString())
					}, new KeyValuePair<string, object>[]
					{
						new KeyValuePair<string, object>("account", AccountLocation.ToString()),
						new KeyValuePair<string, object>("oldkey", this.jws.PublicWebKey),
					}, out string Header, out string Payload, out string Signature);

				AcmeResponse Response = await this.POST(this.directory.KeyChange, AccountLocation,
					new KeyValuePair<string, object>("protected", Header),
					new KeyValuePair<string, object>("payload", Payload),
					new KeyValuePair<string, object>("signature", Signature));

				this.jwkThumbprint = null;
				this.jws.ImportKey(NewKey);

				return new AcmeAccount(this, Response.Location, Response.Payload);
			}
			finally
			{
				Jws2.Dispose();
			}
		}

		/// <summary>
		/// Orders certificate.
		/// </summary>
		/// <param name="AccountLocation">Account resource location.</param>
		/// <param name="Identifiers">Identifiers to include in the certificate.</param>
		/// <param name="NotBefore">If provided, certificate is not valid before this point in time.</param>
		/// <param name="NotAfter">If provided, certificate is not valid after this point in time.</param>
		/// <returns>ACME order object.</returns>
		public async Task<AcmeOrder> OrderCertificate(Uri AccountLocation, AcmeIdentifier[] Identifiers,
			DateTime? NotBefore, DateTime? NotAfter)
		{
			if (this.directory is null)
				await this.GetDirectory();

			int i, c = Identifiers.Length;
			IEnumerable<KeyValuePair<string, object>>[] Identifiers2 = new IEnumerable<KeyValuePair<string, object>>[c];

			for (i = 0; i < c; i++)
			{
				Identifiers2[i] = new KeyValuePair<string, object>[]
				{
					new KeyValuePair<string, object>("type", Identifiers[i].Type),
					new KeyValuePair<string, object>("value", Identifiers[i].Value)
				};
			}

			List<KeyValuePair<string, object>> Payload = new List<KeyValuePair<string, object>>()
			{
				new KeyValuePair<string, object>("identifiers", Identifiers2)
			};

			if (NotBefore.HasValue)
				Payload.Add(new KeyValuePair<string, object>("notBefore", NotBefore.Value));

			if (NotAfter.HasValue)
				Payload.Add(new KeyValuePair<string, object>("notAfter", NotAfter.Value));

			AcmeResponse Response = await this.POST(this.directory.NewOrder, AccountLocation,
				Payload.ToArray());

			return new AcmeOrder(this, AccountLocation, Response.Location, Response.Payload, Response.ResponseMessage);
		}

		/// <summary>
		/// Gets the state of an order.
		/// </summary>
		/// <param name="AccountLocation">URI of account.</param>
		/// <param name="OrderLocation">URI of order.</param>
		/// <returns>ACME order object.</returns>
		public async Task<AcmeOrder> GetOrder(Uri AccountLocation, Uri OrderLocation)
		{
			AcmeResponse Response = await this.POST_as_GET(OrderLocation, AccountLocation);
			return new AcmeOrder(this, AccountLocation, OrderLocation, Response.Payload, Response.ResponseMessage);
		}

		/// <summary>
		/// Gets the list of current orders for an account.
		/// </summary>
		/// <param name="AccountLocation">URI of account.</param>
		/// <param name="OrdersLocation">URI of orders.</param>
		/// <returns>ACME order object.</returns>
		public async Task<AcmeOrder[]> GetOrders(Uri AccountLocation, Uri OrdersLocation)
		{
			AcmeResponse _ = await this.GET(OrdersLocation);
			throw new NotImplementedException("Method not implemented.");
		}

		/// <summary>
		/// Gets the state of an authorization.
		/// </summary>
		/// <param name="AccountLocation">URI of account.</param>
		/// <param name="AuthorizationLocation">URI of authorization.</param>
		/// <returns>ACME authorization object.</returns>
		public async Task<AcmeAuthorization> GetAuthorization(Uri AccountLocation, Uri AuthorizationLocation)
		{
			AcmeResponse Response = await this.POST_as_GET(AuthorizationLocation, AccountLocation);
			return new AcmeAuthorization(this, AccountLocation, AuthorizationLocation, Response.Payload);
		}

		/// <summary>
		/// Deactivates an authorization.
		/// </summary>
		/// <param name="AccountLocation">Account location.</param>
		/// <param name="AuthorizationLocation">Authorization location</param>
		/// <returns>New authorization object.</returns>
		public async Task<AcmeAuthorization> DeactivateAuthorization(Uri AccountLocation, Uri AuthorizationLocation)
		{
			AcmeResponse Response = await this.POST(AuthorizationLocation, AccountLocation,
				new KeyValuePair<string, object>("status", "deactivated"));

			return new AcmeAuthorization(this, AccountLocation, Response.Location, Response.Payload);
		}

		/// <summary>
		/// Acknowledges a challenge from the server.
		/// </summary>
		/// <param name="AccountLocation">Account location.</param>
		/// <param name="ChallengeLocation">Challenge location.</param>
		/// <returns>Acknowledged challenge object.</returns>
		public async Task<AcmeChallenge> AcknowledgeChallenge(Uri AccountLocation, Uri ChallengeLocation)
		{
			AcmeResponse Response = await this.POST(ChallengeLocation, AccountLocation);
			return this.CreateChallenge(AccountLocation, Response.Payload);
		}

		internal AcmeChallenge CreateChallenge(Uri AccountLocation, IEnumerable<KeyValuePair<string, object>> Obj)
		{
			string Type = string.Empty;

			foreach (KeyValuePair<string, object> P2 in Obj)
			{
				if (P2.Key == "type" && P2.Value is string s)
				{
					Type = s;
					break;
				}
			}

			switch (Type)
			{
				case "http-01": return new AcmeHttpChallenge(this, AccountLocation, Obj);
				case "dns-01": return new AcmeDnsChallenge(this, AccountLocation, Obj);
				default: return new AcmeChallenge(this, AccountLocation, Obj);
			}
		}

		/// <summary>
		/// Returns the JWK thumbprint of the current JSon Web Key, as defined in RFC 7638
		/// https://tools.ietf.org/html/rfc7638
		/// </summary>
		internal string JwkThumbprint
		{
			get
			{
				if (this.jwkThumbprint is null)
				{
					SortedDictionary<string, object> Sorted = new SortedDictionary<string, object>();

					foreach (KeyValuePair<string, object> P in this.jws.PublicWebKey)
					{
						switch (P.Key)
						{
							case "kty":
							case "n":
							case "e":
								Sorted[P.Key] = P.Value;
								break;
						}
					}

					string Json = JSON.Encode(Sorted, null);
					byte[] Bin = Encoding.UTF8.GetBytes(Json);
					byte[] Hash = Hashes.ComputeSHA256Hash(Bin);

					this.jwkThumbprint = Base64Url.Encode(Hash);
				}

				return this.jwkThumbprint;
			}
		}

		/// <summary>
		/// Finalize order.
		/// </summary>
		/// <param name="AccountLocation">Account location.</param>
		/// <param name="FinalizeLocation">Finalize location.</param>
		/// <param name="CertificateRequest">Certificate request.</param>
		/// <returns>New order object.</returns>
		public async Task<AcmeOrder> FinalizeOrder(Uri AccountLocation, Uri FinalizeLocation, CertificateRequest CertificateRequest)
		{
			byte[] CSR = CertificateRequest.BuildCSR();
			AcmeResponse Response = await this.POST(FinalizeLocation, AccountLocation,
				new KeyValuePair<string, object>("csr", Base64Url.Encode(CSR)));

			return new AcmeOrder(this, AccountLocation, Response.Location, Response.Payload, Response.ResponseMessage);
		}

		/// <summary>
		/// Downloads a certificate.
		/// </summary>
		/// <param name="AccountLocation">Account location.</param>
		/// <param name="CertificateLocation">URI of certificate.</param>
		/// <returns>Certificate chain.</returns>
		public async Task<X509Certificate2[]> DownloadCertificate(Uri AccountLocation, Uri CertificateLocation)
		{
			string ContentType = PemDecoder.ContentType;
			HttpResponseMessage Response = await this.HttpPost(CertificateLocation, AccountLocation, ContentType, null);
			Response.EnsureSuccessStatusCode();

			byte[] Bin = await Response.Content.ReadAsByteArrayAsync();

			if (Response.Headers.TryGetValues("Content-Type", out IEnumerable<string> Values))
			{
				foreach (string s in Values)
				{
					ContentType = s;
					break;
				}
			}

			object Decoded = InternetContent.Decode(ContentType, Bin, CertificateLocation);
			if (!(Decoded is X509Certificate2[] Certificates))
				throw new Exception("Unexpected response returned. Content-Type: " + ContentType);

			return Certificates;
		}

		/// <summary>
		/// Exports the account key.
		/// </summary>
		/// <param name="IncludePrivateParameters">If private parameters should be included.</param>
		/// <returns>RSA parameters belonging to account.</returns>
		public RSAParameters ExportAccountKey(bool IncludePrivateParameters)
		{
			return this.jws.RSA.ExportParameters(IncludePrivateParameters);
		}

	}
}
