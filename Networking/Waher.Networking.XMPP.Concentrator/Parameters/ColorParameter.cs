using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;

namespace Waher.Networking.XMPP.Concentrator.Parameters
{
	/// <summary>
	/// Color-valued parameter.
	/// </summary>
	public class ColorParameter : Parameter
	{
		private Tuple<byte, byte, byte> value;

		/// <summary>
		/// Color-valued parameter.
		/// </summary>
		/// <param name="Id">Parameter ID.</param>
		/// <param name="Name">Parameter Name.</param>
		/// <param name="Value">Parameter Value</param>
		public ColorParameter(string Id, string Name, Tuple<byte, byte, byte> Color)
			: base(Id, Name)
		{
			this.value = Value;
		}

		/// <summary>
		/// Parameter Value.
		/// </summary>
		public Tuple<byte, byte, byte> Value
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
			Xml.Append(this.value.Item1.ToString("X2"));
			Xml.Append(this.value.Item2.ToString("X2"));
			Xml.Append(this.value.Item3.ToString("X2"));
			Xml.Append("'/>");
		}
	}
}
