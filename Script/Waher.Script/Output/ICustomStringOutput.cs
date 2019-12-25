using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Script.Output
{
	/// <summary>
	/// Interface for custom string output classes. Converts objects of a given type to an expression string.
	/// </summary>
	public interface ICustomStringOutput
	{
		/// <summary>
		/// Type
		/// </summary>
		Type Type
		{
			get;
		}

		/// <summary>
		/// Gets a string representing a value.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>Expression string.</returns>
		string GetString(object Value);
	}
}
