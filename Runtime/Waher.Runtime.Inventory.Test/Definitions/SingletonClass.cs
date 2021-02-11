using System;
using System.Security.Cryptography;

namespace Waher.Runtime.Inventory.Test.Definitions
{
	[Singleton]
	public class SingletonClass
	{
		private readonly string rnd;

		public SingletonClass()
		{
			using (RandomNumberGenerator Rnd = RandomNumberGenerator.Create())
			{
				byte[] Bin = new byte[1024];
				Rnd.GetBytes(Bin);
				this.rnd = Convert.ToBase64String(Bin);
			}
		}

		public string Rnd => this.rnd;
	}
}
