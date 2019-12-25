using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Script.Output
{
	/// <summary>
	/// Converts values of type Boolean to expression strings.
	/// </summary>
	public class BooleanOutput : ICustomStringOutput
	{
		/// <summary>
		/// Type
		/// </summary>
		public Type Type => typeof(bool);

		/// <summary>
		/// Gets a string representing a value.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>Expression string.</returns>
		public string GetString(object Value)
		{
			return Expression.ToString((bool)Value);
		}
	}
}
