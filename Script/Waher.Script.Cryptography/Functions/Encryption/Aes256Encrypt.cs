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
			: base(new ScriptNode[] { Content, Key, IV }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal },
				  Start, Length, Expression)
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
			: base(new ScriptNode[] { Content, Key, IV, CipherMode }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal },
				  Start, Length, Expression)
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
			: base(new ScriptNode[] { Content, Key, IV, CipherMode, PaddingMode }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => "Aes256Encrypt";

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
			byte[] Data = Arguments[0].AssociatedObjectValue as byte[];
			if (Data is null)
				throw new ScriptRuntimeException("Data to encrypt must be binary (i.e. an array of bytes).", this);

			byte[] Key = Arguments[1].AssociatedObjectValue as byte[];
			if (Key is null)
				throw new ScriptRuntimeException("Key to use for encryption must be binary (i.e. an array of bytes).", this);

			byte[] IV = Arguments[2].AssociatedObjectValue as byte[];
			if (IV is null)
				throw new ScriptRuntimeException("Initiation Vector to use for encryption must be binary (i.e. an array of bytes).", this);

			int c = Arguments.Length;
			CipherMode CipherMode;

			if (c <= 3)
				CipherMode = CipherMode.CBC;
			else if (Arguments[3].AssociatedObjectValue is CipherMode CipherMode2)
				CipherMode = CipherMode2;
			else if (!Enum.TryParse<CipherMode>(Arguments[3].AssociatedObjectValue.ToString() ?? string.Empty, out CipherMode))
				throw new ScriptRuntimeException("Invalid Cipher Mode.", this);

			PaddingMode PaddingMode;

			if (c <= 4)
				PaddingMode = PaddingMode.PKCS7;
			else if (Arguments[4].AssociatedObjectValue is PaddingMode PaddingMode2)
				PaddingMode = PaddingMode2;
			else if (!Enum.TryParse<PaddingMode>(Arguments[4].AssociatedObjectValue.ToString() ?? string.Empty, out PaddingMode))
				throw new ScriptRuntimeException("Invalid Padding Mode.", this);


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
