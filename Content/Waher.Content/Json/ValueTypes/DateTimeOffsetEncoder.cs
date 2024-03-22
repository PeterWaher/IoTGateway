using System;
using System.Text;
using Waher.Runtime.Inventory;

namespace Waher.Content.Json.ValueTypes
{
	/// <summary>
	/// Encodes <see cref="DateTimeOffset"/> values.
	/// </summary>
	public class DateTimeOffsetEncoder : IJsonEncoder
	{
		/// <summary>
		/// Encodes <see cref="DateTimeOffset"/> values.
		/// </summary>
		public DateTimeOffsetEncoder()
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
			DateTime TP = ((DateTimeOffset)Object).ToUniversalTime().DateTime;
			long Seconds = (long)(TP - JSON.UnixEpoch).TotalSeconds;

			Json.Append(Seconds.ToString());
		}

		/// <summary>
		/// How well the JSON encoder encodes objects of type <paramref name="ObjectType"/>.
		/// </summary>
		/// <param name="ObjectType">Type of object to encode.</param>
		/// <returns>How well objects of the given type are encoded.</returns>
		public Grade Supports(Type ObjectType)
		{
			return ObjectType == typeof(DateTimeOffset) ? Grade.Ok : Grade.NotAtAll;
		}
	}
}
