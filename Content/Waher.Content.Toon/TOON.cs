using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Waher.Runtime.Inventory;
using Waher.Script.Abstraction.Elements;

namespace Waher.Content.Toon
{
	/// <summary>
	/// Helps with common TOON-related tasks.
	/// </summary>
	public static class TOON
	{
		private static readonly Dictionary<Type, IToonEncoder> encoders;

		static TOON()
		{
			encoders = new Dictionary<Type, IToonEncoder>();
			Types.OnInvalidated += Types_OnInvalidated;
		}

		private static void Types_OnInvalidated(object Sender, EventArgs e)
		{
			lock (encoders)
			{
				encoders.Clear();
			}
		}

		#region Encoding

		/// <summary>
		/// Encodes a string for inclusion in TOON.
		/// </summary>
		/// <param name="s">String to encode.</param>
		/// <returns>Encoded string.</returns>
		public static string Encode(string s)
		{
			switch (s)
			{
				case null:
				case "":
					return "\"\"";

				case "true":
					return "\"true\"";

				case "false":
					return "\"false\"";

				case "null":
					return "\"null\"";

				case "-":
					return "\"-\"";

				default:
					if (CommonTypes.TryParse(s, out double _))
						return "\"" + s + "\"";

					if (JSON.ContainsEscapeCharacters(s))
						return "\"" + JSON.Encode(s) + "\"";

					if (s.StartsWith("[") || s.StartsWith("- "))
						return "\"" + s + "\"";

					return s;
			}
		}

		/// <summary>
		/// Encodes an object as TOON.
		/// </summary>
		/// <param name="Object">Object.</param>
		/// <param name="Indent">If TOON should be indented.</param>
		/// <returns>TOON string.</returns>
		public static string Encode(object Object, bool Indent)
		{
			StringBuilder sb = new StringBuilder();
			Encode(Object, Indent, sb);
			return sb.ToString();
		}

		/// <summary>
		/// Encodes an object as TOON.
		/// </summary>
		/// <param name="Object">Object.</param>
		/// <param name="Toon">TOON Output.</param>
		/// <param name="Indent">If TOON should be indented.</param>
		public static void Encode(object Object, bool Indent, StringBuilder Toon)
		{
			Encode(Object, Indent ? (int?)-1 : null, Toon);
		}

		/// <summary>
		/// Encodes an object as TOON.
		/// </summary>
		/// <param name="Object">Object.</param>
		/// <param name="Indent">If TOON should be indented.</param>
		/// <param name="AdditionalProperties">Optional additional properties.</param>
		/// <returns>Encoded object.</returns>
		public static string Encode(IEnumerable<KeyValuePair<string, object>> Object, int? Indent,
			params KeyValuePair<string, object>[] AdditionalProperties)
		{
			StringBuilder Toon = new StringBuilder();
			Encode(Object, Indent, Toon, AdditionalProperties);
			return Toon.ToString();
		}

		/// <summary>
		/// Encodes an object as TOON.
		/// </summary>
		/// <param name="Object">Object.</param>
		/// <param name="Indent">If TOON should be indented.</param>
		/// <param name="Toon">TOON Output.</param>
		/// <param name="AdditionalProperties">Optional additional properties.</param>
		public static void Encode(IEnumerable<KeyValuePair<string, object>> Object, int? Indent, StringBuilder Toon,
			params KeyValuePair<string, object>[] AdditionalProperties)
		{
			bool First = true;

			if (Indent.HasValue)
				Indent++;

			if (!(Object is null))
			{
				foreach (KeyValuePair<string, object> Member in Object)
				{
					if (First)
						First = false;
					else
						Toon.Append(',');

					EncodeMember(Member.Key, Member.Value, Indent, Toon);
				}
			}

			if (!(AdditionalProperties is null))
			{
				foreach (KeyValuePair<string, object> Member in AdditionalProperties)
				{
					if (First)
						First = false;
					else
						Toon.Append(',');

					EncodeMember(Member.Key, Member.Value, Indent, Toon);
				}
			}

			if (Indent.HasValue)
			{
				Indent--;

				if (!First && Indent.Value > 0)
				{
					Toon.Append('\n');
					JSON.Indent(Toon, Indent.Value);
				}
			}
		}

		/// <summary>
		/// Encodes a member of an object as TOON.
		/// </summary>
		/// <param name="Name">Name of member.</param>
		/// <param name="Value">Value of member.</param>
		/// <param name="Indent">Optional indentation.</param>
		/// <param name="Toon">TOON output.</param>
		public static void EncodeMember(string Name, object Value, int? Indent,
			StringBuilder Toon)
		{
			bool AppendSpaces = Indent.HasValue;

			if (Indent.HasValue && Indent.Value > 0)
			{
				Toon.Append('\n');
				JSON.Indent(Toon, Indent.Value);
			}

			Toon.Append(Encode(Name));

			if (Value is Array A)
			{
				int i, c = A.Length;

				Toon.Append('[');
				Toon.Append(c.ToString());
				Toon.Append("]:");

				if (AppendSpaces)
					Toon.Append(' ');

				if (Indent.HasValue)
					Indent++;

				for (i = 0; i < c; i++)
				{
					if (i > 0)
					{
						if (AppendSpaces)
							Toon.Append(", ");
						else
							Toon.Append(',');
					}

					Encode(A.GetValue(i), Indent, Toon);
				}
			}
			else if (Value is IEnumerable Enumerable)
			{
				int c = 0;
				bool First = true;

				IEnumerator e = Enumerable.GetEnumerator();

				while (e.MoveNext())
					c++;

				Toon.Append('[');
				Toon.Append(c.ToString());
				Toon.Append("]:");

				if (AppendSpaces)
					Toon.Append(' ');

				if (Indent.HasValue)
					Indent++;

				e.Reset();
				while (e.MoveNext())
				{
					if (First)
						First = false;
					else
					{
						if (AppendSpaces)
							Toon.Append(", ");
						else
							Toon.Append(',');
					}

					Encode(e.Current, Indent, Toon);
				}
			}
			else
			{
				Toon.Append(':');

				if (AppendSpaces)
					Toon.Append(' ');

				Encode(Value, Indent, Toon);
			}
		}

		/// <summary>
		/// Encodes an object as TOON.
		/// </summary>
		/// <param name="Object">Object.</param>
		/// <param name="Indent">If TOON should be indented.</param>
		/// <param name="Toon">TOON Output.</param>
		public static void Encode(IEnumerable<KeyValuePair<string, IElement>> Object, int? Indent, StringBuilder Toon)
		{
			bool First = true;

			Toon.Append('{');

			if (Indent.HasValue)
				Indent++;

			if (!(Object is null))
			{
				foreach (KeyValuePair<string, IElement> Member in Object)
				{
					if (First)
						First = false;
					else
						Toon.Append(',');

					EncodeMember(Member.Key, Member.Value.AssociatedObjectValue, Indent, Toon);
				}
			}

			if (Indent.HasValue)
			{
				Indent--;

				if (!First && Indent.Value > 0)
				{
					Toon.Append('\n');
					JSON.Indent(Toon, Indent.Value);
				}
			}

			Toon.Append('}');
		}

		/// <summary>
		/// Extensible encoding of object <paramref name="Object"/>, by using
		/// best available <see cref="IToonEncoder"/> for the corresponding type.
		/// </summary>
		/// <param name="Object">Object to encode.</param>
		/// <param name="Indent">Optional indentation.</param>
		/// <param name="Toon">TOON output.</param>
		public static void Encode(object Object, int? Indent, StringBuilder Toon)
		{
			if (Object is null)
				Toon.Append("null");
			else
			{
				Type T = Object.GetType();
				IToonEncoder Encoder;

				lock (encoders)
				{
					if (!encoders.TryGetValue(T, out Encoder))
					{
						Encoder = Types.FindBest<IToonEncoder, Type>(T)
							?? throw new ArgumentException("Unable to encode objects of type " + T.FullName, nameof(Object));

						encoders[T] = Encoder;
					}
				}

				Encoder.Encode(Object, Indent, Toon);
			}
		}

		#endregion
	}
}
