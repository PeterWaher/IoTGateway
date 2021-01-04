using System;
using System.Text;
using Waher.Runtime.Inventory;
using Waher.Script.Output;

namespace Waher.Script.Graphs3D.Output
{
	/// <summary>
	/// Converts values of type <see cref="PhongMaterial"/> to expression strings.
	/// </summary>
	public class PhongMaterialOutput : ICustomStringOutput
	{
		/// <summary>
		/// If the interface understands objects such as <paramref name="Object"/>.
		/// </summary>
		/// <param name="Object">Object</param>
		/// <returns>How well objects of this type are supported.</returns>
		public Grade Supports(Type Object) => Object == typeof(PhongMaterial) ? Grade.Ok : Grade.NotAtAll;

		/// <summary>
		/// Gets a string representing a value.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>Expression string.</returns>
		public string GetString(object Value)
		{
			PhongMaterial Material = (PhongMaterial)Value;
			StringBuilder sb = new StringBuilder();

			sb.Append("PhongMaterial(");
			sb.Append(Expression.ToString(Material.AmbientReflectionConstant));
			sb.Append(',');
			sb.Append(Expression.ToString(Material.DiffuseReflectionConstant));
			sb.Append(',');
			sb.Append(Expression.ToString(Material.SpecularReflectionConstant));
			sb.Append(',');
			sb.Append(Expression.ToString(Material.Shininess));
			sb.Append(")");

			return sb.ToString();
		}
	}
}
