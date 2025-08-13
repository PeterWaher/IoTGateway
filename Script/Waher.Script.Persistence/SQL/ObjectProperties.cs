using System;
using System.Collections.Generic;
using System.Reflection;
using Waher.Persistence;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Operators.Vectors;

namespace Waher.Script.Persistence.SQL
{
	/// <summary>
	/// Object properties.
	/// </summary>
	public class ObjectProperties : Variables
	{
		private IDictionary<string, object> dictionary;
		private Type type;
		private object obj;
		private Dictionary<string, PropertyRecord> properties = null;
		private readonly bool readOnly;

		private class PropertyRecord
		{
			public PropertyInfo Property;
			public ParameterInfo[] IndexArguments;
			public FieldInfo Field;
			public int NrIndexParameters;
			public bool IsStringIndex;

			public PropertyRecord(PropertyInfo Property, FieldInfo Field)
			{
				this.Property = Property;
				this.Field = Field;
				this.IndexArguments = null;
				this.NrIndexParameters = 0;
				this.IsStringIndex = false;
			}

			public PropertyRecord(PropertyInfo Property, FieldInfo Field, ParameterInfo[] IndexArguments)
			{
				this.Property = Property;
				this.Field = Field;
				this.IndexArguments = IndexArguments;

				if (IndexArguments is null)
				{
					this.NrIndexParameters = 0;
					this.IsStringIndex = false;
				}
				else
				{
					this.NrIndexParameters = this.IndexArguments.Length;
					this.IsStringIndex = this.NrIndexParameters == 1 &&
						this.IndexArguments[0].ParameterType == typeof(string);
				}
			}

			public object[] GetIndexArguments(string Name)
			{
				if (this.IsStringIndex)
					return new object[] { Name };

				object Converted = Expression.ConvertTo(Name, this.IndexArguments[0].ParameterType, null);

				return new object[] { Converted };
			}
		}

		/// <summary>
		/// Object properties.
		/// </summary>
		/// <param name="Object">Object</param>
		/// <param name="ContextVariables">Context Variables</param>
		public ObjectProperties(object Object, Variables ContextVariables)
			: this(Object, ContextVariables, true)
		{
		}

		/// <summary>
		/// Object properties.
		/// </summary>
		/// <param name="Object">Object</param>
		/// <param name="ContextVariables">Context Variables</param>
		/// <param name="ReadOnly">If access to object properties is read-only (default=true).</param>
		public ObjectProperties(object Object, Variables ContextVariables, bool ReadOnly)
			: base()
		{
			if (Object is IElement E)
				this.obj = E.AssociatedObjectValue;
			else
				this.obj = Object;

			this.dictionary = this.obj as IDictionary<string, object>;
			this.type = this.obj.GetType();
			this.readOnly = ReadOnly;
			this.ContextVariables = ContextVariables;
			this.ConsoleOut = ContextVariables.ConsoleOut;
		}

		/// <summary>
		/// Current object.
		/// </summary>
		public object Object
		{
			get => this.obj;
			set
			{
				if (value is IElement E)
					this.obj = E.AssociatedObjectValue;
				else
					this.obj = value;

				this.dictionary = this.obj as IDictionary<string, object>;

				Type T = this.obj.GetType();

				if (this.type != T)
				{
					this.type = T;
					this.properties?.Clear();
				}
			}
		}

		/// <summary>
		/// If the collection contains a variable with a given name.
		/// </summary>
		/// <param name="Name">Variable name.</param>
		/// <returns>If a variable with that name exists.</returns>
		public override bool ContainsVariable(string Name)
		{
			if (!(this.dictionary is null) && this.dictionary.ContainsKey(Name))
				return true;

			if (base.ContainsVariable(Name) || string.Compare(Name, "this", true) == 0)
				return true;

			lock (this.variables)
			{
				if (this.properties is null)
					this.properties = new Dictionary<string, PropertyRecord>();

				if (this.properties.TryGetValue(Name, out PropertyRecord Rec))
				{
					if (Rec.NrIndexParameters == 1)
					{
						try
						{
							ScriptNode.UnnestPossibleTaskSync(Rec.Property.GetValue(this.obj, Rec.GetIndexArguments(Name))); // TODO: Async
							return true;
						}
						catch (Exception)
						{
							return false;
						}
					}
					else
						return !(Rec is null);
				}

				PropertyInfo PI = this.type.GetRuntimeProperty(Name);
				FieldInfo FI;

				if (PI is null)
				{
					FI = this.type.GetRuntimeField(Name);

					if (!(FI is null) && !FI.IsPublic)
					{
						this.properties[Name] = null;
						return false;
					}
				}
				else
				{
					FI = null;

					if (!PI.CanRead || !PI.CanWrite || !PI.GetMethod.IsPublic || !PI.SetMethod.IsPublic)
					{
						this.properties[Name] = null;
						return false;
					}
				}

				if (PI is null && FI is null)
				{
					if (VectorIndex.TryGetIndexProperty(this.type, true, false, out PI,
						out ParameterInfo[] IndexArguments))
					{
						this.properties[Name] = new PropertyRecord(PI, FI, IndexArguments);
						return true;
					}
					else
					{
						this.properties[Name] = null;
						return false;
					}
				}
				else
				{
					this.properties[Name] = new PropertyRecord(PI, FI);
					return true;
				}
			}
		}

		/// <summary>
		/// Tries to get a variable object, given its name.
		/// </summary>
		/// <param name="Name">Variable name.</param>
		/// <param name="Variable">Variable, if found, or null otherwise.</param>
		/// <returns>If a variable with the corresponding name was found.</returns>
		public override bool TryGetVariable(string Name, out Variable Variable)
		{
			PropertyRecord Rec;

			if (string.Compare(Name, "this", true) == 0)
			{
				Variable = this.CreateVariable("this", this.obj);
				return true;
			}

			if (!(this.dictionary is null) && this.dictionary.TryGetValue(Name, out object Value))
			{
				Variable = this.CreateVariable(Name, Value);
				return true;
			}

			lock (this.variables)
			{
				if (this.properties is null)
					this.properties = new Dictionary<string, PropertyRecord>();

				if (!this.properties.TryGetValue(Name, out Rec))
				{
					PropertyInfo PI = this.type.GetRuntimeProperty(Name);
					FieldInfo FI;

					if (PI is null)
					{
						FI = this.type.GetRuntimeField(Name);

						if (!(FI is null) && !FI.IsPublic)
							FI = null;
					}
					else
					{
						FI = null;

						if (!PI.CanRead || !PI.CanWrite || !PI.GetMethod.IsPublic || !PI.SetMethod.IsPublic)
							PI = null;
					}

					if (PI is null && FI is null)
					{
						if (this.dictionary is null)
						{
							if (VectorIndex.TryGetIndexProperty(this.type, true, false, out PI,
								out ParameterInfo[] IndexArguments))
							{
								Rec = new PropertyRecord(PI, FI, IndexArguments);
							}
							else
								Rec = null;
						}
						else
							Rec = null;
					}
					else
						Rec = new PropertyRecord(PI, FI);

					this.properties[Name] = Rec;
				}
			}

			bool Result = false;

			if (!(Rec is null))
			{
				Result = true;  // null may be a valid response. Check variable collections first.

				if (Rec.Property is null)
					Value = ScriptNode.UnnestPossibleTaskSync(Rec.Field.GetValue(this.obj));    // TODO: Async
				else if (Rec.NrIndexParameters == 1)
				{
					try
					{
						Value = ScriptNode.UnnestPossibleTaskSync(Rec.Property.GetValue(this.obj, Rec.GetIndexArguments(Name))); // TODO: Async
					}
					catch (KeyNotFoundException)
					{
						Value = null;
						Result = false;
					}
				}
				else
					Value = ScriptNode.UnnestPossibleTaskSync(Rec.Property.GetValue(this.obj));    // TODO: Async

				if (!(Value is null))
				{
					Variable = this.CreateVariable(Name, Value);
					return true;
				}
			}

			if (base.TryGetVariable(Name, out Variable))
				return true;

			Variable = Result ? this.CreateVariable(Name, null) : null;
			return Result;
		}

		/// <summary>
		/// Tries to get the value, given its variable name.
		/// </summary>
		/// <param name="Name">Variable name.</param>
		/// <param name="Value">Value, if found, or null otherwise.</param>
		/// <returns>If a property or variable with the corresponding name was found.</returns>
		public bool TryGetValue(string Name, out object Value)
		{
			PropertyRecord Rec;

			if (string.Compare(Name, "this", true) == 0)
			{
				Value = this.obj;
				return true;
			}

			if (!(this.dictionary is null) && this.dictionary.TryGetValue(Name, out Value))
				return true;

			lock (this.variables)
			{
				if (this.properties is null)
					this.properties = new Dictionary<string, PropertyRecord>();

				if (!this.properties.TryGetValue(Name, out Rec))
				{
					PropertyInfo PI = this.type.GetRuntimeProperty(Name);
					FieldInfo FI;

					if (PI is null)
					{
						FI = this.type.GetRuntimeField(Name);

						if (!(FI is null) && !FI.IsPublic)
							FI = null;
					}
					else
					{
						FI = null;

						if (!PI.CanRead || !PI.CanWrite || !PI.GetMethod.IsPublic || !PI.SetMethod.IsPublic)
							PI = null;
					}

					if (PI is null && FI is null)
					{
						if (this.dictionary is null)
						{
							if (VectorIndex.TryGetIndexProperty(this.type, true, false, out PI,
								out ParameterInfo[] IndexArguments))
							{
								Rec = new PropertyRecord(PI, FI, IndexArguments);
							}
							else
								Rec = null;
						}
						else
							Rec = null;
					}
					else
						Rec = new PropertyRecord(PI, FI);

					this.properties[Name] = Rec;
				}
			}

			bool Result = false;

			if (!(Rec is null))
			{
				Result = true;  // null may be a valid response. Check variable collections first.

				if (Rec.Property is null)
					Value = ScriptNode.UnnestPossibleTaskSync(Rec.Field.GetValue(this.obj));    // TODO: Async
				else if (Rec.NrIndexParameters == 1)
				{
					try
					{
						Value = ScriptNode.UnnestPossibleTaskSync(Rec.Property.GetValue(this.obj, Rec.GetIndexArguments(Name))); // TODO: Async
					}
					catch (KeyNotFoundException)
					{
						Value = null;
						Result = false;
					}
				}
				else
					Value = ScriptNode.UnnestPossibleTaskSync(Rec.Property.GetValue(this.obj));    // TODO: Async

				if (!(Value is null))
					return true;
			}

			if (base.TryGetVariable(Name, out Variable Variable))
			{
				Value = Variable.ValueObject;
				return true;
			}

			Value = null;
			return Result;
		}

		private Variable CreateVariable(string Name, object Value)
		{
			if (Value is CaseInsensitiveString Cis)
				return new Variable(Name, Cis.Value);
			else
				return new Variable(Name, Value);
		}

		/// <summary>
		/// Adds a variable to the collection.
		/// </summary>
		/// <param name="Name">Variable name.</param>
		/// <param name="Value">Associated variable object value.</param>
		/// <returns>Reference to variable that was added. If the name
		/// refers to an object property or field, null is returned.</returns>
		public override Variable Add(string Name, object Value)
		{
			if (this.readOnly || string.Compare(Name, "this", true) == 0)
				return base.Add(Name, Value);

			PropertyRecord Rec;

			if (!(this.dictionary is null))
			{
				this.dictionary[Name] = Value is IElement Element ? Element.AssociatedObjectValue : Value;
				return null;
			}

			lock (this.variables)
			{
				if (this.properties is null)
					this.properties = new Dictionary<string, PropertyRecord>();

				if (!this.properties.TryGetValue(Name, out Rec))
				{
					PropertyInfo PI = this.type.GetRuntimeProperty(Name);
					FieldInfo FI;

					if (PI is null)
					{
						FI = this.type.GetRuntimeField(Name);

						if (!(FI is null) && !FI.IsPublic)
							FI = null;
					}
					else
					{
						FI = null;

						if (!PI.CanRead || !PI.CanWrite || !PI.GetMethod.IsPublic || !PI.SetMethod.IsPublic)
							PI = null;
					}

					if (PI is null && FI is null)
					{
						if (VectorIndex.TryGetIndexProperty(this.type, true, false, out PI,
							out ParameterInfo[] IndexArguments))
						{
							Rec = new PropertyRecord(PI, FI, IndexArguments);
						}
						else
							Rec = null;
					}
					else
						Rec = new PropertyRecord(PI, FI);

					this.properties[Name] = Rec;
				}
			}

			if (!(Rec is null))
			{
				if (Value is IElement Element)
					Value = Element.AssociatedObjectValue;

				if (Rec.Property is null)
				{
					Type ValueType = Value?.GetType() ?? typeof(object);
					Type FieldType = Rec.Field.FieldType;

					if (!FieldType.IsAssignableFrom(ValueType.GetTypeInfo()))
						Value = Expression.ConvertTo(Value, FieldType, null);

					Rec.Field.SetValue(this.obj, Value);
				}
				else
				{
					if (!Rec.Property.CanWrite)
						throw new InvalidOperationException("Property cannot be set.");
					else if (!Rec.Property.SetMethod.IsPublic)
						throw new InvalidOperationException("Property not accessible.");
					else
					{
						Type ValueType = Value?.GetType() ?? typeof(object);
						Type PropertyType = Rec.Property.PropertyType;

						if (!PropertyType.IsAssignableFrom(ValueType.GetTypeInfo()))
							Value = Expression.ConvertTo(Value, PropertyType, null);

						if (Rec.NrIndexParameters == 1)
							Rec.Property.SetValue(this.obj, Value, Rec.GetIndexArguments(Name));
						else
							Rec.Property.SetValue(this.obj, Value);
					}
				}

				return null;
			}

			if (this.ContextVariables.ContainsVariable(Name))
				return this.ContextVariables.Add(Name, Value);
			else
				return base.Add(Name, Value);
		}

	}
}
