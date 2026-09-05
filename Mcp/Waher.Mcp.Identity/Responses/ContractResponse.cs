using System.Collections.Generic;
using Waher.Mcp.Xmpp.Responses;
using Waher.Networking.HTTP.Mcp.Model.Attributes;

namespace Waher.Mcp.Identity.Responses
{
	/// <summary>
	/// Contract response.
	/// </summary>
	public class ContractResponse : GenericResponse
	{
		/// <summary>
		/// Contract response.
		/// </summary>
		/// <param name="ErrorMessage">Error message.</param>
		public ContractResponse(string ErrorMessage)
			: base(false, ErrorMessage)
		{
		}

		/// <summary>
		/// Contract response.
		/// </summary>
		/// <param name="Contract">Current state of smart contract.</param>
		/// <param name="Message">Success message.</param>
		public ContractResponse(Dictionary<string, object?>? Contract, string Message)
			: base(true, Message)
		{
			this.Contract = Contract;
		}

		/// <summary>
		/// Contract
		/// </summary>
		[McpParameter("Contract", "Current state of smart contract.")]
		public Dictionary<string, object?>? Contract { get; }
	}
}
