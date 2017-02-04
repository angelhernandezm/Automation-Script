
using System;

namespace AutomationScript {
    public partial class Engine : IDisposable {
		#region Delegates

		public delegate void GenericDelegate<T, V>(T t, V v);
		public delegate void AutomationDelegate(IntPtr hWnd, object args);
		private delegate bool EnumChildProc(IntPtr hWnd, int lParam);
		private delegate void SendAsyncProc(IntPtr hWnd, uint Msg, UIntPtr dwData, IntPtr lResult);

		#endregion

		#region Events

		/// <summary>
		/// Occurs when [disposed].
		/// </summary>
		public event EventHandler Disposed;

		/// <summary>
		/// Occurs when [on automation executed].
		/// </summary>
		public event GenericDelegate<uint, IntPtr> OnAutomationExecuted;

		/// <summary>
		/// Occurs when [on automation failed].
		/// </summary>
		public event GenericDelegate<AutomationStep, Exception> OnAutomationFailed;


		/// <summary>
		/// Occurs when [on message sent completed].
		/// </summary>
		public event GenericDelegate<IntPtr, int> OnMessageSentCompleted;

		/// <summary>
		/// Occurs when [on script stopped].
		/// </summary>
		public event GenericDelegate<object, AutomationStep> OnScriptStopped;

		#endregion
	}
}