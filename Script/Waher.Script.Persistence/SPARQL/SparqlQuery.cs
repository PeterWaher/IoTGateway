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
	public class SparqlQuery : ScriptNode, IEvaluateAsync
	{
		private readonly Dictionary<Type, ISemanticLiteral> literalPerType = new Dictionary<Type, ISemanticLiteral>();
		private readonly ScriptNode[] columns;
		private readonly ScriptNode[] columnNames;
		private readonly ScriptNode from;
		private readonly SparqlPattern where;
		private readonly KeyValuePair<ScriptNode, bool>[] orderBy;
		private readonly SparqlPattern construct;
		private readonly bool distinct;

		/// <summary>
		/// Executes a SPARQL SELECT statement.
		/// </summary>
		/// <param name="Distinct">If only distinct (unique) rows are to be returned.</param>
		/// <param name="Columns">Columns to select.</param>
		/// <param name="ColumnNames">Names of selected columns.</param>
		/// <param name="From">Data source.</param>
		/// <param name="Where">Optional where clause</param>
		/// <param name="OrderBy">Order to present result set.</param>
		/// <param name="Construct">Triples to construct.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public SparqlQuery(bool Distinct, ScriptNode[] Columns, ScriptNode[] ColumnNames,
			ScriptNode From, SparqlPattern Where, KeyValuePair<ScriptNode, bool>[] OrderBy,
			SparqlPattern Construct, int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
			this.distinct = Distinct;
			this.construct = Construct;

			this.columns = Columns;
			this.columns?.SetParent(this);

			this.columnNames = ColumnNames;
			this.columnNames?.SetParent(this);

			this.from = From;
			this.from?.SetParent(this);

			this.where = Where;
			this.where?.SetParent(this);

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

			IEnumerable<Possibility> Possibilities;
			Dictionary<string, bool> VariablesProcessed = new Dictionary<string, bool>();

			if (this.where is null)
				Possibilities = null;
			else
				Possibilities = await this.where.Search(Cube, Variables, VariablesProcessed, this);

			if (this.columns is null && this.construct is null)   // ASK
			{
				if (!(Possibilities is null))
					return new ObjectValue(new SparqlResultSet(Possibilities.GetEnumerator().MoveNext()));

				return new ObjectValue(new SparqlResultSet(false));
			}

			if (!(this.construct is null))   // CONSTRUCT
			{
				LinkedList<SemanticTriple> Construction = new LinkedList<SemanticTriple>();
				ObjectProperties RecordVariables = null;

				if (!(Possibilities is null))
				{
					foreach (Possibility P in Possibilities)
					{
						if (RecordVariables is null)
							RecordVariables = new ObjectProperties(P, Variables);
						else
							RecordVariables.Object = P;

						foreach (ISemanticTriple T in this.construct.Triples)
						{
							ISemanticElement Subject = await this.EvaluateSemanticElement(RecordVariables, T.Subject);
							ISemanticElement Predicate = await this.EvaluateSemanticElement(RecordVariables, T.Predicate);
							ISemanticElement Object = await this.EvaluateSemanticElement(RecordVariables, T.Object);

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

			if (this.columns is null)
			{
				foreach (string VariableName in VariablesProcessed.Keys)
					ColumnNames.Add(VariableName);
			}
			else
			{
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

			List<SparqlResultRecord> Records = new List<SparqlResultRecord>();

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

						Loop = Loop.NextVariable;
					}

					if (!(ColumnScript is null))
					{
						Variables RecordVariables = new ObjectProperties(P, Variables);

						foreach (KeyValuePair<ScriptNode, int> P2 in ColumnScript)
						{
							Name = ColumnNames[P2.Value];
							ISemanticElement Literal = await this.EvaluateSemanticElement(RecordVariables, P2.Key);

							Record[Name] = new SparqlResultItem(Name, Literal, P2.Value);
						}
					}

					Records.Add(new SparqlResultRecord(Record));
				}
			}

			if (!(this.orderBy is null))
			{
				KeyValuePair<string, bool>[] Order = new KeyValuePair<string, bool>[c = this.orderBy.Length];

				for (i = 0; i < c; i++)
				{
					ScriptNode Node = this.orderBy[i].Key;

					if (Node is VariableReference Ref)
						Name = Ref.VariableName;
					else
						Name = (await Node.EvaluateAsync(Variables)).AssociatedObjectValue?.ToString() ?? string.Empty;

					Order[i] = new KeyValuePair<string, bool>(Name, this.orderBy[i].Value);
				}

				Records.Sort(new OrderResultSet(Order));
			}

			return new ObjectValue(new SparqlResultSet(ColumnNames.ToArray(), new Uri[0],
				Records.ToArray()));
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

			if (Value is ISemanticElement Element)
				return Element;

			if (Value is Uri Uri)
				return new UriNode(Uri, Uri.OriginalString);

			Type T = Value?.GetType() ?? typeof(object);

			if (!this.literalPerType.TryGetValue(T, out ISemanticLiteral Literal))
			{
				Literal = Types.FindBest<ISemanticLiteral, Type>(T)
					?? new CustomLiteral(string.Empty, string.Empty);

				this.literalPerType[T] = Literal;
			}

			Literal = Literal.Encapsulate(Value);

			return Literal;
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

		private async Task<object> LoadResource(Uri Uri, Variables Variables)
		{
			if (Variables.TryGetVariable(" " + Uri.ToString() + " ", out Variable v))
				return v.ValueObject;

			return await InternetContent.GetAsync(Uri,
				new KeyValuePair<string, string>("Accept", "text/turtle, application/x-turtle, application/rdf+xml;q=0.9"));
		}

	}
}
