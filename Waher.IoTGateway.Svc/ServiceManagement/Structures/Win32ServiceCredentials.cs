using System;

namespace Waher.IoTGateway.Svc.ServiceManagement.Structures
{
	/// <summary>
	/// Windows Service Credentials
	/// </summary>
	/// <param name="userName">User name</param>
	/// <param name="password">Password</param>
	public struct Win32ServiceCredentials(string userName, string password) 
		: IEquatable<Win32ServiceCredentials>
	{
		/// <summary>
		/// User name
		/// </summary>
		public string UserName { get; } = userName;

		/// <summary>
		/// Password
		/// </summary>
		public string Password { get; } = password;

		/// <summary>
		/// LOCAL_SYSTEM user
		/// </summary>
		public static readonly Win32ServiceCredentials LocalSystem = new(@".\LocalSystem", password: null);

		/// <summary>
		/// LOCAL_SERVICE user
		/// </summary>
		public static readonly Win32ServiceCredentials LocalService = new(@"NT AUTHORITY\LocalService", password: null);

		/// <summary>
		/// NETWORK_SERVICE user
		/// </summary>
		public static readonly Win32ServiceCredentials NetworkService = new(@"NT AUTHORITY\NetworkService", password: null);

		/// <summary>
		/// Compares the instance to another.
		/// </summary>
		/// <param name="Other">Other instance</param>
		/// <returns>If they are equal</returns>
		public bool Equals(Win32ServiceCredentials Other)
		{
			return string.Equals(this.UserName, Other.UserName) &&
				string.Equals(this.Password, Other.Password);
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			if (obj is null)
				return false;
			else
			{
				return obj is Win32ServiceCredentials Win32ServiceCredentials &&
					this.Equals(Win32ServiceCredentials);
			}
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			unchecked
			{
				return ((this.UserName?.GetHashCode() ?? 0) * 397) ^
					(this.Password?.GetHashCode() ?? 0);
			}
		}

		/// <summary>
		/// Equal to operator
		/// </summary>
		/// <param name="left">Left operand</param>
		/// <param name="right">Right operand</param>
		/// <returns>If they are equal</returns>
		public static bool operator ==(Win32ServiceCredentials left, Win32ServiceCredentials right)
		{
			return left.Equals(right);
		}

		/// <summary>
		/// Unequal to operator
		/// </summary>
		/// <param name="left">Left operand</param>
		/// <param name="right">Right operand</param>
		/// <returns>If they are unequal</returns>
		public static bool operator !=(Win32ServiceCredentials left, Win32ServiceCredentials right)
		{
			return !left.Equals(right);
		}
	}
}
