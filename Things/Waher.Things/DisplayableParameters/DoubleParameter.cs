using System;
using System.Text;
using Waher.Content;

namespace Waher.Things.DisplayableParameters
{
	/// <summary>
	/// Double-valued parameter.
	/// </summary>
	public class DoubleParameter : Parameter
	{
		private double value;

		/// <summary>
		/// Double-valued parameter.
		/// </summary>
		public DoubleParameter()
			: base()
		{
			this.value = 0;
		}

		/// <summary>
		/// Double-valued parameter.
		/// </summary>
		/// <param name="Id">Parameter ID.</param>
		/// <param name="Name">Parameter Name.</param>
		/// <param name="Value">Parameter Value</param>
		public DoubleParameter(string Id, string Name, double Value)
			: base(Id, Name)
		{
			this.value = Value;
		}

		/// <summary>
		/// Parameter Value.
		/// </summary>
		public double Value
		{
			get { return this.value; }
			set { this.value = value; }
		}

		/// <summary>
		/// Untyped parameter value
		/// </summary>
		public override object UntypedValue
		{
			get { return this.value; }
		}

		/// <summary>
		/// Exports the parameters to XML.
		/// </summary>
		/// <param name="Xml">XML Output.</param>
		public override void Export(StringBuilder Xml)
		{
			Xml.Append("<double");
			base.Export(Xml);
			Xml.Append(" value='");
			Xml.Append(CommonTypes.Encode(this.value));
			Xml.Append("'/>");
		}
	}
}
