using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Waher.Runtime.Threading;

namespace Waher.Persistence.Files
{
	/// <summary>
	/// Maintains an enumerated set of labels for an Object B-Tree File.
	/// </summary>
	public class LabelFile : SerialFile
	{
		private readonly Dictionary<string, uint> codesByLabel = new Dictionary<string, uint>();
		private readonly Dictionary<uint, string> labelsByCode = new Dictionary<uint, string>();

		private LabelFile(string FileName, string CollectionName, int TimeoutMilliseconds, bool Encrypted, FilesProvider Provider)
			: base(FileName, CollectionName, TimeoutMilliseconds, Encrypted, Provider)
		{
		}

		/// <summary>
		/// Reads a label from the file.
		/// </summary>
		/// <param name="Position">Position of label.</param>
		/// <returns>Label, and the position of the following label.</returns>
		public async Task<KeyValuePair<string, long>> ReadLabel(long Position)
		{
			KeyValuePair<byte[], long> P = await ReadBlock(Position);
			return new KeyValuePair<string, long>(Encoding.UTF8.GetString(P.Key), P.Value);
		}

		/// <summary>
		/// Writes a label to the end of the file.
		/// </summary>
		/// <param name="Label">New label to save to the file.</param>
		/// <returns>Position of label.</returns>
		public Task<long> WriteLabel(string Label)
		{
			return this.WriteBlock(Encoding.UTF8.GetBytes(Label));
		}

		/// <summary>
		/// Writes a label to the end of the file.
		/// </summary>
		/// <param name="Label">New label to save to the file.</param>
		/// <returns>Position of label.</returns>
		protected Task<long> WriteLabelLocked(string Label)
		{
			return this.WriteBlockLocked(Encoding.UTF8.GetBytes(Label));
		}

		/// <summary>
		/// Creates a LabelFile object.
		/// </summary>
		/// <param name="CollectionName">Name of collection.</param>
		/// <param name="TimeoutMilliseconds">Timeout, in milliseconds, to wait for access to the database layer.</param>
		/// <param name="Encrypted">If the files should be encrypted or not.</param>
		/// <param name="Provider">Reference to the files provider.</param>
		/// <returns>LabelFile object.</returns>
		public static async Task<LabelFile> Create(string CollectionName, int TimeoutMilliseconds, bool Encrypted, FilesProvider Provider)
		{
			string FileName = Provider.GetFileName(CollectionName);
			string LabelsFileName = FileName + ".labels";
			bool LabelsExists = File.Exists(LabelsFileName);
			LabelFile Result = new LabelFile(LabelsFileName, CollectionName, TimeoutMilliseconds, Encrypted, Provider);
			uint LastCode = 0;
			uint Code = 1;

			await GetKeys(Result);

			if (LabelsExists)
			{
				long Len = await Result.GetLength();
				long Pos = 0;

				while (Pos < Len)
				{
					try
					{
						KeyValuePair<string, long> P = await Result.ReadLabel(Pos);

						Result.codesByLabel[P.Key] = Code;
						Result.labelsByCode[Code] = P.Key;
						LastCode = Code;

						Code++;
						Pos = P.Value;
					}
					catch (Exception)
					{
						await Result.Truncate(Pos);
						Len = Pos;
					}
				}
			}
			else
			{
				string NamesFileName = FileName + ".names";
				if (File.Exists(NamesFileName))
				{
					SortedDictionary<uint, string> NewCodes = null;

					using (StringDictionary Names = await StringDictionary.Create(NamesFileName, string.Empty, CollectionName, Provider, false))
					{
						foreach (KeyValuePair<string, object> Rec in await Names.ToArrayAsync())
						{
							if (Rec.Value is ulong Code2 && Code2 <= uint.MaxValue && !Result.labelsByCode.ContainsKey(Code = (uint)Code2))
							{
								if (NewCodes is null)
									NewCodes = new SortedDictionary<uint, string>();

								Result.codesByLabel[Rec.Key] = Code;
								Result.labelsByCode[Code] = Rec.Key;

								NewCodes[Code] = Rec.Key;
							}
						}
					}

					while (!(NewCodes is null))
					{
						LastCode++;

						if (NewCodes.TryGetValue(LastCode, out string Label))
						{
							await Result.WriteLabel(Label);

							NewCodes.Remove(LastCode);
							if (NewCodes.Count == 0)
								NewCodes = null;
						}
						else
							await Result.WriteLabel(LastCode.ToString());
					}

					File.Delete(NamesFileName);
				}
			}

			return Result;
		}

		/// <summary>
		/// Gets the code for a specific field in a collection.
		/// </summary>
		/// <param name="FieldName">Name of field.</param>
		/// <returns>Field code.</returns>
		public async Task<uint> GetFieldCode(string FieldName)
		{
			uint Result;

			await this.LockRead();
			try
			{
				if (this.codesByLabel.TryGetValue(FieldName, out Result))
					return Result;
			}
			finally
			{
				await this.EndRead();
			}

			await this.LockWrite();
			try
			{
				if (!this.codesByLabel.TryGetValue(FieldName, out Result))
				{
					Result = (uint)this.labelsByCode.Count;
					if (Result == int.MaxValue)
						throw Database.FlagForRepair(this.CollectionName, "Too many labels in " + this.FileName);

					await this.WriteLabelLocked(FieldName);

					Result++;

					this.codesByLabel[FieldName] = Result;
					this.labelsByCode[Result] = FieldName;
				}
			}
			finally
			{
				await this.EndWrite();
			}

			return Result;
		}

		/// <summary>
		/// Tries to get the code for a specific field in a collection.
		/// </summary>
		/// <param name="FieldName">Name of field.</param>
		/// <returns>The field code, if one was found, or null otherwise.</returns>
		public async Task<uint?> TryGetFieldCode(string FieldName)
		{
			await this.LockRead();
			try
			{
				if (this.codesByLabel.TryGetValue(FieldName, out uint Result))
					return Result;
				else
					return null;
			}
			finally
			{
				await this.EndRead();
			}
		}

		/// <summary>
		/// Gets an array of available labels.
		/// </summary>
		/// <returns>Array of labels.</returns>
		public async Task<string[]> GetLabelsAsync()
		{
			string[] Result;

			await this.LockRead();
			try
			{
				Result = new string[this.codesByLabel.Count];
				this.codesByLabel.Keys.CopyTo(Result, 0);
			}
			finally
			{
				await this.EndRead();
			}

			return Result;
		}

		/// <summary>
		/// Gets the name of a field in a collection, given its code.
		/// </summary>
		/// <param name="FieldCode">Field code.</param>
		/// <returns>Field name.</returns>
		/// <exception cref="ArgumentException">If the collection or field code are not known.</exception>
		public async Task<string> GetFieldName(uint FieldCode)
		{
			await this.LockRead();
			try
			{
				if (this.labelsByCode.TryGetValue(FieldCode, out string Result))
					return Result;
				else
					throw Database.FlagForRepair(this.CollectionName, "Field code unknown, Collection: " + this.CollectionName);
			}
			finally
			{
				await this.EndRead();
			}
		}

	}
}
