using System;
using System.Text;
using Waher.Runtime.Inventory;
using Waher.Script.Objects.Matrices;

namespace Waher.Content.Toon.ReferenceTypes
{
	/// <summary>
	/// Encodes <see cref="ObjectMatrix"/> values.
	/// </summary>
	public class ObjectMatrixEncoder : IToonEncoder
	{
		/// <summary>
		/// Encodes <see cref="ObjectMatrix"/> values.
		/// </summary>
		public ObjectMatrixEncoder()
		{
		}

		/// <summary>
		/// Encodes the <paramref name="Object"/> to TOON.
		/// </summary>
		/// <param name="Object">Object to encode.</param>
		/// <param name="Indent">Any indentation to apply.</param>
		/// <param name="Toon">TOON output.</param>
		public void Encode(object Object, int? Indent, StringBuilder Toon)
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

			Toon.Append('[');

			if (Indent.HasValue)
				Indent++;

			for (y = 0; y < Rows; y++)
			{
				if (y > 0)
					Toon.Append(',');

				if (Indent.HasValue)
				{
					Toon.AppendLine();
					Toon.Append(new string('\t', Indent.Value));
					Indent++;
				}

				Toon.Append('{');

				for (x = 0; x < Columns; x++)
				{
					if (x > 0)
						Toon.Append(',');

					if (Indent.HasValue)
					{
						Toon.AppendLine();
						Toon.Append(new string('\t', Indent.Value));
					}

					Toon.Append('"');
					Toon.Append(TOON.Encode(Names[x]));
					Toon.Append("\": ");
					TOON.Encode(M.GetElement(x, y).AssociatedObjectValue, Indent, Toon);
				}

				if (Indent.HasValue)
				{
					Indent--;
					Toon.AppendLine();
					Toon.Append(new string('\t', Indent.Value));
				}

				Toon.Append('}');
			}


			if (Indent.HasValue)
			{
				Indent--;

				if (Rows > 0 && Columns > 0)
				{
					Toon.AppendLine();
					Toon.Append(new string('\t', Indent.Value));
				}
			}

			Toon.Append(']');
		}

		/// <summary>
		/// How well the TOON encoder encodes objects of type <paramref name="ObjectType"/>.
		/// </summary>
		/// <param name="ObjectType">Type of object to encode.</param>
		/// <returns>How well objects of the given type are encoded.</returns>
		public Grade Supports(Type ObjectType)
		{
			return ObjectType == typeof(ObjectMatrix) ? Grade.Excellent : Grade.NotAtAll;
		}
	}
}
