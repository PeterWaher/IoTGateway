using System;
using System.Xml;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Xml.Model
{
	/// <summary>
	/// Base class for all XML Script nodes in a parsed script tree.
	/// </summary>
	public abstract class XmlScriptNode : ScriptNode
	{
		/// <summary>
		/// Base class for all XML Script nodes in a parsed script tree.
		/// </summary>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public XmlScriptNode(int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			return Objects.ObjectValue.Null;
		}

		/// <summary>
		/// Builds an XML Document object
		/// </summary>
		/// <param name="Document">Document being built.</param>
		/// <param name="Parent">Parent element.</param>
		/// <param name="Variables">Current set of variables.</param>
		internal abstract void Build(XmlDocument Document, XmlElement Parent, Variables Variables);

		/// <summary>
		/// Evaluates a script node to a string.
		/// </summary>
		/// <param name="Node">Node to evaluate.</param>
		/// <param name="Variables">Variables.</param>
		/// <returns>String result.</returns>
		protected static string EvaluateString(ScriptNode Node, Variables Variables)
		{
			IElement Element = Node.Evaluate(Variables);

			if (Element is StringValue S)
				return S.Value;
			else if (Element is BooleanValue B)
				return CommonTypes.Encode(B.Value);
			else if (Element is DoubleNumber D)
				return CommonTypes.Encode(D.Value);
			else if (Element is DateTimeValue DT)
				return XML.Encode(DT.Value, DT.Value.TimeOfDay == TimeSpan.Zero);
			else if (Element is Integer I)
				return I.Value.ToString();
			else
			{
				object Value = Element.AssociatedObjectValue;
				if (Value is null)
					return null;
				else
					return Expression.ToString(Value);
			}
		}
	}
}
