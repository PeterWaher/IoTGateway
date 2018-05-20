using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Xml;

namespace Waher.Security.ACME
{
	/// <summary>
	/// ACME Account status enumeration
	/// </summary>
	public enum AcmeAccountStatus
	{
		/// <summary>
		/// Account is valid
		/// </summary>
		valid,

		/// <summary>
		/// Client has deactivated account
		/// </summary>
		deactivated,

		/// <summary>
		/// Server has deactivated account
		/// </summary>
		revoked
	};

	/// <summary>
	/// Represents an ACME account.
	/// </summary>
	public class AcmeAccount : AcmeResource
	{
		private readonly AcmeAccountStatus status;
		private readonly string[] contact = null;
		private readonly string initialIp = null;
		private readonly Uri orders;
		private readonly DateTime? createdAt = null;
		private readonly bool? termsOfServiceAgreed = null;

		internal AcmeAccount(AcmeClient Client, Uri Location, IEnumerable<KeyValuePair<string, object>> Obj)
			: base(Client, Location)
		{
			foreach (KeyValuePair<string, object> P in Obj)
			{
				switch (P.Key)
				{
					case "status":
						if (!Enum.TryParse<AcmeAccountStatus>(P.Value as string, out this.status))
							throw new ArgumentException("Invalid ACME account status: " + P.Value.ToString(), "status");
						break;

					case "contact":
						if (P.Value is Array A)
						{
							List<string> Contact = new List<string>();

							foreach (object Obj2 in A)
							{
								if (Obj2 is string s)
									Contact.Add(s);
							}

							this.contact = Contact.ToArray();
						}
						break;

					case "termsOfServiceAgreed":
						if (CommonTypes.TryParse(P.Value as string, out bool b))
							this.termsOfServiceAgreed = b;
						else
							throw new ArgumentException("Invalid boolean value.", "termsOfServiceAgreed");
						break;

					case "orders":
						this.orders = new Uri(P.Value as string);
						break;

					case "initialIp":
						this.initialIp = P.Value as string;
						break;

					case "createdAt":
						if (XML.TryParse(P.Value as string, out DateTime TP))
							this.createdAt = TP;
						else
							throw new ArgumentException("Invalid date and time value.", "createdAt");
						break;
				}
			}
		}

		/// <summary>
		/// Optional array of URLs that the server can use to contact the client for issues related 
		/// to this account.
		/// </summary>
		public string[] Contact => this.contact;

		/// <summary>
		/// The status of this account.
		/// </summary>
		public AcmeAccountStatus Status => this.status;

		/// <summary>
		/// Including this field in a new-account request, with a value of true, indicates the client's
		/// agreement with the terms of service.This field is not updateable by the client.
		/// </summary>
		public bool? TermsOfServiceAgreed => this.termsOfServiceAgreed;

		/// <summary>
		/// A URL from which a list of orders submitted by this account can be fetched via a GET request
		/// </summary>
		public Uri Orders => this.orders;

		/// <summary>
		/// Initial IP address.
		/// </summary>
		public string InitialIp => this.initialIp;

		/// <summary>
		/// Date and time of creation, if available.
		/// </summary>
		public DateTime? CreatedAt => this.createdAt;

		/// <summary>
		/// Updates the account.
		/// </summary>
		/// <param name="Contact">New contact information.</param>
		/// <returns>New account object.</returns>
		public Task<AcmeAccount> Update(string[] Contact)
		{
			return this.Client.UpdateAccount(this.Location, Contact);
		}

		/// <summary>
		/// Deactivates the account.
		/// </summary>
		/// <returns>New account object.</returns>
		public Task<AcmeAccount> Deactivate()
		{
			return this.Client.DeactivateAccount(this.Location);
		}

		/// <summary>
		/// Creates a new key for the account.
		/// </summary>
		/// <returns>New account key.</returns>
		public Task NewKey()
		{
			return this.Client.NewKey(this.Location);
		}

	}
}
