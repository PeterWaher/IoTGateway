using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content;
using Waher.Content.Xml;

namespace Waher.Security.ACME
{
	/// <summary>
	/// ACME Challenge status enumeration
	/// </summary>
	public enum AcmeChallengeStatus
	{
		/// <summary>
		/// Challenge created
		/// </summary>
		pending,

		/// <summary>
		/// Challenge being processed
		/// </summary>
		processing,

		/// <summary>
		/// Challenge valid
		/// </summary>
		valid,

		/// <summary>
		/// Challenge invalid
		/// </summary>
		invalid
	}

	/// <summary>
	/// Abstract base class of an ACME challenge.
	/// </summary>
	public /*abstract*/ class AcmeChallenge : AcmeObject
	{
		private readonly AcmeChallengeStatus status;
		private readonly Uri url;
		private readonly string type;
		private readonly string token;
		private readonly DateTime? validated;

		internal AcmeChallenge(AcmeClient Client, IEnumerable<KeyValuePair<string, object>> Obj)
			: base(Client)
		{
			foreach (KeyValuePair<string, object> P in Obj)
			{
				switch (P.Key)
				{
					case "status":
						if (!Enum.TryParse<AcmeChallengeStatus>(P.Value as string, out this.status))
							throw new ArgumentException("Invalid ACME challenge status: " + P.Value.ToString(), "status");
						break;

					case "validated":
						if (XML.TryParse(P.Value as string, out DateTime TP))
							this.validated = TP;
						else
							throw new ArgumentException("Invalid date and time value.", "validated");
						break;

					case "url":
						this.url = new Uri(P.Value as string);
						break;

					case "type":
						this.type = P.Value as string;
						break;

					case "token":
						this.token = P.Value as string;
						break;
				}
			}
		}

		/// <summary>
		/// The status of this challenge.
		/// </summary>
		public AcmeChallengeStatus Status => this.status;

		/// <summary>
		/// When the challenge was validated.
		/// </summary>
		public DateTime? Validated => this.validated;

		/// <summary>
		/// Type of challenge.
		/// </summary>
		public string Type => this.type;

		/// <summary>
		/// URL of challenge.
		/// </summary>
		public Uri Url => this.url;

		/// <summary>
		/// Token
		/// </summary>
		public string Token => this.token;
	}
}
