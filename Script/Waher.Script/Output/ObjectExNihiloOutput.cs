using System;
using System.Collections.Generic;
using System.Text;
using Waher.Runtime.Inventory;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Output
{
	/// <summary>
	/// Converts values of type Dictionary{string, IElement} to expression strings.
	/// </summary>
	public class ObjectExNihiloOutput : ICustomStringOutput
	{
		/// <summary>
		/// If the interface understands objects such as <paramref name="Object"/>.
		/// </summary>
		/// <param name="Object">Object</param>
		/// <returns>How well objects of this type are supported.</returns>
		public Grade Supports(Type Object) => Object == typeof(Dictionary<string, IElement>) ? Grade.Ok : Grade.NotAtAll;

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

				if (CanBeVariable(P.Key))
					sb.Append(P.Key);
				else
				{
					sb.Append('"');
					sb.Append(P.Key.
						Replace("\\", "\\\\").
						Replace("\"", "\\\"").
						Replace("\n", "\\n").
						Replace("\r", "\\r").
						Replace("\t", "\\t").
						Replace("\b", "\\b").
						Replace("\f", "\\f").
						Replace("\a", "\\a"));
					sb.Append('"');
				}

				sb.Append(':');
				sb.Append(Expression.ToString(P.Value.AssociatedObjectValue));
			}

			sb.Append('}');

			return sb.ToString();
		}

		/// <summary>
		/// Checks if a label can be used as a variable directly or not.
		/// </summary>
		/// <param name="Label">Label string.</param>
		/// <returns>If the label can act as a variable.</returns>
		public static bool CanBeVariable(string Label)
		{
			bool HasLetters = false;

			foreach (char ch in Label)
			{
				if (char.IsLetter(ch))
					HasLetters = true;
				else if (char.IsDigit(ch))
				{
					if (!HasLetters)
						return false;
				}
				else if (ch != '_')
					return false;
			}

			return HasLetters;
		}
	}
}
