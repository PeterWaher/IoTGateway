using System;
using System.Reflection;
using System.Threading.Tasks;
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
		private PropertyInfo propertyInfo;
		private FieldInfo fieldInfo;
		private IPropertyEvaluator evaluator;
		private bool initialized = false;

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
		/// 
		/// </summary>
		public PropertyType Type { get; set; }

		/// <summary>
		/// Gets the object to index.
		/// </summary>
		/// <param name="Instance">Object instance being indexed.</param>
		/// <returns>Property value to index.</returns>
		public async Task<object> GetValue(object Instance)
		{
			if (!this.initialized)
			{
				this.initialized = true;

				switch (this.Type)
				{
					case PropertyType.Label:
						Type T = Instance.GetType();
						this.propertyInfo = T.GetRuntimeProperty(this.Definition);
						this.fieldInfo = T.GetRuntimeField(this.Definition);
						break;

					case PropertyType.External:
						T = Types.GetType(this.ExternalSource);
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
					if (!(this.propertyInfo is null))
						return this.propertyInfo.GetValue(Instance);
					else if (!(this.fieldInfo is null))
						return this.fieldInfo.GetValue(Instance);
					break;

				case PropertyType.External:
					if (!(this.evaluator is null))
						return await this.evaluator.Evaluate(Instance);
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
