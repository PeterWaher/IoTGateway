using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content.Semantic;
using Waher.Script.Model;

namespace Waher.Script.Persistence.SPARQL.Patterns
{
	/// <summary>
	/// Intersection of two patterns.
	/// </summary>
	public abstract class BinaryOperatorPattern : ISparqlPattern
	{
		private readonly ISparqlPattern left;
		private readonly ISparqlPattern right;

		/// <summary>
		/// Intersection of two patterns.
		/// </summary>
		/// <param name="Left">Left pattern</param>
		/// <param name="Right">Right pattern</param>
		public BinaryOperatorPattern(ISparqlPattern Left, ISparqlPattern Right)
		{
			this.left = Left;
			this.right = Right;
		}

		/// <summary>
		/// Left pattern
		/// </summary>
		public ISparqlPattern Left => this.left;

		/// <summary>
		/// Right pattern
		/// </summary>
		public ISparqlPattern Right => this.right;

		/// <summary>
		/// If pattern is empty.
		/// </summary>
		public bool IsEmpty => false;

		/// <summary>
		/// Calls the callback method for all child nodes.
		/// </summary>
		/// <param name="Callback">Callback method to call.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="Order">Order to traverse the nodes.</param>
		/// <returns>If the process was completed.</returns>
		public bool ForAll(ScriptNodeEventHandler Callback, object State, SearchMethod Order)
		{
			if (!this.left.ForAll(Callback, State, Order))
				return false;

			if (!this.right.ForAll(Callback, State, Order))
				return false;

			return true;
		}

		/// <summary>
		/// Calls the callback method for all child nodes.
		/// </summary>
		/// <param name="Callback">Callback method to call.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="Order">Order to traverse the nodes.</param>
		/// <returns>If the process was completed.</returns>
		public bool ForAllChildNodes(ScriptNodeEventHandler Callback, object State, SearchMethod Order)
		{
			if (!this.left.ForAllChildNodes(Callback, State, Order))
				return false;

			if (!this.right.ForAllChildNodes(Callback, State, Order))
				return false;

			return true;
		}

		/// <summary>
		/// Searches for the pattern on information in a semantic cube.
		/// </summary>
		/// <param name="Cube">Semantic cube.</param>
		/// <param name="Variables">Script variables.</param>
		/// <param name="ExistingMatches">Existing matches.</param>
		/// <param name="Query">SPARQL-query being executed.</param>
		/// <returns>Matches.</returns>
		public abstract Task<IEnumerable<Possibility>> Search(ISemanticCube Cube, Variables Variables, 
			IEnumerable<Possibility> ExistingMatches, SparqlQuery Query);

		/// <summary>
		/// Sets the parent node. Can only be used when expression is being parsed or created.
		/// </summary>
		/// <param name="Parent">Parent Node</param>
		public void SetParent(ScriptNode Parent)
		{
			this.left.SetParent(Parent);
			this.right.SetParent(Parent);
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			if (!(obj is BinaryOperatorPattern Typed) ||
				this.GetType() != obj.GetType())
			{
				return false;
			}

			return this.left.Equals(Typed.left) &&
				this.right.Equals(Typed.right);
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int Result = this.GetType().GetHashCode();
			Result ^= Result << 5 ^ this.left.GetHashCode();
			Result ^= Result << 5 ^ this.right.GetHashCode();

			return Result;
		}
	}
}
