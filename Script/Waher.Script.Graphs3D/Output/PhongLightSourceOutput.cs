using System;
using System.Text;
using Waher.Runtime.Inventory;
using Waher.Script.Output;

namespace Waher.Script.Graphs3D.Output
{
	/// <summary>
	/// Converts values of type <see cref="PhongLightSource"/> to expression strings.
	/// </summary>
	public class PhongLightSourceOutput : ICustomStringOutput
	{
		/// <summary>
		/// If the interface understands objects such as <paramref name="Object"/>.
		/// </summary>
		/// <param name="Object">Object</param>
		/// <returns>How well objects of this type are supported.</returns>
		public Grade Supports(Type Object) => Object == typeof(PhongLightSource) ? Grade.Ok : Grade.NotAtAll;

		/// <summary>
		/// Gets a string representing a value.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>Expression string.</returns>
		public string GetString(object Value)
		{
			PhongLightSource Source = (PhongLightSource)Value;
			StringBuilder sb = new StringBuilder();

			sb.Append("PhongLightSource(");
			sb.Append(Expression.ToString(Source.Diffuse));
			sb.Append(',');
			sb.Append(Expression.ToString(Source.Specular));
			sb.Append(',');
			sb.Append(Expression.ToString(Source.Position));
			sb.Append(")");

			return sb.ToString();
		}
	}
}
