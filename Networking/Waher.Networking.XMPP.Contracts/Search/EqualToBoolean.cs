namespace Waher.Networking.XMPP.Contracts.Search
{
	/// <summary>
	/// Limits searches to items with values equal to this value.
	/// </summary>
	public class EqualToBoolean : SearchFilterBooleanOperand
	{
		/// <summary>
		/// Limits searches to items with values equal to this value.
		/// </summary>
		/// <param name="Value">Boolean value</param>
		public EqualToBoolean(bool Value)
			: base(Value)
		{
		}

		/// <summary>
		/// Local XML element name of double operand.
		/// </summary>
		public override string OperandName => "eq";
	}
}
