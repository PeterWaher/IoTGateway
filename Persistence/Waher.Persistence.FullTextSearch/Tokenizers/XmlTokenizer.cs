using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml;
using Waher.Runtime.Inventory;

namespace Waher.Persistence.FullTextSearch.Tokenizers
{
	/// <summary>
	/// Tokenizes an XML Document or XML Fragment.
	/// </summary>
	public class XmlTokenizer : ITokenizer
	{
		/// <summary>
		/// Tokenizes an XML Document or XML Fragment.
		/// </summary>
		public XmlTokenizer()
		{
		}

		/// <summary>
		/// How well the tokenizer can tokenize objects of type <paramref name="Type"/>.
		/// </summary>
		/// <param name="Type">Type of object to tokenize</param>
		/// <returns>How well such objects are tokenized.</returns>
		public Grade Supports(Type Object)
		{
			if (typeInfo.IsAssignableFrom(Object))
				return Grade.Ok;
			else
				return Grade.NotAtAll;
		}

		private static readonly TypeInfo typeInfo = typeof(XmlNode).GetTypeInfo();

		/// <summary>
		/// Tokenizes an object.
		/// </summary>
		/// <param name="Value">Object to tokenize.</param>
		/// <param name="TokenCounts">Token counts.</param>
		/// <param name="DocumentIndexOffset">Document Index Offset. Used to
		/// identify sequences of tokens in a document.</param>
		public void Tokenize(object Value, Dictionary<string, List<uint>> TokenCounts,
			ref uint DocumentIndexOffset)
		{
			if (Value is XmlNode N)
			{
				StringBuilder sb = new StringBuilder();
				
				GetText(N, sb);

				StringTokenizer.Tokenize(sb.ToString(), TokenCounts, ref DocumentIndexOffset);
			}
		}

		private static void GetText(XmlNode N, StringBuilder Text)
		{
			if (N is XmlElement E)
			{
				foreach (XmlAttribute Attr in E.Attributes)
				{
					Text.Append(' ');
					Text.Append(Attr.Value);
				}
			}
			else if (N is XmlText T)
			{
				Text.Append(' ');
				Text.Append(T.InnerText);
			}
			else if (N is XmlCDataSection C)
			{
				Text.Append(' ');
				Text.Append(C.InnerText);
			}

			if (N.HasChildNodes)
			{
				foreach (XmlNode N2 in N.ChildNodes)
					GetText(N2, Text);
			}
		}
	}
}
