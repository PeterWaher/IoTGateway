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
		private Dictionary<string, Tuple<PropertyInfo, FieldInfo, bool>> properties = null;
		private readonly bool readOnly;

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
					this.properties = new Dictionary<string, Tuple<PropertyInfo, FieldInfo, bool>>();

				if (this.properties.TryGetValue(Name, out Tuple<PropertyInfo, FieldInfo, bool> Rec))
				{
					if (Rec.Item3)
					{
						try
						{
							ScriptNode.UnnestPossibleTaskSync(Rec.Item1.GetValue(this.obj, new object[] { Name })); // TODO: Async
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
					if (VectorIndex.TryGetIndexProperty(this.type, true, false, out PI, out _))
					{
						this.properties[Name] = new Tuple<PropertyInfo, FieldInfo, bool>(PI, FI, true);
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
					this.properties[Name] = new Tuple<PropertyInfo, FieldInfo, bool>(PI, FI, false);
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
			Tuple<PropertyInfo, FieldInfo, bool> Rec;

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
					this.properties = new Dictionary<string, Tuple<PropertyInfo, FieldInfo, bool>>();

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
							if (VectorIndex.TryGetIndexProperty(this.type, true, false, out PI, out _))
								Rec = new Tuple<PropertyInfo, FieldInfo, bool>(PI, FI, true);
							else
								Rec = null;
						}
						else
							Rec = null;
					}
					else
						Rec = new Tuple<PropertyInfo, FieldInfo, bool>(PI, FI, false);

					this.properties[Name] = Rec;
				}
			}

			bool Result = false;

			if (!(Rec is null))
			{
				Result = true;  // null may be a valid response. Check variable collections first.

				if (Rec.Item1 is null)
					Value = ScriptNode.UnnestPossibleTaskSync(Rec.Item2.GetValue(this.obj));    // TODO: Async
				else if (Rec.Item3)
				{
					try
					{
						Value = ScriptNode.UnnestPossibleTaskSync(Rec.Item1.GetValue(this.obj, new object[] { Name })); // TODO: Async
					}
					catch (KeyNotFoundException)
					{
						Value = null;
						Result = false;
					}
				}
				else
					Value = ScriptNode.UnnestPossibleTaskSync(Rec.Item1.GetValue(this.obj));    // TODO: Async

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
			Tuple<PropertyInfo, FieldInfo, bool> Rec;

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
					this.properties = new Dictionary<string, Tuple<PropertyInfo, FieldInfo, bool>>();

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
							if (VectorIndex.TryGetIndexProperty(this.type, true, false, out PI, out _))
								Rec = new Tuple<PropertyInfo, FieldInfo, bool>(PI, FI, true);
							else
								Rec = null;
						}
						else
							Rec = null;
					}
					else
						Rec = new Tuple<PropertyInfo, FieldInfo, bool>(PI, FI, false);

					this.properties[Name] = Rec;
				}
			}

			bool Result = false;

			if (!(Rec is null))
			{
				Result = true;  // null may be a valid response. Check variable collections first.

				if (Rec.Item1 is null)
					Value = ScriptNode.UnnestPossibleTaskSync(Rec.Item2.GetValue(this.obj));    // TODO: Async
				else if (Rec.Item3)
				{
					try
					{
						Value = ScriptNode.UnnestPossibleTaskSync(Rec.Item1.GetValue(this.obj, new object[] { Name })); // TODO: Async
					}
					catch (KeyNotFoundException)
					{
						Value = null;
						Result = false;
					}
				}
				else
					Value = ScriptNode.UnnestPossibleTaskSync(Rec.Item1.GetValue(this.obj));    // TODO: Async

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
		public override void Add(string Name, object Value)
		{
			if (this.readOnly || string.Compare(Name, "this", true) == 0)
			{
				base.Add(Name, Value);
				return;
			}

			Tuple<PropertyInfo, FieldInfo, bool> Rec;

			if (!(this.dictionary is null))
			{
				this.dictionary[Name] = Value is IElement Element ? Element.AssociatedObjectValue : Value;
				return;
			}

			lock (this.variables)
			{
				if (this.properties is null)
					this.properties = new Dictionary<string, Tuple<PropertyInfo, FieldInfo, bool>>();

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
						if (VectorIndex.TryGetIndexProperty(this.type, true, false, out PI, out _))
							Rec = new Tuple<PropertyInfo, FieldInfo, bool>(PI, FI, true);
						else
							Rec = null;
					}
					else
						Rec = new Tuple<PropertyInfo, FieldInfo, bool>(PI, FI, false);

					this.properties[Name] = Rec;
				}
			}

			if (!(Rec is null))
			{
				if (Value is IElement Element)
					Value = Element.AssociatedObjectValue;

				if (Rec.Item1 is null)
					Rec.Item2.SetValue(this.obj, Value);
				else
				{
					if (!Rec.Item1.CanWrite)
						throw new InvalidOperationException("Property cannot be set.");
					else if (!Rec.Item1.SetMethod.IsPublic)
						throw new InvalidOperationException("Property not accessible.");
					else if (Rec.Item3)
						Rec.Item1.SetValue(this.obj, Value, new object[] { Name });
					else
						Rec.Item1.SetValue(this.obj, Value);
				}

				return;
			}

			if (this.ContextVariables.ContainsVariable(Name))
				this.ContextVariables.Add(Name, Value);
			else
				base.Add(Name, Value);
		}

	}
}
