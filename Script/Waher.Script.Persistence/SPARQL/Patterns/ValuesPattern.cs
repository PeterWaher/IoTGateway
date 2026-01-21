using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content.Semantic;
using Waher.Runtime.Collections;
using Waher.Script.Model;

namespace Waher.Script.Persistence.SPARQL.Patterns
{
	/// <summary>
	/// A collection of predefined values
	/// </summary>
	public class ValuesPattern : ISparqlPattern
	{
		private readonly string[] variables;
		private readonly ISemanticElement[][] records;

		/// <summary>
		/// A collection of predefined values
		/// </summary>
		/// <param name="Variable">Variable name.</param>
		/// <param name="Values">Variable values.</param>
		public ValuesPattern(string Variable, ISemanticElement[] Values)
			: this(new string[] { Variable }, ToRecords(Values))
		{
		}

		private static ISemanticElement[][] ToRecords(ISemanticElement[] Values)
		{
			ChunkedList<ISemanticElement[]> Records = new ChunkedList<ISemanticElement[]>();

			foreach (ISemanticElement Element in Values)
				Records.Add(new ISemanticElement[] { Element });

			return Records.ToArray();
		}

		/// <summary>
		/// A collection of predefined values
		/// </summary>
		/// <param name="Variables">Variable names.</param>
		/// <param name="Records">Records.</param>
		public ValuesPattern(string[] Variables, ISemanticElement[][] Records)
		{
			this.variables = Variables;
			this.records = Records;
		}

		/// <summary>
		/// If pattern is empty.
		/// </summary>
		public bool IsEmpty => false;

		/// <summary>
		/// Searches for the pattern on information in a semantic cube.
		/// </summary>
		/// <param name="Cube">Semantic cube.</param>
		/// <param name="Variables">Script variables.</param>
		/// <param name="ExistingMatches">Existing matches.</param>
		/// <param name="Query">SPARQL-query being executed.</param>
		/// <returns>Matches.</returns>
		public Task<IEnumerable<Possibility>> Search(ISemanticCube Cube,
			Variables Variables, IEnumerable<Possibility> ExistingMatches, SparqlQuery Query)
		{
			ChunkedList<Possibility> Result = new ChunkedList<Possibility>();
			ISemanticElement Element;
			int i, c = this.variables.Length;

			if (ExistingMatches is null)
			{
				foreach (ISemanticElement[] Record in this.records)
				{
					for (i = 0; i < c; i++)
					{
						Element = Record[i];
						if (!(Element is null))
							Result.Add(new Possibility(this.variables[i], Element));
					}
				}
			}
			else
			{
				ISemanticElement Element0;
				string s;

				foreach (Possibility P in ExistingMatches)
				{
					foreach (ISemanticElement[] Record in this.records)
					{
						Possibility Extended = P;

						for (i = 0; i < c; i++)
						{
							Element = Record[i];
							if (Element is null)
								continue;

							s = this.variables[i];
							Element0 = P.GetValue(s);
							if (Element0 is null)
								Extended = new Possibility(s, Element, Extended);
							else if (!Element0.Equals(Element))
							{
								Extended = null;
								break;
							}
						}

						if (!(Extended is null))
							Result.Add(Extended);
					}
				}
			}

			return Task.FromResult<IEnumerable<Possibility>>(Result);
		}

		/// <summary>
		/// Sets the parent node. Can only be used when expression is being parsed or created.
		/// </summary>
		/// <param name="Parent">Parent Node</param>
		public void SetParent(ScriptNode Parent)
		{
			foreach (ISemanticElement[] Record in this.records)
			{
				foreach (ISemanticElement Element in Record)
				{
					if (Element is SemanticScriptElement ScriptNode)
						ScriptNode.Node.SetParent(Parent);
				}
			}
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
			if (Order == SearchMethod.DepthFirst)
			{
				foreach (ISemanticElement[] Record in this.records)
				{
					foreach (ISemanticElement Element in Record)
					{
						if (Element is SemanticScriptElement ScriptNode)
						{
							if (!ScriptNode.Node.ForAllChildNodes(Callback, State, Order))
								return false;
						}
					}
				}
			}

			this.ForAll(Callback, State, Order);

			if (Order == SearchMethod.BreadthFirst)
			{
				foreach (ISemanticElement[] Record in this.records)
				{
					foreach (ISemanticElement Element in Record)
					{
						if (Element is SemanticScriptElement ScriptNode)
						{
							if (!ScriptNode.Node.ForAllChildNodes(Callback, State, Order))
								return false;
						}
					}
				}
			}

			return true;
		}

		/// <summary>
		/// Calls the callback method for all child nodes.
		/// </summary>
		/// <param name="Callback">Callback method to call.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="Order">Order to traverse the nodes.</param>
		/// <returns>If the process was completed.</returns>
		public bool ForAll(ScriptNodeEventHandler Callback, object State, SearchMethod Order)
		{
			int i, c;

			foreach (ISemanticElement[] Record in this.records)
			{
				c = Record.Length;

				for (i = 0; i < c; i++)
				{
					ISemanticElement Element = Record[i];

					if (Element is SemanticScriptElement ScriptNode)
					{
						if (!Callback(ScriptNode.Node, out ScriptNode NewNode, State))
							return false;

						if (!(NewNode is null))
							Record[i] = new SemanticScriptElement(NewNode);
					}
				}
			}

			return true;
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			int c, d;

			if (!(obj is ValuesPattern Typed) ||
				(c = this.variables.Length) != Typed.variables.Length ||
				(d = this.records.Length) != Typed.records.Length)
			{
				return false;
			}

			int i, j;

			for (i = 0; i < c; i++)
			{
				if (!this.variables[i].Equals(Typed.variables[i]))
					return false;
			}

			for (j = 0; j < d; j++)
			{
				ISemanticElement[] Rec1 = this.records[j];
				ISemanticElement[] Rec2 = this.records[j];

				for (i = 0; i < c; i++)
				{
					ISemanticElement E1 = Rec1[i];
					ISemanticElement E2 = Rec2[i];

					if ((E1 is null) ^ (E2 is null))
						return false;

					if (!(E1 is null) && !Rec1[i].Equals(Rec2[i]))
						return false;
				}
			}

			return true;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int Result = base.GetHashCode();

			foreach (string s in this.variables)
				Result ^= Result << 5 ^ s.GetHashCode();

			foreach (ISemanticElement[] Record in this.records)
			{
				foreach (ISemanticElement Element in Record)
				{
					if (!(Element is null))
						Result ^= Result << 5 ^ Element.GetHashCode();
				}
			}

			return Result;
		}

	}
}
