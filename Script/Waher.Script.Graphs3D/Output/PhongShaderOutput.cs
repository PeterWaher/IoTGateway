using System;
using System.Text;
using Waher.Runtime.Inventory;
using Waher.Script.Output;

namespace Waher.Script.Graphs3D.Output
{
	/// <summary>
	/// Converts values of type <see cref="PhongShader"/> to expression strings.
	/// </summary>
	public class PhongShaderOutput : ICustomStringOutput
	{
		/// <summary>
		/// If the interface understands objects such as <paramref name="Object"/>.
		/// </summary>
		/// <param name="Object">Object</param>
		/// <returns>How well objects of this type are supported.</returns>
		public Grade Supports(Type Object) => Object == typeof(PhongShader) ? Grade.Ok : Grade.NotAtAll;

		/// <summary>
		/// Gets a string representing a value.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>Expression string.</returns>
		public string GetString(object Value)
		{
			PhongShader Shader = (PhongShader)Value;
			StringBuilder sb = new StringBuilder();

			sb.Append("PhongShader(");
			sb.Append(Expression.ToString(Shader.Material));
			sb.Append(',');
			sb.Append(Expression.ToString(Shader.Ambient));
			sb.Append(",");
			sb.Append(Expression.ToString(Shader.Sources));
			sb.Append(')');

			return sb.ToString();
		}
	}
}
