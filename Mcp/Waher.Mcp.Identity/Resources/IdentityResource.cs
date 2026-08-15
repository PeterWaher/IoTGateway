using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content.Toon;
using Waher.Content.Xml;
using Waher.Networking.HTTP.Mcp.Model.Resources;
using Waher.Networking.HTTP.Mcp.Model.Server;
using Waher.Networking.XMPP.Contracts;

namespace Waher.Mcp.Identity.Resources
{
	/// <summary>
	/// Contains information about a digital identity.
	/// </summary>
	public class IdentityResource : Resource
	{
		private readonly LegalIdentity identity;


		/// <summary>
		/// Contains information about a digital identity.
		/// </summary>
		/// <param name="Identity">Legal Identity object.</param>
		/// <param name="MetaData">Meta-data associated with resource.</param>
		public IdentityResource(LegalIdentity Identity,
			params KeyValuePair<string, object>[] MetaData)
			: base(Identity.Id, XML.Encode(Identity.From, true), Identity.State.ToString() + 
				  " Digital Identity object with identifier " + Identity.Id,
				  Identity.IdUri, MetaData)
		{
			this.identity = Identity;
		}

		/// <summary>
		/// Legal Identity object.
		/// </summary>
		public LegalIdentity Identity => this.identity;

		/// <summary>
		/// Reads the resource.
		/// </summary>
		/// <param name="MetaData">Associated meta-data, if available.</param>
		/// <returns>Content objects read.</returns>
		public override Task<IResourceContent[]> Read(
			Dictionary<string, object>? MetaData)
		{
			Dictionary<string, object> Contents = new Dictionary<string, object>()
			{
				{ "Id", this.identity.Id },
				{ "Provider", this.identity.Provider },
				{ "State", this.identity.State },
				{ "Created", this.identity.Created },
				{ "Updated", this.identity.Updated },
				{ "From", this.identity.From },
				{ "To", this.identity.To },
				{ "Properties", this.identity.Properties },
				{ "Attachments", this.identity.Attachments },
				{ "ClientKeyName", this.identity.ClientKeyName },
				{ "ClientPubKey", this.identity.ClientPubKey },
				{ "ClientSignature", this.identity.ClientSignature }
			};

			string s = TOON.Encode(Contents, false);

			return Task.FromResult(new IResourceContent[]
			{
				new TextContent(this.Uri, s, ToonEncoder.DefaultContentType, MetaData)
			});
		}
	}
}
