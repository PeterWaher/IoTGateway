﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Content.Semantic;
using Waher.Content.Semantic.Model;
using Waher.Persistence;
using Waher.Persistence.Filters;
using Waher.Runtime.Collections;
using Waher.Runtime.Inventory;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Persistence.SPARQL.Filters;
using Waher.Script.Persistence.SPARQL.Patterns;
using Waher.Script.Persistence.SPARQL.Sources;
using Waher.Script.Persistence.SQL;
using Waher.Things;

namespace Waher.Script.Persistence.SPARQL
{
	/// <summary>
	/// SPARQL query type.
	/// </summary>
	public enum QueryType
	{
		/// <summary>
		/// SELECT query
		/// </summary>
		Select,

		/// <summary>
		/// ASK query
		/// </summary>
		Ask,

		/// <summary>
		/// CONSTRUCT query
		/// </summary>
		Construct
	}

	/// <summary>
	/// Executes a SPARQL query.
	/// </summary>
	public class SparqlQuery : ScriptNode, IEvaluateAsync
	{
		private readonly ScriptNode[] columns;
		private readonly ScriptNode[] columnNames;
		private readonly ScriptNode[] groupBy;
		private readonly ScriptNode[] groupByNames;
		private readonly ScriptNode[] from;
		private readonly ISparqlPattern where;
		private readonly ScriptNode having;
		private readonly KeyValuePair<ScriptNode, bool>[] orderBy;
		private readonly SparqlRegularPattern construct;
		private readonly QueryType queryType;
		private readonly int? limit;
		private readonly int? offset;
		private readonly bool distinct;
		private readonly bool reduced;
		private Dictionary<UriNode, ISemanticCube> namedGraphs;
		private UriNode[] namedGraphNames;

		/// <summary>
		/// Executes a SPARQL query.
		/// </summary>
		/// <param name="QueryType">Query type</param>
		/// <param name="Distinct">If only distinct (unique) rows are to be returned.</param>
		/// <param name="Reduced">If duplicate rows rows are allowed to be removed.</param>
		/// <param name="Columns">Columns to select.</param>
		/// <param name="ColumnNames">Names of selected columns.</param>
		/// <param name="From">Data sources.</param>
		/// <param name="NamedGraphs">Named graphs.</param>
		/// <param name="Where">Optional where clause</param>
		/// <param name="GroupBy">Any GROUP BY rules.</param>
		/// <param name="GroupByNames">Optional names for GROUP BY rules.</param>
		/// <param name="Having">Any filter on groups.</param>
		/// <param name="OrderBy">Order to present result set.</param>
		/// <param name="Limit">Limit number of records to return, if defined.</param>
		/// <param name="Offset">Offset into result set, for first record to return.</param>
		/// <param name="Construct">Triples to construct.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public SparqlQuery(QueryType QueryType, bool Distinct, bool Reduced, ScriptNode[] Columns,
			ScriptNode[] ColumnNames, ScriptNode[] From, Dictionary<UriNode, ISemanticCube> NamedGraphs,
			ISparqlPattern Where, ScriptNode[] GroupBy, ScriptNode[] GroupByNames, ScriptNode Having,
			KeyValuePair<ScriptNode, bool>[] OrderBy, int? Limit, int? Offset,
			SparqlRegularPattern Construct, int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
			this.queryType = QueryType;
			this.distinct = Distinct;
			this.reduced = Reduced;
			this.limit = Limit;
			this.offset = Offset;
			this.construct = Construct;

			this.columns = Columns;
			this.columns?.SetParent(this);

			this.columnNames = ColumnNames;
			this.columnNames?.SetParent(this);

			this.from = From;
			this.from?.SetParent(this);
			this.namedGraphs = NamedGraphs;

			this.namedGraphNames = new UriNode[NamedGraphs?.Count ?? 0];
			NamedGraphs?.Keys.CopyTo(this.namedGraphNames, 0);

			this.where = Where;
			this.where?.SetParent(this);

			this.groupBy = GroupBy;
			this.groupBy?.SetParent(this);

			this.groupByNames = GroupByNames;
			this.groupByNames?.SetParent(this);

			this.having = Having;
			this.having?.SetParent(this);

			this.orderBy = OrderBy;

			if (!(this.orderBy is null))
			{
				foreach (KeyValuePair<ScriptNode, bool> P in this.orderBy)
					P.Key.SetParent(this);
			}
		}

		/// <summary>
		/// If the node (or its decendants) include asynchronous evaluation. Asynchronous nodes should be evaluated using
		/// <see cref="EvaluateAsync(Variables)"/>.
		/// </summary>
		public override bool IsAsynchronous => true;

		/// <summary>
		/// Names of named graphs, may be null.
		/// </summary>
		public UriNode[] NamedGraphNames => this.namedGraphNames;

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
		public override Task<IElement> EvaluateAsync(Variables Variables)
		{
			return this.EvaluateAsync(Variables, null);
		}

		/// <summary>
		/// Evaluates the node asynchronously, using the variables provided in 
		/// the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <param name="ExistingMatches">Any existing matches the query needs to consider.</param>
		/// <returns>Result.</returns>
		public async Task<IElement> EvaluateAsync(Variables Variables,
			IEnumerable<Possibility> ExistingMatches)
		{
			SemanticDataSet DataSet = new SemanticDataSet();
			object From;

			if (this.from is null)
			{
				if (Variables.TryGetVariable(" Default Graph ", out Variable v))
					From = v.ValueObject;
				else
					throw new ScriptRuntimeException("Default graph not defined.", this);

				DataSet.Add(await this.GetDataSource(From, Variables, false));
			}
			else
			{
				foreach (ScriptNode Source in this.from)
				{
					From = (await Source.EvaluateAsync(Variables)).AssociatedObjectValue;
					DataSet.Add(await this.GetDataSource(From, Variables, false));
				}
			}

			IEnumerable<ISparqlResultRecord> Possibilities;

			if (this.where is null)
				Possibilities = ExistingMatches;
			else
				Possibilities = await this.where.Search(DataSet, Variables, ExistingMatches, this);

			if (!(this.groupBy is null) && !(Possibilities is null))
			{
				Dictionary<string, bool> VectorProperties = null;

				if (!(this.columns is null))
				{
					foreach (ScriptNode Node in this.columns)
					{
						if (!(Node is VariableReference))
						{
							Node.ForAllChildNodes((ScriptNode Descendant, out ScriptNode NewNode, object State) =>
							{
								if (Descendant is VariableReference Ref)
								{
									if (VectorProperties is null)
										VectorProperties = new Dictionary<string, bool>();

									VectorProperties[Ref.VariableName] = true;
								}

								NewNode = null;
								return true;
							}, null, SearchMethod.TreeOrder);
						}
					}
				}

				GroupResultSet GroupComparer = new GroupResultSet(this.groupBy, this.groupByNames);
				SortedDictionary<ISparqlResultRecord, KeyValuePair<ISparqlResultRecord, ChunkedList<ISparqlResultRecord>>> Groups =
					new SortedDictionary<ISparqlResultRecord, KeyValuePair<ISparqlResultRecord, ChunkedList<ISparqlResultRecord>>>(GroupComparer);
				ChunkedList<ISparqlResultRecord> LastList = null;
				ISparqlResultRecord LastRecord = null;
				bool First = false;

				foreach (ISparqlResultRecord P in Possibilities)
				{
					if (LastRecord is null || GroupComparer.Compare(LastRecord, P) != 0)
					{
						if (Groups.TryGetValue(P, out KeyValuePair<ISparqlResultRecord, ChunkedList<ISparqlResultRecord>> P2))
						{
							LastRecord = P2.Key;
							LastList = P2.Value;
							First = false;
						}
						else
						{
							LastList = new ChunkedList<ISparqlResultRecord>();
							Groups[P] = new KeyValuePair<ISparqlResultRecord, ChunkedList<ISparqlResultRecord>>(P, LastList);
							LastRecord = P;
							First = true;
						}
					}

					LastList.Add(P);

					if (!(VectorProperties is null))
					{
						foreach (string VectorProperty in VectorProperties.Keys)
						{
							ISemanticElement Element = LastRecord[VectorProperty];
							if (!(Element is SemanticElementVector Vector))
							{
								Vector = new SemanticElementVector();
								LastRecord[VectorProperty] = Vector;
							}

							if (First)
								Vector.Add(Element);
							else
								Vector.Add(P[VectorProperty]);
						}
					}

					First = false;
				}

				Possibilities = Groups.Keys;

				if (!(this.having is null))
				{
					ChunkedList<ISparqlResultRecord> Filtered = new ChunkedList<ISparqlResultRecord>();
					ObjectProperties RecordVariables = null;

					foreach (ISparqlResultRecord Record in Possibilities)
					{
						try
						{
							if (RecordVariables is null)
								RecordVariables = new ObjectProperties(Record, Variables);
							else
								RecordVariables.Object = Record;

							object Value = await EvaluateValue(RecordVariables, this.having);
							if (Value is bool b && b)
								Filtered.Add(Record);
						}
						catch (Exception)
						{
							// Ignore record
						}
					}

					Possibilities = Filtered;
				}
			}

			switch (this.queryType)
			{
				case QueryType.Ask:
					if (!(Possibilities is null))
						return new ObjectValue(new SparqlResultSet(Possibilities.GetEnumerator().MoveNext()));

					return new ObjectValue(new SparqlResultSet(false));

				case QueryType.Select:
					Dictionary<string, int> ColumnVariables = new Dictionary<string, int>();
					ChunkedList<KeyValuePair<ScriptNode, int>> ColumnScript = null;
					ChunkedList<string> ColumnNames = new ChunkedList<string>();
					string Name;
					int i, c;
					bool AllNames;

					if (this.columns is null)
						AllNames = true;
					else
					{
						AllNames = false;

						int Columns = this.columns.Length;

						c = this.columnNames?.Length ?? 0;

						for (i = 0; i < Columns; i++)
						{
							if (i < c && !(this.columnNames[i] is null))
							{
								if (this.columnNames[i] is VariableReference Ref2)
									Name = Ref2.VariableName;
								else
									Name = (await this.columnNames[i].EvaluateAsync(Variables)).AssociatedObjectValue?.ToString();

								ColumnVariables[Name] = i;
								ColumnNames.Add(Name);
							}
							else
								Name = null;

							if (this.columns[i] is VariableReference Ref)
							{
								if (Name is null)
								{
									Name = Ref.VariableName;

									ColumnVariables[Name] = i;
									ColumnNames.Add(Name);
								}
								else
								{
									if (ColumnScript is null)
										ColumnScript = new ChunkedList<KeyValuePair<ScriptNode, int>>();

									ColumnScript.Add(new KeyValuePair<ScriptNode, int>(Ref, i));
								}
							}
							else
							{
								if (ColumnScript is null)
									ColumnScript = new ChunkedList<KeyValuePair<ScriptNode, int>>();

								ColumnScript.Add(new KeyValuePair<ScriptNode, int>(this.columns[i], i));

								if (Name is null)
								{
									Name = " c" + i.ToString();
									ColumnNames.Add(Name);
								}
							}
						}
					}

					List<ISparqlResultRecord> Records = new List<ISparqlResultRecord>();
					bool MakeUnique = this.distinct || this.reduced;
					Dictionary<string, bool> Distinct = MakeUnique ? new Dictionary<string, bool>() : null;
					StringBuilder sb = MakeUnique ? new StringBuilder() : null;
					ObjectProperties RecordVariables = null;

					if (!(Possibilities is null))
					{
						foreach (ISparqlResultRecord P in Possibilities)
						{
							Dictionary<string, ISparqlResultItem> Record = new Dictionary<string, ISparqlResultItem>();

							foreach (ISparqlResultItem Loop in P)
							{
								Name = Loop.Name;

								if (ColumnVariables.TryGetValue(Name, out i))
									Record[Name] = new SparqlResultItem(Name, Loop.Value, i);
								else if (AllNames)
								{
									i = ColumnNames.Count;
									ColumnNames.Add(Name);
									ColumnVariables[Name] = i;

									Record[Name] = new SparqlResultItem(Name, Loop.Value, i);
								}
							}

							if (!(ColumnScript is null))
							{
								if (RecordVariables is null)
									RecordVariables = new ObjectProperties(P, Variables);
								else
									RecordVariables.Object = P;

								foreach (KeyValuePair<ScriptNode, int> P2 in ColumnScript)
								{
									Name = ColumnNames[P2.Value];
									ISemanticElement Literal = await this.EvaluateSemanticElement(RecordVariables, P2.Key);

									if (!(Literal is null))
									{
										Record[Name] = new SparqlResultItem(Name, Literal, P2.Value);
										P[Name] = Literal;
									}
								}
							}

							if (MakeUnique)
							{
								bool First = true;

								sb.Clear();

								foreach (ISparqlResultItem Value in Record.Values)
								{
									if (First)
										First = false;
									else
										sb.Append(';');

									sb.Append(Value.Name);
									sb.Append('=');
									sb.Append(Value.Value?.ToString());
								}

								string Key = sb.ToString();

								if (Distinct.ContainsKey(Key))
									continue;

								Distinct[Key] = true;
							}

							Records.Add(new SparqlPatternResultRecord(Record));
						}
					}

					if (!(this.orderBy is null))
						Records.Sort(new OrderResultSet(this.orderBy));

					if (this.offset.HasValue || this.limit.HasValue)
					{
						int Offset = this.offset ?? 0;
						int MaxCount = this.limit ?? int.MaxValue;
						int Count = Records.Count;

						while (Offset > 0 && Count > 0)
						{
							Records.RemoveAt(0);
							Count--;
							Offset--;
						}

						while (Count > MaxCount)
						{
							Records.RemoveAt(MaxCount);
							Count--;
						}
					}

					return new ObjectValue(new SparqlResultSet(ColumnNames.ToArray(), Array.Empty<Uri>(),
						Records.ToArray()));

				case QueryType.Construct:
					Dictionary<string, string> BlankNodeDictionary = null;
					ChunkedList<SemanticTriple> Construction = new ChunkedList<SemanticTriple>();
					IEnumerable<ISparqlResultRecord> Items = Possibilities;

					RecordVariables = null;

					if (!(Items is null))
					{
						if (!(this.orderBy is null))
						{
							List<ISparqlResultRecord> Ordered = new List<ISparqlResultRecord>();

							foreach (ISparqlResultRecord Record in Items)
								Ordered.Add(Record);

							Ordered.Sort(new OrderResultSet(this.orderBy));
							Items = Ordered;
						}

						int Offset = this.offset ?? 0;
						int MaxCount = this.limit ?? int.MaxValue;

						foreach (ISparqlResultRecord P in Items)
						{
							if (Offset > 0)
							{
								Offset--;
								continue;
							}

							if (--MaxCount < 0)
								break;

							BlankNodeDictionary?.Clear();

							if (RecordVariables is null)
								RecordVariables = new ObjectProperties(P, Variables);
							else
								RecordVariables.Object = P;

							foreach (ISemanticTriple T in this.construct.Triples)
							{
								ISemanticElement Subject = await this.EvaluateSemanticElement(RecordVariables, T.Subject);
								if (Subject is null)
									continue;
								else if (Subject is BlankNode BnS)
								{
									if (BlankNodeDictionary is null)
										BlankNodeDictionary = new Dictionary<string, string>();

									if (!BlankNodeDictionary.TryGetValue(BnS.NodeId, out string NewLabel))
									{
										NewLabel = "n" + Guid.NewGuid().ToString();
										BlankNodeDictionary[BnS.NodeId] = NewLabel;
									}

									Subject = new BlankNode(NewLabel);
								}

								ISemanticElement Predicate = await this.EvaluateSemanticElement(RecordVariables, T.Predicate);
								if (Predicate is null)
									continue;
								else if (Predicate is BlankNode BnP)
								{
									if (BlankNodeDictionary is null)
										BlankNodeDictionary = new Dictionary<string, string>();

									if (!BlankNodeDictionary.TryGetValue(BnP.NodeId, out string NewLabel))
									{
										NewLabel = "n" + Guid.NewGuid().ToString();
										BlankNodeDictionary[BnP.NodeId] = NewLabel;
									}

									Predicate = new BlankNode(NewLabel);
								}

								ISemanticElement Object = await this.EvaluateSemanticElement(RecordVariables, T.Object);
								if (Object is null)
									continue;
								else if (Object is BlankNode BnO)
								{
									if (BlankNodeDictionary is null)
										BlankNodeDictionary = new Dictionary<string, string>();

									if (!BlankNodeDictionary.TryGetValue(BnO.NodeId, out string NewLabel))
									{
										NewLabel = "n" + Guid.NewGuid().ToString();
										BlankNodeDictionary[BnO.NodeId] = NewLabel;
									}

									Object = new BlankNode(NewLabel);
								}

								Construction.Add(new SemanticTriple(Subject, Predicate, Object));
							}
						}
					}

					return new ObjectValue(new InMemorySemanticModel(Construction));

				default:
					throw new ScriptRuntimeException("Query type not supported.", this);
			}
		}

		private async Task<ISemanticCube> GetDataSource(object From, Variables Variables,
			bool NullIfNotFound)
		{
			if (From is UriNode UriNode)
				return await this.LoadGraph(UriNode.Uri, Variables, NullIfNotFound);
			else if (From is Uri Uri)
				return await this.LoadGraph(Uri, Variables, NullIfNotFound);
			else if (From is string s)
				return await this.LoadGraph(new Uri(s, UriKind.RelativeOrAbsolute), Variables, NullIfNotFound);
			else if (From is ISemanticCube Cube)
				return Cube;
			else if (From is ISemanticModel Model)
				return await InMemorySemanticCube.Create(Model);

			if (NullIfNotFound)
				return null;
			else
				throw new ScriptRuntimeException("Graph not a semantic cube or semantic model.", this);
		}

		internal static async Task<object> EvaluateValue(Variables RecordVariables, ScriptNode Node)
		{
			try
			{
				return (await Node.EvaluateAsync(RecordVariables)).AssociatedObjectValue;
			}
			catch (ScriptReturnValueException ex)
			{
				return ex.ReturnValue.AssociatedObjectValue;
				//object ReturnValue = ex.ReturnValue.AssociatedObjectValue;
				//ScriptReturnValueException.Reuse(ex);
				//return ReturnValue;
			}
			catch (ScriptBreakLoopException ex)
			{
				return ex.LoopValue?.AssociatedObjectValue;
				//ScriptBreakLoopException.Reuse(ex);
			}
			catch (ScriptContinueLoopException ex)
			{
				return ex.LoopValue?.AssociatedObjectValue;
				//ScriptContinueLoopException.Reuse(ex);
			}
			catch (Exception)
			{
				return null;
			}
		}

		internal static async Task<object> EvaluateValue(Variables RecordVariables,
			IFilterNode Node, ISemanticCube Cube, SparqlQuery Query, Possibility P)
		{
			try
			{
				return (await Node.EvaluateAsync(RecordVariables, Cube, Query, P)).AssociatedObjectValue;
			}
			catch (ScriptReturnValueException ex)
			{
				return ex.ReturnValue.AssociatedObjectValue;
				//object ReturnValue = ex.ReturnValue.AssociatedObjectValue;
				//ScriptReturnValueException.Reuse(ex);
				//return ReturnValue;
			}
			catch (ScriptBreakLoopException ex)
			{
				return ex.LoopValue?.AssociatedObjectValue;
				//ScriptBreakLoopException.Reuse(ex);
			}
			catch (ScriptContinueLoopException ex)
			{
				return ex.LoopValue?.AssociatedObjectValue;
				//ScriptContinueLoopException.Reuse(ex);
			}
			catch (Exception ex)
			{
				return ex;
			}
		}

		internal Task<ISemanticElement> EvaluateSemanticElement(Variables RecordVariables, ISemanticElement Element)
		{
			if (Element is SemanticScriptElement ScriptElement)
				return this.EvaluateSemanticElement(RecordVariables, ScriptElement.Node);
			else
				return Task.FromResult(Element);
		}

		internal async Task<ISemanticElement> EvaluateSemanticElement(Variables RecordVariables, ScriptNode Node)
		{
			object Value = await EvaluateValue(RecordVariables, Node);
			if (Value is null)
				return null;
			else
				return SemanticElements.Encapsulate(Value);
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

				if (!(this.where is null) && !this.where.ForAllChildNodes(Callback, State, Order))
					return false;

				if (!(this.construct is null) && !this.construct.ForAllChildNodes(Callback, State, Order))
					return false;
			}

			if (!this.columns.ForAll(Callback, this, State, Order == SearchMethod.TreeOrder))
				return false;

			if (!(this.where is null) && !this.where.ForAll(Callback, State, Order))
				return false;

			if (!(this.construct is null) && !this.construct.ForAll(Callback, State, Order))
				return false;

			if (Order == SearchMethod.BreadthFirst)
			{
				if (!this.columns.ForAllChildNodes(Callback, State, Order))
					return false;

				if (!(this.where is null) && !this.where.ForAllChildNodes(Callback, State, Order))
					return false;

				if (!(this.construct is null) && !this.construct.ForAllChildNodes(Callback, State, Order))
					return false;
			}

			return true;
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			if (!(obj is SparqlQuery O) ||
				!AreEqual(this.columns, O.columns) ||
				((this.where is null) ^ (O.where is null)) ||
				((this.construct is null) ^ (O.construct is null)) ||
				this.distinct != O.distinct ||
				this.reduced != O.reduced ||
				!base.Equals(obj))
			{
				return false;
			}

			if (!(this.where is null) && !this.where.Equals(O.where))
				return false;

			if (!(this.construct is null) && !this.construct.Equals(O.construct))
				return false;

			return true;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int Result = base.GetHashCode();

			Result ^= Result << 5 ^ GetHashCode(this.columns);
			Result ^= Result << 5 ^ this.distinct.GetHashCode();
			Result ^= Result << 5 ^ this.reduced.GetHashCode();

			if (!(this.where is null))
				Result ^= Result << 5 ^ this.where.GetHashCode();

			if (!(this.construct is null))
				Result ^= Result << 5 ^ this.construct.GetHashCode();

			return Result;
		}

		private async Task<ISemanticCube> LoadGraph(Uri Uri, Variables Variables, bool NullIfNotFound)
		{
			if (Variables.TryGetVariable(" " + Uri.ToString() + " ", out Variable v) &&
				v.ValueObject is ISemanticCube Cube)
			{
				return Cube;
			}

			IGraphSource Source = await GetSourceHandler(Uri, NullIfNotFound);
			if (Source is null)
				return null;

			if (Variables.TryGetVariable("QuickLoginUser", out v) &&
				v.ValueObject is IRequestOrigin Caller)
			{
				return await Source.LoadGraph(Uri, this, NullIfNotFound, await Caller.GetOrigin());
			}
			else if (Variables.TryGetVariable("User", out v) &&
				v.ValueObject is IRequestOrigin Caller2)
			{
				return await Source.LoadGraph(Uri, this, NullIfNotFound, await Caller2.GetOrigin());
			}
			else
				return await Source.LoadGraph(Uri, this, NullIfNotFound, RequestOrigin.Empty);
		}

		/// <summary>
		/// Gets a graph source handler, given the Graph URI
		/// </summary>
		/// <param name="Uri">Graph URI</param>
		/// <param name="NullIfNotFound">If null should be returned, if a handler is not found.</param>
		/// <returns>Graph Source Handler.</returns>
		/// <exception cref="InvalidOperationException"></exception>
		public static async Task<IGraphSource> GetSourceHandler(Uri Uri, bool NullIfNotFound)
		{
			GraphReference Ref = await Database.FindFirstIgnoreRest<GraphReference>(
				new FilterFieldEqualTo("GraphUri", Uri.AbsoluteUri));

			if (!(Ref is null))
				return await Ref.GetGraphSource();

			IGraphSource Source = Types.FindBest<IGraphSource, Uri>(Uri);

			if (Source is null && !NullIfNotFound)
				throw new InvalidOperationException("Unable to get access to graph source: " + Uri.ToString());

			return Source;
		}

		/// <summary>
		/// Gets the graph name from an untyped name.
		/// </summary>
		/// <param name="Name">Untyped name</param>
		/// <returns>Graph name, or null if unable to get graph name.</returns>
		internal UriNode GetGraphName(object Name)
		{
			if (Name is UriNode UriNode)
				return UriNode;
			else if (Name is Uri Uri)
				return new UriNode(Uri, Uri.ToString());
			else if (Name is string s && System.Uri.TryCreate(s, UriKind.RelativeOrAbsolute, out Uri))
				return new UriNode(Uri, s);
			else
				return null;
		}

		/// <summary>
		/// Gets a named source
		/// </summary>
		/// <param name="Name">URI of named data source.</param>
		/// <param name="Variables">Current set of variables.</param>
		/// <returns>Semantic data set, if found, or null, if not found, or not defined.</returns>
		internal Task<ISemanticCube> GetNamedGraph(object Name, Variables Variables)
		{
			UriNode UriNode = this.GetGraphName(Name);
			if (UriNode is null)
				return Task.FromResult<ISemanticCube>(null);
			else
				return this.GetNamedGraph(UriNode, Variables);
		}

		/// <summary>
		/// Gets a named source
		/// </summary>
		/// <param name="Uri">URI of named data source.</param>
		/// <param name="Variables">Current set of variables.</param>
		/// <returns>Semantic data set, if found, or null, if not found, or not defined.</returns>
		internal async Task<ISemanticCube> GetNamedGraph(UriNode Uri, Variables Variables)
		{
			ISemanticCube Cube;

			if (this.namedGraphs is null)
				this.namedGraphs = new Dictionary<UriNode, ISemanticCube>();

			lock (this.namedGraphs)
			{
				if (!(this.namedGraphs is null) &&
					this.namedGraphs.TryGetValue(Uri, out Cube) &&
					!(Cube is null))
				{
					return Cube;
				}
			}

			Cube = await this.GetDataSource(Uri, Variables, true);
			if (Cube is null)
				Cube = new InMemorySemanticCube();

			lock (this.namedGraphs)
			{
				this.namedGraphs[Uri] = Cube;
			}

			return Cube;
		}

		/// <summary>
		/// Loads all unloaded named graphs.
		/// </summary>
		/// <param name="Variables">Current set of variables.</param>
		internal Task LoadUnloadedNamedGraphs(Variables Variables)
		{
			ChunkedList<UriNode> NotLoaded = null;

			lock (this.namedGraphs)
			{
				foreach (KeyValuePair<UriNode, ISemanticCube> P in this.namedGraphs)
				{
					if (P.Value is null)
					{
						if (NotLoaded is null)
							NotLoaded = new ChunkedList<UriNode>();

						NotLoaded.Add(P.Key);
					}
				}
			}

			if (NotLoaded is null)
				return Task.CompletedTask;

			return this.LoadUnloadedNamedGraphs(Variables, NotLoaded.ToArray());
		}

		/// <summary>
		/// Loads unloaded named graphs from the set <paramref name="Names"/>.
		/// </summary>
		/// <param name="Variables">Current set of variables.</param>
		/// <param name="Names">Graphs to make sure are loaded.</param>
		internal Task LoadUnloadedNamedGraphs(Variables Variables, params string[] Names)
		{
			return this.LoadUnloadedNamedGraphs(Variables, Convert(Names));
		}

		/// <summary>
		/// Loads unloaded named graphs from the set <paramref name="Names"/>.
		/// </summary>
		/// <param name="Variables">Current set of variables.</param>
		/// <param name="Names">Graphs to make sure are loaded.</param>
		internal Task LoadUnloadedNamedGraphs(Variables Variables, params Uri[] Names)
		{
			return this.LoadUnloadedNamedGraphs(Variables, Convert(Names));
		}

		/// <summary>
		/// Loads unloaded named graphs from the set <paramref name="Names"/>.
		/// </summary>
		/// <param name="Variables">Current set of variables.</param>
		/// <param name="Names">Graphs to make sure are loaded.</param>
		internal async Task LoadUnloadedNamedGraphs(Variables Variables, params UriNode[] Names)
		{
			ChunkedList<Task> Tasks = new ChunkedList<Task>();

			foreach (UriNode Name in Names)
				Tasks.Add(this.GetNamedGraph(Name, Variables));

			await Task.WhenAll(Tasks.ToArray());
		}

		/// <summary>
		/// Registers implicitly defined named graphs, that may be used by
		/// GRAPH patterns, even if they are not named in the query.
		/// </summary>
		/// <param name="Names">Graph names.</param>
		public void RegisterNamedGraph(params string[] Names)
		{
			this.RegisterNamedGraph(Convert(Names));
		}

		private static UriNode[] Convert(string[] Names)
		{
			int i, c = Names.Length;
			UriNode[] Nodes = new UriNode[Names.Length];

			for (i = 0; i < c; i++)
			{
				if (!Uri.TryCreate(Names[i], UriKind.RelativeOrAbsolute, out Uri Name))
					throw new ArgumentException("Not a valid URI.", nameof(Names));

				Nodes[i] = new UriNode(Name, Names[i]);
			}

			return Nodes;
		}

		/// <summary>
		/// Registers implicitly defined named graphs, that may be used by
		/// GRAPH patterns, even if they are not named in the query.
		/// </summary>
		/// <param name="Names">Graph names.</param>
		public void RegisterNamedGraph(params Uri[] Names)
		{
			this.RegisterNamedGraph(Convert(Names));
		}

		private static UriNode[] Convert(Uri[] Names)
		{
			int i, c = Names.Length;
			UriNode[] Nodes = new UriNode[Names.Length];

			for (i = 0; i < c; i++)
				Nodes[i] = new UriNode(Names[i], Names[i].ToString());

			return Nodes;
		}

		/// <summary>
		/// Registers implicitly defined named graphs, that may be used by
		/// GRAPH patterns, even if they are not named in the query.
		/// </summary>
		/// <param name="Names">Graph names.</param>
		public void RegisterNamedGraph(params UriNode[] Names)
		{
			if (this.namedGraphs is null)
				this.namedGraphs = new Dictionary<UriNode, ISemanticCube>();

			lock (this.namedGraphs)
			{
				foreach (UriNode Name in Names)
				{
					if (!this.namedGraphs.ContainsKey(Name))
						this.namedGraphs[Name] = null;
				}

				this.namedGraphNames = new UriNode[this.namedGraphs.Count];
				this.namedGraphs.Keys.CopyTo(this.namedGraphNames, 0);
			}
		}

	}
}
