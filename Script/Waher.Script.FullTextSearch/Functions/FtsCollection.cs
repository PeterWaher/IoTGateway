using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.FullTextSearch.Functions
{
	/// <summary>
	/// Matches a Full-Text-Search Index with a Database Collection.
	/// </summary>
	public class FtsCollection : FunctionTwoScalarVariables
	{
		/// <summary>
		/// Matches a Full-Text-Search Index with a Database Collection.
		/// </summary>
		/// <param name="IndexCollection">Name of Full-text-search index.</param>
		/// <param name="DatabaseCollection">Name of Database index.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public FtsCollection(ScriptNode IndexCollection, ScriptNode DatabaseCollection,
			int Start, int Length, Expression Expression)
			: base(IndexCollection, DatabaseCollection, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(FtsCollection);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "IndexCollection", "DatabaseCollection" };

		/// <summary>
		/// If the node (or its decendants) include asynchronous evaluation. Asynchronous nodes should be evaluated using
		/// <see cref="ScriptNode.EvaluateAsync(Variables)"/>.
		/// </summary>
		public override bool IsAsynchronous => true;

		/// <summary>
		/// Evaluates the function on two scalar arguments.
		/// </summary>
		/// <param name="Argument1">Function argument 1.</param>
		/// <param name="Argument2">Function argument 2.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(string Argument1, string Argument2, Variables Variables)
		{
			return this.EvaluateScalarAsync(Argument1, Argument2, Variables).Result;
		}

		/// <summary>
		/// Evaluates the function on two scalar arguments.
		/// </summary>
		/// <param name="Argument1">Function argument 1.</param>
		/// <param name="Argument2">Function argument 2.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override async Task<IElement> EvaluateScalarAsync(string Argument1, string Argument2, Variables Variables)
		{
			bool Result = await Waher.Persistence.FullTextSearch.Search.SetFullTextSearchIndexCollection(Argument1, Argument2);
			
			return Result ? BooleanValue.True : BooleanValue.False;
		}
	}
}
