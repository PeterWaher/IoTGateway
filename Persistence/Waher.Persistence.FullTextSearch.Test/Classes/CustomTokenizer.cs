using Waher.Persistence.FullTextSearch.Tokenizers;
using Waher.Runtime.Inventory;

namespace Waher.Persistence.FullTextSearch.Test.Classes
{
	/// <summary>
	/// Tokenizes objects of class <see cref="CustomTokenizationTestClass"/>
	/// </summary>
	public class CustomTokenizer : ITokenizer
	{
		/// <summary>
		/// Tokenizes objects of class <see cref="CustomTokenizationTestClass"/>
		/// </summary>
		public CustomTokenizer()
		{
		}

		/// <summary>
		/// If the interface understands objects such as <paramref name="Object"/>.
		/// </summary>
		/// <param name="Object">Object</param>
		/// <returns>How well objects of this type are supported.</returns>
		public Grade Supports(Type Object)
		{
			if (Object == typeof(CustomTokenizationTestClass))
				return Grade.Ok;
			else
				return Grade.NotAtAll;
		}

		/// <summary>
		/// Tokenizes an object.
		/// </summary>
		/// <param name="Value">Object to tokenize.</param>
		/// <param name="Process">Current tokenization process.</param>
		public Task Tokenize(object Value, TokenizationProcess Process)
		{
			if (Value is CustomTokenizationTestClass Obj)
			{
				StringTokenizer.Tokenize(Obj.IndexedProperty1, Process);
				Process.DocumentIndexOffset++;
				StringTokenizer.Tokenize(Obj.IndexedProperty2, Process);
			}

			return Task.CompletedTask;
		}
	}
}
