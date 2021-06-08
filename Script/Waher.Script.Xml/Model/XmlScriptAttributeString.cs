using System;
using System.Xml;
using Waher.Script.Model;

namespace Waher.Script.Xml.Model
{
	/// <summary>
	/// XML Script attribute node, whose value is defined by script.
	/// </summary>
	public class XmlScriptAttributeString : XmlScriptAttribute 
	{
		private readonly string value;

		/// <summary>
		/// XML Script attribute node, whose value is defined by script.
		/// </summary>
		/// <param name="Name">Element name.</param>
		/// <param name="Value">String value.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public XmlScriptAttributeString(string Name, string Value, int Start, int Length, Expression Expression)
			: base(Name, Start, Length, Expression)
		{
			this.value = Value;
		}

		/// <summary>
		/// Calls the callback method for all child nodes.
		/// </summary>
		/// <param name="Callback">Callback method to call.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="DepthFirst">If calls are made depth first (true) or on each node and then its leaves (false).</param>
		/// <returns>If the process was completed.</returns>
		public override bool ForAllChildNodes(ScriptNodeEventHandler Callback, object State, bool DepthFirst)
		{
			return true;
		}

		/// <summary>
		/// Builds an XML Document object
		/// </summary>
		/// <param name="Document">Document being built.</param>
		/// <param name="Parent">Parent element.</param>
		/// <param name="Variables">Current set of variables.</param>
		internal override void Build(XmlDocument Document, XmlElement Parent, Variables Variables)
		{
			Parent.SetAttribute(this.Name, this.value);
		}

		/// <summary>
		/// Gets the attribute value.
		/// </summary>
		/// <param name="Variables">Current set of variables.</param>
		internal override string GetValue(Variables Variables)
		{
			return this.value;
		}
	}
}
