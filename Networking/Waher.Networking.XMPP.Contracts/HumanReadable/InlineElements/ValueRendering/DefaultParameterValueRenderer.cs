using Waher.Runtime.Inventory;

namespace Waher.Networking.XMPP.Contracts.HumanReadable.InlineElements.ValueRendering
{
	/// <summary>
	/// Default conversion of a parameter value to a human-readable string.
	/// </summary>
	public class DefaultParameterValueRenderer : ParameterValueRenderer
	{
		/// <summary>
		/// Default conversion of a parameter value to a human-readable string.
		/// </summary>
		public DefaultParameterValueRenderer()
			: base()
		{
		}

		/// <summary>
		/// How well a parameter value is supported.
		/// </summary>
		/// <param name="Value">Parameter value.</param>
		/// <returns>How well values of this type are supported.</returns>
		public override Grade Supports(object Value)
		{
			return Grade.Barely;
		}
	}
}
