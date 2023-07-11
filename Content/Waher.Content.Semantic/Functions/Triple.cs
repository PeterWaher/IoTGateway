using System.Threading.Tasks;
using Waher.Content.Semantic.Model;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Content.Semantic.Functions
{
	/// <summary>
	/// Triple(Subject,Predicate,Object)
	/// </summary>
	public class Triple : FunctionMultiVariate
	{
		/// <summary>
		/// Triple(Subject,Predicate,Object)
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Object">Object</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Triple(ScriptNode Subject, ScriptNode Predicate, ScriptNode Object, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Subject, Predicate, Object }, argumentTypes3Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(Triple);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "Subject", "Predicate", "Object" };

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			ISemanticElement Subject = SemanticElements.Encapsulate(Arguments[0].AssociatedObjectValue);
			ISemanticElement Predicate = SemanticElements.Encapsulate(Arguments[1].AssociatedObjectValue);
			ISemanticElement Object = SemanticElements.Encapsulate(Arguments[2].AssociatedObjectValue);

			return new SemanticTriple(Subject, Predicate, Object);
		}

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override Task<IElement> EvaluateAsync(IElement[] Arguments, Variables Variables)
		{
			return Task.FromResult(this.Evaluate(Arguments, Variables));
		}
	}
}
