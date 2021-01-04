using System;
using System.Text;
using Waher.Runtime.Inventory;
using Waher.Script.Output;

namespace Waher.Script.Graphs3D.Output
{
	/// <summary>
	/// Converts values of type <see cref="PhongIntensity"/> to expression strings.
	/// </summary>
	public class PhongIntensityOutput : ICustomStringOutput
	{
		/// <summary>
		/// If the interface understands objects such as <paramref name="Object"/>.
		/// </summary>
		/// <param name="Object">Object</param>
		/// <returns>How well objects of this type are supported.</returns>
		public Grade Supports(Type Object) => Object == typeof(PhongIntensity) ? Grade.Ok : Grade.NotAtAll;

		/// <summary>
		/// Gets a string representing a value.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>Expression string.</returns>
		public string GetString(object Value)
		{
			PhongIntensity Intensity = (PhongIntensity)Value;
			StringBuilder sb = new StringBuilder();

			sb.Append("PhongIntensity(");
			sb.Append(Expression.ToString(Intensity.Red));
			sb.Append(',');
			sb.Append(Expression.ToString(Intensity.Green));
			sb.Append(',');
			sb.Append(Expression.ToString(Intensity.Blue));
			sb.Append(',');
			sb.Append(Expression.ToString(Intensity.Alpha));
			sb.Append(")");

			return sb.ToString();
		}
	}
}
