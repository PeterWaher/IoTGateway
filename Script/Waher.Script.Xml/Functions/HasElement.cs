using System.Threading.Tasks;
using System.Xml;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Xml.Functions
{
	/// <summary>
	/// Checks in an XML Child element exists.
	/// </summary>
	public class HasElement : FunctionTwoScalarVariables
	{
		/// <summary>
		/// Checks in an XML Child element exists.
		/// </summary>
		/// <param name="Xml">XML.</param>
		/// <param name="Name">Name of child element.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public HasElement(ScriptNode Xml, ScriptNode Name, int Start, int Length, Expression Expression)
			: base(Xml, Name, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(HasElement);

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
			object Obj = Argument1.AssociatedObjectValue;
			if (Obj is null)
				return Argument1;

			string Name = Argument2.AssociatedObjectValue?.ToString() ?? string.Empty;
			XmlElement Response;

			if (Obj is XmlElement E)
				Response = E[Name];
			else if (Obj is XmlDocument Doc)
				Response = Doc.DocumentElement[Name];
			else
				throw new ScriptRuntimeException("XML expected.", this);

			return Response is null ? BooleanValue.False : BooleanValue.True;
		}

		/// <summary>
		/// Evaluates the function on two scalar arguments.
		/// </summary>
		/// <param name="Argument1">Function argument 1.</param>
		/// <param name="Argument2">Function argument 2.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override Task<IElement> EvaluateScalarAsync(IElement Argument1, IElement Argument2, Variables Variables)
		{
			return Task.FromResult(this.EvaluateScalar(Argument1, Argument2, Variables));
		}
	}
}
