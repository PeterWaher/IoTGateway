using System;
using System.Collections.Generic;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Persistence.SQL.Groups
{
	/// <summary>
	/// Represents a collection of objects grouped together useing a GROUP BY construct.
	/// </summary>
	public class GroupObject
	{
		private readonly object[] objects;
		private readonly Variables variables;
		private readonly Dictionary<string, object> groupedValues = new Dictionary<string, object>();
		private readonly int objectCount;
		private readonly int groupCount;
		private ObjectProperties properties = null;

		/// <summary>
		/// Represents a collection of objects grouped together useing a GROUP BY construct.
		/// </summary>
		/// <param name="Objects">Objects</param>
		/// <param name="GroupByValues">Grouped values that are constant in the group.</param>
		/// <param name="GroupNames">Names of the grouped values.</param>
		/// <param name="Variables">Current set of variables.</param>
		public GroupObject(object[] Objects, object[] GroupByValues, ScriptNode[] GroupNames, Variables Variables)
		{
			this.objects = Objects;
			this.variables = Variables;

			int i;

			this.objectCount = this.objects.Length;
			this.groupCount = GroupNames.Length;
			if (GroupByValues.Length != this.groupCount)
				throw new ArgumentException("Length mismatch.", nameof(GroupByValues));

			string Name;
			ScriptNode Node;
			IElement E;

			for (i = 0; i < this.groupCount; i++)
			{
				Node = GroupNames[i];
				if (Node is null)
					continue;

				if (Node is VariableReference Ref)
					Name = Ref.VariableName;
				else
				{
					E = Node.Evaluate(Variables);
					Name = E.AssociatedObjectValue?.ToString();
				}

				if (!string.IsNullOrEmpty(Name))
					this.groupedValues[Name] = GroupByValues[i];
			}
		}

		/// <summary>
		/// Access to grouped properties.
		/// </summary>
		/// <param name="Name">Named property</param>
		/// <returns></returns>
		public object this[string Name]
		{
			get
			{
				if (this.groupedValues.TryGetValue(Name, out object Value))
					return Value;

				if (this.objectCount == 1)
				{
					object Result;
					object Item = this.objects[0];

					if (Item is null)
						Result = null;
					else
					{
						if (this.properties is null)
							this.properties = new ObjectProperties(Item, this.variables);
						else
							this.properties.Object = Item;

						if (this.properties.TryGetValue(Name, out Value))
							Result = Value;
						else
							Result = null;
					}

					this.groupedValues[Name] = Result;

					return Result;
				}
				else
				{
					Type ItemType = null;
					Type T;
					object[] Result = new object[this.objectCount];
					object Item;
					int i;

					for (i = 0; i < this.objectCount; i++)
					{
						Item = this.objects[i];

						if (Item is null)
						{
							Result[i] = null;
							ItemType = null;
						}
						else
						{
							if (this.properties is null)
								this.properties = new ObjectProperties(Item, this.variables);
							else
								this.properties.Object = Item;

							if (this.properties.TryGetValue(Name, out Value) && !(Value is null))
							{
								T = Value.GetType();
								if (i == 0)
									ItemType = T;
								else if (T != ItemType)
									ItemType = null;

								Result[i] = Value;
							}
							else
							{
								Result[i] = null;
								ItemType = null;
							}
						}
					}

					if (ItemType == typeof(double))
					{
						double[] TypedArray = new double[this.objectCount];
						Array.Copy(Result, 0, TypedArray, 0, this.objectCount);
						return TypedArray;
					}
					else if (ItemType == typeof(bool))
					{
						bool[] TypedArray = new bool[this.objectCount];
						Array.Copy(Result, 0, TypedArray, 0, this.objectCount);
						return TypedArray;
					}
					else if (ItemType == typeof(string))
					{
						string[] TypedArray = new string[this.objectCount];
						Array.Copy(Result, 0, TypedArray, 0, this.objectCount);
						return TypedArray;
					}
					else if (ItemType == typeof(DateTime))
					{
						DateTime[] TypedArray = new DateTime[this.objectCount];
						Array.Copy(Result, 0, TypedArray, 0, this.objectCount);
						return TypedArray;
					}
					else if (ItemType == typeof(Complex))
					{
						Complex[] TypedArray = new Complex[this.objectCount];
						Array.Copy(Result, 0, TypedArray, 0, this.objectCount);
						return TypedArray;
					}
					else
					{
						this.groupedValues[Name] = Result;
						return Result;
					}
				}
			}

			set
			{
				this.groupedValues[Name] = value;
			}
		}

		/// <summary>
		/// Number of objects in group.
		/// </summary>
		public int Count => this.objects.Length;

		/// <summary>
		/// Objects in group.
		/// </summary>
		public object[] Objects => this.objects;

	}
}
