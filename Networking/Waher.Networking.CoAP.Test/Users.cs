using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waher.Security;

namespace Waher.Networking.CoAP.Test
{
	internal class Users : IUserSource
	{
		private readonly Dictionary<string, IUser> users;

		public Users(params IUser[] Users)
		{
			this.users = new Dictionary<string, IUser>();

			foreach (IUser User in Users)
				this.users[User.UserName] = User;
		}

		public Task<IUser> TryGetUser(string UserName)
		{
			if (this.users.TryGetValue(UserName, out IUser User))
				return Task.FromResult<IUser>(User);
			else
				return Task.FromResult<IUser>(null);
		}
	}
}
