using System;
using System.Security.Cryptography;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Cryptography.Functions.Encryption
{
	/// <summary>
	/// AES Encryption
	/// </summary>
	public class Aes256Encrypt : FunctionMultiVariate
	{
		/// <summary>
		/// AES Encryption
		/// </summary>
		/// <param name="Content">Content to be encrypted.</param>
		/// <param name="Key">Key to use for encryption.</param>
		/// <param name="IV">Initiation Vector to use for encryption.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public Aes256Encrypt(ScriptNode Content, ScriptNode Key, ScriptNode IV, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Content, Key, IV }, argumentTypes3Normal, Start, Length, Expression)
		{
		}

		/// <summary>
		/// AES Encryption
		/// </summary>
		/// <param name="Content">Content to be encrypted.</param>
		/// <param name="Key">Key to use for encryption.</param>
		/// <param name="IV">Initiation Vector to use for encryption.</param>
		/// <param name="CipherMode">Cipher Mode</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public Aes256Encrypt(ScriptNode Content, ScriptNode Key, ScriptNode IV, ScriptNode CipherMode, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Content, Key, IV, CipherMode }, argumentTypes4Normal, Start, Length, Expression)
		{
		}

		/// <summary>
		/// AES Encryption
		/// </summary>
		/// <param name="Content">Content to be encrypted.</param>
		/// <param name="Key">Key to use for encryption.</param>
		/// <param name="IV">Initiation Vector to use for encryption.</param>
		/// <param name="CipherMode">Cipher Mode</param>
		/// <param name="PaddingMode">Padding mode</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public Aes256Encrypt(ScriptNode Content, ScriptNode Key, ScriptNode IV, ScriptNode CipherMode, ScriptNode PaddingMode, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Content, Key, IV, CipherMode, PaddingMode }, argumentTypes5Normal, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(Aes256Encrypt);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "Content", "Key", "IV", "CipherMode", "PaddingMode" };

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			if (!(Arguments[0].AssociatedObjectValue is byte[] Data))
				throw new ScriptRuntimeException("Data to encrypt must be binary (i.e. an array of bytes).", this);

			if (!(Arguments[1].AssociatedObjectValue is byte[] Key))
				throw new ScriptRuntimeException("Key to use for encryption must be binary (i.e. an array of bytes).", this);

			if (!(Arguments[2].AssociatedObjectValue is byte[] IV))
				throw new ScriptRuntimeException("Initiation Vector to use for encryption must be binary (i.e. an array of bytes).", this);

			int c = Arguments.Length;
			CipherMode CipherMode = c <= 3 ? CipherMode.CBC : this.ToEnum<CipherMode>(Arguments[3]);
			PaddingMode PaddingMode = c <= 4 ? PaddingMode.PKCS7 : this.ToEnum<PaddingMode>(Arguments[4]);

			using (Aes Aes = Aes.Create())
			{
				Aes.BlockSize = 128;
				Aes.KeySize = 256;
				Aes.Mode = CipherMode;
				Aes.Padding = PaddingMode;

				using (ICryptoTransform Transform = Aes.CreateEncryptor(Key, IV))
				{
					byte[] Encrypted = Transform.TransformFinalBlock(Data, 0, Data.Length);
					return new ObjectValue(Encrypted);
				}
			}
		}
	}
}
