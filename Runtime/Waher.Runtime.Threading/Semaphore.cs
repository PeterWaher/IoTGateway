using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Waher.Events;

namespace Waher.Runtime.Threading
{
	/// <summary>
	/// Represents a named semaphore, i.e. an object, identified by a name,
	/// that allows single concurrent writers but multiple concurrent readers.
	/// You can create multiple instances of the <see cref="Semaphore"/> class
	/// with the same name, and they all refer to the same named semaphore.
	/// When the object is disposed, it ends any reading or writing locks the
	/// object has started (not all locks pending for the named semaphore).
	/// 
	/// Note: Semaphores are unique in the space of the current application domain
	/// only.
	/// </summary>
	public class Semaphore : IMultiReadSingleWriteObject, IDisposable
	{
		private readonly string name;
		private int nrReaders = 0;
		private bool isWriting = false;

		/// <summary>
		/// Represents a named semaphore, i.e. an object, identified by a name,
		/// that allows single concurrent writers but multiple concurrent readers.
		/// You can create multiple instances of the <see cref="Semaphore"/> class
		/// with the same name, and they all refer to the same named semaphore.
		/// 
		/// Note: Semaphores are unique in the space of the current application domain
		/// only.
		/// </summary>
		/// <param name="Name">Semaphore name.</param>
		public Semaphore(string Name)
		{
			this.name = Name;
		}

		/// <summary>
		/// Represents a named semaphore, i.e. an object, identified by a name,
		/// that allows single concurrent writers but multiple concurrent readers.
		/// You can create multiple instances of the <see cref="Semaphore"/> class
		/// with the same name, and they all refer to the same named semaphore.
		/// 
		/// Note: Semaphores are unique in the space of the current application domain
		/// only.
		/// </summary>
		/// <param name="Name">Semaphore name.</param>
		/// <param name="NrReaders">Number of initial readers.</param>
		internal Semaphore(string Name, int NrReaders)
		{
			this.name = Name;
			this.nrReaders = NrReaders;
		}

		/// <summary>
		/// Represents a named semaphore, i.e. an object, identified by a name,
		/// that allows single concurrent writers but multiple concurrent readers.
		/// You can create multiple instances of the <see cref="Semaphore"/> class
		/// with the same name, and they all refer to the same named semaphore.
		/// 
		/// Note: Semaphores are unique in the space of the current application domain
		/// only.
		/// </summary>
		/// <param name="Name">Semaphore name.</param>
		/// <param name="IsWriting">If the semaphore is initially writing.</param>
		internal Semaphore(string Name, bool IsWriting)
		{
			this.name = Name;
			this.isWriting = IsWriting;
		}

		/// <summary>
		/// Waits until object ready for reading.
		/// Each call to <see cref="BeginRead"/> must be followed by exactly one call to <see cref="EndRead"/>.
		/// </summary>
		public async Task BeginRead()
		{
			await Semaphores.BeginRead(this.name);
			this.nrReaders++;
		}

		/// <summary>
		/// Ends a reading session of the object.
		/// Must be called once for each call to <see cref="BeginRead"/> or successful call to <see cref="TryBeginRead(int)"/>.
		/// </summary>
		public async Task EndRead()
		{
			await Semaphores.EndRead(this.name);
			this.nrReaders--;
		}

		/// <summary>
		/// Waits, at most <paramref name="Timeout"/> milliseconds, until object ready for reading.
		/// Each successful call to <see cref="TryBeginRead"/> must be followed by exactly one call to <see cref="EndRead"/>.
		/// </summary>
		/// <param name="Timeout">Timeout, in milliseconds.</param>
		public async Task<bool> TryBeginRead(int Timeout)
		{
			if (await Semaphores.TryBeginRead(this.name, Timeout))
			{
				this.nrReaders++;
				return true;
			}
			else
				return false;
		}

		/// <summary>
		/// Waits until object ready for writing.
		/// Each call to <see cref="BeginWrite"/> must be followed by exactly one call to <see cref="EndWrite"/>.
		/// </summary>
		public async Task BeginWrite()
		{
			await Semaphores.BeginWrite(this.name);
			this.isWriting = true;
		}

		/// <summary>
		/// Ends a writing session of the object.
		/// Must be called once for each call to <see cref="BeginWrite"/> or successful call to <see cref="TryBeginWrite(int)"/>.
		/// </summary>
		public async Task EndWrite()
		{
			await Semaphores.EndWrite(this.name);
			this.isWriting = false;
		}

		/// <summary>
		/// Waits, at most <paramref name="Timeout"/> milliseconds, until object ready for writing.
		/// Each successful call to <see cref="TryBeginWrite"/> must be followed by exactly one call to <see cref="EndWrite"/>.
		/// </summary>
		/// <param name="Timeout">Timeout, in milliseconds.</param>
		public async Task<bool> TryBeginWrite(int Timeout)
		{
			if (await Semaphores.TryBeginWrite(this.name, Timeout))
			{
				this.isWriting = true;
				return true;
			}
			else
				return false;
		}

		/// <summary>
		/// Disposes of the named semaphore, and releases any locks the object
		/// manages.
		/// </summary>
		public async void Dispose()
		{
			try
			{
				if (this.isWriting)
					await this.EndWrite();

				while (this.nrReaders > 0)
					await this.EndRead();
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}
	}
}
