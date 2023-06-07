using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content.Semantic;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Objects.Matrices;
using Waher.Script.Persistence.SQL;

namespace Waher.Script.Persistence.SPARQL
{
	/// <summary>
	/// Executes a SELECT statement against the object database.
	/// </summary>
	public class Select : ScriptNode, IEvaluateAsync
	{
		private readonly ScriptNode[] columns;
		private readonly SemanticQueryTriple[] where;
		private readonly bool distinct;

		/// <summary>
		/// Executes a SPARQL SELECT statement.
		/// </summary>
		/// <param name="Columns">Columns to select. If null, all columns are selected.</param>
		/// <param name="Where">Optional where clause</param>
		/// <param name="Distinct">If only distinct (unique) rows are to be returned.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Select(ScriptNode[] Columns, SemanticQueryTriple[] Where, bool Distinct,
			int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
			this.columns = Columns;
			this.columns?.SetParent(this);

			this.where = Where;

			foreach (ISemanticTriple T in Where)
			{
				if (T.Subject is SemanticScriptElement S)
					S.Node.SetParent(this);

				if (T.Predicate is SemanticScriptElement P)
					P.Node.SetParent(this);

				if (T.Object is SemanticScriptElement O)
					O.Node.SetParent(this);
			}

			this.distinct = Distinct;
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
			if (!Variables.TryGetVariable(" Default Graph ", out Variable v))
				throw new ScriptRuntimeException("Default graph not defined.", this);

			object Obj = v.ValueObject;

			if (!(Obj is ISemanticCube Cube))
			{
				if (Obj is ISemanticModel Model)
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
						if (await Cube.GetTriplesBySubjectAndPredicateAndObject(T.Subject, T.Predicate, T.Object) is null)
							Possibilities = null;
						break;

					case QueryTripleType.SubjectVariable:
						LinkedList<Possibility> NewPossibilities = null;
						Name = T.SubjectVariable;
						ISemanticElement Value;

						if (VariablesProcessed.ContainsKey(Name))
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
						else
						{
							VariablesProcessed[Name] = true;

							if (!CrossPossibilitiesOnSubject(
								await Cube.GetTriplesByPredicateAndObject(T.Predicate, T.Object),
								Possibilities, Name, ref NewPossibilities))
							{
								Possibilities = null;
								break;
							}
						}

						Possibilities = NewPossibilities;
						break;

					case QueryTripleType.PredicateVariable:
						NewPossibilities = null;
						Name = T.PredicateVariable;

						if (VariablesProcessed.ContainsKey(Name))
						{
							foreach (Possibility P in Possibilities)
							{
								Value = P.GetValue(Name);
								if (Value is null)
									continue;

								if (await Cube.GetTriplesBySubjectAndPredicateAndObject(T.Subject, Value, T.Object) is null)
									continue;

								if (NewPossibilities is null)
									NewPossibilities = new LinkedList<Possibility>();

								NewPossibilities.AddLast(P);
							}
						}
						else
						{
							VariablesProcessed[Name] = true;

							if (!CrossPossibilitiesOnPredicate(
								await Cube.GetTriplesBySubjectAndObject(T.Subject, T.Object),
								Possibilities, Name, ref NewPossibilities))
							{
								Possibilities = null;
								break;
							}
						}

						Possibilities = NewPossibilities;
						break;

					case QueryTripleType.ObjectVariable:
						NewPossibilities = null;
						Name = T.ObjectVariable;

						if (VariablesProcessed.ContainsKey(Name))
						{
							foreach (Possibility P in Possibilities)
							{
								Value = P.GetValue(Name);
								if (Value is null)
									continue;

								if (await Cube.GetTriplesBySubjectAndPredicateAndObject(T.Subject, T.Predicate, Value) is null)
									continue;

								if (NewPossibilities is null)
									NewPossibilities = new LinkedList<Possibility>();

								NewPossibilities.AddLast(P);
							}
						}
						else
						{
							VariablesProcessed[Name] = true;

							if (!CrossPossibilitiesOnObject(
								await Cube.GetTriplesBySubjectAndPredicate(T.Subject, T.Predicate),
								Possibilities, Name, ref NewPossibilities))
							{
								Possibilities = null;
								break;
							}
						}

						Possibilities = NewPossibilities;
						break;

					case QueryTripleType.SubjectPredicateVariables:
						NewPossibilities = null;
						Name = T.SubjectVariable;
						string Name2 = T.PredicateVariable;
						bool IsProcessed = VariablesProcessed.ContainsKey(Name);
						bool IsProcessed2 = VariablesProcessed.ContainsKey(Name2);
						ISemanticElement Value2;
						bool SameName = Name == Name2;

						if (IsProcessed && IsProcessed2)
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

								if (await Cube.GetTriplesBySubjectAndPredicateAndObject(Value, Value2, T.Object) is null)
									continue;

								if (NewPossibilities is null)
									NewPossibilities = new LinkedList<Possibility>();

								NewPossibilities.AddLast(P);
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

								CrossPossibilitiesOnPredicate(
									await Cube.GetTriplesBySubjectAndObject(Value, T.Object),
									Possibilities, Name, ref NewPossibilities);
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

								CrossPossibilitiesOnSubject(
									await Cube.GetTriplesByPredicateAndObject(Value2, T.Object),
									Possibilities, Name, ref NewPossibilities);
							}
						}
						else
						{
							ISemanticPlane Plane = await Cube.GetTriplesByObject(T.Object);

							VariablesProcessed[Name] = true;
							if (!SameName)
								VariablesProcessed[Name2] = true;

							if (Plane is null)
							{
								Possibilities = null;
								break;
							}

							if (Possibilities.First is null)
							{
								foreach (ISemanticTriple T2 in Plane)
								{
									if (SameName)
									{
										if (!T2.Subject.Equals(T2.Predicate))
											continue;

										if (NewPossibilities is null)
											NewPossibilities = new LinkedList<Possibility>();

										NewPossibilities.AddLast(new Possibility(Name, T2.Subject));
									}
									else
									{
										if (NewPossibilities is null)
											NewPossibilities = new LinkedList<Possibility>();

										NewPossibilities.AddLast(
											new Possibility(Name, T2.Subject,
											new Possibility(Name2, T2.Predicate)));
									}
								}
							}
							else
							{
								foreach (Possibility P in Possibilities)
								{
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
						}

						Possibilities = NewPossibilities;
						break;

					case QueryTripleType.SubjectObjectVariable:
						NewPossibilities = null;
						Name = T.SubjectVariable;
						Name2 = T.ObjectVariable;
						IsProcessed = VariablesProcessed.ContainsKey(Name);
						IsProcessed2 = VariablesProcessed.ContainsKey(Name2);
						SameName = Name == Name2;

						if (IsProcessed && IsProcessed2)
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

								if (await Cube.GetTriplesBySubjectAndPredicateAndObject(Value, T.Predicate, Value2) is null)
									continue;

								if (NewPossibilities is null)
									NewPossibilities = new LinkedList<Possibility>();

								NewPossibilities.AddLast(P);
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

								CrossPossibilitiesOnObject(
									await Cube.GetTriplesBySubjectAndPredicate(Value, T.Predicate),
									Possibilities, Name, ref NewPossibilities);
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

								CrossPossibilitiesOnSubject(
									await Cube.GetTriplesByPredicateAndObject(T.Predicate, Value2),
									Possibilities, Name, ref NewPossibilities);
							}
						}
						else
						{
							ISemanticPlane Plane = await Cube.GetTriplesByPredicate(T.Predicate);

							VariablesProcessed[Name] = true;
							if (!SameName)
								VariablesProcessed[Name2] = true;

							if (Plane is null)
							{
								Possibilities = null;
								break;
							}

							if (Possibilities.First is null)
							{
								foreach (ISemanticTriple T2 in Plane)
								{
									if (SameName)
									{
										if (!T2.Subject.Equals(T2.Object))
											continue;

										if (NewPossibilities is null)
											NewPossibilities = new LinkedList<Possibility>();

										NewPossibilities.AddLast(new Possibility(Name, T2.Subject));
									}
									else
									{
										if (NewPossibilities is null)
											NewPossibilities = new LinkedList<Possibility>();

										NewPossibilities.AddLast(
											new Possibility(Name, T2.Subject,
											new Possibility(Name2, T2.Object)));
									}
								}
							}
							else
							{
								foreach (Possibility P in Possibilities)
								{
									foreach (ISemanticTriple T2 in Plane)
									{
										if (SameName)
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
												new Possibility(Name2, T2.Object, P)));
										}
									}
								}
							}
						}

						Possibilities = NewPossibilities;
						break;

					case QueryTripleType.PredicateObjectVariable:
						NewPossibilities = null;
						Name = T.PredicateVariable;
						Name2 = T.ObjectVariable;
						IsProcessed = VariablesProcessed.ContainsKey(Name);
						IsProcessed2 = VariablesProcessed.ContainsKey(Name2);
						SameName = Name == Name2;

						if (IsProcessed && IsProcessed2)
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

								if (await Cube.GetTriplesBySubjectAndPredicateAndObject(T.Subject, Value, Value2) is null)
									continue;

								if (NewPossibilities is null)
									NewPossibilities = new LinkedList<Possibility>();

								NewPossibilities.AddLast(P);
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

								CrossPossibilitiesOnObject(
									await Cube.GetTriplesBySubjectAndPredicate(T.Subject, Value),
									Possibilities, Name, ref NewPossibilities);
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

								CrossPossibilitiesOnPredicate(
									await Cube.GetTriplesByPredicateAndObject(T.Predicate, Value2),
									Possibilities, Name, ref NewPossibilities);
							}
						}
						else
						{
							ISemanticPlane Plane = await Cube.GetTriplesByObject(T.Object);

							VariablesProcessed[Name] = true;
							if (!SameName)
								VariablesProcessed[Name2] = true;

							if (Plane is null)
							{
								Possibilities = null;
								break;
							}

							if (Possibilities.First is null)
							{
								foreach (ISemanticTriple T2 in Plane)
								{
									if (SameName)
									{
										if (!T2.Predicate.Equals(T2.Object))
											continue;

										if (NewPossibilities is null)
											NewPossibilities = new LinkedList<Possibility>();

										NewPossibilities.AddLast(new Possibility(Name, T2.Predicate));
									}
									else
									{
										if (NewPossibilities is null)
											NewPossibilities = new LinkedList<Possibility>();

										NewPossibilities.AddLast(
											new Possibility(Name, T2.Predicate,
											new Possibility(Name2, T2.Object)));
									}
								}
							}
							else
							{
								foreach (Possibility P in Possibilities)
								{
									foreach (ISemanticTriple T2 in Plane)
									{
										if (SameName)
										{
											if (!T2.Predicate.Equals(T2.Object))
												continue;

											if (NewPossibilities is null)
												NewPossibilities = new LinkedList<Possibility>();

											NewPossibilities.AddLast(
												new Possibility(Name, T2.Predicate, P));
										}
										else
										{
											if (NewPossibilities is null)
												NewPossibilities = new LinkedList<Possibility>();

											NewPossibilities.AddLast(
												new Possibility(Name, T2.Predicate,
												new Possibility(Name2, T2.Object, P)));
										}
									}
								}
							}
						}

						Possibilities = NewPossibilities;
						break;

					case QueryTripleType.SubjectPredicateObjectVariable:
						NewPossibilities = null;
						Name = T.SubjectVariable;
						Name2 = T.PredicateVariable;
						string Name3 = T.ObjectVariable;
						IsProcessed = VariablesProcessed.ContainsKey(Name);
						IsProcessed2 = VariablesProcessed.ContainsKey(Name2);
						bool IsProcessed3 = VariablesProcessed.ContainsKey(Name3);
						SameName = Name == Name2;
						bool SameName2 = Name == Name3;
						bool SameName3 = Name2 == Name3;
						ISemanticElement Value3;

						if (IsProcessed && IsProcessed2 && IsProcessed3)
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

								CrossPossibilitiesOnObject(
									await Cube.GetTriplesBySubjectAndPredicate(Value, Value2),
									Possibilities, Name3, ref NewPossibilities);
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

								CrossPossibilitiesOnPredicate(
									await Cube.GetTriplesBySubjectAndObject(Value, Value3),
									Possibilities, Name2, ref NewPossibilities);
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

								CrossPossibilitiesOnSubject(
									await Cube.GetTriplesByPredicateAndObject(Value2, Value3),
									Possibilities, Name, ref NewPossibilities);
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

						Possibilities = NewPossibilities;
						break;
				}

				if (Possibilities?.First is null)
					break;
			}

			Dictionary<string, int> ColumnIndices = new Dictionary<string, int>();
			int Columns = this.columns.Length;
			string[] ColumnNames = new string[Columns];
			bool HasScriptColumns = false;
			int i, j;

			for (i = 0; i < Columns; i++)
			{
				if (this.columns[i] is VariableReference Ref)
				{
					Name = Ref.VariableName;
					j = 1;

					while (ColumnIndices.ContainsKey(Name))
					{
						j++;
						Name = Ref.VariableName + " " + j.ToString();
					}

					ColumnIndices[Name] = i;
				}
				else
				{
					ColumnNames[i] = null;
					HasScriptColumns = true;
				}
			}

			List<IElement> Records = new List<IElement>();
			
			if (!(Possibilities is null))
			{
				foreach (Possibility P in Possibilities)
				{
					IElement[] Record = new IElement[Columns];
					Possibility Loop = P;

					while (!(Loop is null))
					{
						if (ColumnIndices.TryGetValue(Loop.VariableName, out i))
						{
							j = 1;
							while (!(Record[i] is null))
							{
								j++;
								Name = Loop.VariableName + " " + j.ToString();
								if (!ColumnIndices.TryGetValue(Name, out i))
								{
									i = -1;
									break;
								}
							}

							if (i >= 0)
								Record[i] = new ObjectValue(Loop.Value);
						}

						Loop = Loop.Prev;
					}

					Variables RecordVariables = HasScriptColumns ? Variables : new ObjectProperties(P, Variables);

					for (i = 0; i < Columns; i++)
					{
						if (Record[i] is null)
						{
							try
							{
								Record[i] = await this.columns[i].EvaluateAsync(RecordVariables);
							}
							catch (ScriptReturnValueException ex)
							{
								Record[i] = ex.ReturnValue;
							}
							catch (Exception ex)
							{
								Record[i] = new ObjectValue(ex);
							}
						}
					}

					Records.AddRange(Record);
				}
			}

			return new ObjectMatrix(Records.Count, Columns, Records)
			{
				ColumnNames = ColumnNames
			};
		}

		private static bool CrossPossibilitiesOnSubject(IEnumerable<ISemanticTriple> NewTriples,
			LinkedList<Possibility> Possibilities, string Name,
			ref LinkedList<Possibility> NewPossibilities)
		{
			if (NewTriples is null)
				return false;

			if (NewPossibilities is null)
				NewPossibilities = new LinkedList<Possibility>();

			if (Possibilities.First is null)
			{
				foreach (ISemanticTriple T2 in NewTriples)
					NewPossibilities.AddLast(new Possibility(Name, T2.Subject));
			}
			else
			{
				foreach (Possibility P in Possibilities)
				{
					foreach (ISemanticTriple T2 in NewTriples)
						NewPossibilities.AddLast(new Possibility(Name, T2.Subject, P));
				}
			}

			return true;
		}

		private static bool CrossPossibilitiesOnPredicate(IEnumerable<ISemanticTriple> NewTriples,
			LinkedList<Possibility> Possibilities, string Name,
			ref LinkedList<Possibility> NewPossibilities)
		{
			if (NewTriples is null)
				return false;

			if (NewPossibilities is null)
				NewPossibilities = new LinkedList<Possibility>();

			if (Possibilities.First is null)
			{
				foreach (ISemanticTriple T2 in NewTriples)
					NewPossibilities.AddLast(new Possibility(Name, T2.Predicate));
			}
			else
			{
				foreach (Possibility P in Possibilities)
				{
					foreach (ISemanticTriple T2 in NewTriples)
						NewPossibilities.AddLast(new Possibility(Name, T2.Predicate, P));
				}
			}

			return true;
		}

		private static bool CrossPossibilitiesOnObject(IEnumerable<ISemanticTriple> NewTriples,
			LinkedList<Possibility> Possibilities, string Name,
			ref LinkedList<Possibility> NewPossibilities)
		{
			if (NewTriples is null)
				return false;

			if (NewPossibilities is null)
				NewPossibilities = new LinkedList<Possibility>();

			if (Possibilities.First is null)
			{
				foreach (ISemanticTriple T2 in NewTriples)
					NewPossibilities.AddLast(new Possibility(Name, T2.Object));
			}
			else
			{
				foreach (Possibility P in Possibilities)
				{
					foreach (ISemanticTriple T2 in NewTriples)
						NewPossibilities.AddLast(new Possibility(Name, T2.Object, P));
				}
			}

			return true;
		}

		private class Possibility
		{
			public Possibility(string VariableName, ISemanticElement Value)
				: this(VariableName, Value, null)
			{
			}

			public Possibility(string VariableName, ISemanticElement Value, Possibility Prev)
			{
				this.VariableName = VariableName;
				this.Value = Value;
				this.Prev = Prev;
			}

			public string VariableName { get; }
			public ISemanticElement Value { get; }
			public Possibility Prev { get; }

			public ISemanticElement this[string VariableName]
			{
				get => this.GetValue(VariableName);
			}

			public ISemanticElement GetValue(string VariableName)
			{
				if (this.VariableName == VariableName)
					return this.Value;

				Possibility Loop = this.Prev;
				while (!(Loop is null))
				{
					if (Loop.VariableName == VariableName)
						return Loop.Value;

					Loop = Loop.Prev;
				}

				return null;
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

	}
}
