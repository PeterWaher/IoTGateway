namespace Waher.Networking.XMPP.Provisioning.SearchOperators
{
	/// <summary>
	/// Filters things with a named string-valued tag lesser than a given value.
	/// </summary>
	public class StringTagLesserThan : SearchOperatorString
	{
		/// <summary>
		/// Filters things with a named string-valued tag lesser than a given value.
		/// </summary>
		/// <param name="Name">Tag name.</param>
		/// <param name="Value">Tag value.</param>
		public StringTagLesserThan(string Name, string Value)
			: this(Name, null, Value)
		{
		}

		/// <summary>
		/// Filters things with a named string-valued tag lesser than a given value.
		/// </summary>
		/// <param name="Name">Tag name.</param>
		/// <param name="NameWildcard">Optional wildcard used in the name.</param>
		/// <param name="Value">Tag value.</param>
		public StringTagLesserThan(string Name, string NameWildcard, string Value)
			: base(Name, NameWildcard, Value)
		{
		}

		internal override string TagName => "strLt";
	}
}
