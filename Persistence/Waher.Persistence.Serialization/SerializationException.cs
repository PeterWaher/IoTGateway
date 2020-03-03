using System;
using System.Collections.Generic;
using System.Text;
using Waher.Events;

namespace Waher.Persistence.Serialization
{
	/// <summary>
	/// Exception ocurring during serialization or deserialization of objects.
	/// </summary>
	public class SerializationException : Exception, IEventObject
	{
		private readonly Type type;

		/// <summary>
		/// Exception ocurring during serialization or deserialization of objects.
		/// </summary>
		/// <param name="Message">Exception message</param>
		/// <param name="Type">Type of object being serialized or deserialized.</param>
		public SerializationException(string Message, Type Type)
			: base(Message)
		{
			this.type = Type;
		}

		/// <summary>
		/// Type of object being serialized or deserialized.
		/// </summary>
		public Type Type => this.type;

		/// <summary>
		/// Object identifier related to the object.
		/// </summary>
		public string Object => this.type.FullName;
	}
}
