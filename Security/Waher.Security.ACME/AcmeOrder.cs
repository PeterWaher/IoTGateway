using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Security.PKCS;

namespace Waher.Security.ACME
{
	/// <summary>
	/// ACME Order status enumeration
	/// </summary>
	public enum AcmeOrderStatus
	{
		/// <summary>
		/// The server does not believe that the client has fulfilled the requirements.  
		/// Check the "authorizations" array for entries that are still pending.
		/// </summary>
		pending,

		/// <summary>
		/// The server agrees that the requirements have been fulfilled, and is awaiting finalization.  
		/// Submit a finalization request.
		/// </summary>
		ready,

		/// <summary>
		/// The certificate is being issued. Send a GET request after the time given in the 
		/// "Retry-After" header field of the response, if any.
		/// </summary>
		processing,

		/// <summary>
		/// The server has issued the certificate and provisioned its URL to the "certificate" field 
		/// of the order.  Download the certificate.
		/// </summary>
		valid,

		/// <summary>
		/// The certificate will not be issued.  Consider this order process abandoned.
		/// </summary>
		invalid
	};

	/// <summary>
	/// Represents an ACME order.
	/// </summary>
	public class AcmeOrder : AcmeResource
	{
		private readonly AcmeOrderStatus status;
		private readonly DateTime? expires = null;
		private readonly DateTime? notBefore = null;
		private readonly DateTime? notAfter = null;
		private readonly AcmeIdentifier[] identifiers = null;
		private AcmeAuthorization[] authorizations = null;
		private readonly Uri[] authorizationUris = null;
		private readonly AcmeException error = null; // TODO: Problem document
		private readonly Uri finalize = null;
		private readonly Uri certificate = null;

		internal AcmeOrder(AcmeClient Client, Uri AccountLocation, Uri Location,
			IEnumerable<KeyValuePair<string, object>> Obj, HttpResponseMessage Response)
			: base(Client, AccountLocation, Location)
		{
			foreach (KeyValuePair<string, object> P in Obj)
			{
				switch (P.Key)
				{
					case "status":
						if (!Enum.TryParse<AcmeOrderStatus>(P.Value as string, out this.status))
							throw new ArgumentException("Invalid ACME order status: " + P.Value.ToString(), "status");
						break;

					case "expires":
						if (XML.TryParse(P.Value as string, out DateTime TP))
							this.expires = TP;
						else
							throw new ArgumentException("Invalid date and time value.", "expires");
						break;

					case "notBefore":
						if (XML.TryParse(P.Value as string, out TP))
							this.notBefore = TP;
						else
							throw new ArgumentException("Invalid date and time value.", "notBefore");
						break;

					case "notAfter":
						if (XML.TryParse(P.Value as string, out TP))
							this.notAfter = TP;
						else
							throw new ArgumentException("Invalid date and time value.", "notAfter");
						break;

					case "identifiers":
						if (P.Value is Array A)
						{
							List<AcmeIdentifier> Identifiers = new List<AcmeIdentifier>();

							foreach (object Obj2 in A)
							{
								if (Obj2 is IEnumerable<KeyValuePair<string, object>> Obj3)
									Identifiers.Add(new AcmeIdentifier(Client, Obj3));
							}

							this.identifiers = Identifiers.ToArray();
						}
						break;

					case "authorizations":
						if (P.Value is Array A2)
						{
							List<Uri> Authorizations = new List<Uri>();

							foreach (object Obj2 in A2)
							{
								if (Obj2 is string s)
									Authorizations.Add(new Uri(s));
							}

							this.authorizationUris = Authorizations.ToArray();
						}
						break;

					case "finalize":
						this.finalize = new Uri(P.Value as string);
						break;

					case "certificate":
						this.certificate = new Uri(P.Value as string);
						break;

					case "error":
						if (P.Value is IEnumerable<KeyValuePair<string, object>> ErrorObj)
							this.error = AcmeClient.CreateException(ErrorObj, Response);
						break;
				}
			}
		}

		/// <summary>
		/// An array of identifier objects that the order pertains to.
		/// </summary>
		public AcmeIdentifier[] Identifiers => this.identifiers;

		/// <summary>
		/// For pending orders, the authorizations that the client needs to complete before the
		/// requested certificate can be issued.
		/// </summary>
		public Uri[] AuthorizationUris => this.authorizationUris;

		/// <summary>
		/// The status of this order.
		/// </summary>
		public AcmeOrderStatus Status => this.status;

		/// <summary>
		/// The timestamp after which the server will consider this order invalid
		/// </summary>
		public DateTime? Expires => this.expires;

		/// <summary>
		/// The requested value of the notBefore field in the certificate
		/// </summary>
		public DateTime? NotBefore => this.notBefore;

		/// <summary>
		/// The requested value of the notAfter field in the certificate
		/// </summary>
		public DateTime? NotAfter => this.notAfter;

		/// <summary>
		/// A URL that a CSR must be POSTed to once all of the order's authorizations are satisfied to finalize the order.
		/// </summary>
		public Uri Finalize => this.finalize;

		/// <summary>
		/// A URL for the certificate that has been issued in response to this order.
		/// </summary>
		public Uri Certificate => this.certificate;

		/// <summary>
		/// Any error, if available.
		/// </summary>
		public AcmeException Error => this.error;

		/// <summary>
		/// Gets the current state of the order.
		/// </summary>
		/// <returns>Current state of the order.</returns>
		public Task<AcmeOrder> Poll()
		{
			return this.Client.GetOrder(this.AccountLocation, this.Location);
		}

		/// <summary>
		/// Gets current authorization objects.
		/// </summary>
		/// <returns>Arary of authorization objects.</returns>
		public async Task<AcmeAuthorization[]> GetAuthorizations()
		{
			if (this.authorizations == null)
			{
				int i, c = this.authorizationUris.Length;
				AcmeAuthorization[] Result = new AcmeAuthorization[c];

				for (i = 0; i < c; i++)
				{
					Uri Location = this.authorizationUris[i];
					Result[i] = new AcmeAuthorization(this.Client, this.AccountLocation, Location, 
						(await this.Client.GET(Location)).Payload);
				}

				this.authorizations = Result;
			}

			return this.authorizations;
		}

		/// <summary>
		/// Curernt authorization objects.
		/// </summary>
		public Task<AcmeAuthorization[]> Authorizations
		{
			get { return this.GetAuthorizations(); }
		}

		/// <summary>
		/// Finalize order.
		/// </summary>
		/// <param name="CertificateRequest">Certificate request.</param>
		/// <returns>New order object.</returns>
		public Task<AcmeOrder> FinalizeOrder(CertificateRequest CertificateRequest)
		{
			return this.Client.FinalizeOrder(this.AccountLocation, this.finalize, CertificateRequest);
		}

		/// <summary>
		/// Downloads the certificate.
		/// </summary>
		/// <returns>Certificate chain.</returns>
		public Task<X509Certificate2[]> DownloadCertificate()
		{
			if (this.certificate == null)
				throw new Exception("No certificate URI available.");

			return this.Client.DownloadCertificate(this.certificate);
		}
	}
}
