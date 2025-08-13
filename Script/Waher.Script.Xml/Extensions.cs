using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Runtime.Collections;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects.Matrices;

namespace Waher.Script.Xml
{
	/// <summary>
	/// Script extensions helping with convertsion of script results to XML.
	/// </summary>
	public static class Extensions
	{
		/// <summary>
		/// Exports the matrix to XML
		/// </summary>
		/// <param name="M"><see cref="ObjectMatrix"/> to be exported to XML.</param>
		/// <param name="Namespace">Namespace of collection</param>
		/// <param name="CollectionName">Local name of collection element.</param>
		/// <param name="RecordName">Local name of record element.</param>
		/// <returns>XML string.</returns>
		public static string ToXml(this ObjectMatrix M, string Namespace, string CollectionName, string RecordName)
		{
			StringBuilder Xml = new StringBuilder();
			M.ToXml(Xml, Namespace, CollectionName, RecordName);
			return Xml.ToString();
		}

		/// <summary>
		/// Exports the matrix to XML
		/// </summary>
		/// <param name="M"><see cref="ObjectMatrix"/> to be exported to XML.</param>
		/// <param name="Xml">XML Output</param>
		/// <param name="Namespace">Namespace of collection</param>
		/// <param name="CollectionName">Local name of collection element.</param>
		/// <param name="RecordName">Local name of record element.</param>
		/// <returns>XML string.</returns>
		public static void ToXml(this ObjectMatrix M, StringBuilder Xml, string Namespace, string CollectionName, string RecordName)
		{
			int c = M.ColumnNames?.Length ?? 0;

			Xml.Append('<');
			Xml.Append(CollectionName);
			Xml.Append(" xmlns='");
			Xml.Append(Namespace);
			Xml.Append("'>");

			foreach (IElement Row in M.VectorElements)
			{
				if (Row is IVector RowVector)
				{
					int i = 0;

					Xml.Append('<');
					Xml.Append(RecordName);

					foreach (IElement Element in RowVector.VectorElements)
					{
						Xml.Append(' ');

						if (i < c)
							Xml.Append(M.ColumnNames[i++]);
						else
						{
							Xml.Append('c');
							Xml.Append((++i).ToString());
						}

						Xml.Append("='");
						Xml.Append(XML.Encode(Expression.ToString(Element.AssociatedObjectValue)));
						Xml.Append('\'');
					}

					Xml.Append("/>");
				}
			}

			Xml.Append("</");
			Xml.Append(CollectionName);
			Xml.Append('>');
		}

		/// <summary>
		/// Exports the matrix to XML
		/// </summary>
		/// <param name="Expression">Expression to expirt.</param>
		/// <returns>XML string.</returns>
		public static string ToXml(this Expression Expression)
		{
			StringBuilder Xml = new StringBuilder();
			Expression.ToXml(Xml);
			return Xml.ToString();
		}

		/// <summary>
		/// Exports the matrix to XML
		/// </summary>
		/// <param name="Expression">Expression to expirt.</param>
		/// <param name="Xml">XML Output</param>
		/// <returns>XML string.</returns>
		public static void ToXml(this Expression Expression, StringBuilder Xml)
		{
			ToXml(Expression, new StringWriter(Xml));
		}

		/// <summary>
		/// Exports the matrix to XML
		/// </summary>
		/// <param name="Expression">Expression to expirt.</param>
		/// <param name="Xml">XML Output</param>
		/// <returns>XML string.</returns>
		public static void ToXml(this Expression Expression, TextWriter Xml)
		{
			XmlWriterSettings Settings = new XmlWriterSettings()
			{
				Indent = true,
				IndentChars = "\t",
				CheckCharacters = false,
				OmitXmlDeclaration = true
			};

			using (XmlWriter w = XmlWriter.Create(Xml, Settings))
			{
				Expression.ToXml(w);
				w.Flush();
			}
		}

		/// <summary>
		/// Exports the matrix to XML
		/// </summary>
		/// <param name="Expression">Expression to expirt.</param>
		/// <param name="Xml">XML Output</param>
		/// <returns>XML string.</returns>
		public static void ToXml(this Expression Expression, XmlWriter Xml)
		{
			ChunkedList<ScriptNode> Stack = new ChunkedList<ScriptNode>();
			int c = 0;

			Stack.Add(null);

			Expression.ForAll((ScriptNode Node, out ScriptNode NewNode, object Stata) =>
			{
				NewNode = null;

				while (Stack.HasLastItem && Stack.LastItem != Node.Parent)
				{
					if (Node.Parent is null)
						throw new InvalidOperationException("Parent node reference not set correctly on node.");

					if (c == 0)
						throw new ScriptException("Script tree not consistent.");

					Xml.WriteEndElement();
					c--;

					Stack.RemoveLast();
				}

				Type T = Node.GetType();

				Xml.WriteStartElement(T.FullName);
				c++;

				foreach (PropertyInfo PI in T.GetRuntimeProperties())
				{
					if (!PI.CanRead || !PI.GetMethod.IsPublic)
						continue;

					if (PI.PropertyType.IsArray)
						continue;

					if (PI.PropertyType == typeof(ScriptNode))
						continue;

					if (typeof(IEnumerable<ScriptNode>).IsAssignableFrom(PI.PropertyType.GetTypeInfo()))
						continue;

					switch (PI.Name)
					{
						case "IsAsynchronous":
						case "DefaultVariableName":
						case "Start":
						case "Length":
						case "Parent":
						case "Expression":
						case "AssociatedObjectValue":
						case "AssociatedSet":
						case "ChildElements":
						case "IsScalar":
							continue;
					}

					object Value = PI.GetValue(Node);

					if (Value is string s)
						Xml.WriteAttributeString(PI.Name, s);
					else if (Value is bool b)
						Xml.WriteAttributeString(PI.Name, CommonTypes.Encode(b));
					else
						Xml.WriteAttributeString(PI.Name, Expression.ToString(Value));
				}

				Stack.Add(Node);
				
				return true;

			}, null, SearchMethod.TreeOrder);

			while (c-- > 0)
				Xml.WriteEndElement();
		}

	}
}
