using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Security.JWS;

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

		/// <summary>
		/// Implements an ACME client for the generation of certificates using ACME-compliant certificate servers.
		/// </summary>
		/// <param name="DirectoryEndpoint">HTTP endpoint for the ACME directory resource.</param>
		public AcmeClient(string DirectoryEndpoint)
			: this(new Uri(DirectoryEndpoint))
		{
		}

		/// <summary>
		/// Implements an ACME client for the generation of certificates using ACME-compliant certificate servers.
		/// </summary>
		/// <param name="DirectoryEndpoint">HTTP endpoint for the ACME directory resource.</param>
		public AcmeClient(Uri DirectoryEndpoint)
		{
			this.directoryEndpoint = DirectoryEndpoint;
			this.jws = new RsaSsaPkcsSha256(KeySize, DirectoryEndpoint.ToString());

			this.httpClient = new HttpClient(new HttpClientHandler()
			{
				AllowAutoRedirect = true,
				AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
				CheckCertificateRevocationList = true,
				SslProtocols = SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12
			}, true);

			this.httpClient.DefaultRequestHeaders.Add("User-Agent", typeof(AcmeClient).Namespace);
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
			if (this.directory == null)
				this.directory = new AcmeDirectory(this, await this.GET(this.directoryEndpoint));

			return this.directory;
		}

		internal async Task<IEnumerable<KeyValuePair<string, object>>> GET(Uri URL)
		{
			HttpResponseMessage Response = await this.httpClient.GetAsync(URL);

			Stream Stream = await Response.Content.ReadAsStreamAsync();
			byte[] Bin = await Response.Content.ReadAsByteArrayAsync();
			string CharSet = Response.Content.Headers.ContentType?.CharSet;
			Encoding Encoding;

			if (string.IsNullOrEmpty(CharSet))
				Encoding = Encoding.UTF8;
			else
				Encoding = System.Text.Encoding.GetEncoding(CharSet);

			string JsonResponse = Encoding.GetString(Bin);

			if (!(JSON.Parse(JsonResponse) is IEnumerable<KeyValuePair<string, object>> Obj))
				throw new Exception("Unexpected response returned.");

			if (Response.IsSuccessStatusCode)
				return Obj;
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

			if (this.directory == null)
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
				if (this.directory == null)
					return this.GetDirectory();
				else
					return Task.FromResult<AcmeDirectory>(this.directory);
			}
		}

		internal class AcmeResponse
		{
			public IEnumerable<KeyValuePair<string, object>> Payload;
			public Uri Location;
			public string Json;
		}

		internal async Task<AcmeResponse> POST(Uri URL, Uri KeyID, params KeyValuePair<string, object>[] Payload)
		{
			string HeaderString;
			string PayloadString;
			string Signature;

			if (KeyID == null)
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
			HttpResponseMessage Response = await this.httpClient.PostAsync(URL, Content);

			this.GetNextNonce(Response);

			Stream Stream = await Response.Content.ReadAsStreamAsync();
			byte[] Bin = await Response.Content.ReadAsByteArrayAsync();
			string CharSet = Response.Content.Headers.ContentType?.CharSet;
			Encoding Encoding;

			if (string.IsNullOrEmpty(CharSet))
				Encoding = Encoding.UTF8;
			else
				Encoding = System.Text.Encoding.GetEncoding(CharSet);

			AcmeResponse AcmeResponse = new AcmeResponse()
			{
				Json = Encoding.GetString(Bin),
				Location = URL,
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
			else if ((AcmeResponse.Payload = JSON.Parse(AcmeResponse.Json) as IEnumerable<KeyValuePair<string, object>>) == null)
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
			if (this.directory == null)
				await this.GetDirectory();

			AcmeResponse Response = await this.POST(this.directory.NewAccount, null,
				new KeyValuePair<string, object>("termsOfServiceAgreed", TermsOfServiceAgreed),
				new KeyValuePair<string, object>("contact", ContactURLs));

			AcmeAccount Account;

			if (Response.Payload == null)
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
			if (this.directory == null)
				await this.GetDirectory();

			AcmeResponse Response = await this.POST(this.directory.NewAccount, null,
				new KeyValuePair<string, object>("onlyReturnExisting", true));

			if (Response.Payload == null)
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
			if (this.directory == null)
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
			if (this.directory == null)
				await this.GetDirectory();

			AcmeResponse Response = await this.POST(AccountLocation, AccountLocation,
				new KeyValuePair<string, object>("status", "deactivated"));

			this.jws.DeleteRsaKeyFromCsp();
			this.jws = null;
			this.jws = new RsaSsaPkcsSha256(KeySize, this.directoryEndpoint.ToString());

			return new AcmeAccount(this, Response.Location, Response.Payload);
		}

		/// <summary>
		/// Generates a new key for the account. (Account keys are managed by the CSP.)
		/// </summary>
		/// <param name="AccountLocation">URL of the account resource.</param>
		public async Task NewKey(Uri AccountLocation)
		{
			if (this.directory == null)
				await this.GetDirectory();

			RSACryptoServiceProvider NewKey = new RSACryptoServiceProvider(KeySize);
			RsaSsaPkcsSha256 Jws2 = new RsaSsaPkcsSha256(NewKey);

			try
			{
				Jws2.Sign(new KeyValuePair<string, object>[]
					{
						new KeyValuePair<string, object>("url", this.directory.KeyChange.ToString())
					}, new KeyValuePair<string, object>[]
					{
						new KeyValuePair<string, object>("account", AccountLocation.ToString()),
						new KeyValuePair<string, object>("newkey", Jws2.PublicWebKey)
					}, out string Header, out string Payload, out string Signature);

				await this.POST(this.directory.KeyChange, AccountLocation,
					new KeyValuePair<string, object>("protected", Header),
					new KeyValuePair<string, object>("payload", Payload),
					new KeyValuePair<string, object>("signature", Signature));

				this.jws.ImportKey(NewKey);
			}
			finally
			{
				Jws2.Dispose();
			}
		}

	}
}
