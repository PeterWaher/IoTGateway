using System.Collections.Generic;

namespace Waher.Networking.HTTP.JsonRpc.Transports
{
	/// <summary>
	/// SSE notification event arguments.
	/// </summary>
	public class NotificationEventArgs
	{
		/// <summary>
		/// SSE notification event arguments.
		/// </summary>
		/// <param name="Comment">Comment</param>
		/// <param name="Fields">SSE Fields to emit</param>
		public NotificationEventArgs(string? Comment, 
			IEnumerable<KeyValuePair<string, object>> Fields)
		{
			this.Comment = Comment;
			this.Fields = Fields;
		}

		/// <summary>
		/// Comment
		/// </summary>
		public string? Comment { get; }

		/// <summary>
		/// SSE Fields to emit.
		/// </summary>
		public IEnumerable<KeyValuePair<string, object>> Fields { get; }

		/// <summary>
		/// Gets a field value by name.
		/// </summary>
		/// <param name="FieldName">Name of the field.</param>
		/// <returns>Value of the field, if found, null otherwise.</returns>
		public object? this[string FieldName]
		{
			get
			{
				foreach (KeyValuePair<string, object> P in this.Fields)
				{
					if (P.Key == FieldName)
						return P.Value;
				}

				return null;
			}
		}
	}
}
