using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Persistence.Attributes;
using Waher.Persistence.Serialization;
using Waher.Runtime.Inventory;

namespace Waher.Persistence.FullTextSearch
{
	/// <summary>
	/// Type of indexed property.
	/// </summary>
	public enum PropertyType
	{
		/// <summary>
		/// A label (field or property) containing the value to be tokenized.
		/// </summary>
		Label,

		/// <summary>
		/// Reference to external evaluator, returning an object that is tokenized.
		/// </summary>
		External
	}

	/// <summary>
	/// Defines an indexable property.
	/// </summary>
	[TypeName(TypeNameSerialization.None)]
	public class PropertyDefinition
	{
		private readonly Dictionary<string, KeyValuePair<PropertyInfo, FieldInfo>> memberInfo = new Dictionary<string, KeyValuePair<PropertyInfo, FieldInfo>>();
		private IPropertyEvaluator evaluator;
		private bool initialized;

		/// <summary>
		/// Defines an indexable property.
		/// </summary>
		public PropertyDefinition()
		{
		}

		/// <summary>
		/// Defines an indexable property.
		/// </summary>
		/// <param name="Label">Property of Field name.</param>
		public PropertyDefinition(string Label)
		{
			this.ExternalSource = null;
			this.Definition = Label;
			this.Type = PropertyType.Label;
		}

		/// <summary>
		/// Defines an indexable property.
		/// </summary>
		/// <param name="ExternalSource">External Source</param>
		/// <param name="Definition">Definition</param>
		public PropertyDefinition(string ExternalSource, string Definition)
		{
			this.ExternalSource = ExternalSource;
			this.Definition = Definition;
			this.Type = PropertyType.External;
		}

		/// <summary>
		/// Creates an array of property definitions from an array of property names.
		/// </summary>
		/// <param name="PropertyNames">Property names</param>
		/// <returns>Array of property definitions</returns>
		public static PropertyDefinition[] ToArray(string[] PropertyNames)
		{
			int i, c = PropertyNames.Length;
			PropertyDefinition[] Result = new PropertyDefinition[c];

			for (i = 0; i < c; i++)
				Result[i] = new PropertyDefinition(PropertyNames[i]);

			return Result;
		}

		/// <summary>
		/// Definition, as a string.
		/// </summary>
		public string Definition { get; set; }

		/// <summary>
		/// External source.
		/// </summary>
		[DefaultValueStringEmpty]
		public string ExternalSource { get; set; }

		/// <summary>
		/// Type of property definition
		/// </summary>
		public PropertyType Type { get; set; }

		/// <summary>
		/// Gets the object to index.
		/// </summary>
		/// <param name="Instance">Object instance being indexed.</param>
		/// <returns>Property value to index.</returns>
		public async Task<object> GetValue(object Instance)
		{
			if (Instance is null)
				return null;

			Type T;

			switch (this.Type)
			{
				case PropertyType.Label:
					KeyValuePair<PropertyInfo, FieldInfo> P;

					T = Instance.GetType();

					lock (this.memberInfo)
					{
						if (!this.memberInfo.TryGetValue(T.FullName, out P))
						{
							P = new KeyValuePair<PropertyInfo, FieldInfo>(
								T.GetRuntimeProperty(this.Definition),
								T.GetRuntimeField(this.Definition));

							this.memberInfo[T.FullName] = P;
						}
					}

					if (!(P.Key is null))
					{
						if (P.Key.DeclaringType.IsAssignableFrom(Instance.GetType()))
							return P.Key.GetValue(Instance);
					}
					else if (!(P.Value is null))
					{
						if (P.Value.DeclaringType.IsAssignableFrom(Instance.GetType()))
							return P.Value.GetValue(Instance);
					}
					break;

				case PropertyType.External:
					if (!this.initialized)
					{
						this.initialized = true;

						T = Types.GetType(this.ExternalSource);
						if (!(T is null))
						{
							this.evaluator = Types.Create(true, T) as IPropertyEvaluator;
							if (!(this.evaluator is null))
								await this.evaluator.Prepare(this.Definition);
						}

					}

					if (!(this.evaluator is null))
					{
						try
						{
							return await this.evaluator.Evaluate(Instance);
						}
						catch (Exception ex)
						{
							Log.Critical(ex);
							return null;
						}
					}
					break;
			}

			return null;
		}

		/// <summary>
		/// Gets the object to index.
		/// </summary>
		/// <param name="Instance">Object instance being indexed.</param>
		/// <returns>Property value to index.</returns>
		public async Task<object> GetValue(GenericObject Instance)
		{
			if (!this.initialized)
			{
				this.initialized = true;

				switch (this.Type)
				{
					case PropertyType.Label:
						break;

					case PropertyType.External:
						Type T = Types.GetType(this.ExternalSource);
						if (!(T is null))
						{
							this.evaluator = Types.Create(true, T) as IPropertyEvaluator;
							if (!(this.evaluator is null))
								await this.evaluator.Prepare(this.Definition);
						}
						break;
				}
			}

			switch (this.Type)
			{
				case PropertyType.Label:
					if (Instance.TryGetFieldValue(this.Definition, out object Value))
						return Value;
					break;

				case PropertyType.External:
					if (!(this.evaluator is null))
						return await this.evaluator.Evaluate(Instance);
					break;
			}

			return null;
		}

	}
}
