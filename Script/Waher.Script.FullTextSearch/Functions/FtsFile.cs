using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Waher.Persistence;
using Waher.Persistence.Filters;
using Waher.Persistence.FullTextSearch.Files;
using Waher.Persistence.FullTextSearch.Tokenizers;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.FullTextSearch.Functions
{
	/// <summary>
	/// Indexes a specific file.
	/// </summary>
	public class FtsFile : FunctionMultiVariate
	{
		/// <summary>
		/// Indexes a specific file.
		/// </summary>
		/// <param name="IndexCollection">Name of Full-text-search index.</param>
		/// <param name="FileName">Folder to index.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public FtsFile(ScriptNode IndexCollection, ScriptNode FileName,
			int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { IndexCollection, FileName }, argumentTypes2Scalar,
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(FtsFile);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "IndexCollection", "FileName" };

		/// <summary>
		/// If the node (or its decendants) include asynchronous evaluation. Asynchronous nodes should be evaluated using
		/// <see cref="ScriptNode.EvaluateAsync(Variables)"/>.
		/// </summary>
		public override bool IsAsynchronous => true;

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			return this.EvaluateAsync(Arguments, Variables).Result;
		}

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override async Task<IElement> EvaluateAsync(IElement[] Arguments, Variables Variables)
		{
			int i = 0;
			string Index = Arguments[i++].AssociatedObjectValue?.ToString() ?? string.Empty;
			string FileName = Arguments[i++].AssociatedObjectValue?.ToString() ?? string.Empty;

			if (string.IsNullOrEmpty(FileName))
				throw new ScriptRuntimeException("Empty filename.", this);

			bool Result = await Persistence.FullTextSearch.Search.IndexFile(Index, FileName);

			return Result ? BooleanValue.True : BooleanValue.False;
		}
	}
}
