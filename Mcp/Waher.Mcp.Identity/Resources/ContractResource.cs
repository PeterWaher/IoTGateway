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
	/// Contains information about a smart contract.
	/// </summary>
	public class ContractResource : Resource
	{
		private readonly Contract contract;


		/// <summary>
		/// Contains information about a smart contract.
		/// </summary>
		/// <param name="Contract">Smart Contract object.</param>
		/// <param name="MetaData">Meta-data associated with resource.</param>
		public ContractResource(Contract Contract,
			params KeyValuePair<string, object>[] MetaData)
			: base(Contract.ContractId, XML.Encode(Contract.From, true), 
				  Contract.State.ToString() + " Smart Contract object with identifier " + 
				  Contract.ContractId, Contract.ContractIdUri, MetaData)
		{
			this.contract = Contract;
		}

		/// <summary>
		/// Smart Contract object.
		/// </summary>
		public Contract Contract => this.contract;

		/// <summary>
		/// Reads the resource.
		/// </summary>
		/// <param name="MetaData">Associated meta-data, if available.</param>
		/// <returns>Content objects read.</returns>
		public override Task<IResourceContent[]> Read(
			Dictionary<string, object>? MetaData)
		{
			Dictionary<string, object?>? Contents = this.contract.ToJson();
			string s = TOON.Encode(Contents, false);

			return Task.FromResult(new IResourceContent[]
			{
				new TextContent(this.Uri, s, ToonEncoder.DefaultContentType, MetaData)
			});
		}


	}
}
