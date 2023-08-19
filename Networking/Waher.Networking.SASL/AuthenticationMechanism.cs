using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Networking.SASL
{
    /// <summary>
    /// Base class for all authentication mechanisms.
    /// </summary>
    public abstract class AuthenticationMechanism : IAuthenticationMechanism
    {
		private static readonly RandomNumberGenerator rnd = RandomNumberGenerator.Create();

        /// <summary>
        /// Base class for all authentication mechanisms.
        /// </summary>
        public AuthenticationMechanism()
        {
        }

        /// <summary>
        /// Name of the mechanism.
        /// </summary>
        public abstract string Name
        {
            get;
        }

        /// <summary>
        /// Weight of mechanisms. The higher the value, the more preferred.
        /// </summary>
        public abstract int Weight
        {
            get;
        }

		/// <summary>
		/// Checks if a mechanism is allowed during the current conditions.
		/// </summary>
		/// <param name="SslStream">SSL stream, if available.</param>
		/// <returns>If mechanism is allowed.</returns>
		public abstract bool Allowed(SslStream SslStream);

		/// <summary>
		/// Parses a parameter list in a challenge string.
		/// </summary>
		/// <param name="s">Encoded parameter list.</param>
		/// <returns>Parsed parameters.</returns>
		protected KeyValuePair<string, string>[] ParseCommaSeparatedParameterList(string s)
        {
            List<KeyValuePair<string, string>> Result = new List<KeyValuePair<string, string>>();
            StringBuilder sb = new StringBuilder();
            string Key = string.Empty;
            int State = 0;

            foreach (char ch in s)
            {
                switch (State)
                {
                    case 0:     // ID
                        if (ch == '=')
                        {
                            Key = sb.ToString();
                            sb.Clear();
                            State++;
                        }
                        else if (ch == ',')
                        {
                            Result.Add(new KeyValuePair<string, string>(sb.ToString(), string.Empty));
                            sb.Clear();
                        }
                        else
                            sb.Append(ch);
                        break;

                    case 1: // Value, first character
                        if (ch == '"')
                            State += 2;
                        else if (ch == ',')
                        {
                            Result.Add(new KeyValuePair<string, string>(Key, string.Empty));
                            sb.Clear();
                            State = 0;
                            Key = string.Empty;
                        }
                        else
                        {
                            sb.Append(ch);
                            State++;
                        }
                        break;

                    case 2: // Value, following characters
                        if (ch == ',')
                        {
                            Result.Add(new KeyValuePair<string, string>(Key, sb.ToString()));
                            sb.Clear();
                            State = 0;
                            Key = string.Empty;
                        }
                        else
                            sb.Append(ch);
                        break;

                    case 3: // Value, between quotes
                        if (ch == '"')
                            State--;
                        else if (ch == '\\')
                            State++;
                        else
                            sb.Append(ch);
                        break;

                    case 4: // Escaped character
                        sb.Append(ch);
                        State--;
                        break;
                }
            }

            if (State == 2 && !string.IsNullOrEmpty(Key))
                Result.Add(new KeyValuePair<string, string>(Key, sb.ToString()));

            return Result.ToArray();
        }

        protected static byte[] CONCAT(params byte[][] Data)
        {
            int c = 0;

            foreach (byte[] Part in Data)
                c += Part.Length;

            int i = 0;
            int j;
            byte[] Result = new byte[c];

            foreach (byte[] Part in Data)
            {
                j = Part.Length;
                Array.Copy(Part, 0, Result, i, j);
                i += j;
            }

            return Result;
        }

        protected static string CONCAT(params string[] Parameters)
        {
            StringBuilder sb = new StringBuilder();

            foreach (string s in Parameters)
                sb.Append(s);

            return sb.ToString();
        }

        protected static byte[] CONCAT(byte[] Data, params string[] Parameters)
        {
            return CONCAT(Data, System.Text.Encoding.UTF8.GetBytes(CONCAT(Parameters)));
        }

        protected static string HEX(byte[] Data)
        {
            StringBuilder sb = new StringBuilder();

            foreach (byte b in Data)
                sb.Append(b.ToString("x2"));

            return sb.ToString();
        }

        protected static byte[] XOR(byte[] U1, byte[] U2)
        {
            int i, c = U1.Length;
            if (U2.Length != c)
                throw new Exception("Arrays must be of the same size.");

            byte[] Response = new byte[c];

            for (i = 0; i < c; i++)
                Response[i] = (byte)(U1[i] ^ U2[i]);

            return Response;
        }

        /// <summary>
        /// Authentication request has been made.
        /// </summary>
        /// <param name="Data">Data in authentication request.</param>
        /// <param name="Connection">Connection performing the authentication.</param>
        /// <param name="PersistenceLayer">Persistence layer.</param>
        /// <returns>If authentication was successful (true). If false, mechanism must send the corresponding challenge.</returns>
        public abstract Task<bool?> AuthenticationRequest(string Data, ISaslServerSide Connection, ISaslPersistenceLayer PersistenceLayer);

        /// <summary>
        /// Response request has been made.
        /// </summary>
        /// <param name="Data">Data in response request.</param>
        /// <param name="Connection">Connection performing the authentication.</param>
        /// <param name="PersistenceLayer">Persistence layer.</param>
        /// <returns>If authentication was successful (true).</returns>
        public abstract Task<bool?> ResponseRequest(string Data, ISaslServerSide Connection, ISaslPersistenceLayer PersistenceLayer);

		/// <summary>
		/// Performs intitialization of the mechanism. Can be used to set
		/// static properties that will be used through-out the runtime of the
		/// server.
		/// </summary>
		public abstract Task Initialize();

		/// <summary>
		/// Authenticates the user using the provided credentials.
		/// </summary>
		/// <param name="UserName">User Name</param>
		/// <param name="Password">Password</param>
		/// <param name="Connection">Connection</param>
		/// <returns>If authentication was successful or not. If null is returned, the mechanism did not perform authentication.</returns>
		public abstract Task<bool?> Authenticate(string UserName, string Password, ISaslClientSide Connection);

		/// <summary>
		/// Gets an array of random bytes.
		/// </summary>
		/// <param name="Count">Number of random bytes to generate.</param>
		/// <returns>Array of random bytes.</returns>
		protected static byte[] GetRandomBytes(int Count)
		{
			if (Count < 0)
				throw new ArgumentException("Count must be positive.", nameof(Count));

			byte[] Result = new byte[Count];

			lock(rnd)
			{
				rnd.GetBytes(Result);
			}

			return Result;
		}
	}
}
