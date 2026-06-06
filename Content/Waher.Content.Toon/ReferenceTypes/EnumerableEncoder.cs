using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Waher.Content.Toon.Model;
using Waher.Runtime.Inventory;

namespace Waher.Content.Toon.ReferenceTypes
{
	/// <summary>
	/// Encodes <see cref="IEnumerable"/> values.
	/// </summary>
	public class EnumerableEncoder : VectorToonEncoder
	{
		/// <summary>
		/// Encodes <see cref="IEnumerable"/> values.
		/// </summary>
		public EnumerableEncoder()
		{
		}

		/// <summary>
		/// If the encoder encodes a value as a vector.
		/// </summary>
		public override bool EncodesAsVector(object Value) => true;

		/// <summary>
		/// Encodes the <paramref name="Object"/> to TOON.
		/// </summary>
		/// <param name="Object">Object to encode.</param>
		/// <param name="Indent">Any indentation to apply.</param>
		/// <param name="Toon">TOON output.</param>
		/// <param name="Brackets">How to manage brackets when encoding vectors.</param>
		public override void Encode(object Object, int? Indent, ToonOutput Toon, BracketsMode Brackets)
		{
			IEnumerable E = (IEnumerable)Object;
			IEnumerator e = E.GetEnumerator();
			bool First;

			switch (Brackets)
			{
				case BracketsMode.Embed:
					Toon.Append('[');

					if (Indent.HasValue)
						Indent++;
					break;
				case BracketsMode.Ignore:
					break;

				case BracketsMode.Count:
					int Count = 0;

					while (e.MoveNext())
						Count++;

					e.Reset();

					if (Count == 0)
					{
						if (Toon.Empty)
							Toon.Append("[]");
						else
							Toon.Append(": []");

						return;
					}

					Toon.Append('[');
					Toon.Append(Count.ToString());

					if (!Toon.StandardDelimiter)
						Toon.AppendDelimiter();

					Toon.Append(']');

					Dictionary<string, object> Parameters = new Dictionary<string, object>();
					LinkedList<string> ParameterOrder = new LinkedList<string>();
					LinkedList<Dictionary<string, object>> ParameterSets = new LinkedList<Dictionary<string, object>>();
					bool SameParameters = true;
					bool MultiRowElements = false;

					First = true;

					while (e.MoveNext())
					{
						object Element = e.Current;
						IToonEncoder ElementEncoder = TOON.GetEncoder(Element);

						MultiRowElements |= ElementEncoder.EncodesMultipleRows;

						if (!(Parameters is null))
						{
							IEnumerator<KeyValuePair<string, object>> e2 =
								ElementEncoder.GetParameters(Element);

							if (e2 is null)
							{
								Parameters = null;
								continue;
							}

							Dictionary<string, object> ParameterSet = new Dictionary<string, object>();
							Count = 0;

							while (e2.MoveNext())
							{
								KeyValuePair<string, object> P = e2.Current;

								if (!Parameters.ContainsKey(P.Key))
								{
									if (First)
									{
										Parameters[P.Key] = P.Value;
										ParameterOrder.AddLast(P.Key);
									}
									else
										SameParameters = false;
								}
								else if (First)
								{
									Parameters = null;
									continue;
								}

								ParameterSet[P.Key] = P.Value;
								Count++;

								if (SameParameters && !(P.Value is null))
								{
									ElementEncoder = TOON.GetEncoder(P.Value);

									if (ElementEncoder.EncodesAsObject(P.Value) ||
										ElementEncoder.EncodesAsVector(P.Value))
									{
										SameParameters = false;
									}
								}
							}

							if (Parameters is null)
								continue;
							else
								First = false;

							if (Count != Parameters.Count)
								SameParameters = false;

							ParameterSets.AddLast(ParameterSet);
						}
					}

					if (!(Parameters is null) && Count > 0)
					{
						if (SameParameters)
						{
							// Arrays of objects with same parameters.

							First = true;

							Toon.Append('{');

							foreach (string ParameterName in ParameterOrder)
							{
								if (First)
									First = false;
								else
									Toon.AppendDelimiter();

								Toon.AppendEncoded(ParameterName, true);
							}

							Toon.Append("}:");

							foreach (Dictionary<string, object> ParameterSet in ParameterSets)
							{
								Toon.AppendLine();
								Toon.Indent(Math.Max(Indent ?? 0, 0) + 1);

								First = true;

								foreach (string ParameterName in ParameterOrder)
								{
									if (First)
										First = false;
									else
										Toon.AppendDelimiter();

									if (ParameterSet.TryGetValue(ParameterName, out object Element))
									{
										IToonEncoder ElementEncoder = TOON.GetEncoder(Element);
										ElementEncoder.Encode(Element, null, Toon);
									}
								}
							}

							return;
						}
						else
						{
							// Arrays of objects with different parameters,
							// or encoded using list mode.

							Toon.Append(':');

							Indent = Math.Max(Indent ?? 0, 0) + 1;

							foreach (Dictionary<string, object> ParameterSet in ParameterSets)
							{
								First = true;

								foreach (KeyValuePair<string, object> P in ParameterSet)
								{
									Toon.AppendLine();
									Toon.Indent(Indent.Value);

									if (First)
									{
										Toon.Append("- ");
										First = false;
									}
									else
										Toon.Append("  ");

									Toon.AppendEncoded(P.Key, true);

									IToonEncoder ElementEncoder = TOON.GetEncoder(P.Value);

									if (ElementEncoder.EncodesAsObject(P.Value))
									{
										Toon.Append(':');
										ElementEncoder.Encode(P.Value, Math.Max(Indent ?? 0, 0) + 1, Toon);
									}
									else if (ElementEncoder.EncodesAsVector(P.Value))
										ElementEncoder.Encode(P.Value, Math.Max(Indent ?? 0, 0) + 1, Toon, BracketsMode.Count);
									else
									{
										Toon.Append(": ");
										ElementEncoder.Encode(P.Value, null, Toon);
									}
								}
							}

							return;
						}
					}

					e.Reset();

					LinkedList<IEnumerator> ElementVectors = new LinkedList<IEnumerator>();

					while (e.MoveNext())
					{
						object Element = e.Current;
						IToonEncoder ElementEncoder = TOON.GetEncoder(Element);

						if (!ElementEncoder.EncodesAsVector(Element))
						{
							ElementVectors = null;
							break;
						}

						IEnumerator e2 = ElementEncoder.GetElements(Element);
						if (e2 is null)
						{
							ElementVectors = null;
							break;
						}

						ElementVectors.AddLast(e2);
					}

					Toon.Append(':');

					if (!(ElementVectors is null))
					{
						// Array of arrays

						Indent = Math.Max(Indent ?? 0, 0) + 1;

						foreach (IEnumerator e2 in ElementVectors)
						{
							Toon.AppendLine();
							Toon.Indent(Indent.Value);
							Toon.Append("- ");

							Count = 0;

							while (e2.MoveNext())
								Count++;

							Toon.Append('[');
							Toon.Append(Count.ToString());

							if (!Toon.StandardDelimiter)
								Toon.AppendDelimiter();

							Toon.Append("]:");

							First = true;

							e2.Reset();

							while (e2.MoveNext())
							{
								if (First)
								{
									First = false;
									Toon.Append(' ');
								}
								else
									Toon.AppendDelimiter();

								object Element = e2.Current;
								IToonEncoder ElementEncoder = TOON.GetEncoder(Element);
								ElementEncoder.Encode(Element, null, Toon);
							}
						}

						return;
					}
					else if (MultiRowElements)
					{
						// At least one element of the array is/can be encoded using
						// multiple rows. Use list mode.

						Indent = Math.Max(Indent ?? 0, 0) + 1;
						e.Reset();

						while (e.MoveNext())
						{
							Toon.AppendLine();
							Toon.Indent(Indent.Value);
							Toon.AppendListItem();

							IToonEncoder ElementEncoder = TOON.GetEncoder(e.Current);

							if (ElementEncoder.EncodesMultipleRows)
							{
								if (ElementEncoder.EncodesAsVector(e.Current))
									ElementEncoder.Encode(e.Current, Indent, Toon, BracketsMode.Count);
								else
									ElementEncoder.Encode(e.Current, Indent, Toon);
							}
							else
							{
								Toon.Append(' ');
								ElementEncoder.Encode(e.Current, Indent, Toon);
							}
						}

						return;
					}

					Toon.Append(' ');
					e.Reset();
					break;
			}

			First = true;

			while (e.MoveNext())
			{
				if (First)
					First = false;
				else
					Toon.AppendDelimiter();

				if (Brackets == BracketsMode.Embed)
				{
					if (Indent.HasValue && Indent.Value > 0)
					{
						Toon.AppendLine();
						Toon.Indent(Indent.Value);
					}

					TOON.Encode(e.Current, Indent, Toon);
				}
				else
					TOON.Encode(e.Current, null, Toon);
			}

			if (Brackets == BracketsMode.Embed)
			{
				if (Indent.HasValue)
				{
					Indent--;

					if (!First && Indent.Value > 0)
					{
						Toon.AppendLine();
						Toon.Indent(Indent.Value);
					}
				}

				Toon.Append(']');
			}
		}

		/// <summary>
		/// How well the TOON encoder encodes objects of type <paramref name="ObjectType"/>.
		/// </summary>
		/// <param name="ObjectType">Type of object to encode.</param>
		/// <returns>How well objects of the given type are encoded.</returns>
		public override Grade Supports(Type ObjectType)
		{
			return typeof(IEnumerable).IsAssignableFrom(ObjectType.GetTypeInfo()) ? Grade.Barely : Grade.NotAtAll;
		}

		/// <summary>
		/// Gets an enumerator for the child-elements of an object.
		/// </summary>
		/// <param name="Object">Object to get child-elements from.</param>
		/// <returns>Enumerator for the child-elements, or null if not a vector.</returns>
		public override IEnumerator GetElements(object Object)
		{
			return ((IEnumerable)Object).GetEnumerator();
		}
	}
}
