using System;
using Waher.Persistence.Attributes;

#if !LW
namespace Waher.Persistence.Queues.Test.Classes
#else
using Waher.Persistence.Queues;
namespace Waher.Persistence.QueuesLW.Test.Classes
#endif
{
	[TypeName(TypeNameSerialization.FullName)]
	public class Simple
	{
		public int Counter;
		public string Message;

		public Simple()
		{
		}

		public Simple(int Counter, string Message)
		{
			this.Counter = Counter;
			this.Message = Message;
		}

		public override string ToString()
		{
			return this.Counter.ToString() + ": " + this.Message;
		}
	}
}
