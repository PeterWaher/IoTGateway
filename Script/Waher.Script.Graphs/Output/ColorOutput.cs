using System;
using System.Text;
using SkiaSharp;
using Waher.Runtime.Inventory;
using Waher.Script.Output;

namespace Waher.Script.Graphs.Output
{
	/// <summary>
	/// Converts values of type <see cref="SKColor"/> to expression strings.
	/// </summary>
	public class ColorOutput : ICustomStringOutput
	{
		/// <summary>
		/// If the interface understands objects such as <paramref name="Object"/>.
		/// </summary>
		/// <param name="Object">Object</param>
		/// <returns>How well objects of this type are supported.</returns>
		public Grade Supports(Type Object) => Object == typeof(SKColor) ? Grade.Ok : Grade.NotAtAll;

		/// <summary>
		/// Gets a string representing a value.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>Expression string.</returns>
		public string GetString(object Value)
		{
			SKColor Color = (SKColor)Value;
			StringBuilder sb = new StringBuilder();
			
			sb.Append("Color(\"");
			sb.Append(Color.Red.ToString("X2"));
			sb.Append(Color.Green.ToString("X2"));
			sb.Append(Color.Blue.ToString("X2"));

			if (Color.Alpha != 255)
				sb.Append(Color.Alpha.ToString("X2"));

			sb.Append("\")");

			return sb.ToString();
		}
	}
}
