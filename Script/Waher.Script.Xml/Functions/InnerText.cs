using System.Xml;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Xml.Functions
{
	/// <summary>
	/// Returns the Inner Text of an XML Node.
	/// </summary>
	public class InnerText : FunctionOneScalarVariable
	{
		/// <summary>
		/// Returns the Inner Text of an XML Node.
		/// </summary>
		/// <param name="Xml">XML.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public InnerText(ScriptNode Xml, int Start, int Length, Expression Expression)
			: base(Xml, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(InnerText);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "XML" };

		/// <summary>
		/// Evaluates the function on two scalar arguments.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(IElement Argument, Variables Variables)
		{
			object Obj = Argument.AssociatedObjectValue;

			if (Obj is null)
				return Argument;
			else if (Obj is XmlNode N)
				return new StringValue(N.InnerText);
			else
				throw new ScriptRuntimeException("XML node expected.", this);
		}
	}
}
