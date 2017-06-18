using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;
using Waher.Content;

namespace Waher.Things.DisplayableParameters
{
	/// <summary>
	/// Color-valued parameter.
	/// </summary>
	public class ColorParameter : Parameter
	{
		private SKColor value;

		/// <summary>
		/// Color-valued parameter.
		/// </summary>
		/// <param name="Id">Parameter ID.</param>
		/// <param name="Name">Parameter Name.</param>
		/// <param name="Value">Parameter Value</param>
		public ColorParameter(string Id, string Name, SKColor Color)
			: base(Id, Name)
		{
			this.value = Value;
		}

		/// <summary>
		/// Parameter Value.
		/// </summary>
		public SKColor Value
		{
			get { return this.value; }
		}

		/// <summary>
		/// Exports the parameters to XML.
		/// </summary>
		/// <param name="Xml">XML Output.</param>
		public override void Export(StringBuilder Xml)
		{
			Xml.Append("<color");
			base.Export(Xml);
			Xml.Append(" value='");
			Xml.Append(this.value.Red.ToString("X2"));
			Xml.Append(this.value.Green.ToString("X2"));
			Xml.Append(this.value.Blue.ToString("X2"));
			Xml.Append("'/>");
		}
	}
}
