using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.IoTGateway.Svc.ServiceManagement.Structures
{
    public struct Win32ServiceCredentials : IEquatable<Win32ServiceCredentials>
    {
        public string UserName { get; }

        public string Password { get; }

        public Win32ServiceCredentials(string userName, string password)
        {
            UserName = userName;
            Password = password;
        }

        public static Win32ServiceCredentials LocalSystem = new Win32ServiceCredentials(@".\LocalSystem", password: null);

        public static Win32ServiceCredentials LocalService = new Win32ServiceCredentials(@"NT AUTHORITY\LocalService", password: null);

        public static Win32ServiceCredentials NetworkService = new Win32ServiceCredentials(@"NT AUTHORITY\NetworkService", password: null);
        
        public bool Equals(Win32ServiceCredentials other)
        {
            return string.Equals(UserName, other.UserName) && string.Equals(Password, other.Password);
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;
            else
                return obj is Win32ServiceCredentials Win32ServiceCredentials && Equals(Win32ServiceCredentials);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((UserName?.GetHashCode() ?? 0)*397) ^ (Password?.GetHashCode() ?? 0);
            }
        }

        public static bool operator ==(Win32ServiceCredentials left, Win32ServiceCredentials right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Win32ServiceCredentials left, Win32ServiceCredentials right)
        {
            return !left.Equals(right);
        }
    }
}
