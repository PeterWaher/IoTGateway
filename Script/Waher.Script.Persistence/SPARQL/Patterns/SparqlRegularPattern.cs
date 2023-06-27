using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content.Semantic;
using Waher.Script.Model;
using Waher.Script.Persistence.SPARQL.Filters;
using Waher.Script.Persistence.SQL;

namespace Waher.Script.Persistence.SPARQL.Patterns
{
	/// <summary>
	/// Represents a pattern in a SPARQL query.
	/// </summary>
	public class SparqlRegularPattern : ISparqlPattern
	{
		private LinkedList<SemanticQueryTriple> triples = null;
		private LinkedList<KeyValuePair<ScriptNode, ScriptNode>> boundVariables = null;
		private LinkedList<IFilterNode> filter = null;

		/// <summary>
		/// Represents a pattern in a SPARQL query.
		/// </summary>
		public SparqlRegularPattern()
		{
		}

		/// <summary>
		/// If pattern is empty.
		/// </summary>
		public bool IsEmpty => !(this.HasTriples || this.HasBoundVariables || this.HasFilter);

		/// <summary>
		/// Triples, null if none.
		/// </summary>
		public IEnumerable<SemanticQueryTriple> Triples => this.triples;

		/// <summary>
		/// Bound variables, null if none.
		/// </summary>
		public IEnumerable<KeyValuePair<ScriptNode, ScriptNode>> BoundVariables => this.boundVariables;

		/// <summary>
		/// Filter, null if none.
		/// </summary>
		public IEnumerable<IFilterNode> Filter => this.filter;

		/// <summary>
		/// If pattern has triples
		/// </summary>
		public bool HasTriples => !(this.triples is null);

		/// <summary>
		/// If pattern has bound variables
		/// </summary>
		public bool HasBoundVariables => !(this.boundVariables is null);

		/// <summary>
		/// If pattern has filter
		/// </summary>
		public bool HasFilter => !(this.filter is null);

		/// <summary>
		/// Adds a triple to the pattern
		/// </summary>
		/// <param name="Triple">Triple</param>
		public void AddTriple(SemanticQueryTriple Triple)
		{
			if (this.triples is null)
				this.triples = new LinkedList<SemanticQueryTriple>();

			this.triples.AddLast(Triple);
		}

		/// <summary>
		/// Adds a variable binding to the pattern.
		/// </summary>
		/// <param name="Value">Value to bind.</param>
		/// <param name="Variable">Variable to bind to.</param>
		public void AddVariableBinding(ScriptNode Value, ScriptNode Variable)
		{
			if (this.boundVariables is null)
				this.boundVariables = new LinkedList<KeyValuePair<ScriptNode, ScriptNode>>();

			this.boundVariables.AddLast(new KeyValuePair<ScriptNode, ScriptNode>(Value, Variable));
		}

		/// <summary>
		/// Adds a filter to the pattern.
		/// </summary>
		/// <param name="Filter">Filter.</param>
		public void AddFilter(ScriptNode Filter)
		{
			if (this.filter is null)
				this.filter = new LinkedList<IFilterNode>();

			if (Filter is IFilterNode FilterNode)
				this.filter.AddLast(FilterNode);
			else
				this.filter.AddLast(new FilterScriptNode(Filter));
		}

		/// <summary>
		/// Searches for the pattern on information in a semantic cube.
		/// </summary>
		/// <param name="Cube">Semantic cube.</param>
		/// <param name="Variables">Script variables.</param>
		/// <param name="Query">SPARQL-query being executed.</param>
		/// <returns>Matches.</returns>
		public Task<IEnumerable<Possibility>> Search(ISemanticCube Cube, Variables Variables,
			SparqlQuery Query)
		{
			return this.Search(Cube, Variables, null, Query);
		}

		/// <summary>
		/// Searches for the pattern on information in a semantic cube.
		/// </summary>
		/// <param name="Cube">Semantic cube.</param>
		/// <param name="Variables">Script variables.</param>
		/// <param name="ExistingMatches">Existing matches.</param>
		/// <param name="Query">SPARQL-query being executed.</param>
		/// <returns>Matches.</returns>
		public async Task<IEnumerable<Possibility>> Search(ISemanticCube Cube,
			Variables Variables, IEnumerable<Possibility> ExistingMatches,
			SparqlQuery Query)
		{
			if (this.HasTriples)
			{
				foreach (SemanticQueryTriple T in this.triples)
				{
					switch (T.Type)
					{
						case QueryTripleType.Constant:
							if (await Cube.GetTriplesBySubjectAndPredicateAndObject(T.Subject, T.Predicate, T.Object) is null)
								return null;
							break;

						case QueryTripleType.SubjectVariable:
							ExistingMatches = await this.CrossPossibilitiesOneVariable(ExistingMatches, T, 0, 1, 2, Cube);
							break;

						case QueryTripleType.PredicateVariable:
							ExistingMatches = await this.CrossPossibilitiesOneVariable(ExistingMatches, T, 1, 0, 2, Cube);
							break;

						case QueryTripleType.ObjectVariable:
							ExistingMatches = await this.CrossPossibilitiesOneVariable(ExistingMatches, T, 2, 0, 1, Cube);
							break;

						case QueryTripleType.SubjectPredicateVariables:
							ExistingMatches = await this.CrossPossibilitiesTwoVariables(ExistingMatches, T, 0, 1, 2, Cube);
							break;

						case QueryTripleType.SubjectObjectVariable:
							ExistingMatches = await this.CrossPossibilitiesTwoVariables(ExistingMatches, T, 0, 2, 1, Cube);
							break;

						case QueryTripleType.PredicateObjectVariable:
							ExistingMatches = await this.CrossPossibilitiesTwoVariables(ExistingMatches, T, 1, 2, 0, Cube);
							break;

						case QueryTripleType.SubjectPredicateObjectVariable:
							ExistingMatches = await this.CrossPossibilitiesThreeVariables(ExistingMatches, T, Cube);
							break;
					}

					if (ExistingMatches is null || !ExistingMatches.GetEnumerator().MoveNext())
						return null;
				}
			}

			if (this.HasBoundVariables && !(ExistingMatches is null))
			{
				LinkedList<Possibility> NewMatches = new LinkedList<Possibility>();
				ObjectProperties RecordVariables = null;
				Possibility P;
				string Name;

				foreach (Possibility Possibility in ExistingMatches)
				{
					P = Possibility;

					foreach (KeyValuePair<ScriptNode, ScriptNode> P2 in this.boundVariables)
					{
						if (RecordVariables is null)
							RecordVariables = new ObjectProperties(P, Variables);
						else
							RecordVariables.Object = P;

						if (P2.Value is VariableReference Ref)
							Name = Ref.VariableName;
						else
						{
							Name = (await SparqlQuery.EvaluateValue(RecordVariables, P2.Value))?.ToString();
							if (string.IsNullOrEmpty(Name))
								continue;
						}

						ISemanticElement Literal = await Query.EvaluateSemanticElement(RecordVariables, P2.Key);

						P = new Possibility(Name, Literal, P);
					}

					NewMatches.AddLast(P);
				}

				ExistingMatches = NewMatches;
			}

			if (this.HasFilter && !(ExistingMatches is null))
			{
				LinkedList<Possibility> Filtered = null;
				ObjectProperties RecordVariables = null;
				bool Pass;

				foreach (Possibility P in ExistingMatches)
				{
					if (RecordVariables is null)
						RecordVariables = new ObjectProperties(P, Variables);
					else
						RecordVariables.Object = P;

					Pass = true;

					foreach (IFilterNode Filter in this.filter)
					{
						object Value = await SparqlQuery.EvaluateValue(RecordVariables, Filter, Cube, Query, P);
						if (!(Value is bool b) || !b)
						{
							Pass = false;
							break;
						}
					}

					if (Pass)
					{
						if (Filtered is null)
							Filtered = new LinkedList<Possibility>();

						Filtered.AddLast(P);
					}
				}

				ExistingMatches = Filtered;
			}

			return ExistingMatches;
		}

		private async Task<IEnumerable<Possibility>> CrossPossibilitiesOneVariable(
			IEnumerable<Possibility> Possibilities, SemanticQueryTriple T, int VariableIndex,
			int ValueIndex1, int ValueIndex2, ISemanticCube Cube)
		{
			LinkedList<Possibility> NewPossibilities = null;
			string Name = T.VariableName(VariableIndex);
			ISemanticElement Value;

			if (Possibilities is null)
			{
				IEnumerable<ISemanticTriple> NewTriples = await Cube.GetTriples(
					T[ValueIndex1], ValueIndex1, T[ValueIndex2], ValueIndex2);

				if (NewTriples is null)
					return null;

				NewPossibilities = new LinkedList<Possibility>();

				foreach (ISemanticTriple T2 in NewTriples)
					NewPossibilities.AddLast(new Possibility(Name, T2[VariableIndex]));
			}
			else
			{
				foreach (Possibility P in Possibilities)
				{
					Value = P.GetValue(Name);
					if (Value is null)
						continue;

					if (await Cube.GetTriplesBySubjectAndPredicateAndObject(Value, T.Predicate, T.Object) is null)
						continue;

					if (NewPossibilities is null)
						NewPossibilities = new LinkedList<Possibility>();

					NewPossibilities.AddLast(P);
				}
			}

			return NewPossibilities;
		}

		private async Task<IEnumerable<Possibility>> CrossPossibilitiesTwoVariables(
			IEnumerable<Possibility> Possibilities, SemanticQueryTriple T, int VariableIndex1,
			int VariableIndex2, int ValueIndex, ISemanticCube Cube)
		{
			LinkedList<Possibility> NewPossibilities = null;
			string Name = T.VariableName(VariableIndex1);
			string Name2 = T.VariableName(VariableIndex2);
			ISemanticElement Value;
			ISemanticElement Value2;
			bool SameName = Name == Name2;

			if (Possibilities is null)
			{
				IEnumerable<ISemanticTriple> Triples = await Cube.GetTriples(T[ValueIndex], ValueIndex);
				if (Triples is null)
					return null;

				foreach (ISemanticTriple T2 in Triples)
				{
					if (SameName)
					{
						ISemanticElement E = T2[VariableIndex1];

						if (!E.Equals(T2[VariableIndex2]))
							continue;

						if (NewPossibilities is null)
							NewPossibilities = new LinkedList<Possibility>();

						NewPossibilities.AddLast(new Possibility(Name, E));
					}
					else
					{
						if (NewPossibilities is null)
							NewPossibilities = new LinkedList<Possibility>();

						NewPossibilities.AddLast(
							new Possibility(Name, T2[VariableIndex1],
							new Possibility(Name2, T2[VariableIndex2])));
					}
				}
			}
			else
			{
				foreach (Possibility P in Possibilities)
				{
					Value = P.GetValue(Name);

					if (SameName)
						Value2 = Value;
					else
						Value2 = P.GetValue(Name2);

					bool IsProcessed = !(Value is null);
					bool IsProcessed2 = !(Value2 is null);

					if (IsProcessed && IsProcessed2)
					{
						if (await Cube.GetTriplesBySubjectAndPredicateAndObject(Value, Value2, T[ValueIndex]) is null)
							continue;

						if (NewPossibilities is null)
							NewPossibilities = new LinkedList<Possibility>();

						NewPossibilities.AddLast(P);
					}
					else if (IsProcessed)
					{
						this.CrossPossibilities(
							await Cube.GetTriples(Value, VariableIndex1, T[ValueIndex], ValueIndex),
							P, Name2, VariableIndex2, ref NewPossibilities);
					}
					else if (IsProcessed2)
					{
						this.CrossPossibilities(
							await Cube.GetTriples(Value2, VariableIndex2, T[ValueIndex], ValueIndex),
							P, Name, VariableIndex1, ref NewPossibilities);
					}
					else
					{
						IEnumerable<ISemanticTriple> Triples = await Cube.GetTriples(T[ValueIndex], ValueIndex);
						if (Triples is null)
							return null;

						foreach (ISemanticTriple T2 in Triples)
						{
							if (SameName)
							{
								ISemanticElement E = T2[VariableIndex1];

								if (!E.Equals(T2[VariableIndex2]))
									continue;

								if (NewPossibilities is null)
									NewPossibilities = new LinkedList<Possibility>();

								NewPossibilities.AddLast(
									new Possibility(Name, E, P));
							}
							else
							{
								if (NewPossibilities is null)
									NewPossibilities = new LinkedList<Possibility>();

								NewPossibilities.AddLast(
									new Possibility(Name, T2[VariableIndex1],
									new Possibility(Name2, T2[VariableIndex2], P)));
							}
						}
					}

				}
			}

			return NewPossibilities;
		}

		private async Task<IEnumerable<Possibility>> CrossPossibilitiesThreeVariables(
			IEnumerable<Possibility> Possibilities, SemanticQueryTriple T, ISemanticCube Cube)
		{
			LinkedList<Possibility> NewPossibilities = null;
			string Name = T.SubjectVariable;
			string Name2 = T.PredicateVariable;
			string Name3 = T.ObjectVariable;
			bool SameName = Name == Name2;
			bool SameName2 = Name == Name3;
			bool SameName3 = Name2 == Name3;

			if (Possibilities is null)
			{
				foreach (ISemanticTriple T2 in Cube)
				{
					if (SameName && !T2.Subject.Equals(T2.Predicate))
						continue;

					if (SameName2 && !T2.Subject.Equals(T2.Object))
						continue;

					if (SameName3 && !T2.Predicate.Equals(T2.Object))
						continue;

					Possibility NewPossibility = new Possibility(Name, T2.Subject);

					if (!SameName)
						NewPossibility = new Possibility(Name2, T2.Predicate, NewPossibility);

					if (!SameName2 && !SameName3)
						NewPossibility = new Possibility(Name3, T2.Object, NewPossibility);

					if (NewPossibilities is null)
						NewPossibilities = new LinkedList<Possibility>();

					NewPossibilities.AddLast(NewPossibility);
				}
			}
			else
			{
				foreach (Possibility P in Possibilities)
				{
					ISemanticElement Value = P.GetValue(Name);
					ISemanticElement Value2 = SameName ? Value : P.GetValue(Name2);
					ISemanticElement Value3 = SameName2 ? Value : SameName3 ? Value2 : P.GetValue(Name3);
					bool IsProcessed = !(Value is null);
					bool IsProcessed2 = !(Value2 is null);
					bool IsProcessed3 = !(Value3 is null);

					if (IsProcessed && IsProcessed2 && IsProcessed3)
					{
						if (await Cube.GetTriplesBySubjectAndPredicateAndObject(Value, Value2, Value3) is null)
							continue;

						if (NewPossibilities is null)
							NewPossibilities = new LinkedList<Possibility>();

						NewPossibilities.AddLast(P);
					}
					else if (IsProcessed && IsProcessed2)   // Subject & Predicate variables already processed
					{
						this.CrossPossibilities(
							await Cube.GetTriplesBySubjectAndPredicate(Value, Value2),
							P, Name3, 2, ref NewPossibilities);
					}
					else if (IsProcessed && IsProcessed3)   // Subject & Object variables already processed
					{
						this.CrossPossibilities(
							await Cube.GetTriplesBySubjectAndObject(Value, Value3),
							P, Name2, 1, ref NewPossibilities);
					}
					else if (IsProcessed2 && IsProcessed3)  // Predicate & Object variables already processed
					{
						this.CrossPossibilities(
							await Cube.GetTriplesByPredicateAndObject(Value2, Value3),
							P, Name, 0, ref NewPossibilities);
					}
					else if (IsProcessed)                   // Subject variable processed
					{
						ISemanticPlane Plane = await Cube.GetTriplesBySubject(Value);
						if (Plane is null)
							continue;

						foreach (ISemanticTriple T2 in Plane)
						{
							if (SameName3)
							{
								if (!T2.Predicate.Equals(T2.Object))
									continue;

								if (NewPossibilities is null)
									NewPossibilities = new LinkedList<Possibility>();

								NewPossibilities.AddLast(
									new Possibility(Name2, T2.Predicate, P));
							}
							else
							{
								if (NewPossibilities is null)
									NewPossibilities = new LinkedList<Possibility>();

								NewPossibilities.AddLast(
									new Possibility(Name2, T2.Predicate,
									new Possibility(Name3, T2.Object, P)));
							}
						}
					}
					else if (IsProcessed2)                  // Predicate variable processed
					{
						ISemanticPlane Plane = await Cube.GetTriplesByPredicate(Value2);
						if (Plane is null)
							continue;

						foreach (ISemanticTriple T2 in Plane)
						{
							if (SameName2)
							{
								if (!T2.Subject.Equals(T2.Object))
									continue;

								if (NewPossibilities is null)
									NewPossibilities = new LinkedList<Possibility>();

								NewPossibilities.AddLast(
									new Possibility(Name, T2.Subject, P));
							}
							else
							{
								if (NewPossibilities is null)
									NewPossibilities = new LinkedList<Possibility>();

								NewPossibilities.AddLast(
									new Possibility(Name, T2.Subject,
									new Possibility(Name3, T2.Object, P)));
							}
						}
					}
					else if (IsProcessed3)                  // Object variable processed
					{
						ISemanticPlane Plane = await Cube.GetTriplesByObject(Value3);
						if (Plane is null)
							continue;

						foreach (ISemanticTriple T2 in Plane)
						{
							if (SameName)
							{
								if (!T2.Subject.Equals(T2.Predicate))
									continue;

								if (NewPossibilities is null)
									NewPossibilities = new LinkedList<Possibility>();

								NewPossibilities.AddLast(
									new Possibility(Name, T2.Subject, P));
							}
							else
							{
								if (NewPossibilities is null)
									NewPossibilities = new LinkedList<Possibility>();

								NewPossibilities.AddLast(
									new Possibility(Name, T2.Subject,
									new Possibility(Name2, T2.Predicate, P)));
							}
						}
					}
					else
					{
						foreach (ISemanticTriple T2 in Cube)
						{
							if (SameName && !T2.Subject.Equals(T2.Predicate))
								continue;

							if (SameName2 && !T2.Subject.Equals(T2.Object))
								continue;

							if (SameName3 && !T2.Predicate.Equals(T2.Object))
								continue;

							Possibility NewPossibility = new Possibility(Name, T2.Subject, P);

							if (!SameName)
								NewPossibility = new Possibility(Name2, T2.Predicate, NewPossibility);

							if (!SameName2 && !SameName3)
								NewPossibility = new Possibility(Name3, T2.Object, NewPossibility);

							if (NewPossibilities is null)
								NewPossibilities = new LinkedList<Possibility>();

							NewPossibilities.AddLast(NewPossibility);
						}
					}
				}
			}

			return NewPossibilities;
		}

		private void CrossPossibilities(IEnumerable<ISemanticTriple> NewTriples,
			Possibility Possibility, string Name, int Index, ref LinkedList<Possibility> NewPossibilities)
		{
			if (NewTriples is null)
				return;

			if (NewPossibilities is null)
				NewPossibilities = new LinkedList<Possibility>();

			foreach (ISemanticTriple T2 in NewTriples)
				NewPossibilities.AddLast(new Possibility(Name, T2[Index], Possibility));
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
				if (!(this.triples is null))
				{
					foreach (ISemanticTriple T in this.triples)
					{
						if (T.Subject is SemanticScriptElement S)
						{
							if (!S.Node.ForAllChildNodes(Callback, State, Order))
								return false;
						}

						if (T.Predicate is SemanticScriptElement P)
						{
							if (!P.Node.ForAllChildNodes(Callback, State, Order))
								return false;
						}

						if (T.Object is SemanticScriptElement O)
						{
							if (!O.Node.ForAllChildNodes(Callback, State, Order))
								return false;
						}
					}
				}

				if (!(this.boundVariables is null))
				{
					foreach (KeyValuePair<ScriptNode, ScriptNode> P in this.boundVariables)
					{
						if (!P.Key.ForAllChildNodes(Callback, State, Order))
							return false;

						if (!P.Value.ForAllChildNodes(Callback, State, Order))
							return false;
					}
				}

				if (!(this.filter is null))
				{
					foreach (IFilterNode P in this.filter)
					{
						if (!P.ScriptNode.ForAllChildNodes(Callback, State, Order))
							return false;
					}
				}
			}

			this.ForAll(Callback, State, Order);

			if (Order == SearchMethod.BreadthFirst)
			{
				if (!(this.triples is null))
				{
					foreach (ISemanticTriple T in this.triples)
					{
						if (T.Subject is SemanticScriptElement S)
						{
							if (!S.Node.ForAllChildNodes(Callback, State, Order))
								return false;
						}

						if (T.Predicate is SemanticScriptElement P)
						{
							if (!P.Node.ForAllChildNodes(Callback, State, Order))
								return false;
						}

						if (T.Object is SemanticScriptElement O)
						{
							if (!O.Node.ForAllChildNodes(Callback, State, Order))
								return false;
						}
					}
				}

				if (!(this.boundVariables is null))
				{
					foreach (KeyValuePair<ScriptNode, ScriptNode> P in this.boundVariables)
					{
						if (!P.Key.ForAllChildNodes(Callback, State, Order))
							return false;

						if (!P.Value.ForAllChildNodes(Callback, State, Order))
							return false;
					}
				}

				if (!(this.filter is null))
				{
					foreach (IFilterNode P in this.filter)
					{
						if (!P.ScriptNode.ForAllChildNodes(Callback, State, Order))
							return false;
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
			if (!(this.triples is null))
			{
				foreach (ISemanticTriple T in this.triples)
				{
					if (T.Subject is SemanticScriptElement S)
					{
						if (!S.ForAll(Callback, State, Order))
							return false;
					}

					if (T.Predicate is SemanticScriptElement P)
					{
						if (!P.ForAll(Callback, State, Order))
							return false;
					}

					if (T.Object is SemanticScriptElement O)
					{
						if (!O.ForAll(Callback, State, Order))
							return false;
					}
				}
			}

			if (!(this.boundVariables is null))
			{
				LinkedListNode<KeyValuePair<ScriptNode, ScriptNode>> Loop = this.boundVariables.First;

				while (!(Loop is null))
				{
					if (!Callback(Loop.Value.Key, out ScriptNode NewKey, State))
						return false;

					if (!Callback(Loop.Value.Value, out ScriptNode NewValue, State))
						return false;

					if (!(NewKey is null) || !(NewValue is null))
					{
						Loop.Value = new KeyValuePair<ScriptNode, ScriptNode>(
							NewKey ?? Loop.Value.Key, NewValue ?? Loop.Value.Value);
					}

					Loop = Loop.Next;
				}
			}

			if (!(this.filter is null))
			{
				LinkedListNode<IFilterNode> Loop = this.filter.First;

				while (!(Loop is null))
				{
					if (!Callback(Loop.Value.ScriptNode, out ScriptNode NewValue, State))
						return false;

					if (!(NewValue is null))
					{
						if (NewValue is IFilterNode NewFilterNode)
							Loop.Value = NewFilterNode;
						else
							Loop.Value = new FilterScriptNode(NewValue);
					}

					Loop = Loop.Next;
				}
			}

			return true;
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			if (!(obj is SparqlRegularPattern Typed) ||
				this.triples is null ^ Typed.triples is null ||
				this.boundVariables is null ^ Typed.boundVariables is null ||
				this.filter is null ^ Typed.filter is null)
			{
				return false;
			}

			if (!(this.boundVariables is null))
			{
				IEnumerator<KeyValuePair<ScriptNode, ScriptNode>> e1 = this.boundVariables.GetEnumerator();
				IEnumerator<KeyValuePair<ScriptNode, ScriptNode>> e2 = Typed.boundVariables.GetEnumerator();
				bool b1 = e1.MoveNext();
				bool b2 = e2.MoveNext();

				while (b1 && b2)
				{
					if (!e1.Current.Equals(e2.Current))
						return false;

					b1 = e1.MoveNext();
					b2 = e2.MoveNext();
				}

				if (b1 || b2)
					return false;
			}

			if (!(this.filter is null))
			{
				IEnumerator<IFilterNode> e1 = this.filter.GetEnumerator();
				IEnumerator<IFilterNode> e2 = Typed.filter.GetEnumerator();
				bool b1 = e1.MoveNext();
				bool b2 = e2.MoveNext();

				while (b1 && b2)
				{
					if (!e1.Current.Equals(e2.Current))
						return false;

					b1 = e1.MoveNext();
					b2 = e2.MoveNext();
				}

				if (b1 || b2)
					return false;
			}

			return true;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int Result = base.GetHashCode();

			if (!(this.triples is null))
			{
				foreach (SemanticQueryTriple T in this.triples)
					Result ^= Result << 5 ^ T.GetHashCode();
			}

			if (!(this.boundVariables is null))
			{
				foreach (KeyValuePair<ScriptNode, ScriptNode> P in this.boundVariables)
				{
					Result ^= Result << 5 ^ P.Key.GetHashCode();
					Result ^= Result << 5 ^ P.Value.GetHashCode();
				}
			}

			if (!(this.filter is null))
			{
				foreach (IFilterNode N in this.filter)
					Result ^= Result << 5 ^ N.GetHashCode();
			}

			return Result;
		}

		/// <summary>
		/// Sets the parent node. Can only be used when expression is being parsed.
		/// </summary>
		/// <param name="Parent">Parent Node</param>
		public void SetParent(ScriptNode Parent)
		{
			if (!(this.triples is null))
			{
				foreach (ISemanticTriple T in this.triples)
				{
					if (T.Subject is SemanticScriptElement S)
						S.Node.SetParent(Parent);

					if (T.Predicate is SemanticScriptElement P)
						P.Node.SetParent(Parent);

					if (T.Object is SemanticScriptElement O)
						O.Node.SetParent(Parent);
				}
			}

			if (!(this.boundVariables is null))
			{
				foreach (KeyValuePair<ScriptNode, ScriptNode> P in this.boundVariables)
				{
					P.Key.SetParent(Parent);
					P.Value.SetParent(Parent);
				}
			}

			if (!(this.filter is null))
			{
				foreach (IFilterNode P in this.filter)
					P.ScriptNode.SetParent(Parent);
			}
		}
	}
}
