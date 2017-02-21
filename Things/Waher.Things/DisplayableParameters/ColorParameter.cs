using System;
using System.Collections.Generic;
#if !WINDOWS_UWP
using System.Drawing;
#endif
using System.Text;
using System.Threading.Tasks;
using Waher.Content;

namespace Waher.Things.DisplayableParameters
{
#if WINDOWS_UWP
	/// <summary>
	/// Color value.
	/// </summary>
	public struct Color
	{
		private byte a, r, g, b;

		/// <summary>
		/// Color value.
		/// </summary>
		/// <param name="R">Red component.</param>
		/// <param name="G">Green component.</param>
		/// <param name="B">Blue component.</param>
		/// <param name="A">Alpha component.</param>
		public Color(byte R, byte G, byte B, byte A)
		{
			this.r = R;
			this.g = G;
			this.b = B;
			this.a = A;
		}

		/// <summary>
		/// Red component.
		/// </summary>
		public byte R { get { return this.r; } }

		/// <summary>
		/// Green component.
		/// </summary>
		public byte G { get { return this.g; } }

		/// <summary>
		/// Blue component.
		/// </summary>
		public byte B { get { return this.b; } }

		/// <summary>
		/// Alpha component.
		/// </summary>
		public byte A { get { return this.a; } }
	}
#endif

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
