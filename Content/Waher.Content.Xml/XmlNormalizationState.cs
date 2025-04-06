using System.Collections.Generic;
using System.Text;
using Waher.Runtime.Collections;

namespace Waher.Content.Xml
{
	/// <summary>
	/// Current state of XML normalization process.
	/// </summary>
	public class XmlNormalizationState
	{
		private readonly StringBuilder output = new StringBuilder();
		private readonly Dictionary<string, string> namespceByPrefix = new Dictionary<string, string>();
		private readonly ChunkedList<ChunkedList<KeyValuePair<string, string>>> prefixStack = new ChunkedList<ChunkedList<KeyValuePair<string, string>>>();
		private ChunkedList<KeyValuePair<string, string>> newPrefixes = null;

		/// <summary>
		/// Current state of XML normalization process.
		/// </summary>
		public XmlNormalizationState()
		{
		}

		/// <summary>
		/// Appends a string to the output.
		/// </summary>
		/// <param name="s">String to append.</param>
		public void Append(string s)
		{
			this.output.Append(s);
		}

		/// <summary>
		/// Appends a character to the output.
		/// </summary>
		/// <param name="ch">Character to append.</param>
		public void Append(char ch)
		{
			this.output.Append(ch);
		}

		/// <summary>
		/// XML output.
		/// </summary>
		/// <returns>Normalized XML.</returns>
		public override string ToString()
		{
			return this.output.ToString();
		}

		/// <summary>
		/// Registers prefix for element.
		/// </summary>
		/// <param name="Prefix">Prefix</param>
		/// <param name="Namespace">Namespace</param>
		/// <returns>If prefix was new.</returns>
		public bool RegisterPrefix(string Prefix, string Namespace)
		{
			if (this.namespceByPrefix.TryGetValue(Prefix, out string PrevNamespace))
			{
				if (PrevNamespace == Namespace)
					return false;
			}
			else
				PrevNamespace = null;

			if (this.newPrefixes is null)
				this.newPrefixes = new ChunkedList<KeyValuePair<string, string>>();

			this.newPrefixes.Add(new KeyValuePair<string, string>(Prefix, PrevNamespace));
			this.namespceByPrefix[Prefix] = Namespace;

			return true;
		}

		/// <summary>
		/// Pushes current prefix state to the stack.
		/// </summary>
		public void PushPrefixes()
		{
			this.prefixStack.Add(this.newPrefixes);
			this.newPrefixes = null;
		}

		/// <summary>
		/// Pops previous prefix state from the stack.
		/// </summary>
		public void PopPrefixes()
		{
			if (!this.prefixStack.HasLastItem)
				return;

			ChunkedList<KeyValuePair<string, string>> ToRemove = this.prefixStack.RemoveLast();

			if (!(ToRemove is null))
			{
				foreach (KeyValuePair<string, string> P in ToRemove)
				{
					if (P.Value is null)
						this.namespceByPrefix.Remove(P.Key);
					else
						this.namespceByPrefix[P.Key] = P.Value;
				}
			}
		}
	}
}
