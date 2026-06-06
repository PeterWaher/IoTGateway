using System;
using System.Collections.Generic;
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

		/// <summary>
		/// Gets a TOON encoder for a given object, by looking up the best available
		/// encoder, based on the type of the object.
		/// </summary>
		/// <param name="Value">Value to encode.</param>
		/// <returns>Encoder</returns>
		public static IToonEncoder GetEncoder(object Value)
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

		#endregion
	}
}
