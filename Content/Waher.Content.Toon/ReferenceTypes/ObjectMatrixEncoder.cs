using System;
using System.Collections.Generic;
using Waher.Content.Toon.Model;
using Waher.Runtime.Inventory;
using Waher.Script.Objects.Matrices;

namespace Waher.Content.Toon.ReferenceTypes
{
	/// <summary>
	/// Encodes <see cref="ObjectMatrix"/> values.
	/// </summary>
	public class ObjectMatrixEncoder : MultiRowToonEncoder
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
		public override void Encode(object Object, int? Indent, ToonOutput Toon)
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
					Toon.AppendDelimiter();

				if (Indent.HasValue && Indent.Value > 0)
				{
					Toon.AppendLine();
					JSON.Indent(Toon.Output, Indent.Value);
					Indent++;
				}

				Toon.Append('{');

				for (x = 0; x < Columns; x++)
				{
					if (x > 0)
						Toon.AppendDelimiter();

					if (Indent.HasValue && Indent.Value > 0)
					{
						Toon.AppendLine();
						Toon.Indent(Indent.Value);
					}

					Toon.Append('"');
					Toon.Append(TOON.Encode(Names[x], true));
					Toon.Append("\": ");
					TOON.Encode(M.GetElement(x, y).AssociatedObjectValue, Indent, Toon);
				}

				if (Indent.HasValue)
				{
					Indent--;

					if (Indent.Value > 0)
					{
						Toon.AppendLine();
						Toon.Indent(Indent.Value);
					}
				}

				Toon.Append('}');
			}


			if (Indent.HasValue)
			{
				Indent--;

				if (Rows > 0 && Columns > 0 && Indent.Value > 0)
				{
					Toon.AppendLine();
					Toon.Indent(Indent.Value);
				}
			}

			Toon.Append(']');
		}

		/// <summary>
		/// Gets available parameters to encode from an object.
		/// </summary>
		/// <param name="Object">Object to get parameters from.</param>
		/// <returns>Enumerator for the parameters, or null if not applicable.</returns>
		public override IEnumerator<KeyValuePair<string, object>> GetParameters(object Object)
		{
			return null;
		}

		/// <summary>
		/// How well the TOON encoder encodes objects of type <paramref name="ObjectType"/>.
		/// </summary>
		/// <param name="ObjectType">Type of object to encode.</param>
		/// <returns>How well objects of the given type are encoded.</returns>
		public override Grade Supports(Type ObjectType)
		{
			return ObjectType == typeof(ObjectMatrix) ? Grade.Excellent : Grade.NotAtAll;
		}
	}
}
