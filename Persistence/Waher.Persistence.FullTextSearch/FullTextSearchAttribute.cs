using System;
using System.Reflection;

namespace Waher.Persistence.FullTextSearch
{
	/// <summary>
	/// This attribute defines that objects of this type should be indexed in the full-text-search index.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
	public class FullTextSearchAttribute : Attribute
	{
		private readonly string indexCollection;
		private readonly PropertyDefinition[] properties;
		private readonly bool hasPropertyDefinitions;
		private readonly bool isPropertyReference;

		/// <summary>
		/// This attribute defines that objects of this type should be indexed in the full-text-search index.
		/// </summary>
		/// <param name="IndexCollection">Name of full-text-search index collection.</param>
		public FullTextSearchAttribute(string IndexCollection)
			: this(IndexCollection, new PropertyDefinition[0])
		{
		}

		/// <summary>
		/// This attribute defines that objects of this type should be indexed in the full-text-search index.
		/// </summary>
		/// <param name="IndexCollection">Name of full-text-search index collection.</param>
		/// <param name="Properties">Array of property (or field) names used to index objects of this type. 
		/// If not provided, and a <see cref="ITokenizer"/> exists for objects of this
		/// class, that tokenizer will be used instead of the property array, to extract
		/// tokens from the object.</param>
		public FullTextSearchAttribute(string IndexCollection, params string[] Properties)
			: this(IndexCollection, PropertyDefinition.ToArray(Properties))
		{
		}

		/// <summary>
		/// This attribute defines that objects of this type should be indexed in the full-text-search index.
		/// </summary>
		/// <param name="IndexCollection">Name of full-text-search index collection.</param>
		/// <param name="Properties">Array of property (or field) definitions used to index objects of this type. 
		/// If not provided, and a <see cref="ITokenizer"/> exists for objects of this
		/// class, that tokenizer will be used instead of the property array, to extract
		/// tokens from the object.</param>
		public FullTextSearchAttribute(string IndexCollection, params PropertyDefinition[] Properties)
		{
			this.indexCollection = IndexCollection;
			this.properties = Properties;
			this.hasPropertyDefinitions = (Properties?.Length ?? 0) > 0;
			this.isPropertyReference = false;
		}

		/// <summary>
		/// This attribute defines that objects of this type should be indexed in the full-text-search index.
		/// </summary>
		/// <param name="IndexCollection">Name of full-text-search index collection.</param>
		/// <param name="PropertyReference">If the <paramref name="IndexCollection"/> reference
		/// is pointing to a property on the object (true) or is a constant index collection
		/// reference (false).
		/// 
		/// Note: Classes using dynamic index collection names require custom
		/// tokenizers to be tokenized properly.</param>
		public FullTextSearchAttribute(string IndexCollection, bool PropertyReference)
		{
			this.indexCollection = IndexCollection;
			this.properties = new PropertyDefinition[0];
			this.hasPropertyDefinitions = (Properties?.Length ?? 0) > 0;
			this.isPropertyReference = PropertyReference;
		}

		/// <summary>
		/// If the index collection is dynamic (i.e. depends on object instance).
		/// </summary>
		public bool DynamicIndexCollection => this.isPropertyReference;

		/// <summary>
		/// Name of full-text-search index collection.
		/// </summary>
		public string GetIndexCollection(object Reference)
		{
			if (this.isPropertyReference)
			{
				Type T = Reference.GetType();
				PropertyInfo PI = T.GetRuntimeProperty(this.indexCollection);
				if (PI is null)
					throw new ArgumentException("Object lacks a property named " + this.indexCollection, nameof(Reference));

				object Obj = PI.GetValue(Reference);

				if (Obj is string s)
					return s;
				else
					throw new ArgumentException("Object property " + this.indexCollection + " does not return a string.", nameof(Reference));
			}
			else
				return this.indexCollection;
		}

		/// <summary>
		/// Array of property (or field) definitions used to index objects of this type.
		/// </summary>
		public PropertyDefinition[] Properties => this.properties;

		/// <summary>
		/// If property names are defined for this class (true), or
		/// if objects are to be tokenized using a specialized tokenizer (false).
		/// </summary>
		public bool HasPropertyDefinitions => this.hasPropertyDefinitions;
	}
}
