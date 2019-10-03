using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waher.Content.Xml;

namespace Waher.Things.DisplayableParameters
{
	/// <summary>
	/// Base class for all node parameters.
	/// </summary>
	public abstract class Parameter
	{
		private string id;
		private string name;

		/// <summary>
		/// Base class for all node parameters.
		/// </summary>
		public Parameter()
		{
			this.id = string.Empty;
			this.name = string.Empty;
		}

		/// <summary>
		/// Base class for all node parameters.
		/// </summary>
		/// <param name="Id">Parameter ID.</param>
		/// <param name="Name">Parameter Name.</param>
		public Parameter(string Id, string Name)
		{
			this.id = Id;
			this.name = Name;
		}

		/// <summary>
		/// Parameter ID.
		/// </summary>
		public string Id
		{
			get { return this.id; }
			set { this.id = value; }
		}

		/// <summary>
		/// Parameter Name.
		/// </summary>
		public string Name
		{
			get { return this.name; }
			set { this.name = value; }
		}

		/// <summary>
		/// Untyped parameter value
		/// </summary>
		public abstract object UntypedValue
		{
			get;
		}

		/// <summary>
		/// String representation of parameter value
		/// </summary>
		public virtual object StringValue
		{
			get
			{
				object Obj = this.UntypedValue;

				if (Obj is null)
					return string.Empty;
				else
					return Obj.ToString();
			}
		}

		/// <summary>
		/// Exports the parameters to XML.
		/// </summary>
		/// <param name="Xml">XML Output.</param>
		public virtual void Export(StringBuilder Xml)
		{
			Xml.Append(" id='");
			Xml.Append(XML.Encode(this.id));
			Xml.Append("' name='");
			Xml.Append(XML.Encode(this.name));
			Xml.Append("'");
		}
	}
}
