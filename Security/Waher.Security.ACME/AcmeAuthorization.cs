using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Xml;

namespace Waher.Security.ACME
{
	/// <summary>
	/// ACME Authorization status enumeration
	/// </summary>
	public enum AcmeAuthorizationStatus
	{
		/// <summary>
		/// Authorization is pending
		/// </summary>
		pending,

		/// <summary>
		/// Authorization is valid
		/// </summary>
		valid,

		/// <summary>
		/// Authorization is invalid
		/// </summary>
		invalid,

		/// <summary>
		/// Authorization is deactivated
		/// </summary>
		deactivated,

		/// <summary>
		/// Authorization has expired
		/// </summary>
		expired,

		/// <summary>
		/// Authorization has been revoked
		/// </summary>
		revoked
	};

	/// <summary>
	/// Represents an ACME authorization.
	/// </summary>
	public class AcmeAuthorization : AcmeResource
	{
		private readonly AcmeAuthorizationStatus status;
		private readonly DateTime? expires = null;
		private readonly AcmeChallenge[] challenges = null;
		private readonly string type = null;
		private readonly string value = null;
		private readonly bool? wildcard = null;

		internal AcmeAuthorization(AcmeClient Client, Uri AccountLocation, Uri Location, IEnumerable<KeyValuePair<string, object>> Obj)
			: base(Client, AccountLocation, Location)
		{
			foreach (KeyValuePair<string, object> P in Obj)
			{
				switch (P.Key)
				{
					case "status":
						if (!Enum.TryParse<AcmeAuthorizationStatus>(P.Value as string, out this.status))
							throw new ArgumentException("Invalid ACME authorization status: " + P.Value.ToString(), "status");
						break;

					case "expires":
						if (XML.TryParse(P.Value as string, out DateTime TP))
							this.expires = TP;
						else
							throw new ArgumentException("Invalid date and time value.", "expires");
						break;

					case "identifier":
						if (P.Value is IEnumerable<KeyValuePair<string, object>> Obj2)
						{
							foreach (KeyValuePair<string, object> P2 in Obj2)
							{
								switch (P2.Key)
								{
									case "type":
										this.type = P2.Value as string;
										break;

									case "value":
										this.value = P2.Value as string;
										break;
								}
							}
						}
						break;

					case "challenges":
						if (P.Value is Array A2)
						{
							List<AcmeChallenge> Challenges = new List<AcmeChallenge>();

							foreach (object Obj3 in A2)
							{
								if (Obj3 is IEnumerable<KeyValuePair<string, object>> Obj4)
									Challenges.Add(this.Client.CreateChallenge(AccountLocation, Obj4));
							}

							this.challenges = Challenges.ToArray();
						}
						break;

					case "wildcard":
						if (CommonTypes.TryParse(P.Value as string, out bool b))
							this.wildcard = b;
						else
							throw new ArgumentException("Invalid boolean value.", "wildcard");
						break;
				}
			}
		}

		/// <summary>
		/// The type of identifier.
		/// </summary>
		public string Type => this.type;

		/// <summary>
		/// The identifier itself.
		/// </summary>
		public string Value => this.value;

		/// <summary>
		/// The status of this authorization.
		/// </summary>
		public AcmeAuthorizationStatus Status => this.status;

		/// <summary>
		/// The timestamp after which the server will consider this authorization invalid
		/// </summary>
		public DateTime? Expires => this.expires;

		/// <summary>
		/// For pending authorizations, the challenges that the client can fulfill in order to prove
		/// possession of the identifier.
		/// </summary>
		public AcmeChallenge[] Challenges => this.challenges;

		/// <summary>
		/// For authorizations created as a result of a newOrder request containing a DNS identifier with a value
		/// that contained a wildcard prefix this field MUST be present, and true.
		/// </summary>
		public bool? Wildcard => this.wildcard;

		/// <summary>
		/// Gets the current state of the order.
		/// </summary>
		/// <returns>Current state of the order.</returns>
		public Task<AcmeAuthorization> Poll()
		{
			return this.Client.GetAuthorization(this.AccountLocation, this.Location);
		}

		/// <summary>
		/// Deactivates the authorization.
		/// </summary>
		/// <returns>New authorization object.</returns>
		public Task<AcmeAuthorization> Deactivate()
		{
			return this.Client.DeactivateAuthorization(this.AccountLocation, this.Location);
		}

	}
}
