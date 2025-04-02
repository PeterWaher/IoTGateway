using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content.Semantic;
using Waher.Runtime.Collections;

namespace Waher.Script.Persistence.SPARQL.Patterns
{
	/// <summary>
	/// Optional pattern.
	/// </summary>
	public class OptionalPattern : BinaryOperatorPattern
	{
		/// <summary>
		/// Optional pattern.
		/// </summary>
		/// <param name="Left">Left pattern</param>
		/// <param name="Right">Right pattern</param>
		public OptionalPattern(ISparqlPattern Left, ISparqlPattern Right)
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
		public override async Task<IEnumerable<Possibility>> Search(ISemanticCube Cube, 
			Variables Variables, IEnumerable<Possibility> ExistingMatches, SparqlQuery Query)
		{
			ExistingMatches = await this.Left.Search(Cube, Variables, ExistingMatches, Query);
			if (ExistingMatches is null)
				return null;

			ChunkedList<Possibility> NewMatches2 = new ChunkedList<Possibility>();

			foreach (Possibility P in ExistingMatches)
			{
				IEnumerable<Possibility> NewMatches = await this.Right.Search(Cube, Variables,
					new Possibility[] { P }, Query);

				if (NewMatches is null)
					NewMatches2.Add(P);
				else
				{
					foreach (Possibility P2 in NewMatches)
						NewMatches2.Add(P2);
				}
			}

			return NewMatches2;
		}
	}
}
