using System;
using System.Text;
using Waher.Runtime.Inventory;
using Waher.Script.Objects.Matrices;

namespace Waher.Content.Json.ReferenceTypes
{
	/// <summary>
	/// Encodes <see cref="ObjectMatrix"/> values.
	/// </summary>
	public class ObjectMatrixEncoder : IJsonEncoder
	{
		/// <summary>
		/// Encodes <see cref="ObjectMatrix"/> values.
		/// </summary>
		public ObjectMatrixEncoder()
		{
		}

		/// <summary>
		/// Encodes the <paramref name="Object"/> to JSON.
		/// </summary>
		/// <param name="Object">Object to encode.</param>
		/// <param name="Indent">Any indentation to apply.</param>
		/// <param name="Json">JSON output.</param>
		public void Encode(object Object, int? Indent, StringBuilder Json)
		{
			ObjectMatrix M = (ObjectMatrix)Object;
			string[] Names;
			int Rows = M.Rows;
			int Columns = M.Columns;
			int x, y;

			if (!(M.ColumnNames is null))
				Names = M.ColumnNames;
			else
			{
				Names = new string[Columns];

				for (x = 0; x < Columns; x++)
					Names[x] = "C" + (x + 1).ToString();
			}

			Json.Append('[');

			if (Indent.HasValue)
				Indent++;

			for (y = 0; y < Rows; y++)
			{
				if (y > 0)
					Json.Append(',');

				if (Indent.HasValue)
				{
					Json.AppendLine();
					Json.Append(new string('\t', Indent.Value));
					Indent++;
				}

				Json.Append('{');

				for (x = 0; x < Columns; x++)
				{
					if (x > 0)
						Json.Append(',');

					if (Indent.HasValue)
					{
						Json.AppendLine();
						Json.Append(new string('\t', Indent.Value));
					}

					Json.Append('"');
					Json.Append(JSON.Encode(Names[x]));
					Json.Append("\":");
					Encode(M.GetElement(x, y).AssociatedObjectValue, Indent, Json);
				}

				if (Indent.HasValue)
				{
					Indent--;
					Json.AppendLine();
					Json.Append(new string('\t', Indent.Value));
				}

				Json.Append('}');
			}


			if (Indent.HasValue)
			{
				Indent--;

				if (Rows > 0 && Columns > 0)
				{
					Json.AppendLine();
					Json.Append(new string('\t', Indent.Value));
				}
			}

			Json.Append(']');
		}

		/// <summary>
		/// How well the JSON encoder encodes objects of type <paramref name="ObjectType"/>.
		/// </summary>
		/// <param name="ObjectType">Type of object to encode.</param>
		/// <returns>How well objects of the given type are encoded.</returns>
		public Grade Supports(Type ObjectType)
		{
			return ObjectType == typeof(ObjectMatrix) ? Grade.Ok : Grade.NotAtAll;
		}
	}
}
