using System;

namespace Waher.Things.Attributes
{
	/// <summary>
	/// Validates input against a regular expression.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public class RegularExpressionAttribute : Attribute
	{
		private readonly string pattern;

		/// <summary>
		/// Validates input against a regular expression.
		/// </summary>
		/// <param name="Pattern">Regular expression to validate against.</param>
		public RegularExpressionAttribute(string Pattern)
		{
			this.pattern = Pattern;
		}

		/// <summary>
		/// Regular expression to validate against.
		/// </summary>
		public string Pattern
		{
			get { return this.pattern; }
		}
	}
}
