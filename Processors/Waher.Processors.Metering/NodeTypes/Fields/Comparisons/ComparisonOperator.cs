namespace Waher.Processors.Metering.NodeTypes.Fields.Comparisons
{
	/// <summary>
	/// Identifies a comparison operator.
	/// </summary>
	public enum ComparisonOperator
	{
		/// <summary>
		/// Equals operator (=)
		/// </summary>
		Equal,

		/// <summary>
		/// Not equal operator (&lt;&gt;)
		/// </summary>
		NotEqual,

		/// <summary>
		/// Lesser than operator (&lt;)
		/// </summary>
		LesserThan,
		/// <summary>
		/// Lesser than or equal operator (&lt;=)
		/// </summary>
		LesserThanOrEqual,

		/// <summary>
		/// Greater than operator (&gt;)
		/// </summary>
		GreaterThan,

		/// <summary>
		/// Greater than or equal operator (&gt;=)
		/// </summary>
		GreaterThanOrEqual
	}
}
