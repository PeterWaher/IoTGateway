using System;
using System.Reflection;
using System.Text;
using Waher.Runtime.Inventory;

namespace Waher.Script.Output
{
	/// <summary>
	/// Converts values of type Variables to expression strings.
	/// </summary>
	public class VariablesOutput : ICustomStringOutput
	{
		private static readonly TypeInfo variablesTypeInfo = typeof(Variables).GetTypeInfo();
		private const string recursionMarker = "  IsExporting  ";

		/// <summary>
		/// If the interface understands objects such as <paramref name="Object"/>.
		/// </summary>
		/// <param name="Object">Object</param>
		/// <returns>How well objects of this type are supported.</returns>
		public Grade Supports(Type Object) => variablesTypeInfo.IsAssignableFrom(Object.GetTypeInfo()) ? Grade.Ok : Grade.NotAtAll;

		/// <summary>
		/// Gets a string representing a value.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>Expression string.</returns>
		public string GetString(object Value)
		{
			Variables Variables = (Variables)Value;

			if (Variables.ContainsVariable(recursionMarker))
				return Value.GetType().FullName;

			Variables[recursionMarker] = true;

			StringBuilder sb = new StringBuilder();
			bool First = true;

			sb.Append('(');

			foreach (Variable Variable in Variables)
			{
				if (First)
					First = false;
				else
					sb.Append(';');

				sb.Append(Variable.Name);
				sb.Append(":=");
				sb.Append(Expression.ToString(Variable.ValueObject));
			}

			sb.Append(')');

			Variables.Remove(recursionMarker);
		
			return sb.ToString();
		}
	}
}
