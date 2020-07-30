using System;
using System.Collections.Generic;
using System.Text;
using Waher.Persistence;
using Waher.Runtime.Inventory;
using Waher.Script.Output;

namespace Waher.Script.Persistence.Output
{
	/// <summary>
	/// Converts values of type <see cref="CaseInsensitiveString"/> to expression strings.
	/// </summary>
	public class CaseInsensitiveStringOutput : ICustomStringOutput
	{
		/// <summary>
		/// If the interface understands objects such as <paramref name="Object"/>.
		/// </summary>
		/// <param name="Object">Object</param>
		/// <returns>How well objects of this type are supported.</returns>
		public Grade Supports(Type Object) => Object == typeof(CaseInsensitiveString) ? Grade.Ok : Grade.NotAtAll;

		/// <summary>
		/// Gets a string representing a value.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>Expression string.</returns>
		public string GetString(object Value)
		{
			if (Value is CaseInsensitiveString cs)
				return Expression.ToString(cs.Value);
			else
				return Value.ToString();
		}
	}
}
