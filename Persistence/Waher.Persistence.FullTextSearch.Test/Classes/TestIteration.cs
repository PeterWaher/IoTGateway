using Waher.Events;
using Waher.Persistence.Serialization;

namespace Waher.Persistence.FullTextSearch.Test.Classes
{
	internal class TestIteration<T> : IDatabaseIteration<T>
		where T : class
	{
		private readonly List<string> collections = new List<string>();
		private readonly List<T> objects = new List<T>();
		private readonly List<object> incompatibleObjectIds = new List<object>();
		private bool incompatibleLogged = false;
		private bool exceptionsLogged = false;

		public string[] Collections => this.collections.ToArray();
		public T[] Objects => this.objects.ToArray();
		public object[] IncompatibleObjectIds => this.incompatibleObjectIds.ToArray();
		public bool IncompatibleLogged => this.incompatibleLogged;
		public bool ExceptionsLogged => this.exceptionsLogged;

		public Task EndCollection()
		{
			return Task.CompletedTask;
		}

		public Task EndDatabase()
		{
			return Task.CompletedTask;
		}

		public Task IncompatibleObject(object ObjectId)
		{
			this.incompatibleLogged = true;
			this.incompatibleObjectIds.Add(ObjectId);
			return Task.CompletedTask;
		}

		public Task ProcessObject(T Object)
		{
			this.objects.Add(Object);
			return Task.CompletedTask;
		}

		public Task ReportException(Exception Exception)
		{
			this.exceptionsLogged = true;
			Log.Critical(Exception);
			return Task.CompletedTask;
		}

		public Task StartCollection(string CollectionName)
		{
			this.collections.Add(CollectionName);
			return Task.CompletedTask;
		}

		public Task StartDatabase()
		{
			return Task.CompletedTask;
		}
	}
}
