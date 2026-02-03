using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Xml.Attributes;
using Waher.Networking.HTTP.Interfaces;

namespace Waher.Security.WAF.Model.Actions
{
	/// <summary>
	/// Blocks the endpoint.
	/// </summary>
	public class BlockEndpoint : WafAction
	{
		private readonly StringAttribute reason;

		/// <summary>
		/// Blocks the endpoint.
		/// </summary>
		public BlockEndpoint()
			: base()
		{
		}

		/// <summary>
		/// Blocks the endpoint.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		public BlockEndpoint(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document)
			: base(Xml, Parent, Document)
		{
			this.reason = new StringAttribute(Xml, "reason");
		}

		/// <summary>
		/// XML Local Name for the XML element defining the action.
		/// </summary>
		public override string LocalName => nameof(BlockEndpoint);

		/// <summary>
		/// Creates a WAF action from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		/// <returns>Created action object.</returns>
		public override WafAction Create(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document) => new BlockEndpoint(Xml, Parent, Document);

		/// <summary>
		/// Reviews the processing state, and returns a WAF result, if any.
		/// </summary>
		/// <param name="State">Current state.</param>
		/// <returns>Result to return, if any.</returns>
		public override async Task<WafResult?> Review(ProcessingState State)
		{
			string Reason = await this.reason.EvaluateAsync(State.Variables, string.Empty);

			await State.Firewall.LoginAuditor.BlockEndpoint(State.Request.RemoteEndPoint, "HTTP", Reason);

			return null;
		}
	}
}
