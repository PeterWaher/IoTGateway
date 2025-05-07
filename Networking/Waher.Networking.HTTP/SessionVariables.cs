using System;
using System.Threading;
using System.Threading.Tasks;
using Waher.Script;
using Waher.Script.Exceptions;
using Waher.Script.Objects;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// Collection of session variables.
	/// </summary>
	public class SessionVariables : Variables
	{
		private static readonly Variables globalVariables = new Variables();
		private static readonly ObjectValue globalVariablesElement = new ObjectValue(globalVariables);

		private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);
		private PostedInformation lastPost = null;
		private HttpRequest currentRequest = null;
		private HttpResponse currentResponse = null;
		private Variables currentPageVariables = null;
		private ObjectValue currentPageVariablesElement = ObjectValue.Null;
		private string currentPageUrl = null;

		/// <summary>
		/// Collection of session variables.
		/// </summary>
		public SessionVariables(params Variable[] Variables)
			: base(Variables)
		{
		}

		/// <summary>
		/// Reference to global collection of variables.
		/// </summary>
		public static Variables GlobalVariables => globalVariables;

		/// <summary>
		/// Script element with reference to global collection of variables.
		/// </summary>
		public static ObjectValue GlobalVariablesElement => globalVariablesElement;

		/// <summary>
		/// Information about the content in the last POST request.
		/// </summary>
		public PostedInformation LastPost
		{
			get => this.lastPost;
			set => this.lastPost = value;
		}

		/// <summary>
		/// Reference to current request.
		/// </summary>
		public HttpRequest CurrentRequest
		{
			get => this.currentRequest;
			set => this.currentRequest = value;
		}

		/// <summary>
		/// Reference to current response.
		/// </summary>
		public HttpResponse CurrentResponse
		{
			get => this.currentResponse;
			set => this.currentResponse = value;
		}

		/// <summary>
		/// Reference to current page collection of variables.
		/// </summary>
		public Variables CurrentPageVariables
		{
			get => this.currentPageVariables;
			set
			{
				this.currentPageVariables = value;
				this.currentPageVariablesElement = this.currentPageVariables is null ?
					ObjectValue.Null : new ObjectValue(this.currentPageVariables);
			}
		}

		/// <summary>
		/// Script element with reference to current page collection of variables.
		/// </summary>
		public ObjectValue CurrentPageVariablesElement => this.currentPageVariablesElement;

		/// <summary>
		/// URL of current page.
		/// </summary>
		public string CurrentPageUrl
		{
			get => this.currentPageUrl;
			set => this.currentPageUrl = value;
		}

		/// <summary>
		/// Locks the collection. The collection is by default thread safe. But if longer transactions require unique access,
		/// this method can be used to aquire such unique access. This works, as long as all callers that affect the corresponding
		/// state call this method also.
		/// 
		/// Each successful call to this method must be followed by exacty one call to <see cref="Release"/>.
		/// </summary>
		/// <exception cref="TimeoutException">If access to the collection was not granted in the alotted time</exception>
		public Task LockAsync()
		{
			return this.LockAsync(30000);
		}

		/// <summary>
		/// Locks the collection. The collection is by default thread safe. But if longer transactions require unique access,
		/// this method can be used to aquire such unique access. This works, as long as all callers that affect the corresponding
		/// state call this method also.
		/// 
		/// Each successful call to this method must be followed by exacty one call to <see cref="Release"/>.
		/// </summary>
		/// <param name="Timeout">Timeout, in milliseconds. Default timeout is 30000 milliseconds (30 s).</param>
		/// <exception cref="TimeoutException">If access to the collection was not granted in the alotted time</exception>
		public async Task LockAsync(int Timeout)
		{
			if (this.Aborted)
				throw new ScriptAbortedException();

			if (!await this.semaphore.WaitAsync(Timeout))
				throw new TimeoutException("Unique access to variables connection was not granted.");
		}

		/// <summary>
		/// Releases the collection, previously locked through a call to <see cref="LockAsync()"/>.
		/// </summary>
		public void Release()
		{
			this.semaphore.Release();
		}

	}
}
