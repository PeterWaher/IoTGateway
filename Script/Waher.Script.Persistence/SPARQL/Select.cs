using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Semantic;
using Waher.Content.Semantic.Model;
using Waher.Content.Semantic.Model.Literals;
using Waher.Runtime.Inventory;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Persistence.SQL;

namespace Waher.Script.Persistence.SPARQL
{
	/// <summary>
	/// Executes a SELECT statement against the object database.
	/// </summary>
	public class Select : ScriptNode, IEvaluateAsync
	{
		private readonly ScriptNode[] columns;
		private readonly ScriptNode from;
		private readonly SemanticQueryTriple[] where;
		private readonly KeyValuePair<string, bool>[] orderBy;
		private readonly bool distinct;

		/// <summary>
		/// Executes a SPARQL SELECT statement.
		/// </summary>
		/// <param name="Distinct">If only distinct (unique) rows are to be returned.</param>
		/// <param name="Columns">Columns to select. If null, all columns are selected.</param>
		/// <param name="From">Data source.</param>
		/// <param name="Where">Optional where clause</param>
		/// <param name="OrderBy">Order to present result set.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Select(bool Distinct, ScriptNode[] Columns, ScriptNode From, SemanticQueryTriple[] Where,
			KeyValuePair<string, bool>[] OrderBy, int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
			this.distinct = Distinct;

			this.columns = Columns;
			this.columns?.SetParent(this);

			this.from = From;
			this.from?.SetParent(this);

			this.where = Where;
			this.orderBy = OrderBy;

			foreach (ISemanticTriple T in Where)
			{
				if (T.Subject is SemanticScriptElement S)
					S.Node.SetParent(this);

				if (T.Predicate is SemanticScriptElement P)
					P.Node.SetParent(this);

				if (T.Object is SemanticScriptElement O)
					O.Node.SetParent(this);
			}
		}

		/// <summary>
		/// If the node (or its decendants) include asynchronous evaluation. Asynchronous nodes should be evaluated using
		/// <see cref="EvaluateAsync(Variables)"/>.
		/// </summary>
		public override bool IsAsynchronous => true;

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			return this.EvaluateAsync(Variables).Result;
		}

		/// <summary>
		/// Evaluates the node asynchronously, using the variables provided in 
		/// the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override async Task<IElement> EvaluateAsync(Variables Variables)
		{
			object From;

			if (!(this.from is null))
				From = (await this.from.EvaluateAsync(Variables)).AssociatedObjectValue;
			else if (Variables.TryGetVariable(" Default Graph ", out Variable v))
				From = v.ValueObject;
			else
				throw new ScriptRuntimeException("Default graph not defined.", this);

			if (From is UriNode UriNode)
				From = await this.LoadResource(UriNode.Uri, Variables);
			else if (From is Uri Uri)
				From = await this.LoadResource(Uri, Variables);
			else if (From is string s)
				From = await this.LoadResource(new Uri(s), Variables);

			if (!(From is ISemanticCube Cube))
			{
				if (From is ISemanticModel Model)
					Cube = await InMemorySemanticCube.Create(Model);
				else
					throw new ScriptRuntimeException("Default graph not a semantic cube or semantic model.", this);
			}

			Dictionary<string, bool> VariablesProcessed = new Dictionary<string, bool>();
			LinkedList<Possibility> Possibilities = new LinkedList<Possibility>();
			string Name;

			foreach (SemanticQueryTriple T in this.where)
			{
				switch (T.Type)
				{
					case QueryTripleType.Constant:
						if (T.Required && await Cube.GetTriplesBySubjectAndPredicateAndObject(T.Subject, T.Predicate, T.Object) is null)
							Possibilities = null;
						break;

					case QueryTripleType.SubjectVariable:
						Possibilities = await CrossPossibilitiesOneVariable(Possibilities, T,
							0, 1, 2, VariablesProcessed, Cube);
						break;

					case QueryTripleType.PredicateVariable:
						Possibilities = await CrossPossibilitiesOneVariable(Possibilities, T,
							1, 0, 2, VariablesProcessed, Cube);
						break;

					case QueryTripleType.ObjectVariable:
						Possibilities = await CrossPossibilitiesOneVariable(Possibilities, T,
							2, 0, 1, VariablesProcessed, Cube);
						break;

					case QueryTripleType.SubjectPredicateVariables:
						Possibilities = await CrossPossibilitiesTwoVariables(Possibilities, T,
							0, 1, 2, VariablesProcessed, Cube);
						break;

					case QueryTripleType.SubjectObjectVariable:
						Possibilities = await CrossPossibilitiesTwoVariables(Possibilities, T,
							0, 2, 1, VariablesProcessed, Cube);
						break;

					case QueryTripleType.PredicateObjectVariable:
						Possibilities = await CrossPossibilitiesTwoVariables(Possibilities, T,
							1, 2, 0, VariablesProcessed, Cube);
						break;

					case QueryTripleType.SubjectPredicateObjectVariable:
						Possibilities = await CrossPossibilitiesThreeVariables(Possibilities, T,
							VariablesProcessed, Cube);
						break;
				}

				if (Possibilities?.First is null)
					break;
			}

			Dictionary<string, int> ColumnVariables = new Dictionary<string, int>();
			LinkedList<KeyValuePair<ScriptNode, int>> ColumnScript = null;
			List<string> ColumnNames = new List<string>();
			int Columns = this.columns.Length;
			int i;

			for (i = 0; i < Columns; i++)
			{
				if (this.columns[i] is VariableReference Ref)
				{
					ColumnVariables[Ref.VariableName] = i;
					ColumnNames.Add(Ref.VariableName);
				}
				else
				{
					if (ColumnScript is null)
						ColumnScript = new LinkedList<KeyValuePair<ScriptNode, int>>();

					ColumnScript.AddLast(new KeyValuePair<ScriptNode, int>(this.columns[i], i));
					ColumnNames.Add(" c" + i.ToString());
				}
			}

			List<SparqlResultRecord> Records = new List<SparqlResultRecord>();
			Dictionary<Type, ISemanticLiteral> LiteralPerType = null;

			if (!(Possibilities is null))
			{
				foreach (Possibility P in Possibilities)
				{
					Dictionary<string, SparqlResultItem> Record = new Dictionary<string, SparqlResultItem>();
					Possibility Loop = P;

					while (!(Loop is null))
					{
						Name = Loop.VariableName;

						if (ColumnVariables.TryGetValue(Name, out i))
							Record[Name] = new SparqlResultItem(Name, Loop.Value, i);

						Loop = Loop.Prev;
					}

					if (!(ColumnScript is null))
					{
						Variables RecordVariables = new ObjectProperties(P, Variables);
						object Value;

						foreach (KeyValuePair<ScriptNode, int> P2 in ColumnScript)
						{
							Name = " c" + P2.Value.ToString();

							try
							{
								Value = (await this.columns[i].EvaluateAsync(RecordVariables)).AssociatedObjectValue;
							}
							catch (ScriptReturnValueException ex)
							{
								Value = ex.ReturnValue.AssociatedObjectValue;
							}
							catch (Exception ex)
							{
								Value = ex;
							}

							if (!(Value is ISemanticLiteral Literal))
							{
								Type T = Value?.GetType() ?? typeof(object);

								if (LiteralPerType is null)
									LiteralPerType = new Dictionary<Type, ISemanticLiteral>();

								if (!LiteralPerType.TryGetValue(T, out Literal))
								{
									Literal = Types.FindBest<ISemanticLiteral, Type>(T);
									if (Literal is null)
										Literal = new CustomLiteral(string.Empty, string.Empty);

									LiteralPerType[T] = Literal;
								}

								Literal = Literal.Encapsulate(Value);
							}

							Record[Name] = new SparqlResultItem(Name, Literal, P2.Value);
						}
					}

					Records.Add(new SparqlResultRecord(Record));
				}
			}

			if (!(this.orderBy is null))
				Records.Sort(new OrderResultSet(this.orderBy));

			return new ObjectValue(new SparqlResultSet(ColumnNames.ToArray(), new Uri[0],
				Records.ToArray()));
		}

		private static async Task<LinkedList<Possibility>> CrossPossibilitiesOneVariable(
			LinkedList<Possibility> Possibilities, SemanticQueryTriple T, int VariableIndex,
			int ValueIndex1, int ValueIndex2, Dictionary<string, bool> VariablesProcessed,
			ISemanticCube Cube)
		{
			LinkedList<Possibility> NewPossibilities = null;
			string Name = T.VariableName(VariableIndex);
			ISemanticElement Value;

			if (VariablesProcessed.ContainsKey(Name))
			{
				if (T.Optional)
					NewPossibilities = Possibilities;
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
			}
			else
			{
				VariablesProcessed[Name] = true;

				if (!CrossPossibilities(
					await Cube.GetTriples(T[ValueIndex1], ValueIndex1, T[ValueIndex2], ValueIndex2),
					Possibilities, Name, VariableIndex, ref NewPossibilities, T.Optional))
				{
					return null;
				}
			}

			return NewPossibilities;
		}

		private static async Task<LinkedList<Possibility>> CrossPossibilitiesTwoVariables(
			LinkedList<Possibility> Possibilities, SemanticQueryTriple T, int VariableIndex1,
			int VariableIndex2, int ValueIndex, Dictionary<string, bool> VariablesProcessed,
			ISemanticCube Cube)
		{
			LinkedList<Possibility> NewPossibilities = null;
			string Name = T.VariableName(VariableIndex1);
			string Name2 = T.VariableName(VariableIndex2);
			bool IsProcessed = VariablesProcessed.ContainsKey(Name);
			bool IsProcessed2 = VariablesProcessed.ContainsKey(Name2);
			ISemanticElement Value;
			ISemanticElement Value2;
			bool SameName = Name == Name2;

			if (IsProcessed && IsProcessed2)
			{
				if (T.Optional)
					NewPossibilities = Possibilities;
				else
				{
					foreach (Possibility P in Possibilities)
					{
						Value = P.GetValue(Name);
						if (Value is null)
							continue;

						if (SameName)
							Value2 = Value;
						else
						{
							Value2 = P.GetValue(Name2);
							if (Value2 is null)
								continue;
						}

						if (await Cube.GetTriplesBySubjectAndPredicateAndObject(Value, Value2, T[ValueIndex]) is null)
							continue;

						if (NewPossibilities is null)
							NewPossibilities = new LinkedList<Possibility>();

						NewPossibilities.AddLast(P);
					}
				}
			}
			else if (IsProcessed)
			{
				VariablesProcessed[Name2] = true;

				foreach (Possibility P in Possibilities)
				{
					Value = P.GetValue(Name);
					if (Value is null)
						continue;

					CrossPossibilities(
						await Cube.GetTriples(Value, VariableIndex1, T[ValueIndex], ValueIndex),
						P, Name2, VariableIndex2, ref NewPossibilities, T.Optional);
				}
			}
			else if (IsProcessed2)
			{
				VariablesProcessed[Name] = true;

				foreach (Possibility P in Possibilities)
				{
					Value2 = P.GetValue(Name2);
					if (Value2 is null)
						continue;

					CrossPossibilities(
						await Cube.GetTriples(Value2, VariableIndex2, T[ValueIndex], ValueIndex),
						P, Name, VariableIndex1, ref NewPossibilities, T.Optional);
				}
			}
			else
			{
				IEnumerable<ISemanticTriple> Triples = await Cube.GetTriples(T[ValueIndex], ValueIndex);

				VariablesProcessed[Name] = true;
				if (!SameName)
					VariablesProcessed[Name2] = true;

				if (Triples is null && T.Required)
					return null;

				if (Possibilities.First is null)
				{
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

		private static async Task<LinkedList<Possibility>> CrossPossibilitiesThreeVariables(
			LinkedList<Possibility> Possibilities, SemanticQueryTriple T,
			Dictionary<string, bool> VariablesProcessed, ISemanticCube Cube)
		{
			LinkedList<Possibility> NewPossibilities = null;
			string Name = T.SubjectVariable;
			string Name2 = T.PredicateVariable;
			string Name3 = T.ObjectVariable;
			bool IsProcessed = VariablesProcessed.ContainsKey(Name);
			bool IsProcessed2 = VariablesProcessed.ContainsKey(Name2);
			bool IsProcessed3 = VariablesProcessed.ContainsKey(Name3);
			bool SameName = Name == Name2;
			bool SameName2 = Name == Name3;
			bool SameName3 = Name2 == Name3;
			ISemanticElement Value;
			ISemanticElement Value2;
			ISemanticElement Value3;

			if (IsProcessed && IsProcessed2 && IsProcessed3)
			{
				if (T.Optional)
					NewPossibilities = Possibilities;
				else
				{
					foreach (Possibility P in Possibilities)
					{
						Value = P.GetValue(Name);
						if (Value is null)
							continue;

						if (SameName)
							Value2 = Value;
						else
						{
							Value2 = P.GetValue(Name2);
							if (Value2 is null)
								continue;
						}

						if (SameName2)
							Value3 = Value;
						else if (SameName3)
							Value3 = Value2;
						else
						{
							Value3 = P.GetValue(Name3);
							if (Value3 is null)
								continue;
						}

						if (await Cube.GetTriplesBySubjectAndPredicateAndObject(Value, Value2, Value3) is null)
							continue;

						if (NewPossibilities is null)
							NewPossibilities = new LinkedList<Possibility>();

						NewPossibilities.AddLast(P);
					}
				}
			}
			else if (IsProcessed && IsProcessed2)   // Subject & Predicate variables already processed
			{
				VariablesProcessed[Name3] = true;

				foreach (Possibility P in Possibilities)
				{
					Value = P.GetValue(Name);
					if (Value is null)
						continue;

					if (SameName)
						Value2 = Value;
					else
					{
						Value2 = P.GetValue(Name2);
						if (Value2 is null)
							continue;
					}

					CrossPossibilities(
						await Cube.GetTriplesBySubjectAndPredicate(Value, Value2),
						P, Name3, 2, ref NewPossibilities, T.Optional);
				}
			}
			else if (IsProcessed && IsProcessed3)   // Subject & Object variables already processed
			{
				VariablesProcessed[Name2] = true;

				foreach (Possibility P in Possibilities)
				{
					Value = P.GetValue(Name);
					if (Value is null)
						continue;

					if (SameName2)
						Value3 = Value;
					else
					{
						Value3 = P.GetValue(Name3);
						if (Value3 is null)
							continue;
					}

					CrossPossibilities(
						await Cube.GetTriplesBySubjectAndObject(Value, Value3),
						P, Name2, 1, ref NewPossibilities, T.Optional);
				}
			}
			else if (IsProcessed2 && IsProcessed3)  // Predicate & Object variables already processed
			{
				VariablesProcessed[Name] = true;

				foreach (Possibility P in Possibilities)
				{
					Value2 = P.GetValue(Name2);
					if (Value2 is null)
						continue;

					if (SameName3)
						Value3 = Value2;
					else
					{
						Value3 = P.GetValue(Name3);
						if (Value3 is null)
							continue;
					}

					CrossPossibilities(
						await Cube.GetTriplesByPredicateAndObject(Value2, Value3),
						P, Name, 0, ref NewPossibilities, T.Optional);
				}
			}
			else if (IsProcessed)                   // Subject variable processed
			{
				VariablesProcessed[Name2] = true;
				if (!SameName3)
					VariablesProcessed[Name3] = true;

				foreach (Possibility P in Possibilities)
				{
					Value = P.GetValue(Name);
					if (Value is null)
						continue;

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
			}
			else if (IsProcessed2)                  // Predicate variable processed
			{
				VariablesProcessed[Name] = true;
				if (!SameName2)
					VariablesProcessed[Name3] = true;

				foreach (Possibility P in Possibilities)
				{
					Value2 = P.GetValue(Name2);
					if (Value2 is null)
						continue;

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
			}
			else if (IsProcessed3)                  // Object variable processed
			{
				VariablesProcessed[Name] = true;
				if (!SameName)
					VariablesProcessed[Name2] = true;

				foreach (Possibility P in Possibilities)
				{
					Value3 = P.GetValue(Name3);
					if (Value3 is null)
						continue;

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
			}
			else
			{
				VariablesProcessed[Name] = true;
				if (!SameName)
					VariablesProcessed[Name2] = true;
				if (!SameName2 && !SameName3)
					VariablesProcessed[Name3] = true;

				foreach (Possibility P in Possibilities)
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

			return NewPossibilities;
		}

		private static bool CrossPossibilities(IEnumerable<ISemanticTriple> NewTriples,
			LinkedList<Possibility> Possibilities, string Name, int Index,
			ref LinkedList<Possibility> NewPossibilities, bool Optional)
		{
			if (NewTriples is null && !Optional)
				return false;

			if (Possibilities.First is null)
			{
				if (NewPossibilities is null)
					NewPossibilities = new LinkedList<Possibility>();

				if (NewTriples is null)
					NewPossibilities.AddLast(new Possibility(Name, null));
				else
				{
					foreach (ISemanticTriple T2 in NewTriples)
						NewPossibilities.AddLast(new Possibility(Name, T2[Index]));
				}
			}
			else
			{
				foreach (Possibility P in Possibilities)
					CrossPossibilities(NewTriples, P, Name, Index, ref NewPossibilities, Optional);
			}

			return true;
		}

		private static void CrossPossibilities(IEnumerable<ISemanticTriple> NewTriples,
			Possibility Possibility, string Name, int Index, ref LinkedList<Possibility> NewPossibilities,
			bool Optional)
		{
			if (NewTriples is null && !Optional)
				return;

			if (NewPossibilities is null)
				NewPossibilities = new LinkedList<Possibility>();

			if (NewTriples is null)
				NewPossibilities.AddLast(new Possibility(Name, null, Possibility));
			else
			{
				foreach (ISemanticTriple T2 in NewTriples)
					NewPossibilities.AddLast(new Possibility(Name, T2[Index], Possibility));
			}
		}

		/// <summary>
		/// Calls the callback method for all child nodes.
		/// </summary>
		/// <param name="Callback">Callback method to call.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="Order">Order to traverse the nodes.</param>
		/// <returns>If the process was completed.</returns>
		public override bool ForAllChildNodes(ScriptNodeEventHandler Callback, object State, SearchMethod Order)
		{
			if (Order == SearchMethod.DepthFirst)
			{
				if (!this.columns.ForAllChildNodes(Callback, State, Order))
					return false;

				foreach (ISemanticTriple T in this.where)
				{
					if (T.Subject is SemanticScriptElement S)
						S.Node.ForAllChildNodes(Callback, State, Order);

					if (T.Predicate is SemanticScriptElement P)
						P.Node.ForAllChildNodes(Callback, State, Order);

					if (T.Object is SemanticScriptElement O)
						O.Node.ForAllChildNodes(Callback, State, Order);
				}
			}

			if (!this.columns.ForAll(Callback, this, State, Order == SearchMethod.TreeOrder))
				return false;

			foreach (ISemanticTriple T in this.where)
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

			if (Order == SearchMethod.BreadthFirst)
			{
				if (!this.columns.ForAllChildNodes(Callback, State, Order))
					return false;

				foreach (ISemanticTriple T in this.where)
				{
					if (T.Subject is SemanticScriptElement S)
						S.Node.ForAllChildNodes(Callback, State, Order);

					if (T.Predicate is SemanticScriptElement P)
						P.Node.ForAllChildNodes(Callback, State, Order);

					if (T.Object is SemanticScriptElement O)
						O.Node.ForAllChildNodes(Callback, State, Order);
				}
			}

			return true;
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			if (!(obj is Select O &&
				AreEqual(this.columns, O.columns) &&
				AreEqual(this.where, O.where) &&
				this.distinct == O.distinct &&
				base.Equals(obj)))
			{
				return false;
			}

			return true;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int Result = base.GetHashCode();
			Result ^= Result << 5 ^ GetHashCode(this.columns);
			Result ^= Result << 5 ^ GetHashCode(this.where);
			Result ^= Result << 5 ^ this.distinct.GetHashCode();

			return Result;
		}

		private async Task<object> LoadResource(Uri Uri, Variables Variables)
		{
			if (Variables.TryGetVariable(" " + Uri.ToString() + " ", out Variable v))
				return v.ValueObject;

			return await InternetContent.GetAsync(Uri,
				new KeyValuePair<string, string>("Accept", "text/turtle, application/x-turtle, application/rdf+xml;q=0.9"));
		}

	}
}
