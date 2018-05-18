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
		private readonly string keyChange = null;
		private readonly string termsOfService = null;
		private readonly string website = null;
		private readonly string newAccount = null;
		private readonly string newNonce = null;
		private readonly string newOrder = null;
		private readonly string revokeCert = null;
		private readonly string newAuthz = null;
		private readonly bool externalAccountRequired = false;

		internal AcmeDirectory(AcmeClient Client, IEnumerable<KeyValuePair<string, object>> Obj)
			: base(Client)
		{
			foreach (KeyValuePair<string, object> P in Obj)
			{
				switch (P.Key)
				{
					case "keyChange":
						this.keyChange = P.Value as string;
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
										this.termsOfService = P2.Value as string;
										break;

									case "website":
										this.website = P2.Value as string;
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
						this.newAccount = P.Value as string;
						break;

					case "newNonce":
						this.newNonce = P.Value as string;
						break;

					case "newOrder":
						this.newOrder = P.Value as string;
						break;

					case "revokeCert":
						this.revokeCert = P.Value as string;
						break;

					case "newAuthz":
						this.newAuthz = P.Value as string;
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
		public string KeyChange => this.keyChange;

		/// <summary>
		/// URL to terms of service.
		/// </summary>
		public string TermsOfService => this.termsOfService;

		/// <summary>
		/// URL to website.
		/// </summary>
		public string Website => this.website;

		/// <summary>
		/// URL for newAccount method.
		/// </summary>
		public string NewAccount => this.newAccount;

		/// <summary>
		/// URL for newNonce method.
		/// </summary>
		public string NewNonce => this.newNonce;

		/// <summary>
		/// URL for newOrder method.
		/// </summary>
		public string NewOrder => this.newOrder;

		/// <summary>
		/// URL for revokeCert method.
		/// </summary>
		public string RevokeCert => this.revokeCert;

		/// <summary>
		/// URL for newAuthz method.
		/// </summary>
		public string NewAuthz => this.newAuthz;

		/// <summary>
		/// If an external account is required.
		/// </summary>
		public bool ExternalAccountRequired => this.externalAccountRequired;
	}
}
