using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Waher.Runtime.Inventory;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Operators.Comparisons;
using Waher.Script.Units.DerivedQuantities;

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
		/// <param name="InObject">If the string is part of an object construct.</param>
		/// <returns>Encoded string.</returns>
		public static string Encode(string s, bool InObject)
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
					if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
						return "\"" + s + "\"";

					if (JSON.ContainsEscapeCharacters(s))
						return "\"" + JSON.Encode(s) + "\"";

					if (s.StartsWith("[") ||
						s.StartsWith("{") ||
						s.StartsWith("-"))
					{
						return "\"" + s + "\"";
					}

					if (InObject)
					{
						if (s.IndexOfAny(objectKeyCharacters) >= 0)
							return "\"" + s + "\"";
					}
					else
					{
						if (s.IndexOfAny(keyCharacters) >= 0)
							return "\"" + s + "\"";
					}

					return s;
			}
		}

		private static readonly char[] objectKeyCharacters = new char[] { ':', ',', ' ' };
		private static readonly char[] keyCharacters = new char[] { ':', ',' };

		/// <summary>
		/// Encodes an object as TOON.
		/// </summary>
		/// <param name="Object">Object.</param>
		/// <param name="Indent">If TOON should be indented.</param>
		/// <returns>TOON string.</returns>
		public static string Encode(object Object, bool Indent)
		{
			ToonOutput sb = new ToonOutput();
			Encode(Object, Indent, sb);
			return sb.ToString();
		}

		/// <summary>
		/// Encodes an object as TOON.
		/// </summary>
		/// <param name="Object">Object.</param>
		/// <param name="Indent">If TOON should be indented.</param>
		/// <param name="Toon">TOON Output.</param>
		public static void Encode(object Object, bool Indent, ToonOutput Toon)
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
			ToonOutput Toon = new ToonOutput();
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
		public static void Encode(IEnumerable<KeyValuePair<string, object>> Object, int? Indent,
			ToonOutput Toon, params KeyValuePair<string, object>[] AdditionalProperties)
		{
			bool MultiRow = Indent.HasValue;
			bool First = true;

			if (MultiRow)
				Indent++;

			if (!(Object is null))
			{
				foreach (KeyValuePair<string, object> Member in Object)
				{
					if (!MultiRow)
					{
						if (!MultiRow)
						{
							if (First)
								First = false;
							else
								Toon.AppendDelimiter();
						}
					}

					EncodeMember(Member.Key, Member.Value, Indent, Toon);
				}
			}

			if (!(AdditionalProperties is null))
			{
				foreach (KeyValuePair<string, object> Member in AdditionalProperties)
				{
					if (!MultiRow)
					{
						if (!MultiRow)
						{
							if (First)
								First = false;
							else
								Toon.AppendDelimiter();
						}
					}

					EncodeMember(Member.Key, Member.Value, Indent, Toon);
				}
			}

			if (MultiRow)
			{
				Indent--;

				if (!First && Indent.Value > 0)
				{
					Toon.AppendLine();
					Toon.Indent(Indent.Value);
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
			ToonOutput Toon)
		{
			bool AppendSpaces = Indent.HasValue;

			if (AppendSpaces && (Indent.Value > 0 || (Indent.Value == 0 && !Toon.Empty)))
			{
				Toon.AppendLine();
				Toon.Indent(Indent.Value);
			}

			Toon.Append(Encode(Name, true));

			if (Value is null)
			{
				if (AppendSpaces)
					Toon.Append(": ");
				else
					Toon.Append(':');

				Toon.Append("null");
				return;
			}

			IToonEncoder Encoder = GetEncoder(Value);

			if (Encoder.EncodesVectors)
			{
				int c = Encoder.GetCount(Value) ?? -1;

				if (c > 0)
				{
					Toon.Append('[');
					Toon.Append(c.ToString());

					if (!Toon.StandardDelimiter)
						Toon.AppendDelimiter();

					Toon.Append(']');

					IEnumerator e = Encoder.GetElements(Value);

					if (!(e is null))
					{
						Dictionary<string, object> Parameters = new Dictionary<string, object>();
						LinkedList<string> ParameterOrder = new LinkedList<string>();
						LinkedList<Dictionary<string,object>> ParameterSets = new LinkedList<Dictionary<string, object>>();

						while (e.MoveNext())
						{
							object Element = e.Current;
							IToonEncoder ElementEncoder = GetEncoder(Element);
							IEnumerator<KeyValuePair<string, object>> e2 =
								ElementEncoder.GetParameters(Element);

							if (e2 is null)
							{
								Parameters = null;
								ParameterOrder = null;
								ParameterSets = null;
								break;
							}

							Dictionary<string,object> ParameterSet = new Dictionary<string, object>();

							while (e2.MoveNext())
							{
								KeyValuePair<string, object> P = e2.Current;

								if (!Parameters.ContainsKey(P.Key))
								{
									Parameters[P.Key] = P.Value;
									ParameterOrder.AddLast(P.Key);
								}

								ParameterSet[P.Key] = P.Value;
							}

							ParameterSets.AddLast(ParameterSet);
						}

						if (!(Parameters is null))
						{
							bool First = true;

							Toon.Append('{');

							foreach (string ParameterName in ParameterOrder)
							{
								if (First)
									First = false;
								else
									Toon.AppendDelimiter();

								Toon.Append(Encode(ParameterName, true));
							}

							Toon.Append("}:");

							foreach (Dictionary<string, object> ParameterSet in ParameterSets)
							{
								Toon.AppendLine();
								Toon.Indent((Indent ?? 0) + 1);

								First = true;

								foreach (string ParameterName in ParameterOrder)
								{
									if (First)
										First = false;
									else
										Toon.AppendDelimiter();

									if (ParameterSet.TryGetValue(ParameterName, out object Element))
									{
										IToonEncoder ElementEncoder = GetEncoder(Element);
										ElementEncoder.Encode(Element, null, Toon);
									}
								}
							}

							return;
						}
					}

					Toon.Append(':');

					if (AppendSpaces)
						Toon.Append(' ');

					Encoder.Encode(Value, Indent, Toon, false);

					return;
				}
			}
			else if (Encoder.EncodesMultipleRows || !AppendSpaces)
				Toon.Append(':');
			else
				Toon.Append(": ");

			Encoder.Encode(Value, Indent, Toon);
		}

		private static IToonEncoder GetEncoder(object Value)
		{
			Type T = Value?.GetType() ?? typeof(object);
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

			return Encoder;
		}

		/// <summary>
		/// Encodes an object as TOON.
		/// </summary>
		/// <param name="Object">Object.</param>
		/// <param name="Indent">If TOON should be indented.</param>
		/// <param name="Toon">TOON Output.</param>
		public static void Encode(IEnumerable<KeyValuePair<string, IElement>> Object, int? Indent,
			ToonOutput Toon)
		{
			bool MultiRow = Indent.HasValue;
			bool First = true;

			if (MultiRow)
				Indent++;

			if (!(Object is null))
			{
				foreach (KeyValuePair<string, IElement> Member in Object)
				{
					if (!MultiRow)
					{
						if (First)
							First = false;
						else
							Toon.AppendDelimiter();
					}

					EncodeMember(Member.Key, Member.Value.AssociatedObjectValue, Indent, Toon);
				}
			}

			if (MultiRow)
			{
				Indent--;

				if (!First && Indent.Value > 0)
				{
					Toon.AppendLine();
					Toon.Indent(Indent.Value);
				}
			}
		}

		/// <summary>
		/// Extensible encoding of object <paramref name="Object"/>, by using
		/// best available <see cref="IToonEncoder"/> for the corresponding type.
		/// </summary>
		/// <param name="Object">Object to encode.</param>
		/// <param name="Indent">Optional indentation.</param>
		/// <param name="Toon">TOON output.</param>
		public static void Encode(object Object, int? Indent, ToonOutput Toon)
		{
			if (Object is null)
				Toon.Append("null");
			else
			{
				IToonEncoder Encoder = GetEncoder(Object);
				Encoder.Encode(Object, Indent, Toon);
			}
		}

		#endregion
	}
}
