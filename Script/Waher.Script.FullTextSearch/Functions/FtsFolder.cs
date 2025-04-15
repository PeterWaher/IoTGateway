using System;
using System.Threading.Tasks;
using Waher.Persistence.FullTextSearch.Files;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.FullTextSearch.Functions
{
	/// <summary>
	/// Indexes files in a folder.
	/// </summary>
	public class FtsFolder : FunctionMultiVariate
	{
		/// <summary>
		/// Indexes files in a folder.
		/// </summary>
		/// <param name="IndexCollection">Name of Full-text-search index.</param>
		/// <param name="Folder">Folder to index.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public FtsFolder(ScriptNode IndexCollection, ScriptNode Folder,
			int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { IndexCollection, Folder }, argumentTypes2Scalar,
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Indexes files in a folder.
		/// </summary>
		/// <param name="IndexCollection">Name of Full-text-search index.</param>
		/// <param name="Folder">Folder to index.</param>
		/// <param name="Recursive">If search should be recursive through sub folders.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public FtsFolder(ScriptNode IndexCollection, ScriptNode Folder, ScriptNode Recursive,
			int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { IndexCollection, Folder, Recursive },
				  argumentTypes3Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Indexes files in a folder.
		/// </summary>
		/// <param name="IndexCollection">Name of Full-text-search index.</param>
		/// <param name="Folder">Folder to index.</param>
		/// <param name="Recursive">If search should be recursive through sub folders.</param>
		/// <param name="SubFoldersToExclude">Sub-folders to exclude from index.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public FtsFolder(ScriptNode IndexCollection, ScriptNode Folder, ScriptNode Recursive, ScriptNode SubFoldersToExclude,
			int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { IndexCollection, Folder, Recursive, SubFoldersToExclude },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Vector }, 
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(FtsFolder);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "IndexCollection", "Folder", "Recursive" };

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
			int c = Arguments.Length;
			string Index = Arguments[i++].AssociatedObjectValue?.ToString() ?? string.Empty;
			string Folder = Arguments[i++].AssociatedObjectValue?.ToString() ?? string.Empty;
			bool? Recursive = i < c ? ToBoolean(Arguments[i++]) : false;

			if (!Recursive.HasValue)
				throw new ScriptRuntimeException("Expected Boolean Value for Recursive argument.", this);

			string[] SubFoldersToExclude;

			if (i < c)
			{
				if (Arguments[i] is IVector V)
				{
					int j, d = V.Dimension;

					SubFoldersToExclude = new string[d];

					for (j = 0; j < d; j++)
						SubFoldersToExclude[j] = V.GetElement(j).AssociatedObjectValue?.ToString();
				}
				else
					SubFoldersToExclude = new string[] { Arguments[i].AssociatedObjectValue?.ToString() };
			}
			else
				SubFoldersToExclude = Array.Empty<string>();

			FolderIndexationStatistics Result = await Waher.Persistence.FullTextSearch.Search.IndexFolder(Index, Folder, Recursive.Value, SubFoldersToExclude);

			return new ObjectValue(Result);
		}
	}
}
