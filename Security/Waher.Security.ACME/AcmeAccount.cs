using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content;

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
	public class AcmeAccount : AcmeObject
	{
		private readonly AcmeAccountStatus status;
		private readonly string[] contact = null;
		private readonly string orders;
		private readonly bool? termsOfServiceAgreed = null;

		internal AcmeAccount(AcmeClient Client, IEnumerable<KeyValuePair<string, object>> Obj)
			: base(Client)
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
						this.orders = P.Value as string;
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
		public string Orders => this.orders;
	}
}
