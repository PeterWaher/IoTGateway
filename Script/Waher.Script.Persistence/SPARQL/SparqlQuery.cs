using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Semantic;
using Waher.Content.Semantic.Model;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Persistence.SPARQL.Filters;
using Waher.Script.Persistence.SPARQL.Patterns;
using Waher.Script.Persistence.SQL;

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
		private readonly Dictionary<UriNode, ISemanticCube> namedGraphs;
		private readonly UriNode[] namedGraphNames;
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
		private readonly bool distinct;

		/// <summary>
		/// Executes a SPARQL query.
		/// </summary>
		/// <param name="QueryType">Query type</param>
		/// <param name="Distinct">If only distinct (unique) rows are to be returned.</param>
		/// <param name="Columns">Columns to select.</param>
		/// <param name="ColumnNames">Names of selected columns.</param>
		/// <param name="From">Data sources.</param>
		/// <param name="NamedGraphs">Named graphs.</param>
		/// <param name="Where">Optional where clause</param>
		/// <param name="GroupBy">Any GROUP BY rules.</param>
		/// <param name="GroupByNames">Optional names for GROUP BY rules.</param>
		/// <param name="Having">Any filter on groups.</param>
		/// <param name="OrderBy">Order to present result set.</param>
		/// <param name="Construct">Triples to construct.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public SparqlQuery(QueryType QueryType, bool Distinct, ScriptNode[] Columns,
			ScriptNode[] ColumnNames, ScriptNode[] From, Dictionary<UriNode, ISemanticCube> NamedGraphs,
			ISparqlPattern Where, ScriptNode[] GroupBy, ScriptNode[] GroupByNames, ScriptNode Having,
			KeyValuePair<ScriptNode, bool>[] OrderBy, SparqlRegularPattern Construct,
			int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
			this.queryType = QueryType;
			this.distinct = Distinct;
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
		/// Names of named graphs.
		/// </summary>
		internal UriNode[] NamedGraphNames => this.namedGraphNames;

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
				SortedDictionary<ISparqlResultRecord, KeyValuePair<ISparqlResultRecord, LinkedList<ISparqlResultRecord>>> Groups =
					new SortedDictionary<ISparqlResultRecord, KeyValuePair<ISparqlResultRecord, LinkedList<ISparqlResultRecord>>>(GroupComparer);
				LinkedList<ISparqlResultRecord> LastList = null;
				ISparqlResultRecord LastRecord = null;
				bool First = false;

				foreach (ISparqlResultRecord P in Possibilities)
				{
					if (LastRecord is null || GroupComparer.Compare(LastRecord, P) != 0)
					{
						if (Groups.TryGetValue(P, out KeyValuePair<ISparqlResultRecord, LinkedList<ISparqlResultRecord>> P2))
						{
							LastRecord = P2.Key;
							LastList = P2.Value;
							First = false;
						}
						else
						{
							LastList = new LinkedList<ISparqlResultRecord>();
							Groups[P] = new KeyValuePair<ISparqlResultRecord, LinkedList<ISparqlResultRecord>>(P, LastList);
							LastRecord = P;
							First = true;
						}
					}

					LastList.AddLast(P);

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
					LinkedList<ISparqlResultRecord> Filtered = new LinkedList<ISparqlResultRecord>();
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
								Filtered.AddLast(Record);
						}
						catch (Exception)
						{
							// Ignore record
						}
					}

					Possibilities = Filtered;
				}
			}

			if (this.queryType == QueryType.Ask)
			{
				if (!(Possibilities is null))
					return new ObjectValue(new SparqlResultSet(Possibilities.GetEnumerator().MoveNext()));

				return new ObjectValue(new SparqlResultSet(false));
			}

			if (this.queryType == QueryType.Construct)
			{
				LinkedList<SemanticTriple> Construction = new LinkedList<SemanticTriple>();
				ObjectProperties RecordVariables = null;

				if (!(Possibilities is null))
				{
					foreach (ISparqlResultRecord P in Possibilities)
					{
						if (RecordVariables is null)
							RecordVariables = new ObjectProperties(P, Variables);
						else
							RecordVariables.Object = P;

						foreach (ISemanticTriple T in this.construct.Triples)
						{
							ISemanticElement Subject = await this.EvaluateSemanticElement(RecordVariables, T.Subject);
							if (Subject is null)
								continue;

							ISemanticElement Predicate = await this.EvaluateSemanticElement(RecordVariables, T.Predicate);
							if (Predicate is null)
								continue;

							ISemanticElement Object = await this.EvaluateSemanticElement(RecordVariables, T.Object);
							if (Object is null)
								continue;

							Construction.AddLast(new SemanticTriple(Subject, Predicate, Object));
						}
					}
				}

				return new ObjectValue(new InMemorySemanticModel(Construction));
			}

			Dictionary<string, int> ColumnVariables = new Dictionary<string, int>();
			LinkedList<KeyValuePair<ScriptNode, int>> ColumnScript = null;
			List<string> ColumnNames = new List<string>();
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
					}
					else
					{
						if (ColumnScript is null)
							ColumnScript = new LinkedList<KeyValuePair<ScriptNode, int>>();

						ColumnScript.AddLast(new KeyValuePair<ScriptNode, int>(this.columns[i], i));

						if (Name is null)
						{
							Name = " c" + i.ToString();
							ColumnNames.Add(Name);
						}
					}
				}
			}

			List<ISparqlResultRecord> Records = new List<ISparqlResultRecord>();
			Dictionary<string, bool> Distinct = this.distinct ? new Dictionary<string, bool>() : null;
			StringBuilder sb = this.distinct ? new StringBuilder() : null;

			if (!(Possibilities is null))
			{
				ObjectProperties RecordVariables = null;

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
								Record[Name] = new SparqlResultItem(Name, Literal, P2.Value);
						}
					}

					if (this.distinct)
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

			return new ObjectValue(new SparqlResultSet(ColumnNames.ToArray(), new Uri[0],
				Records.ToArray()));
		}

		private async Task<ISemanticCube> GetDataSource(object From, Variables Variables,
			bool NullIfNotFound)
		{
			if (From is UriNode UriNode)
				From = await LoadResource(UriNode.Uri, Variables);
			else if (From is Uri Uri)
				From = await LoadResource(Uri, Variables);
			else if (From is string s)
				From = await LoadResource(new Uri(s, UriKind.RelativeOrAbsolute), Variables);

			if (!(From is ISemanticCube Cube))
			{
				if (From is ISemanticModel Model)
					Cube = await InMemorySemanticCube.Create(Model);
				else if (NullIfNotFound)
					return null;
				else
					throw new ScriptRuntimeException("Default graph not a semantic cube or semantic model.", this);
			}

			return Cube;
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

			if (!(this.where is null))
				Result ^= Result << 5 ^ this.where.GetHashCode();

			if (!(this.construct is null))
				Result ^= Result << 5 ^ this.construct.GetHashCode();

			return Result;
		}

		private static async Task<object> LoadResource(Uri Uri, Variables Variables)
		{
			if (Variables.TryGetVariable(" " + Uri.ToString() + " ", out Variable v))
				return v.ValueObject;

			// TODO: Check locally hosted sources.

			if (!Uri.IsAbsoluteUri)
				throw new InvalidOperationException("URI not absolute.");

			return await InternetContent.GetAsync(Uri,
				new KeyValuePair<string, string>("Accept", "text/turtle, application/x-turtle, application/rdf+xml;q=0.9"));
		}


		/// <summary>
		/// Gets a named source
		/// </summary>
		/// <param name="Name">URI of named data source.</param>
		/// <param name="Variables">Current set of variables.</param>
		/// <returns>Semantic data set, if found, or null, if not found, or not defined.</returns>
		internal Task<ISemanticCube> GetNamedGraph(object Name, Variables Variables)
		{
			if (Name is UriNode UriNode)
				return this.GetNamedGraph(UriNode, Variables);
			else if (Name is Uri Uri)
				return this.GetNamedGraph(new UriNode(Uri, Uri.ToString()), Variables);
			else if (Name is string s && System.Uri.TryCreate(s, UriKind.RelativeOrAbsolute, out Uri))
				return this.GetNamedGraph(new UriNode(Uri, s), Variables);
			else
				return null;
		}

		/// <summary>
		/// Gets a named source
		/// </summary>
		/// <param name="Uri">URI of named data source.</param>
		/// <param name="Variables">Current set of variables.</param>
		/// <returns>Semantic data set, if found, or null, if not found, or not defined.</returns>
		internal async Task<ISemanticCube> GetNamedGraph(UriNode Uri, Variables Variables)
		{
			if (!this.namedGraphs.TryGetValue(Uri, out ISemanticCube Cube))
				return null;

			if (!(Cube is null))
				return Cube;

			Cube = await this.GetDataSource(Uri, Variables, true);
			if (Cube is null)
				Cube = new InMemorySemanticCube();

			this.namedGraphs[Uri] = Cube;

			return Cube;
		}

	}
}
