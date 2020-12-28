using System;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Async {

	///<summary>A lock object that can be initialized to do async tasks within a lock statement.</summary>
	public class AsyncLock {

		///<summary>The lock that can be used to wait asynchrounously.</summary>
		private readonly SemaphoreSlim _semaphore;

		public AsyncLock() {
			_semaphore=new SemaphoreSlim(1, 1);
		}

		///<summary>Waits to lock the async lock. Once it is locked, executes the passed in worker.</summary>
		public async Task Lock(Action worker) {
			await _semaphore.WaitAsync();
			try {
				worker();
			}
			finally {
				_semaphore.Release();
			}
		}

		[Obsolete("Cannot pass an async task into the synchronous lock method.")]
		public Task Lock(Func<Task> worker) {
			throw new NotImplementedException($"Use {nameof(LockAsync)} with async code.");
		}

		///<summary>Waits to lock the async lock. Once it is locked, executes the passed in worker.</summary>
		public async Task LockAsync(Func<Task> worker) {
			await _semaphore.WaitAsync();
			try {
				await worker();
			}
			finally {
				_semaphore.Release();
			}
		}

	}
}
