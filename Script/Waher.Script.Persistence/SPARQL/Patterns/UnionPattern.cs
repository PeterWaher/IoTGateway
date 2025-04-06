using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content.Semantic;
using Waher.Runtime.Collections;

namespace Waher.Script.Persistence.SPARQL.Patterns
{
	/// <summary>
	/// Union of two patterns.
	/// </summary>
	public class UnionPattern : BinaryOperatorPattern
	{
		/// <summary>
		/// Union of two patterns.
		/// </summary>
		/// <param name="Left">Left pattern</param>
		/// <param name="Right">Right pattern</param>
		public UnionPattern(ISparqlPattern Left, ISparqlPattern Right)
			: base(Left, Right)
		{
		}

		/// <summary>
		/// Searches for the pattern on information in a semantic cube.
		/// </summary>
		/// <param name="Cube">Semantic cube.</param>
		/// <param name="Variables">Script variables.</param>
		/// <param name="ExistingMatches">Existing matches.</param>
		/// <param name="Query">SPARQL-query being executed.</param>
		/// <returns>Matches.</returns>
		public override async Task<IEnumerable<Possibility>> Search(ISemanticCube Cube, Variables Variables,
			IEnumerable<Possibility> ExistingMatches, SparqlQuery Query)
		{
			IEnumerable<Possibility> Matches1 = await this.Left.Search(Cube, Variables, ExistingMatches, Query);
			IEnumerable<Possibility> Matches2 = await this.Right.Search(Cube, Variables, ExistingMatches, Query);

			if (Matches1 is null)
				return Matches2;
			else if (Matches2 is null)
				return Matches1;

			if (!(Matches1 is ChunkedList<Possibility> Result))
			{
				Result = new ChunkedList<Possibility>();
				Result.AddRange(Matches1);
			}

			Result.AddRange(Matches2);

			return Result;
		}
	}
}
