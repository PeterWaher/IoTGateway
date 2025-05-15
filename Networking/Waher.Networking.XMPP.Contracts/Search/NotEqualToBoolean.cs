namespace Waher.Networking.XMPP.Contracts.Search
{
	/// <summary>
	/// Limits searches to items with values not equal to this value.
	/// </summary>
	public class NotEqualToBoolean : SearchFilterBooleanOperand
	{
		/// <summary>
		/// Limits searches to items with values not equal to this value.
		/// </summary>
		/// <param name="Value">Boolean value</param>
		public NotEqualToBoolean(bool Value)
			: base(Value)
		{
		}

		/// <summary>
		/// Local XML element name of string operand.
		/// </summary>
		public override string OperandName => "neq";
	}
}
