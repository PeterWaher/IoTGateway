using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;

namespace Waher.Networking.XMPP.Concentrator.DisplayableParameters
{
	/// <summary>
	/// Color-valued parameter.
	/// </summary>
	public class ColorParameter : Parameter
	{
		private Color value;

		/// <summary>
		/// Color-valued parameter.
		/// </summary>
		/// <param name="Id">Parameter ID.</param>
		/// <param name="Name">Parameter Name.</param>
		/// <param name="Value">Parameter Value</param>
		public ColorParameter(string Id, string Name, Color Color)
			: base(Id, Name)
		{
			this.value = Value;
		}

		/// <summary>
		/// Parameter Value.
		/// </summary>
		public Color Value
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
			Xml.Append(this.value.R.ToString("X2"));
			Xml.Append(this.value.G.ToString("X2"));
			Xml.Append(this.value.B.ToString("X2"));
			Xml.Append("'/>");
		}
	}
}
