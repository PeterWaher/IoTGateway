using System;
using System.Collections.Generic;
using System.Reflection;
using Waher.Persistence;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Persistence.SQL
{
	/// <summary>
	/// Object properties.
	/// </summary>
	public class ObjectProperties : Variables
	{
		private IDictionary<string, object> dictionary;
		private Type type;
		private readonly Variables variables2;
		private object obj;
		private Dictionary<string, Tuple<PropertyInfo, FieldInfo, bool>> properties = null;
		private readonly bool readOnly;

		/// <summary>
		/// Object properties.
		/// </summary>
		/// <param name="Object">Object</param>
		/// <param name="Variables">Variables</param>
		public ObjectProperties(object Object, Variables Variables)
			: this(Object, Variables, true)
		{
		}

		/// <summary>
		/// Object properties.
		/// </summary>
		/// <param name="Object">Object</param>
		/// <param name="Variables">Variables</param>
		/// <param name="ReadOnly">If access to object properties is read-only (default=true).</param>
		public ObjectProperties(object Object, Variables Variables, bool ReadOnly)
			: base()
		{
			this.obj = Object;
			this.dictionary = Object as IDictionary<string, object>;
			this.type = Object.GetType();
			this.variables2 = Variables;
			this.readOnly = ReadOnly;
		}

		/// <summary>
		/// Current object.
		/// </summary>
		public object Object
		{
			get => this.obj;
			set
			{
				this.obj = value;
				this.dictionary = value as IDictionary<string, object>;
				this.type = value.GetType();
			}
		}

		/// <summary>
		/// If the collection contains a variable with a given name.
		/// </summary>
		/// <param name="Name">Variable name.</param>
		/// <returns>If a variable with that name exists.</returns>
		public override bool ContainsVariable(string Name)
		{
			if (this.dictionary != null && this.dictionary.ContainsKey(Name))
				return true;

			if (this.variables2.ContainsVariable(Name) || base.ContainsVariable(Name) || string.Compare(Name, "this", true) == 0)
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
							Rec.Item1.GetValue(this.obj, new object[] { Name });
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
				FieldInfo FI = PI is null ? this.type.GetRuntimeField(Name) : null;

				if (PI is null && FI is null)
				{
					PI = this.type.GetRuntimeProperty("Item");

					if (PI is null)
					{
						this.properties[Name] = null;
						return false;
					}
					else
					{
						this.properties[Name] = new Tuple<PropertyInfo, FieldInfo, bool>(PI, FI, true);
						return true;
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

			if (this.dictionary != null && this.dictionary.TryGetValue(Name, out object Value))
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
					FieldInfo FI = PI is null ? this.type.GetRuntimeField(Name) : null;

					if (PI is null && FI is null)
					{
						PI = this.type.GetRuntimeProperty("Item");

						if (PI is null)
							Rec = null;
						else
							Rec = new Tuple<PropertyInfo, FieldInfo, bool>(PI, FI, true);
					}
					else
						Rec = new Tuple<PropertyInfo, FieldInfo, bool>(PI, FI, false);

					this.properties[Name] = Rec;
				}
			}

			bool Result = false;

			if (!(Rec is null))
			{
				if (Rec.Item1 is null)
					Value = Rec.Item2.GetValue(this.obj);
				else if (Rec.Item3)
					Value = Rec.Item1.GetValue(this.obj, new object[] { Name });
				else
					Value = Rec.Item1.GetValue(this.obj);

				if (!(Value is null))
				{
					Variable = this.CreateVariable(Name, Value);
					return true;
				}

				Result = true;  // null may be a valid response. Check variable collections first.
			}

			if (this.variables2.TryGetVariable(Name, out Variable))
				return true;

			if (base.TryGetVariable(Name, out Variable))
				return true;

			Variable = Result ? Variable = this.CreateVariable(Name, null) : null;
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
			if (this.readOnly ||
				string.Compare(Name, "this", true) == 0)
			{
				base.Add(Name, Value);
				return;
			}

			Tuple<PropertyInfo, FieldInfo, bool> Rec;

			if (this.dictionary != null)
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
					FieldInfo FI = PI is null ? this.type.GetRuntimeField(Name) : null;

					if (PI is null && FI is null)
					{
						PI = this.type.GetRuntimeProperty("Item");

						if (PI is null)
							Rec = null;
						else
							Rec = new Tuple<PropertyInfo, FieldInfo, bool>(PI, FI, true);
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
				else if (Rec.Item3)
					Rec.Item1.SetValue(this.obj, Value, new object[] { Name });
				else
					Rec.Item1.SetValue(this.obj, Value);

				return;
			}

			if (this.variables2.ContainsVariable(Name))
				this.variables2.Add(Name, Value);
			else
				base.Add(Name, Value);
		}

	}
}
