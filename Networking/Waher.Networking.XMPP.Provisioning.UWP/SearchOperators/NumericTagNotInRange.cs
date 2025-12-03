namespace Waher.Networking.XMPP.Provisioning.SearchOperators
{
	/// <summary>
	/// Filters things with a named numeric-valued tag outside a given range.
	/// </summary>
	public class NumericTagNotInRange : NumericTagRange 
	{
		/// <summary>
		/// Filters things with a named numeric-valued tag outside a given range.
		/// </summary>
		/// <param name="Name">Tag name.</param>
		/// <param name="Min">Minimum value.</param>
		/// <param name="MinIncluded">If the minimum value is included in the range.</param>
		/// <param name="Max">Maximum value.</param>
		/// <param name="MaxIncluded">If the maximum value is included in the range.</param>
		public NumericTagNotInRange(string Name, double Min, bool MinIncluded, double Max, bool MaxIncluded)
			: this(Name, null, Min, MinIncluded, Max, MaxIncluded)
		{
		}

		/// <summary>
		/// Filters things with a named numeric-valued tag outside a given range.
		/// </summary>
		/// <param name="Name">Tag name.</param>
		/// <param name="NameWildcard">Optional wildcard used in the name.</param>
		/// <param name="Min">Minimum value.</param>
		/// <param name="MinIncluded">If the minimum value is included in the range.</param>
		/// <param name="Max">Maximum value.</param>
		/// <param name="MaxIncluded">If the maximum value is included in the range.</param>
		public NumericTagNotInRange(string Name, string NameWildcard, double Min, 
			bool MinIncluded, double Max, bool MaxIncluded)
			: base(Name, NameWildcard, Min, MinIncluded, Max, MaxIncluded)
		{
		}

		internal override string TagName => "numNRange";
	}
}
