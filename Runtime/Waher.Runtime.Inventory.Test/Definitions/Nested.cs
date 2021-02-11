using System;

namespace Waher.Runtime.Inventory.Test.Definitions
{
	public class Nested
	{
		private readonly DefaultConstructor defaultConstructor;
		private readonly OneArgument oneArgument;
		private readonly TwoArguments twoArguments;

		public Nested(DefaultConstructor DefaultConstructor,
			OneArgument OneArgument, TwoArguments TwoArguments)
		{
			this.defaultConstructor = DefaultConstructor;
			this.oneArgument = OneArgument;
			this.twoArguments = TwoArguments;
		}

		public DefaultConstructor DefaultConstructor => this.defaultConstructor;
		public OneArgument OneArgument => this.oneArgument;
		public TwoArguments TwoArguments => this.twoArguments;
	}
}
