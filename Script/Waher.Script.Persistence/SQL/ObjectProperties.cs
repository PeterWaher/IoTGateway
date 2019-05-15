using System;
using System.Collections.Generic;
using System.Reflection;
using Waher.Persistence;

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
		private Dictionary<string, Tuple<PropertyInfo, FieldInfo>> properties = null;

		/// <summary>
		/// Object properties.
		/// </summary>
		/// <param name="Object">Object</param>
		/// <param name="Variables">Variables</param>
		public ObjectProperties(object Object, Variables Variables)
			: base()
		{
			this.obj = Object;
			this.dictionary = Object as IDictionary<string, object>;
			this.type = Object.GetType();
			this.variables2 = Variables;
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
					this.properties = new Dictionary<string, Tuple<PropertyInfo, FieldInfo>>();

				if (this.properties.TryGetValue(Name, out Tuple<PropertyInfo, FieldInfo> Rec))
					return Rec != null;

				PropertyInfo PI = this.type.GetRuntimeProperty(Name);
				FieldInfo FI = PI is null ? this.type.GetRuntimeField(Name) : null;

				if (PI is null && FI is null)
				{
					this.properties[Name] = null;
					return false;
				}
				else
				{
					this.properties[Name] = new Tuple<PropertyInfo, FieldInfo>(PI, FI);
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
			Tuple<PropertyInfo, FieldInfo> Rec;
			object Value;

			if (string.Compare(Name, "this", true) == 0)
			{
                Variable = this.CreateVariable("this", this.obj);
				return true;
			}

			if (this.dictionary != null && this.dictionary.TryGetValue(Name, out Value))
			{
				Variable = this.CreateVariable(Name, Value);
				return true;
			}

			lock (this.variables)
			{
				if (this.properties is null)
					this.properties = new Dictionary<string, Tuple<PropertyInfo, FieldInfo>>();

				if (!this.properties.TryGetValue(Name, out Rec))
				{
					PropertyInfo PI = this.type.GetRuntimeProperty(Name);
					FieldInfo FI = PI is null ? this.type.GetRuntimeField(Name) : null;

					if (PI is null && FI is null)
						Rec = null;
					else
						Rec = new Tuple<PropertyInfo, FieldInfo>(PI, FI);

					this.properties[Name] = Rec;
				}
			}

			if (Rec != null)
			{
				if (Rec.Item1 != null)
					Value = Rec.Item1.GetValue(this.obj);
				else
					Value = Rec.Item2.GetValue(this.obj);

				Variable = this.CreateVariable(Name, Value);

				return true;
			}

			if (this.variables2.TryGetVariable(Name, out Variable))
				return true;

			if (base.TryGetVariable(Name, out Variable))
				return true;

			Variable = null;
			return false;
		}

        private Variable CreateVariable(string Name, object Value)
        {
            if (Value is CaseInsensitiveString Cis)
                return new Variable(Name, Cis.Value);
            else
                return new Variable(Name, Value);
        }

    }
}
