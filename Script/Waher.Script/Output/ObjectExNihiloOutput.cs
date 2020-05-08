using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Output
{
	/// <summary>
	/// Converts values of type Dictionary{string, IElement} to expression strings.
	/// </summary>
	public class ObjectExNihiloOutput : ICustomStringOutput
	{
		/// <summary>
		/// Type
		/// </summary>
		public Type Type => typeof(Dictionary<string, IElement>);

		/// <summary>
		/// Gets a string representing a value.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>Expression string.</returns>
		public string GetString(object Value)
		{
			Dictionary<string, IElement> ObjExNihilo = (Dictionary<string, IElement>)Value;

			StringBuilder sb = new StringBuilder();
			bool First = true;

			sb.Append('{');

			foreach (KeyValuePair<string, IElement> P in ObjExNihilo)
			{
				if (First)
					First = false;
				else
					sb.Append(',');

				sb.Append(P.Key);
				sb.Append(':');
				sb.Append(Expression.ToString(P.Value.AssociatedObjectValue));
			}

			sb.Append('}');

			return sb.ToString();
		}
	}
}
