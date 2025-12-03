namespace Waher.Networking.XMPP.Provisioning.SearchOperators
{
	/// <summary>
	/// Filters things with a named numeric-valued tag lesser than a given value.
	/// </summary>
	public class NumericTagLesserThan : SearchOperatorNumeric
	{
		/// <summary>
		/// Filters things with a named numeric-valued tag lesser than a given value.
		/// </summary>
		/// <param name="Name">Tag name.</param>
		/// <param name="Value">Tag value.</param>
		public NumericTagLesserThan(string Name, double Value)
			: this(Name, null, Value)
		{
		}

		/// <summary>
		/// Filters things with a named numeric-valued tag lesser than a given value.
		/// </summary>
		/// <param name="Name">Tag name.</param>
		/// <param name="NameWildcard">Optional wildcard used in the name.</param>
		/// <param name="Value">Tag value.</param>
		public NumericTagLesserThan(string Name, string NameWildcard, double Value)
			: base(Name, NameWildcard, Value)
		{
		}

		internal override string TagName => "numLt";
	}
}
