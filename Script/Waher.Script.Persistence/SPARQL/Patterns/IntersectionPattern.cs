using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content.Semantic;

namespace Waher.Script.Persistence.SPARQL.Patterns
{
	/// <summary>
	/// Intersection of two patterns.
	/// </summary>
	public class IntersectionPattern : BinaryOperatorPattern
	{
		/// <summary>
		/// Intersection of two patterns.
		/// </summary>
		/// <param name="Left">Left pattern</param>
		/// <param name="Right">Right pattern</param>
		public IntersectionPattern(ISparqlPattern Left, ISparqlPattern Right)
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
			ExistingMatches = await this.Left.Search(Cube, Variables, ExistingMatches, Query);
			if (ExistingMatches is null)
				return null;

			return await this.Right.Search(Cube, Variables, ExistingMatches, Query);
		}
	}
}
