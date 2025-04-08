using System;
using System.Reflection;
using System.Threading.Tasks;
using Waher.Persistence.FullTextSearch;
using Waher.Persistence.FullTextSearch.Keywords;
using Waher.Persistence.Serialization;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects.VectorSpaces;
using Waher.Runtime.Inventory;
using Waher.Script.Operators.Vectors;

namespace Waher.Script.FullTextSearch.Functions
{
	/// <summary>
	/// Full-Text-search
	/// </summary>
	public class Search : FunctionMultiVariate
	{
		/// <summary>
		/// Full-Text-search
		/// </summary>
		/// <param name="IndexCollection">Full-text-search index.</param>
		/// <param name="Query">Query string.</param>
		/// <param name="Strict">If keywords should be used as defined (true), or
		/// as prefixes (false).</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public Search(ScriptNode IndexCollection, ScriptNode Query, ScriptNode Strict,
			int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { IndexCollection, Query, Strict }, argumentTypes3Scalar,
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Full-Text-search
		/// </summary>
		/// <param name="IndexCollection">Full-text-search index.</param>
		/// <param name="Query">Query string.</param>
		/// <param name="Strict">If keywords should be used as defined (true), or
		/// as prefixes (false).</param>
		/// <param name="Offset">Offset into search result.</param>
		/// <param name="MaxCount">Maximum number of items to return.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public Search(ScriptNode IndexCollection, ScriptNode Query, ScriptNode Strict,
			ScriptNode Offset, ScriptNode MaxCount, int Start, int Length,
			Expression Expression)
			: base(new ScriptNode[] { IndexCollection, Query, Strict, Offset, MaxCount },
				  argumentTypes5Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Full-Text-search
		/// </summary>
		/// <param name="IndexCollection">Full-text-search index.</param>
		/// <param name="Query">Query string.</param>
		/// <param name="Strict">If keywords should be used as defined (true), or
		/// as prefixes (false).</param>
		/// <param name="Offset">Offset into search result.</param>
		/// <param name="MaxCount">Maximum number of items to return.</param>
		/// <param name="Order">Sort order.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public Search(ScriptNode IndexCollection, ScriptNode Query, ScriptNode Strict,
			ScriptNode Offset, ScriptNode MaxCount, ScriptNode Order, int Start, int Length,
			Expression Expression)
			: base(new ScriptNode[] { IndexCollection, Query, Strict, Offset, MaxCount, Order },
				  argumentTypes6Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Full-Text-search
		/// </summary>
		/// <param name="IndexCollection">Full-text-search index.</param>
		/// <param name="Query">Query string.</param>
		/// <param name="Strict">If keywords should be used as defined (true), or
		/// as prefixes (false).</param>
		/// <param name="Offset">Offset into search result.</param>
		/// <param name="MaxCount">Maximum number of items to return.</param>
		/// <param name="Order">Sort order.</param>
		/// <param name="PaginationStrategy">Pagination Strategy</param>
		/// <param name="Type">Type of object to search for.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public Search(ScriptNode IndexCollection, ScriptNode Query, ScriptNode Strict,
			ScriptNode Offset, ScriptNode MaxCount, ScriptNode Order,
			ScriptNode Type, ScriptNode PaginationStrategy, int Start, int Length,
			Expression Expression)
			: base(new ScriptNode[] { IndexCollection, Query, Strict, Offset, MaxCount, Order, Type, PaginationStrategy },
				  argumentTypes8Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(Search);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[]
		{
			"Index", "Query", "Strict", "Offset", "MaxCount", "Order", "PaginationStrategy"
		};

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

			string IndexCollection = Arguments[i++].AssociatedObjectValue?.ToString() ?? string.Empty;
			string Query = Arguments[i++].AssociatedObjectValue?.ToString() ?? string.Empty;
			bool? Strict = ToBoolean(Arguments[i++]);

			if (!Strict.HasValue)
				throw new ScriptRuntimeException("Expected boolean Strict argument.", this);

			Keyword[] Keywords = Waher.Persistence.FullTextSearch.Search.ParseKeywords(Query, !Strict.Value);

			int Offset = i < c ? (int)Expression.ToDouble(Arguments[i++]) : 0;
			int MaxCount = i < c ? (int)Expression.ToDouble(Arguments[i++]) : int.MaxValue;
			FullTextSearchOrder Order = i < c ? this.ToEnum<FullTextSearchOrder>(Arguments[i++]) : FullTextSearchOrder.Relevance;
			PaginationStrategy Strategy = i < c ? this.ToEnum<PaginationStrategy>(Arguments[i++]) : PaginationStrategy.PaginateOverObjectsNullIfIncompatible;

			if (i >= c)
			{
				GenericObject[] Result = await Waher.Persistence.FullTextSearch.Search.FullTextSearch<GenericObject>(
					IndexCollection, Offset, MaxCount, Order, Strategy, Keywords);

				return new ObjectVector(Result);
			}
			else
			{
				object Obj = Arguments[i++]?.AssociatedObjectValue;

				if (!(Obj is Type Type))
				{
					string TypeName = Obj?.ToString() ?? string.Empty;
					Type = Types.GetType(TypeName);
					if (Type is null)
						throw new ScriptRuntimeException("Type not found: " + TypeName, this);
				}

				MethodInfo MI = fullTextSearchMethod.MakeGenericMethod(Type);

				Obj = MI.Invoke(null, new object[]
				{
					IndexCollection, Offset, MaxCount, Order, Strategy, Keywords
				});

				Obj = await WaitPossibleTask(Obj);

				if (Obj is Array A)
					return VectorDefinition.Encapsulate(A, false, this);
				else
					return Expression.Encapsulate(Obj);
			}
		}

		private static readonly Type searchClass = typeof(Waher.Persistence.FullTextSearch.Search);
		private static readonly MethodInfo fullTextSearchMethod = GetFullTextSearchMethod();

		private static MethodInfo GetFullTextSearchMethod()
		{
			foreach (MethodInfo MI in searchClass.GetTypeInfo().GetDeclaredMethods("FullTextSearch"))
			{
				ParameterInfo[] P = MI.GetParameters();
				if (P.Length != 6 ||
					P[0].ParameterType != typeof(string) ||
					P[1].ParameterType != typeof(int) ||
					P[2].ParameterType != typeof(int) ||
					P[3].ParameterType != typeof(FullTextSearchOrder) ||
					P[3].ParameterType != typeof(PaginationStrategy) ||
					P[3].ParameterType != typeof(Keyword[]))
				{
					continue;
				}

				return MI;
			}

			return null;
		}
	}
}
