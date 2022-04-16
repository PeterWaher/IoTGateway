using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml;
using Waher.Content.Xml;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects.Matrices;

namespace Waher.Script.Xml
{
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
			Dictionary<ScriptNode, List<ScriptNode>> ChildrenByNode =
				new Dictionary<ScriptNode, List<ScriptNode>>();

			Expression.ForAll((ScriptNode Node, out ScriptNode NewNode, object Stata) =>
			{
				if (!(Node.Parent is null))
				{
					if (!ChildrenByNode.TryGetValue(Node.Parent, out List<ScriptNode> Children))
					{
						Children = new List<ScriptNode>();
						ChildrenByNode[Node.Parent] = Children;
					}
					else if (Children.Contains(Node))
						throw new Exception("Recursion.");	// TODO: Remove check

					Children.Add(Node);
				}

				NewNode = null;
				return true;

			}, null, false);

			void Export(ScriptNode Node)
			{
				Type T = Node.GetType();

				Xml.WriteStartElement(T.FullName);

				foreach (PropertyInfo PI in T.GetRuntimeProperties())
				{
					if (!PI.CanRead || !PI.GetMethod.IsPublic)
						continue;

					switch (PI.Name)
					{
						case "IsAsynchronous":
						case "DefaultVariableName":
						case "Start":
						case "Length":
						case "Parent":
						case "Expression":
							continue;
					}

					Xml.WriteAttributeString(PI.Name, PI.GetValue(Node)?.ToString());
				}

				if (ChildrenByNode.TryGetValue(Node, out List<ScriptNode> Children))
				{
					foreach (ScriptNode Child in Children)
						Export(Child);
				}

				Xml.WriteEndElement();
			}

			Export(Expression.Root);
		}

	}
}
