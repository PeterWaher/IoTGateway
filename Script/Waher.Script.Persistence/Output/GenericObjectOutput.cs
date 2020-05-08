using System;
using System.Collections.Generic;
using System.Text;
using Waher.Persistence.Serialization;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Output;

namespace Waher.Script.Persistence.Output
{
	/// <summary>
	/// Converts values of type <see cref="GenericObject"/> to expression strings.
	/// </summary>
	public class GenericObjectOutput : ICustomStringOutput
	{
		/// <summary>
		/// Type
		/// </summary>
		public Type Type => typeof(GenericObject);

		/// <summary>
		/// Gets a string representing a value.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>Expression string.</returns>
		public string GetString(object Value)
		{
			GenericObject Obj = (GenericObject)Value;

			StringBuilder sb = new StringBuilder();
			bool First = true;

			sb.Append('{');

			foreach (KeyValuePair<string, object> P in Obj.Properties)
			{
				if (First)
					First = false;
				else
					sb.Append(',');

				sb.Append(P.Key);
				sb.Append(':');
				sb.Append(Expression.ToString(P.Value));
			}

			sb.Append('}');

			return sb.ToString();
		}
	}
}
