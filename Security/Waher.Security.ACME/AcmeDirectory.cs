using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Security.ACME
{
	/// <summary>
	/// Represents an ACME directory.
	/// </summary>
	public class AcmeDirectory : AcmeObject
	{
		private readonly string[] caaIdentities = null;
		private readonly Uri keyChange = null;
		private readonly Uri termsOfService = null;
		private readonly Uri website = null;
		private readonly Uri newAccount = null;
		private readonly Uri newNonce = null;
		private readonly Uri newOrder = null;
		private readonly Uri revokeCert = null;
		private readonly Uri newAuthz = null;
		private readonly bool externalAccountRequired = false;

		internal AcmeDirectory(AcmeClient Client, IEnumerable<KeyValuePair<string, object>> Obj)
			: base(Client)
		{
			foreach (KeyValuePair<string, object> P in Obj)
			{
				switch (P.Key)
				{
					case "keyChange":
						this.keyChange = new Uri(P.Value as string);
						break;

					case "meta":
						if (P.Value is IEnumerable<KeyValuePair<string, object>> Obj2)
						{
							foreach (KeyValuePair<string, object> P2 in Obj2)
							{
								switch (P2.Key)
								{
									case "caaIdentities":
										if (P2.Value is Array A)
										{
											List<string> CaaIdentities = new List<string>();

											foreach (object Obj3 in A)
											{
												if (Obj3 is string s)
													CaaIdentities.Add(s);
											}

											this.caaIdentities = CaaIdentities.ToArray();
										}
										break;

									case "termsOfService":
										this.termsOfService = new Uri(P2.Value as string);
										break;

									case "website":
										this.website = new Uri(P2.Value as string);
										break;

									case "externalAccountRequired":
										if (P2.Value is bool b)
											this.externalAccountRequired = b;
										break;
								}
							}
						}
						break;

					case "newAccount":
						this.newAccount = new Uri(P.Value as string);
						break;

					case "newNonce":
						this.newNonce = new Uri(P.Value as string);
						break;

					case "newOrder":
						this.newOrder = new Uri(P.Value as string);
						break;

					case "revokeCert":
						this.revokeCert = new Uri(P.Value as string);
						break;

					case "newAuthz":
						this.newAuthz = new Uri(P.Value as string);
						break;
				}
			}
		}

		/// <summary>
		/// CAA Identities.
		/// </summary>
		public string[] CaaIdentities => this.caaIdentities;

		/// <summary>
		/// URL for keyChange method.
		/// </summary>
		public Uri KeyChange => this.keyChange;

		/// <summary>
		/// URL to terms of service.
		/// </summary>
		public Uri TermsOfService => this.termsOfService;

		/// <summary>
		/// URL to website.
		/// </summary>
		public Uri Website => this.website;

		/// <summary>
		/// URL for newAccount method.
		/// </summary>
		public Uri NewAccount => this.newAccount;

		/// <summary>
		/// URL for newNonce method.
		/// </summary>
		public Uri NewNonce => this.newNonce;

		/// <summary>
		/// URL for newOrder method.
		/// </summary>
		public Uri NewOrder => this.newOrder;

		/// <summary>
		/// URL for revokeCert method.
		/// </summary>
		public Uri RevokeCert => this.revokeCert;

		/// <summary>
		/// URL for newAuthz method.
		/// </summary>
		public Uri NewAuthz => this.newAuthz;

		/// <summary>
		/// If an external account is required.
		/// </summary>
		public bool ExternalAccountRequired => this.externalAccountRequired;
	}
}
