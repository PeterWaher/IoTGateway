using System.Xml;

namespace Waher.Security.WAF.Model.Actions
{
	/// <summary>
	/// Blocks the endpoint.
	/// </summary>
	public class BlockEndpoint : WafAction
	{
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
		public BlockEndpoint(XmlElement Xml)
			: base(Xml)
		{
		}

		/// <summary>
		/// XML Local Name for the XML element defining the action.
		/// </summary>
		public override string LocalName => nameof(BlockEndpoint);

		/// <summary>
		/// Creates a WAF action from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <returns>Created action object.</returns>
		public override WafAction Create(XmlElement Xml) => new BlockEndpoint(Xml);
	}
}
