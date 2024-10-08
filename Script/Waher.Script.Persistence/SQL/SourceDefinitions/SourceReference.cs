using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using Waher.Events;
using Waher.Persistence.Attributes;
using Waher.Runtime.Inventory;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects.Matrices;
using Waher.Script.Operators.Matrices;
using Waher.Script.Persistence.SQL.Sources;

namespace Waher.Script.Persistence.SQL.SourceDefinitions
{
	/// <summary>
	/// Direct reference to a data source.
	/// </summary>
	public class SourceReference : SourceDefinition
	{
		private ScriptNode source;
		private ScriptNode alias;

		/// <summary>
		/// Direct reference to a data source.
		/// </summary>
		/// <param name="Source">Source definition.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public SourceReference(ScriptNode Source, int Start, int Length, Expression Expression)
			: this(Source, null, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Direct reference to a data source.
		/// </summary>
		/// <param name="Source">Source definition.</param>
		/// <param name="Alias">Alias definition.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public SourceReference(ScriptNode Source, ScriptNode Alias, int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
			this.source = Source;
			this.source?.SetParent(this);

			this.alias = Alias;
			this.alias?.SetParent(this);
		}

		/// <summary>
		/// Gets the actual data source, from its definition.
		/// </summary>
		/// <param name="Variables">Current set of variables.</param>
		/// <returns>Data Source</returns>
		public override async Task<IDataSource> GetSource(Variables Variables)
		{
			string Alias;

			if (this.alias is null)
				Alias = string.Empty;
			else if (this.alias is VariableReference Ref)
				Alias = Ref.VariableName;
			else
				Alias = (await this.alias.EvaluateAsync(Variables)).AssociatedObjectValue?.ToString();

			if (this.source is VariableReference Ref2)
				return GetDataSource(Ref2, Alias, Variables);
			else
			{
				if (this.source is IEvaluateAsync AsyncSource)
					return GetDataSource(string.Empty, Alias, await AsyncSource.EvaluateAsync(Variables), this.source);
				else
					return GetDataSource(string.Empty, Alias, await this.source.EvaluateAsync(Variables), this.source);
			}
		}

		private static IDataSource GetDataSource(string Name, string Alias, IElement E, ScriptNode Source)
		{
			object Obj = E.AssociatedObjectValue;

			if (Obj is IToMatrix ToMatrix)
			{
				E = ToMatrix.ToMatrix();
				Obj = E.AssociatedObjectValue;
			}

			if (Obj is Type T)
				return new TypeSource(T, Alias);
			else if (Obj is string s)
				return new CollectionSource(s, Alias);
			else if (E is ObjectMatrix OM && OM.HasColumnNames)
				return new VectorSource(Name, Alias, VectorSource.ToGenericObjectVector(OM), Source);
			else if (E is IVector V)
				return new VectorSource(Name, Alias, V, Source);
			else if (Obj is XmlDocument Doc)
				return new XmlSource(Name, Alias, Doc, Source);
			else if (Obj is XmlNode N)
				return new XmlSource(Name, Alias, N, Source);
			else if (Obj is IDataSource DataSource)
				return DataSource;

			throw new ScriptRuntimeException("Data source type not supported: " + E.AssociatedObjectValue?.GetType()?.FullName, Source);
		}

		private static IDataSource GetDataSource(VariableReference Source, string Alias, Variables Variables)
		{
			string Name = Source.VariableName;

			if (Variables.TryGetVariable(Name, out Variable v))
				return GetDataSource(Name, Alias, v.ValueElement, Source);

			if (Expression.TryGetConstant(Name, Variables, out IElement ValueElement))
				return GetDataSource(Name, Alias, ValueElement, Source);

			if (Types.TryGetQualifiedNames(Name, out string[] QualifiedNames))
			{
				if (QualifiedNames.Length == 1)
				{
					Type T = Types.GetType(QualifiedNames[0]);

					if (!(T is null))
						return new TypeSource(T, Alias);
				}
				else
				{
					List<KeyValuePair<string, object>> TypesWithCollectionNames = null;
					Type TypeWithCollection = null;
					bool CollectionUnique = true;

					foreach (string QualifiedName in QualifiedNames)
					{
						Type T = Types.GetType(QualifiedName);
						if (T is null)
							continue;

						TypeInfo TI = T.GetTypeInfo();
						int Nr = 1;

						if (!(TI.GetCustomAttribute<CollectionNameAttribute>() is null))
						{
							if (TypeWithCollection is null)
								TypeWithCollection = T;
							else
							{
								CollectionUnique = false;

								if (TypesWithCollectionNames is null)
								{
									TypesWithCollectionNames = new List<KeyValuePair<string, object>>
									{
										new KeyValuePair<string, object>("Type " + Nr.ToString(), TypeWithCollection.FullName)
									};

									Nr++;
								}

								TypesWithCollectionNames.Add(new KeyValuePair<string, object>(Nr.ToString(), T.FullName));
								Nr++;
							}
						}
					}

					if (TypeWithCollection is null)
					{
						Log.Warning("A collection was referenced using a relative type name. The type does not have a collection name defined. To avoid confusion, reference the collection name as a string constant instead of a variable reference.",
							Name, string.Empty, "DBOpt");
					}
					else
					{
						if (CollectionUnique)
						{
							Log.Warning("A collection was referenced using a relative type name. Multiple types are available with the same relative type name, but only one had a collection name defined for it. Using this type. Use fully.qualified type names to avoid confusion.",
								Name, TypeWithCollection.FullName, "DBOpt");

							return new TypeSource(TypeWithCollection, Alias);
						}
						else
						{
							Log.Error("A collection was referenced using a relative type name. Multiple types are available with the same relative type name and with collection names. Use fully.qualified type names to avoid confusion.",
								Name, string.Empty, "DBOpt", TypesWithCollectionNames.ToArray());
						}
					}
				}
			}

			return new CollectionSource(Name, Alias);
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
				if (!(this.source?.ForAllChildNodes(Callback, State, Order) ?? true) ||
					!(this.alias?.ForAllChildNodes(Callback, State, Order) ?? true))
				{
					return false;
				}
			}

			ScriptNode NewNode;
			bool b;

			if (!(this.source is null))
			{
				b = !Callback(this.source, out NewNode, State);
				if (!(NewNode is null))
				{
					this.source = NewNode;
					this.source.SetParent(this);
				}

				if (b || (Order == SearchMethod.TreeOrder && !this.source.ForAllChildNodes(Callback, State, Order)))
					return false;
			}

			if (!(this.alias is null))
			{
				b = !Callback(this.alias, out NewNode, State);
				if (!(NewNode is null))
				{
					this.alias = NewNode;
					this.alias.SetParent(this);
				}

				if (b || (Order == SearchMethod.TreeOrder && !this.alias.ForAllChildNodes(Callback, State, Order)))
					return false;
			}

			if (Order == SearchMethod.BreadthFirst)
			{
				if (!(this.source?.ForAllChildNodes(Callback, State, Order) ?? true) ||
					!(this.alias?.ForAllChildNodes(Callback, State, Order) ?? true))
				{
					return false;
				}
			}

			return true;
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return (obj is SourceReference O &&
				AreEqual(this.source, O.source) &&
				AreEqual(this.alias, O.alias) &&
				base.Equals(obj));
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int Result = base.GetHashCode();
			Result ^= Result << 5 ^ GetHashCode(this.source);
			Result ^= Result << 5 ^ GetHashCode(this.alias);

			return Result;
		}

	}
}
