using System;
using System.Text;

namespace Waher.Runtime.Inventory
{
	/// <summary>
	/// Represents a type and a set of arguments, for which an object instance is the single instantiation.
	/// </summary>
	public class SingletonKey
	{
		private readonly Type type;
		private readonly object[] arguments;

		/// <summary>
		/// Represents a type and a set of arguments, for which an object instance is the single instantiation.
		/// </summary>
		/// <param name="Type">Singleton type.</param>
		/// <param name="Arguments">Arguments, for which the instance is unique.</param>
		public SingletonKey(Type Type, object[] Arguments)
		{
			this.type = Type;
			this.arguments = Arguments;
		}

		/// <summary>
		/// Singleton type.
		/// </summary>
		public Type Type => this.type;

		/// <summary>
		/// Arguments, for which the instance is unique.
		/// </summary>
		public object[] Arguments => this.arguments;

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			int i, c;

			if (!(obj is SingletonKey Key) ||
				this.type != Key.type ||
				(this.arguments is null) ^ (Key.arguments is null) ||
				(c = this.arguments?.Length ?? 0) != (Key.arguments?.Length ?? 0))
			{
				return false;
			}

			for (i = 0; i < c; i++)
			{
				if (!this.arguments[i].Equals(Key.arguments[i]))
					return false;
			}

			return true;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int Result = this.type.GetHashCode();

			if (!(this.arguments is null))
			{
				foreach (object Obj in this.arguments)
					Result ^= Result << 5 ^ (Obj?.GetHashCode() ?? 0);
			}

			return Result;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			int i, c;

			sb.Append(this.type.FullName);
			sb.Append('(');

			for (i = 0, c = this.arguments?.Length ?? 0; i < c; i++)
			{
				if (i > 0)
					sb.Append(", ");

				if (this.arguments[i] is null)
					sb.Append("null");
				else
					sb.Append(this.arguments[i].GetType().FullName);
			}

			sb.Append(')');

			return sb.ToString();
		}
	}
}
