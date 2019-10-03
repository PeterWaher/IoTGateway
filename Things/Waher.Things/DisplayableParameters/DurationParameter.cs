using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;

namespace Waher.Things.DisplayableParameters
{
	/// <summary>
	/// Duration-valued parameter.
	/// </summary>
	public class DurationParameter : Parameter
	{
		private Duration value;

		/// <summary>
		/// Duration-valued parameter.
		/// </summary>
		public DurationParameter()
			: base()
		{
			this.value = Duration.Zero;
		}

		/// <summary>
		/// Duration-valued parameter.
		/// </summary>
		/// <param name="Id">Parameter ID.</param>
		/// <param name="Name">Parameter Name.</param>
		/// <param name="Value">Parameter Value</param>
		public DurationParameter(string Id, string Name, Duration Value)
			: base(Id, Name)
		{
			this.value = Value;
		}

		/// <summary>
		/// Parameter Value.
		/// </summary>
		public Duration Value
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
			Xml.Append("<duration");
			base.Export(Xml);
			Xml.Append(" value='");
			Xml.Append(this.value.ToString());
			Xml.Append("'/>");
		}
	}
}
