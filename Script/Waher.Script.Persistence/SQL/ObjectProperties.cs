using System;
using System.Collections.Generic;
using System.Reflection;

namespace Waher.Script.Persistence.SQL
{
	/// <summary>
	/// Object properties.
	/// </summary>
	public class ObjectProperties : Variables
	{
		private readonly object obj;
		private readonly Type type;
		private Dictionary<string, Tuple<PropertyInfo, FieldInfo>> properties = null;
		/// <summary>
		/// Object properties.
		/// </summary>
		/// <param name="Object">Object</param>
		public ObjectProperties(object Object)
			: base()
		{
			this.obj = Object;
			this.type = Object.GetType();
		}

		/// <summary>
		/// If the collection contains a variable with a given name.
		/// </summary>
		/// <param name="Name">Variable name.</param>
		/// <returns>If a variable with that name exists.</returns>
		public override bool ContainsVariable(string Name)
		{
			if (base.ContainsVariable(Name))
				return true;

			lock (this.variables)
			{
				if (this.properties == null)
					this.properties = new Dictionary<string, Tuple<PropertyInfo, FieldInfo>>();

				if (this.properties.TryGetValue(Name, out Tuple<PropertyInfo, FieldInfo> Rec))
					return Rec != null;

				PropertyInfo PI = this.type.GetRuntimeProperty(Name);
				FieldInfo FI = PI == null ? this.type.GetRuntimeField(Name) : null;

				if (PI == null && FI == null)
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
			if (base.TryGetVariable(Name, out Variable))
				return true;

			Tuple<PropertyInfo, FieldInfo> Rec;

			lock (this.variables)
			{
				if (this.properties == null)
					this.properties = new Dictionary<string, Tuple<PropertyInfo, FieldInfo>>();

				if (!this.properties.TryGetValue(Name, out Rec))
				{
					PropertyInfo PI = this.type.GetRuntimeProperty(Name);
					FieldInfo FI = PI == null ? this.type.GetRuntimeField(Name) : null;

					if (PI == null && FI == null)
						Rec = null;
					else
						Rec = new Tuple<PropertyInfo, FieldInfo>(PI, FI);

					this.properties[Name] = Rec;
				}
			}

			if (Rec == null)
			{
				Variable = null;
				return false;
			}
			else
			{
				object Value;

				if (Rec.Item1 != null)
					Value = Rec.Item1.GetValue(this.obj);
				else
					Value = Rec.Item2.GetValue(this.obj);

				Variable = new Variable(Name, Value);

				return true;
			}
		}
	}
}
