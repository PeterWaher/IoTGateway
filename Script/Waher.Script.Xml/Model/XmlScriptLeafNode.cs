using Waher.Script.Model;

namespace Waher.Script.Xml.Model
{
	/// <summary>
	/// Base class for all XML Script leaf nodes in a parsed script tree.
	/// </summary>
	public abstract class XmlScriptLeafNode : XmlScriptNode
	{
		/// <summary>
		/// Base class for all XML Script leaf nodes in a parsed script tree.
		/// </summary>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public XmlScriptLeafNode(int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
		}

		/// <summary>
		/// Calls the callback method for all child nodes.
		/// </summary>
		/// <param name="Callback">Callback method to call.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="Order">Order to traverse the nodes.</param>
		/// <returns>If the process was completed.</returns>
		public override bool ForAllChildNodes(ScriptNodeEventHandler Callback, object State, SearchMethod Order)
		{
			return true;
		}

	}
}
