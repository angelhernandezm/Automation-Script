using System;

namespace AutomationScript {
    public partial class Engine : IDisposable {
		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="Engine"/> class.
		/// </summary>
		protected Engine() {

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Engine"/> class.
		/// </summary>
		/// <param name="hWnd">The h WND.</param>
		protected Engine(IntPtr hWnd)
			: this() {
			targetApplicationHwnd = hWnd;
		}

		#endregion

		#region Initializers

		/// <summary>
		/// Gets the instance.
		/// </summary>
		/// <param name="hWnd">The h WND.</param>
		/// <returns></returns>
		public static Engine GetInstance(IntPtr hWnd) {
			return new Engine(hWnd);
		}

		/// <summary>
		/// Gets the instance.
		/// </summary>
		/// <returns></returns>
		public static Engine GetInstance() {
			return new Engine();
		}

		/// <summary>
		/// Gets the instance.
		/// </summary>
		/// <param name="hWnd">The h WND.</param>
		/// <param name="tag">The tag.</param>
		/// <returns></returns>
		public static Engine GetInstance(IntPtr hWnd, object tag) {
			return new Engine(hWnd) {
				Tag = tag
			};
		}

		/// <summary>
		/// Gets the instance.
		/// </summary>
		/// <param name="tag">The tag.</param>
		/// <returns></returns>
		public static Engine GetInstance(object tag) {
			return new Engine() {
				Tag = tag
			};
		}

		#endregion

		#region Finalizers & destructors

		/// <summary>
		/// Releases unmanaged resources and performs other cleanup operations before the
		/// <see cref="Engine"/> is reclaimed by garbage collection.
		/// </summary>
		~Engine() {
			Dispose(false);
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		private void Dispose(bool disposing) {
			if (!disposed) {
				if (disposing) {
					if (pinnedAsyncCallback != null)
						pinnedAsyncCallback.Value.Free();

					disposed = true;

					if (Disposed != null)
						Disposed(this, new EventArgs());
				}
			}
		}

		#endregion

		#region IDisposable Members

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion

	}
}