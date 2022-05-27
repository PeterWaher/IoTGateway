using System.Xml;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Xml.Functions
{
	/// <summary>
	/// Checks if an attribute eixts for an XML Element.
	/// </summary>
	public class HasAttribute : FunctionTwoScalarVariables
	{
		/// <summary>
		/// Checks if an attribute eixts for an XML Element.
		/// </summary>
		/// <param name="Xml">XML.</param>
		/// <param name="Name">Name of child element.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public HasAttribute(ScriptNode Xml, ScriptNode Name, int Start, int Length, Expression Expression)
			: base(Xml, Name, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(HasAttribute);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "XML", "Name" };

		/// <summary>
		/// Evaluates the function on two scalar arguments.
		/// </summary>
		/// <param name="Argument1">Function argument 1.</param>
		/// <param name="Argument2">Function argument 2.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(IElement Argument1, IElement Argument2, Variables Variables)
		{
			string Name = Argument2.AssociatedObjectValue?.ToString() ?? string.Empty;

			if (Argument1.AssociatedObjectValue is XmlElement E)
				return E.HasAttribute(Name) ? BooleanValue.True : BooleanValue.False;
			else
				throw new ScriptRuntimeException("XML element expected.", this);
		}
	}
}
