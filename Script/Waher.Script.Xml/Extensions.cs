using System.Text;
using Waher.Content.Xml;
using Waher.Script.Abstraction.Elements;
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

	}
}
