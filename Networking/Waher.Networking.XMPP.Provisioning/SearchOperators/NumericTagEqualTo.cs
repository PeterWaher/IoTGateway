namespace Waher.Networking.XMPP.Provisioning.SearchOperators
{
	/// <summary>
	/// Filters things with a named numeric-valued tag equal to a given value.
	/// </summary>
	public class NumericTagEqualTo : SearchOperatorNumeric
	{
		/// <summary>
		/// Filters things with a named numeric-valued tag equal to a given value.
		/// </summary>
		/// <param name="Name">Tag name.</param>
		/// <param name="Value">Tag value.</param>
		public NumericTagEqualTo(string Name, double Value)
			: this(Name, null, Value)
		{
		}

		/// <summary>
		/// Filters things with a named numeric-valued tag equal to a given value.
		/// </summary>
		/// <param name="Name">Tag name.</param>
		/// <param name="NameWildcard">Optional wildcard used in the name.</param>
		/// <param name="Value">Tag value.</param>
		public NumericTagEqualTo(string Name, string NameWildcard, double Value)
			: base(Name, NameWildcard, Value)
		{
		}

		internal override string TagName => "numEq";
	}
}
