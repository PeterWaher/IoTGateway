using System;
using System.Numerics;
using System.Text;
using Waher.Runtime.Inventory;
using Waher.Script.Output;

namespace Waher.Script.Graphs3D.Output
{
	/// <summary>
	/// Converts values of type <see cref="Matrix4x4"/> to expression strings.
	/// </summary>
	public class Matrix4x4Output : ICustomStringOutput
	{
		/// <summary>
		/// If the interface understands objects such as <paramref name="Object"/>.
		/// </summary>
		/// <param name="Object">Object</param>
		/// <returns>How well objects of this type are supported.</returns>
		public Grade Supports(Type Object) => Object == typeof(Matrix4x4) ? Grade.Ok : Grade.NotAtAll;

		/// <summary>
		/// Gets a string representing a value.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>Expression string.</returns>
		public string GetString(object Value)
		{
			Matrix4x4 M = (Matrix4x4)Value;
			StringBuilder sb = new StringBuilder();

			sb.Append("Matrix4x4(");
			sb.Append(Expression.ToString(M.M11));
			sb.Append(',');
			sb.Append(Expression.ToString(M.M12));
			sb.Append(',');
			sb.Append(Expression.ToString(M.M13));
			sb.Append(',');
			sb.Append(Expression.ToString(M.M14));
			sb.Append(',');
			sb.Append(Expression.ToString(M.M21));
			sb.Append(',');
			sb.Append(Expression.ToString(M.M22));
			sb.Append(',');
			sb.Append(Expression.ToString(M.M23));
			sb.Append(',');
			sb.Append(Expression.ToString(M.M24));
			sb.Append(',');
			sb.Append(Expression.ToString(M.M31));
			sb.Append(',');
			sb.Append(Expression.ToString(M.M32));
			sb.Append(',');
			sb.Append(Expression.ToString(M.M33));
			sb.Append(',');
			sb.Append(Expression.ToString(M.M34));
			sb.Append(',');
			sb.Append(Expression.ToString(M.M41));
			sb.Append(',');
			sb.Append(Expression.ToString(M.M42));
			sb.Append(',');
			sb.Append(Expression.ToString(M.M43));
			sb.Append(',');
			sb.Append(Expression.ToString(M.M44));
			sb.Append(')');

			return sb.ToString();
		}
	}
}
