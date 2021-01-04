using System;
using System.Numerics;
using System.Text;
using Waher.Runtime.Inventory;
using Waher.Script.Output;

namespace Waher.Script.Graphs3D.Output
{
	/// <summary>
	/// Converts values of type <see cref="Vector4"/> to expression strings.
	/// </summary>
	public class Vector4Output : ICustomStringOutput
	{
		/// <summary>
		/// If the interface understands objects such as <paramref name="Object"/>.
		/// </summary>
		/// <param name="Object">Object</param>
		/// <returns>How well objects of this type are supported.</returns>
		public Grade Supports(Type Object) => Object == typeof(Vector4) ? Grade.Ok : Grade.NotAtAll;

		/// <summary>
		/// Gets a string representing a value.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>Expression string.</returns>
		public string GetString(object Value)
		{
			Vector4 v = (Vector4)Value;
			StringBuilder sb = new StringBuilder();

			sb.Append("Vector4(");
			sb.Append(Expression.ToString(v.X));
			sb.Append(',');
			sb.Append(Expression.ToString(v.Y));
			sb.Append(',');
			sb.Append(Expression.ToString(v.Z));
			sb.Append(',');
			sb.Append(Expression.ToString(v.W));
			sb.Append(')');

			return sb.ToString();
		}
	}
}
