namespace Waher.Networking.XMPP.Provisioning.SearchOperators
{
	/// <summary>
	/// Filters things with a named string-valued tag greater than a given value.
	/// </summary>
	public class StringTagGreaterThan : SearchOperatorString
	{
		/// <summary>
		/// Filters things with a named string-valued tag greater than a given value.
		/// </summary>
		/// <param name="Name">Tag name.</param>
		/// <param name="Value">Tag value.</param>
		public StringTagGreaterThan(string Name, string Value)
			: this(Name, null, Value)
		{
		}

		/// <summary>
		/// Filters things with a named string-valued tag greater than a given value.
		/// </summary>
		/// <param name="Name">Tag name.</param>
		/// <param name="NameWildcard">Optional wildcard used in the name.</param>
		/// <param name="Value">Tag value.</param>
		public StringTagGreaterThan(string Name, string NameWildcard, string Value)
			: base(Name, NameWildcard, Value)
		{
		}

		internal override string TagName => "strGt";
	}
}
