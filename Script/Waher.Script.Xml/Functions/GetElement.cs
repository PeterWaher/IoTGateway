using System.Xml;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Xml.Functions
{
	/// <summary>
	/// Gets an XML Child element from an XML Element.
	/// </summary>
	public class GetElement : FunctionTwoScalarVariables
	{
		/// <summary>
		/// Gets an XML Child element from an XML Element.
		/// </summary>
		/// <param name="Xml">XML.</param>
		/// <param name="Name">Name of child element.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public GetElement(ScriptNode Xml, ScriptNode Name, int Start, int Length, Expression Expression)
			: base(Xml, Name, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(GetElement);

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
			XmlElement Response;

			if (Argument1.AssociatedObjectValue is XmlElement E)
				Response = E[Name];
			else if (Argument1.AssociatedObjectValue is XmlDocument Doc)
				Response = Doc[Name];
			else
				throw new ScriptRuntimeException("XML expected.", this);

			if (Response is null)
				throw new ScriptRuntimeException("Child element not found.", this);
			else
				return new ObjectValue(Response);
		}
	}
}
