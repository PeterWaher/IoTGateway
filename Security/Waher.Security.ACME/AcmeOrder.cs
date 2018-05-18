using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content;
using Waher.Content.Xml;

namespace Waher.Security.ACME
{
	/// <summary>
	/// ACME Order status enumeration
	/// </summary>
	public enum AcmeOrderStatus
	{
		/// <summary>
		/// Order is pending
		/// </summary>
		pending,

		/// <summary>
		/// Order is ready
		/// </summary>
		ready,

		/// <summary>
		/// Order is being processed
		/// </summary>
		processing,

		/// <summary>
		/// Order is valid
		/// </summary>
		valid,

		/// <summary>
		/// Order is invalid
		/// </summary>
		invalid
	};

	/// <summary>
	/// Represents an ACME order.
	/// </summary>
	public class AcmeOrder : AcmeObject
	{
		private readonly AcmeOrderStatus status;
		private readonly DateTime? expires = null;
		private readonly DateTime? notBefore = null;
		private readonly DateTime? notAfter = null;
		private readonly KeyValuePair<string, string>[] identifiers = null;
		private readonly Uri[] authorizations = null;
		private readonly object error = null; // TODO: Problem document
		private readonly Uri finalize = null;
		private readonly Uri certificate = null;

		internal AcmeOrder(AcmeClient Client, IEnumerable<KeyValuePair<string, object>> Obj)
			: base(Client)
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
							List<KeyValuePair<string, string>> Identifiers = new List<KeyValuePair<string, string>>();

							foreach (object Obj2 in A)
							{
								if (Obj2 is IEnumerable<KeyValuePair<string, object>> Obj3)
								{
									string Type = null;
									string Value = null;

									foreach (KeyValuePair<string, object> P2 in Obj3)
									{
										switch (P2.Key)
										{
											case "type":
												Type = P2.Key;
												break;

											case "value":
												Value = P2.Value as string;
												break;
										}
									}

									Identifiers.Add(new KeyValuePair<string, string>(Type, Value));
								}
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

							this.authorizations = Authorizations.ToArray();
						}
						break;

					case "finalize":
						this.finalize = new Uri(P.Value as string);
						break;

					case "certificate":
						this.certificate = new Uri(P.Value as string);
						break;
				}
			}
		}

		/// <summary>
		/// An array of identifier objects that the order pertains to.
		/// </summary>
		public KeyValuePair<string, string>[] Identifiers => this.identifiers;

		/// <summary>
		/// For pending orders, the authorizations that the client needs to complete before the
		/// requested certificate can be issued.
		/// </summary>
		public Uri[] Authorizations => this.authorizations;

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
	}
}
