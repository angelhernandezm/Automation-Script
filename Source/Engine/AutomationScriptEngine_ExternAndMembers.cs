using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace AutomationScript {
    public partial class Engine : IDisposable {

		#region Imported Functions

		/// <summary>
		/// Gets the length of the window text.
		/// </summary>
		/// <param name="hWnd">The h WND.</param>
		/// <returns></returns>
		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
		private static extern int GetWindowTextLength(IntPtr hWnd);

		/// <summary>
		/// Gets the window text.
		/// </summary>
		/// <param name="hWnd">The h WND.</param>
		/// <param name="lpString">The lp string.</param>
		/// <param name="nMaxCount">The n max count.</param>
		/// <returns></returns>
		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
		private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

		/// <summary>
		/// Gets the parent.
		/// </summary>
		/// <param name="hWnd">The h WND.</param>
		/// <returns></returns>
		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
		private static extern IntPtr GetParent(IntPtr hWnd);

		/// <summary>
		/// Enums the child windows.
		/// </summary>
		/// <param name="hWndParent">The h WND parent.</param>
		/// <param name="lpEnumFunc">The lp enum func.</param>
		/// <param name="lParam">The l param.</param>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Bool)]
		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
		private static extern bool EnumChildWindows(IntPtr hWndParent, EnumChildProc lpEnumFunc, int lParam);

		/// <summary>
		/// Determines whether the specified h WND is window.
		/// </summary>
		/// <param name="hWnd">The h WND.</param>
		/// <returns>
		/// 	<c>true</c> if the specified h WND is window; otherwise, <c>false</c>.
		/// </returns>
		[return: MarshalAs(UnmanagedType.Bool)]
		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
		private static extern bool IsWindow(IntPtr hWnd);

		/// <summary>
		/// Sends the message callback.
		/// </summary>
		/// <param name="hWnd">The h WND.</param>
		/// <param name="Msg">The MSG.</param>
		/// <param name="wParam">The w param.</param>
		/// <param name="lParam">The l param.</param>
		/// <param name="lpCallback">The lp callback.</param>
		/// <param name="dwData">The dw data.</param>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Bool)]
		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
		private static extern bool SendMessageCallback(IntPtr hWnd, uint Msg, uint wParam, int lParam, SendAsyncProc lpCallback, UIntPtr dwData);

		/// <summary>
		/// Sends the message.
		/// </summary>
		/// <param name="hWnd">The h WND.</param>
		/// <param name="Msg">The MSG.</param>
		/// <param name="wParam">The w param.</param>
		/// <param name="lParam">The l param.</param>
		/// <returns></returns>
		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
		private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

		/// <summary>
		/// Gets the window.
		/// </summary>
		/// <param name="hwnd">The HWND.</param>
		/// <param name="flag">The flag.</param>
		/// <returns></returns>
		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
		private extern static IntPtr GetWindow(IntPtr hwnd, int flag);

		/// <summary>
		/// Enums the windows.
		/// </summary>
		/// <param name="lpEnumFunc">The lp enum func.</param>
		/// <param name="lParam">The l param.</param>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Bool)]
		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
		private static extern bool EnumWindows(EnumChildProc lpEnumFunc, int lParam);

		/// <summary>
		/// Brings the window to top.
		/// </summary>
		/// <param name="hWnd">The h WND.</param>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Bool)]
		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
		private static extern bool BringWindowToTop(IntPtr hWnd);

		/// <summary>
		/// Gets the name of the class.
		/// </summary>
		/// <param name="hWnd">The h WND.</param>
		/// <param name="lpClassName">Name of the lp class.</param>
		/// <param name="nMaxCount">The n max count.</param>
		/// <returns></returns>
		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
		private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

		/// <summary>
		/// Gets the window thread process id.
		/// </summary>
		/// <param name="hWnd">The h WND.</param>
		/// <param name="lpdwProcessId">The LPDW process id.</param>
		/// <returns></returns>
		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
		private static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

		/// <summary>
		/// Determines whether [is window visible] [the specified h WND].
		/// </summary>
		/// <param name="hWnd">The h WND.</param>
		/// <returns>
		/// 	<c>true</c> if [is window visible] [the specified h WND]; otherwise, <c>false</c>.
		/// </returns>
		[return: MarshalAs(UnmanagedType.Bool)]
		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
		private static extern bool IsWindowVisible(IntPtr hWnd);

		/// <summary>
		/// Gets the ancestor.
		/// </summary>
		/// <param name="hWnd">The h WND.</param>
		/// <param name="gaFlags">The ga flags.</param>
		/// <returns></returns>
		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
		private static extern IntPtr GetAncestor(IntPtr hWnd, uint gaFlags);

		/// <summary>
		/// Gets the top window.
		/// </summary>
		/// <param name="hWnd">The h WND.</param>
		/// <returns></returns>
		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
		private static extern IntPtr GetTopWindow(IntPtr hWnd);

		/// <summary>
		/// Destroys the window.
		/// </summary>
		/// <param name="hWnd">The h WND.</param>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Bool)]
		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
		private static extern bool DestroyWindow(IntPtr hWnd);

		/// <summary>
		/// Ends the dialog.
		/// </summary>
		/// <param name="hWnd">The h WND.</param>
		/// <param name="nResult">The n result.</param>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Bool)]
		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
		private static extern bool EndDialog(IntPtr hWnd, IntPtr nResult);

		/// <summary>
		/// Gets the window long.
		/// </summary>
		/// <param name="hWnd">The h WND.</param>
		/// <param name="nIndex">Index of the n.</param>
		/// <returns></returns>
		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
		private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

		/// <summary>
		/// Posts the message.
		/// </summary>
		/// <param name="hWnd">The h WND.</param>
		/// <param name="Msg">The MSG.</param>
		/// <param name="wParam">The w param.</param>
		/// <param name="lParam">The l param.</param>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Bool)]
		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
		private static extern bool PostMessage(IntPtr hWnd, uint Msg, uint wParam, int lParam);

		#endregion

		#region Members

		// Consts

		private const int GW_OWNER = 4;
		private const int GW_CHILD = 5;
		private const int WM_PASTE = 770;
		private const int GW_HWNDNEXT = 2;
		private const int GW_HWNDLAST = 1;
		private const int GW_HWNDPREV = 3;
		private const int GW_HWNDFIRST = 0;
		private const uint WM_GETTEXT = 13;
		private const string ZORDER = "Next Previous First Last Owner";
		private const uint GA_PARENT = 1;
		private const uint GA_ROOT = 2;
		private const uint GA_ROOTOWNER = 3;
		private const string DIALOG_CLASS_NAME = "#32770";
		private const int GWL_STYLE = -16;
		private const int WM_CLOSE = 16;
		private const int WM_DESTROY = 2;
		private const int WM_QUERYENDSESSION = 17;
		private const int WM_KEYDOWN = 256;
		private const int LPARAM_VK_ESCAPE = 65537;


		// Fields

		private int stepCount = 0;
		private object tag = null;
		private int totalSteps = 0;
		private bool disposed = false;
		private int currentProcessId = 0;
		private string autoScriptVersion = "1.0";
		private GCHandle? pinnedAsyncCallback = null;
		private IntPtr childElementHandle = IntPtr.Zero;
		private IntPtr targetApplicationHwnd = IntPtr.Zero;

		private Dictionary<string, int> windowZOrder = new Dictionary<string, int>()
        {
            {"Next", GW_HWNDNEXT}, {"Previous", GW_HWNDPREV}, {"First", GW_HWNDFIRST}, 
            {"Last", GW_HWNDLAST}, {"Owner", GW_OWNER}, {"FirstChild", GW_CHILD}
        };


		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the total steps.
		/// </summary>
		/// <value>The total steps.</value>
		protected int TotalSteps {
			get {
				return totalSteps;
			}
			set {
				totalSteps = value;
			}
		}

		/// <summary>
		/// Gets or sets the step count.
		/// </summary>
		/// <value>The step count.</value>
		protected int StepCount {
			get {
				return stepCount;
			}
			set {
				stepCount = value;
			}
		}

		/// <summary>
		/// Gets or sets the child element handle.
		/// </summary>
		/// <value>The child element handle.</value>
		protected IntPtr ChildElementHandle {
			get {
				return childElementHandle;
			}
			set {
				childElementHandle = value;
			}
		}

		/// <summary>
		/// Gets or sets the current process id.
		/// </summary>
		/// <value>The current process id.</value>
		protected int CurrentProcessId {
			get {
				return currentProcessId;
			}
			set {
				currentProcessId = value;
			}
		}


		/// <summary>
		/// Gets or sets the auto script version.
		/// </summary>
		/// <value>The auto script version.</value>
		public string AutoScriptVersion {
			get {
				return autoScriptVersion;
			}
			set {
				autoScriptVersion = value;
			}
		}

		/// <summary>
		/// Gets or sets the tag.
		/// </summary>
		/// <value>The tag.</value>
		public object Tag {
			get {
				return tag;
			}
			set {
				tag = value;
			}
		}


		/// <summary>
		/// Gets the target application main window HWND.
		/// </summary>
		/// <value>The target application main window HWND.</value>
		public IntPtr TargetApplicationMainWindowHwnd {
			get {
				return targetApplicationHwnd;
			}
		}

		/// <summary>
		/// Gets or sets the window closed on failure.
		/// </summary>
		/// <value>The window closed on failure.</value>
		public string WindowClosedOnFailure {
			get;
			set;
		}

		#endregion

		#region Indexers

		/// <summary>
		/// Gets the <see cref="System.Int32"/> with the specified key.
		/// </summary>
		/// <value></value>
		public int this[string key] {
			get {
				int retval = -1;

				if (windowZOrder.ContainsKey(key))
					retval = windowZOrder[key];

				return retval;
			}
		}

		#endregion

		#region Enums

		public enum ConditionType : int {
			None = 0,
			Dialog = 1
		}

		public enum BehaviorType : int {
			None = 0,
			Dismiss = 1,
			Show
		}

		public enum StepAction : int {
			None = 0,
			KeyStroke = 1
		}


		public enum VirtualKeyCode : int {
			V = 86,
			None = 0,
			VK_DOWN = 40,
			VK_SPACE = 32,
			VK_ESCAPE = 27,
			VK_CONTROL = 17,
			VK_ENTER = 13
		}

		#endregion
	}
}