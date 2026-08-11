using Waher.Networking.XMPP.Contracts;

namespace Waher.Mcp.Identity.Responses
{
	/// <summary>
	/// Identity property.
	/// </summary>
	public class IdentityProperty
	{
		/// <summary>
		/// Identity property.
		/// </summary>
		/// <param name="Property">Identity property</param>
		public IdentityProperty(Property Property)
		{
			this.Name = Property.Name;
			this.Value = Property.Value;
		}

		/// <summary>
		/// Name of property.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Value of property.
		/// </summary>
		public string Value { get; set; }
	}
}
