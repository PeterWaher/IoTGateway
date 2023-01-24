using System.Collections.Generic;
using Waher.Script.Abstraction.Elements;

namespace Waher.Content
{
	/// <summary>
	/// A Named dictionary is a dictionary, with a local name and a namespace.
	/// Use it to return content that can be encoded both as JSON or XML.
	/// </summary>
	/// <typeparam name="TKey">Key Type</typeparam>
	/// <typeparam name="TValue">Value Type</typeparam>
	public class NamedDictionary<TKey, TValue> : Dictionary<TKey, TValue>
	{
		/// <summary>
		/// A Named dictionary is a dictionary, with a local name and a namespace.
		/// </summary>
		public NamedDictionary()
			: base()
		{
		}

		/// <summary>
		/// A Named dictionary is a dictionary, with a local name and a namespace.
		/// </summary>
		/// <param name="LocalName">Local Name</param>
		/// <param name="Namespace">Namespace</param>
		public NamedDictionary(string LocalName, string Namespace)
			: base()
		{
			this.LocalName = LocalName;
			this.Namespace = Namespace;
		}

		/// <summary>
		/// A Named dictionary is a dictionary, with a local name and a namespace.
		/// </summary>
		/// <param name="Dictionary">Dictionary with items.</param>
		public NamedDictionary(IDictionary<TKey, TValue> Dictionary)
			: base(Dictionary)
		{
		}

		/// <summary>
		/// A Named dictionary is a dictionary, with a local name and a namespace.
		/// </summary>
		/// <param name="Comparer">Comparer</param>
		public NamedDictionary(IEqualityComparer<TKey> Comparer)
			: base(Comparer)
		{
		}

		/// <summary>
		/// A Named dictionary is a dictionary, with a local name and a namespace.
		/// </summary>
		/// <param name="Capacity">Initial Capacity.</param>
		public NamedDictionary(int Capacity)
			: base(Capacity)
		{
		}

		/// <summary>
		/// A Named dictionary is a dictionary, with a local name and a namespace.
		/// </summary>
		/// <param name="Dictionary">Dictionary with items.</param>
		/// <param name="Comparer">Comparer</param>
		public NamedDictionary(IDictionary<TKey, TValue> Dictionary, IEqualityComparer<TKey> Comparer)
			: base(Dictionary, Comparer)
		{
		}

		/// <summary>
		/// A Named dictionary is a dictionary, with a local name and a namespace.
		/// </summary>
		/// <param name="Capacity">Initial Capacity.</param>
		/// <param name="Comparer">Comparer</param>
		public NamedDictionary(int Capacity, IEqualityComparer<TKey> Comparer)
			: base(Capacity, Comparer)
		{
		}

		/// <summary>
		/// Creates a Named Dictionary from a script object ex-nihilo
		/// </summary>
		/// <param name="Dictionary">Script object.</param>
		public static NamedDictionary<string, object> ToNamedDictionary(IEnumerable<KeyValuePair<string, IElement>> Dictionary)
		{
			NamedDictionary<string, object> Result = new NamedDictionary<string, object>();

			foreach (KeyValuePair<string, IElement> P in Dictionary)
			{
				switch (P.Key)
				{
					case "__name":
					case "__local":
						Result.LocalName = P.Value.AssociatedObjectValue?.ToString() ?? string.Empty;
						break;

					case "__namespace":
					case "__xmlns":
						Result.Namespace = P.Value.AssociatedObjectValue?.ToString() ?? string.Empty;
						break;

					default:
						Result[P.Key] = P.Value.AssociatedObjectValue;
						break;
				}
			}

			return Result;
		}

		/// <summary>
		/// Local Name of dictionary.
		/// </summary>
		public string LocalName { get; set; }

		/// <summary>
		/// Namespace of dictionary.
		/// </summary>
		public string Namespace { get; set; }
	}
}
