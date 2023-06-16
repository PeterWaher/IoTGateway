using System.Collections.Generic;
using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Runtime.Inventory;
using Waher.Content.Semantic.Model.Literals;

namespace Waher.Content.Semantic.Model
{
	/// <summary>
	/// Set of semantic elements.
	/// </summary>
	public class SemanticElements : Set, IOrderedSet
	{
		private static readonly Dictionary<Type, ISemanticLiteral> literalPerType = new Dictionary<Type, ISemanticLiteral>();
		private static readonly Dictionary<string, ISemanticLiteral> dataTypes = new Dictionary<string, ISemanticLiteral>();

		static SemanticElements()
		{
			Types.OnInvalidated += (Sender, e) =>
			{
				lock (literalPerType)
				{
					literalPerType.Clear();
				}

				lock (dataTypes)
				{
					dataTypes.Clear();
				}
			};
		}

		/// <summary>
		/// Instance reference to the set of semantic elements.
		/// </summary>
		public static readonly SemanticElements Instance = new SemanticElements();

		/// <summary>
		/// Set of semantic elements.
		/// </summary>
		public SemanticElements()
		{
		}

		/// <summary>
		/// Checks if the set contains an element.
		/// </summary>
		/// <param name="Element">Element.</param>
		/// <returns>If the element is contained in the set.</returns>
		public override bool Contains(IElement Element)
		{
			return Element is ISemanticElement;
		}

		/// <summary>
		/// Size of set, if finite and known, otherwise null is returned.
		/// </summary>
		public override int? Size => null;

		/// <summary>
		/// Parses a string literal value.
		/// </summary>
		/// <param name="Value">String representation of value.</param>
		/// <param name="DataType">Optional datatype.</param>
		/// <param name="Language">Optional language.</param>
		/// <returns>Parsed literal value.</returns>
		public static ISemanticElement Parse(string Value, string DataType, string Language)
		{
			ISemanticLiteral LiteralType;

			lock (dataTypes)
			{
				if (!dataTypes.TryGetValue(DataType, out LiteralType))
				{
					LiteralType = Types.FindBest<ISemanticLiteral, string>(DataType)
						?? new CustomLiteral(string.Empty, DataType);

					dataTypes[DataType] = LiteralType;
				}
			}

			return LiteralType.Parse(Value, DataType, Language);
		}

		/// <summary>
		/// Encapsulates an object as a semantic element.
		/// </summary>
		/// <param name="Value">Value to encapsulate.</param>
		/// <returns>Encapsulated value.</returns>
		public static ISemanticElement Encapsulate(object Value)
		{
			if (Value is ISemanticElement Element)
				return Element;

			if (Value is Uri Uri)
				return new UriNode(Uri, Uri.OriginalString);

			return EncapsulateLiteral(Value);
		}

		/// <summary>
		/// Encapsulates an object as a semantic literal.
		/// </summary>
		/// <param name="Value">Value to encapsulate.</param>
		/// <returns>Encapsulated value.</returns>
		public static ISemanticLiteral EncapsulateLiteral(object Value)
		{
			Type T = Value?.GetType() ?? typeof(object);
			ISemanticLiteral Literal;

			lock (literalPerType)
			{
				if (!literalPerType.TryGetValue(T, out Literal))
				{
					Literal = Types.FindBest<ISemanticLiteral, Type>(T)
						?? new CustomLiteral(string.Empty, string.Empty);

					literalPerType[T] = Literal;
				}
			}

			return Literal.Encapsulate(Value);
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj.GetType() == typeof(SemanticElements);
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return typeof(SemanticElements).GetHashCode();
		}

		/// <summary>
		/// Compares two elements.
		/// </summary>
		/// <param name="x">Element 1</param>
		/// <param name="y">Element 2</param>
		/// <returns>Comparison result.</returns>
		public int Compare(IElement x, IElement y)
		{
			if (x is ISemanticElement Left)
				return Left.CompareTo(y);
			else
				return 1;
		}
	}
}
