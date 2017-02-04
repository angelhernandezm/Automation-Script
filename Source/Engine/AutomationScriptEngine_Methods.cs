using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Reflection;

namespace AutomationScript {
    public partial class Engine : IDisposable {
		#region Public methods

		/// <summary>
		/// Runs the automation script.
		/// </summary>
		/// <param name="autoScript">The auto script.</param>
		/// <param name="targetHwnd">The target HWND.</param>
		/// <param name="code">The code.</param>
		public void RunAutomationScript(XDocument autoScript, IntPtr targetHwnd, AutomationDelegate code) {
			// Should we consider the hWnd passed or the class' one?
			IntPtr hWnd = targetHwnd != IntPtr.Zero ? targetHwnd : TargetApplicationMainWindowHwnd;

			// Are we Ok to go?
			if (autoScript != null && IsVersionOk(autoScript))
				ParseAndExecuteScript(autoScript, hWnd, code);
		}


		#endregion

		#region Private methods

		/// <summary>
		/// Determines whether [is version ok] [the specified auto script].
		/// </summary>
		/// <param name="autoScript">The auto script.</param>
		/// <returns>
		/// 	<c>true</c> if [is version ok] [the specified auto script]; otherwise, <c>false</c>.
		/// </returns>
		private bool IsVersionOk(XDocument autoScript) {
			bool retval = false;
			XElement version = null;
			XAttribute versionNumber = null;

			if (autoScript != null && (version = autoScript.Element("autoscript")) != null)
				retval = (versionNumber = version.Attribute("version")) != null ?
					versionNumber.Value.Equals(AutoScriptVersion, StringComparison.InvariantCultureIgnoreCase) : false;

			return retval;
		}

		/// <summary>
		/// Gets the control or window text.
		/// </summary>
		/// <param name="hWnd">The h WND.</param>
		/// <returns></returns>
		private string GetControlOrWindowText(IntPtr hWnd) {
			string retval = string.Empty;
			IntPtr buffer = Marshal.AllocHGlobal(1024); // Store up to 255 characters
			SendMessage(hWnd, WM_GETTEXT, new IntPtr(1024), buffer);
			retval = Marshal.PtrToStringUni(buffer);
			Marshal.FreeHGlobal(buffer);

			return retval;
		}


		/// <summary>
		/// Gets the HWND for step.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		private static IntPtr GetHwndForStep(string value) {
			int parseTestInt = 0;
			IntPtr retval = IntPtr.Zero;

			if (int.TryParse(value, out parseTestInt)) {
				retval = new IntPtr(parseTestInt);
			} else {
				var selectedProcess = Process.GetProcessesByName(value).ToList();

				if (selectedProcess != null && selectedProcess.Count > 0)
					retval = selectedProcess[0].MainWindowHandle;
			}
			return retval;
		}

		/// <summary>
		/// Extracts the attributes.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="elements">The elements.</param>
		/// <param name="mapping">The mapping.</param>
		/// <returns></returns>
		private static List<object> ExtractAttributes<T>(IEnumerable<XElement> elements, Dictionary<string, string> mapping) {
			Type currentType = null;
			object newObject = null;
			XAttribute selected = null;
			object propertyValue = null;
			PropertyInfo propInfo = null;
			List<object> retval = new List<object>();

			BindingFlags flags = BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.NonPublic |
								 BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty;

			if (elements != null && (mapping != null && mapping.Count > 0)) {
				elements.ToList().ForEach(element => {
					newObject = typeof(T).GetConstructor(Type.EmptyTypes).Invoke(null);
					currentType = newObject.GetType();
					mapping.ToList().ForEach(item => {
						if ((selected = element.Attribute(item.Key)) != null && !string.IsNullOrEmpty(selected.Value)) {
							propInfo = currentType.GetProperty(item.Value, flags);
							propertyValue = AutomationStep.GetSafePropertyValue(propInfo, selected.Value);
							propInfo.SetValue(newObject, propertyValue, null);
						}
					});
					retval.Add(newObject);
				});
			}
			return retval;
		}

		/// <summary>
		/// Determines whether [is prequisite required] [the specified step].
		/// </summary>
		/// <param name="step">The step.</param>
		/// <param name="conditions">The conditions.</param>
		/// <param name="fors">The fors.</param>
		/// <returns>
		/// 	<c>true</c> if [is prequisite required] [the specified step]; otherwise, <c>false</c>.
		/// </returns>
		private static bool IsPrequisiteRequired(AutomationStep step, out List<Condition> conditions, out List<For> fors) {
			fors = null;
			conditions = null;
			bool retval = false;
			XElement preReqs = null;

			if (step.StepAsXml != null && (preReqs = step.StepAsXml.Element("preRequisites")) != null) {
				conditions = ExtractAttributes<Condition>(step.StepAsXml.Element("preRequisites").Elements("condition"),
							 new Dictionary<string, string>() 
                             {
                                 {"type","Type"}, {"behavior","Behavior"}, 
                                 {"retries","MaximumRetries"}, {"timeInterval","TimeInterval"}
                             }).Cast<Condition>().ToList();

				fors = ExtractAttributes<For>(step.StepAsXml.Element("preRequisites").Elements("for"),
						   new Dictionary<string, string>()
                           {
                               {"stepCount","StepCount"}, {"stepAction","Action"}, {"virtualKeyCode","KeyCode"}
                           }).Cast<For>().ToList();

				retval = conditions.Count > 0 || fors.Count > 0;
			}

			return retval;
		}

		/// <summary>
		/// Applies the rules to step.
		/// </summary>
		/// <param name="selectedStep">The selected step.</param>
		/// <returns></returns>
		private AutomationStep ApplyRulesToStep(AutomationStep selectedStep) {
			int messageRecipient = 0;
			AutomationStep retval = selectedStep;

			// Is clipboard required?
			if (!string.IsNullOrEmpty(retval.ClipboardData)) {
				// Fixing issue 2856 - Invoking dispatcher
				if (System.Windows.Application.Current != null) {
					System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate {
						Clipboard.SetText(retval.ClipboardData);
					});
				} else
					Clipboard.SetText(retval.ClipboardData);
			}

			// is there a pathOrTitle attribute?
			if (!string.IsNullOrEmpty(retval.PathOrTitle))
				ProcessPathOrTitle(ref retval);

			// If there's a targetClass specified then it takes precedence over a childElement (e.g.: Grid, options group, panel)
			// This is useful in situations where a caption/title doesn't exist and it's mutually exclusive, in other words,
			// childElement and targetClass can't be used together, but targetClass can be used with sendMessageTo (Z-order)
			if (!string.IsNullOrEmpty(retval.TargetClass)) {
				FindStepWindowByClassName(ref retval, retval.TargetClass);

				// Should we send the message to the found window or any other in the Z-order?
				if (!string.IsNullOrEmpty(retval.SendMessageTo) && (messageRecipient = this[retval.SendMessageTo]) > -1)
					retval.hWnd = GetWindow(retval.hWnd, messageRecipient);
			} else {
				// Is there a childElement attribute?
				if (!string.IsNullOrEmpty(retval.ChildElement)) {
					ProcessChildElement(ref retval);

					// Is there any data that need to be pasted into a child element?
					if (!string.IsNullOrEmpty(retval.ClipboardData))
						FindStepWindowByClassName(ref retval, "Edit");
				} else if (!string.IsNullOrEmpty(retval.ClipboardData)) // Find the first textbox available
					FindStepWindowByClassName(ref retval, "Edit");
			}

			return retval;
		}


		/// <summary>
		/// Sends the key.
		/// </summary>
		/// <param name="hWnd">The h WND.</param>
		/// <param name="msgs">The MSGS.</param>
		/// <param name="keyCode">The key code.</param>
		/// <param name="sendAsyncProc">The send async proc.</param>
		private void SendKey(IntPtr hWnd, List<int> msgs, uint keyCode, UIntPtr sendAsyncProc) {
			if (msgs != null && msgs.Count > 0) {
				BringWindowToTop(hWnd);

				msgs.ForEach(msg => SendMessageCallback(hWnd, (uint)msg, keyCode,
									(int)(msg.Equals(256) ? 22020097 : 3243245569),
									(SendAsyncProc)pinnedAsyncCallback.Value.Target, sendAsyncProc));
			}
		}

		/// <summary>
		/// Sends the space bar.
		/// </summary>
		/// <param name="hWnd">The h WND.</param>
		/// <param name="sendAsyncProc">The send async proc.</param>
		private void SendSpaceBar(IntPtr hWnd, UIntPtr sendAsyncProc) {
			Dictionary<int, List<long>> sequence = new Dictionary<int, List<long>>()
            {
                // Message, wParam, lParam
                {256, new List<long>() {32, 3735553}}, // WM_KEYDOWN
                {258, new List<long>() {32, 3735553}}, // WM_CHAR
                {257, new List<long>() {32, 3224961025}} // WM_KEYUP
            };

			BringWindowToTop(hWnd);

			sequence.ToList().ForEach(seq => SendMessageCallback(hWnd, (uint)seq.Key, (uint)seq.Value[0],
			   (int)seq.Value[1], (SendAsyncProc)pinnedAsyncCallback.Value.Target,
			   (seq.Key.Equals(257) ? sendAsyncProc : UIntPtr.Zero)));
		}

		/// <summary>
		/// Evaluates the conditions.
		/// </summary>
		/// <param name="hWnd">The h WND.</param>
		/// <param name="conditions">The conditions.</param>
		/// <returns></returns>
		private bool EvaluateConditions(IntPtr hWnd, List<Condition> conditions) {
			bool retval = false;

			if (conditions != null && conditions.Count > 0) {
				conditions.TakeWhile(x => !retval).ToList().ForEach(condition => {
					switch (condition.Type) {
						case ConditionType.None:
							break;
						case ConditionType.Dialog:
							retval = EvaluateConditionForDialog(hWnd, condition);
							break;
					}
				});
			}
			return retval;
		}

		/// <summary>
		/// Evaluates the condition for dialog.
		/// </summary>
		/// <param name="hWnd">The h WND.</param>
		/// <param name="condition">The condition.</param>
		/// <returns></returns>
		private bool EvaluateConditionForDialog(IntPtr hWnd, Condition condition) {
			bool retval = false;
			IntPtr dialogHwnd = IntPtr.Zero;

			// Valid condition check found?
			if (condition.MaximumRetries > 0 && condition.TimeInterval > 0) {
				for (int nRetry = 0; nRetry < condition.MaximumRetries; nRetry++) {
					if (condition.Behavior.Equals(BehaviorType.Show))
						retval = IsADialogPresent(hWnd, out dialogHwnd);
					else if (condition.Behavior.Equals(BehaviorType.Dismiss))
						retval = !IsADialogPresent(hWnd, out dialogHwnd);

					if (retval) // Has condition been met?
						break;
					else // Otherwise, let's try again after the specified time has elapsed
						Thread.Sleep(condition.TimeInterval);
				}
			}
			return retval;
		}

		/// <summary>
		/// Restores the layout.
		/// </summary>
		/// <param name="hWnd">The h WND.</param>
		/// <param name="windowToCloseOnFailure">The window to close on failure.</param>
		private void RestoreLayout(IntPtr hWnd, string windowToCloseOnFailure) {
			IntPtr mainHwnd = IntPtr.Zero, dialogHwnd = IntPtr.Zero;
			IntPtr windowToClosePtr = Marshal.StringToBSTR(windowToCloseOnFailure);

			// If there's a modal dialog, we dismiss it then
			if (IsADialogPresent(hWnd, out dialogHwnd) && dialogHwnd != IntPtr.Zero)
				PostMessage(dialogHwnd, WM_KEYDOWN, (uint)VirtualKeyCode.VK_ESCAPE, LPARAM_VK_ESCAPE);

			if ((mainHwnd = GetAncestor(hWnd, GA_ROOTOWNER)) != IntPtr.Zero)
				EnumChildWindows(mainHwnd, CloseWindowWhenRestoringLayout, windowToClosePtr.ToInt32());

			Marshal.FreeBSTR(windowToClosePtr);
		}

		/// <summary>
		/// Closes the window when restoring layout.
		/// </summary>
		/// <param name="hWnd">The h WND.</param>
		/// <param name="lParam">The l param.</param>
		/// <returns></returns>
		private bool CloseWindowWhenRestoringLayout(IntPtr hWnd, int lParam) {
			bool retval = true;
			string windowCaption = string.Empty;
			string windowToClose = Marshal.PtrToStringUni(new IntPtr(lParam));

			// Let's process visible windows that belong to the automated application only
			if (IsWindowVisible(hWnd) && !string.IsNullOrEmpty((windowCaption = GetControlOrWindowText(hWnd))) &&
				 windowToClose.Equals(windowCaption, StringComparison.InvariantCultureIgnoreCase) && pinnedAsyncCallback.HasValue) {
				SendMessageCallback(hWnd, WM_CLOSE, 0, 0, (SendAsyncProc)pinnedAsyncCallback.Value.Target, UIntPtr.Zero);
				SendMessageCallback(hWnd, WM_QUERYENDSESSION, 0, 0, (SendAsyncProc)pinnedAsyncCallback.Value.Target, UIntPtr.Zero);
				retval = false;
			}
			return retval;
		}

		/// <summary>
		/// Determines whether [is A dialog present] [the specified h WND].
		/// </summary>
		/// <param name="hWnd">The h WND.</param>
		/// <param name="dialogHwnd">The dialog HWND.</param>
		/// <returns>
		/// 	<c>true</c> if [is A dialog present] [the specified h WND]; otherwise, <c>false</c>.
		/// </returns>
		private bool IsADialogPresent(IntPtr hWnd, out IntPtr dialogHwnd) {
			int processId = 0;
			bool retval = false;
			dialogHwnd = IntPtr.Zero;

			// We have to process hWnd that belong to the same process
			GetWindowThreadProcessId(hWnd, out processId);
			CurrentProcessId = processId;

			// Is it a top-level window?
			IntPtr classPtr = Marshal.StringToBSTR(DIALOG_CLASS_NAME);
			EnumWindows(EnumChildWindowProcedureByClassName, classPtr.ToInt32());
			Marshal.FreeBSTR(classPtr);

			if (ChildElementHandle != IntPtr.Zero) {
				retval = true;
				dialogHwnd = ChildElementHandle;
			}

			// Is it a child window?
			if (!retval) {
				classPtr = Marshal.StringToBSTR(DIALOG_CLASS_NAME);
				EnumChildWindows(hWnd, EnumChildWindowProcedureByClassName, classPtr.ToInt32());
				Marshal.FreeBSTR(classPtr);

				if (ChildElementHandle != IntPtr.Zero) {
					retval = true;
					dialogHwnd = ChildElementHandle;
				}
			}
			CurrentProcessId = 0;
			ChildElementHandle = IntPtr.Zero;

			return retval;
		}



		/// <summary>
		/// Executes for.
		/// </summary>
		/// <param name="hWnd">The h WND.</param>
		/// <param name="fors">The fors.</param>
		private void ExecuteFor(IntPtr hWnd, List<For> fors) {
			List<int> msgs = null;

			if (fors != null && fors.Count > 0) {
				fors.ForEach(loop => {
					BringWindowToTop(hWnd);
					for (int nLoop = 0; nLoop < loop.StepCount; nLoop++) {
						if (loop.Action.Equals(StepAction.KeyStroke)) // Keystroke 
                        {
							// Let's grab the message translation based on the action
							if ((msgs = ActionMessageTranslation.GetMessageMapping(loop.Action)) != null && msgs.Count > 0)
								msgs.ForEach(msg => SendMessageCallback(hWnd, (uint)msg, (uint)loop.KeyCode,
													(int)(msg.Equals(256) ? 22020097 : (msg.Equals(257) ? 3243245569 : 1835009)),
													(SendAsyncProc)pinnedAsyncCallback.Value.Target, UIntPtr.Zero));
						}
					}
				});
			}
		}


		/// <summary>
		/// Processes the child element.
		/// </summary>
		/// <param name="step">The step.</param>
		private void ProcessChildElement(ref AutomationStep step) {
			FindStepWindow(ref step, step.ChildElement);
		}

		/// <summary>
		/// Gets the first name of the control by class.
		/// </summary>
		/// <param name="step">The step.</param>
		/// <param name="className">Name of the class.</param>
		private void GetFirstControlByClassName(ref AutomationStep step, string className) {
			FindStepWindowByClassName(ref step, className);
		}

		/// <summary>
		/// Determines whether [is A valid Z order path] [the specified path].
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns>
		/// 	<c>true</c> if [is A valid Z order path] [the specified path]; otherwise, <c>false</c>.
		/// </returns>
		private bool IsAValidZOrderPath(string path) {
			bool retval = false;
			string[] splitPath = null;

			if (!string.IsNullOrEmpty(path) && (splitPath = path.Split(';')) != null && splitPath.Length > 0) {
				var pathQry = from values in windowZOrder.Keys.Where(value => splitPath.ToList().Contains(value))
							  select values;

				retval = pathQry.Count() > 0;
			}
			return retval;
		}

		/// <summary>
		/// Processes the path or title.
		/// </summary>
		/// <param name="step">The step.</param>
		private void ProcessPathOrTitle(ref AutomationStep step) {
			string[] splitPath = null;
			IntPtr hWnd = IntPtr.Zero;

			// Is it a path (Z-Order based) or Title?
			if (IsAValidZOrderPath(step.PathOrTitle)) {
				if ((splitPath = step.PathOrTitle.Split(';')) != null && splitPath.Length > 0) {
					hWnd = step.hWnd;
					splitPath.ToList().ForEach(part => {
						if (windowZOrder.ContainsKey(part))
							hWnd = GetWindow(hWnd, windowZOrder[part]);
					});
					step.hWnd = hWnd;
				}
			} else {
				FindStepWindow(ref step, step.PathOrTitle);
			}
		}

		/// <summary>
		/// Finds the name of the step window by class.
		/// </summary>
		/// <param name="step">The step.</param>
		/// <param name="className">Name of the class.</param>
		private void FindStepWindowByClassName(ref AutomationStep step, string className) {
			int processId = 0;
			bool found = false;

			// We have to process hWnd that belong to the same process
			GetWindowThreadProcessId(step.hWnd, out processId);
			CurrentProcessId = processId;

			// Is it a top-level window?
			IntPtr classPtr = Marshal.StringToBSTR(className);
			EnumWindows(EnumChildWindowProcedureByClassName, classPtr.ToInt32());
			Marshal.FreeBSTR(classPtr);

			if (ChildElementHandle != IntPtr.Zero) {
				found = true;
				step.hWnd = ChildElementHandle;
			}

			// Is it a child window?
			if (!found) {
				classPtr = Marshal.StringToBSTR(className);
				EnumChildWindows(step.hWnd, EnumChildWindowProcedureByClassName, classPtr.ToInt32());
				Marshal.FreeBSTR(classPtr);

				if (ChildElementHandle != IntPtr.Zero)
					step.hWnd = ChildElementHandle;
			}
			CurrentProcessId = 0;
			ChildElementHandle = IntPtr.Zero;
		}

		/// <summary>
		/// Finds the step window.
		/// </summary>
		/// <param name="step">The step.</param>
		/// <param name="searchExpr">The search expr.</param>
		private void FindStepWindow(ref AutomationStep step, string searchExpr) {
			bool found = false;

			// Is it a top-level window?
			IntPtr namePtr = Marshal.StringToBSTR(searchExpr);
			EnumWindows(EnumChildWindowProcedure, namePtr.ToInt32());
			Marshal.FreeBSTR(namePtr);

			if (ChildElementHandle != IntPtr.Zero) {
				found = true;
				step.hWnd = ChildElementHandle;
			}

			// Is it a child Window?
			if (!found) {
				namePtr = Marshal.StringToBSTR(searchExpr);
				EnumChildWindows(step.hWnd, EnumChildWindowProcedure, namePtr.ToInt32());
				Marshal.FreeBSTR(namePtr);

				if (ChildElementHandle != IntPtr.Zero)
					step.hWnd = ChildElementHandle;
			}
			ChildElementHandle = IntPtr.Zero;
		}

		/// <summary>
		/// Dispatches the message.
		/// </summary>
		/// <param name="autoScript">The auto script.</param>
		/// <param name="targetHwnd">The target HWND.</param>
		/// <param name="code">The code.</param>
		private void DispatchMessage(XDocument autoScript, IntPtr targetHwnd, AutomationDelegate code) {
			List<For> fors = null;
			XElement script = null;
			bool canContinue = true;
			List<Condition> conditions = null;
			UIntPtr ptrCodeToRun = UIntPtr.Zero;
			IEnumerable<AutomationStep> steps = null;
			AutomationStep step = AutomationStep.Empty;

			// Let's extract the automation steps
			if ((script = autoScript.Element("autoscript")) != null &&
				(steps = AutomationStep.GetScriptSteps(autoScript)) != null && (TotalSteps = steps.Count()) > 0) {
				// Let's grab the address for our callback to be executed at the end
				ptrCodeToRun = new UIntPtr((uint)Marshal.GetFunctionPointerForDelegate(code).ToInt32());

				// Let's pin our async callback
				if (pinnedAsyncCallback == null)
					pinnedAsyncCallback = GCHandle.Alloc(new SendAsyncProc(SendAsyncProcedure));

				foreach (var current in steps) {
					try {
						++StepCount; // Increase step counter

						// I had to refactor this method (well, VS did it) because it was too long and it could 
						// be really cumbersome to maintain (That's the reason for passing by ref of some variables)
						step = PrepareStep(targetHwnd, canContinue, step, current);
						CheckPreRequisites(ref fors, ref canContinue, ref conditions, step);
						ExecuteStep(canContinue, ptrCodeToRun, step);

						if (!canContinue)  // If the automation failed we just exit
							break;
					} catch (Exception ex) {
						LogErrorHelper(step, ex);
					}
				}
			} else {
				LogErrorHelper(step, new Exception("Steps weren't found to execute any automation"));
			}
		}

		/// <summary>
		/// Prepares the step.
		/// </summary>
		/// <param name="targetHwnd">The target HWND.</param>
		/// <param name="canContinue">if set to <c>true</c> [can continue].</param>
		/// <param name="step">The step.</param>
		/// <param name="current">The current.</param>
		/// <returns></returns>
		private AutomationStep PrepareStep(IntPtr targetHwnd, bool canContinue, AutomationStep step, AutomationStep current) {
			if (canContinue) {
				// If the script has a hWnd it takes precedence before the one passed
				if (current.hWnd == IntPtr.Zero)
					current.hWnd = targetHwnd;

				//Apply rules to this step
				step = ApplyRulesToStep(current);
			}
			return step;
		}

		/// <summary>
		/// Checks the pre requisites.
		/// </summary>
		/// <param name="fors">The fors.</param>
		/// <param name="canContinue">if set to <c>true</c> [can continue].</param>
		/// <param name="conditions">The conditions.</param>
		/// <param name="step">The step.</param>
		private void CheckPreRequisites(ref List<For> fors, ref bool canContinue, ref List<Condition> conditions, AutomationStep step) {
			// Is there any pre-requisite?
			if (IsPrequisiteRequired(step, out conditions, out fors)) {
				ExecuteFor(step.hWnd, fors); // Execute For (takes precedence before conditions)

				// Conditions have been met?
				if (!(canContinue = EvaluateConditions(step.hWnd, conditions)) && step.ExitAutomationOnFailure) {
					if (!string.IsNullOrEmpty(WindowClosedOnFailure)) // Should we restore the main window layout?
						RestoreLayout(step.hWnd, WindowClosedOnFailure);

					if (OnScriptStopped != null) // Fire event
						OnScriptStopped(this, step);
				} else
					canContinue = true; // If the ExitAutomationOnFailure hasn't been specified then continue
			}
		}

		/// <summary>
		/// Executes the step.
		/// </summary>
		/// <param name="canContinue">if set to <c>true</c> [can continue].</param>
		/// <param name="ptrCodeToRun">The PTR code to run.</param>
		/// <param name="step">The step.</param>
		private void ExecuteStep(bool canContinue, UIntPtr ptrCodeToRun, AutomationStep step) {
			if (canContinue) {
				// Is this step disabled?
				if (!step.IsDisabled) {
					// Let's asynchronously send the message
					BringWindowToTop(step.hWnd);

					// If we don't have any StepAction, we just send a message
					if (step.Action.Equals(StepAction.None)) {
						SendMessageCallback(step.hWnd, step.Msg, step.wParam, step.lParam,
										   (SendAsyncProc)pinnedAsyncCallback.Value.Target, ptrCodeToRun);
					} else if (step.Action.Equals(StepAction.KeyStroke)) {
						if (step.KeyCode.Equals(VirtualKeyCode.VK_SPACE))
							SendSpaceBar(step.hWnd, ptrCodeToRun);
						else
							SendKey(step.hWnd, ActionMessageTranslation.GetMessageMapping(step.Action),
								   (uint)step.KeyCode, ptrCodeToRun);
					}

					// Should we notify about this step being executed?
					if (OnAutomationExecuted != null)
						OnAutomationExecuted(step.Msg, step.hWnd);

					// Should we pause/hold the execution before processing the next step?
					if (step.Delay > 0)
						Thread.Sleep(step.Delay);
				}
			}
		}


		/// <summary>
		/// Logs the error helper.
		/// </summary>
		/// <param name="step">The step.</param>
		/// <param name="ex">The ex.</param>
		private void LogErrorHelper(AutomationStep step, Exception ex) {
			if (OnAutomationFailed != null)
				OnAutomationFailed(step, ex);
		}


		/// <summary>
		/// Sends the async procedure.
		/// </summary>
		/// <param name="hWnd">The h WND.</param>
		/// <param name="Msg">The MSG.</param>
		/// <param name="dwData">The dw data.</param>
		/// <param name="lResult">The l result.</param>
		private void SendAsyncProcedure(IntPtr hWnd, uint Msg, UIntPtr dwData, IntPtr lResult) {
			AutomationDelegate codeToRun = null;

			if (dwData != UIntPtr.Zero) // This apply to steps only
            {
				if (TotalSteps > 0 && StepCount.Equals(TotalSteps)) {
					if ((codeToRun = (AutomationDelegate)Marshal.GetDelegateForFunctionPointer(
							new IntPtr(dwData.ToUInt32()), typeof(AutomationDelegate))) != null)
						codeToRun.Invoke(hWnd, Tag);
					StepCount = TotalSteps = 0;
				}
				// Let's fire OnMessageSentCompleted event
				if (OnMessageSentCompleted != null)
					OnMessageSentCompleted(hWnd, lResult.ToInt32());
			}
		}

		/// <summary>
		/// Enums the child window procedure.
		/// </summary>
		/// <param name="hWnd">The h WND.</param>
		/// <param name="lParam">The l param.</param>
		/// <returns></returns>
		private bool EnumChildWindowProcedure(IntPtr hWnd, int lParam) {
			bool retval = true;
			IntPtr search = new IntPtr(lParam);
			string caption = GetControlOrWindowText(hWnd);
			string searchCaption = Marshal.PtrToStringAuto(search);

			// We remove the hotkey from the control (if any)
			if (caption.IndexOf('&') >= 0)
				caption = caption.Replace("&", string.Empty);

			if (searchCaption.Equals(caption, StringComparison.InvariantCultureIgnoreCase)) {
				ChildElementHandle = hWnd;
				retval = false;
			}
			return retval;
		}

		/// <summary>
		/// Enums the name of the child window procedure by class.
		/// </summary>
		/// <param name="hWnd">The h WND.</param>
		/// <param name="lParam">The l param.</param>
		/// <returns></returns>
		private bool EnumChildWindowProcedureByClassName(IntPtr hWnd, int lParam) {
			int processId = 0;
			bool retval = true;
			IntPtr classPtr = new IntPtr(lParam);
			StringBuilder buffer = new StringBuilder(50);
			string className = Marshal.PtrToStringUni(classPtr);
			GetWindowThreadProcessId(hWnd, out processId);

			// This applies to Visible windows only
			if (IsWindowVisible(hWnd) && (processId != 0 && CurrentProcessId.Equals(processId)) &&
				GetClassName(hWnd, buffer, buffer.Capacity) > 0 && className.Equals(buffer.ToString(),
				StringComparison.InvariantCultureIgnoreCase)) {
				ChildElementHandle = hWnd;
				retval = false;
			}
			return retval;
		}

		#endregion

		#region Protected virtual methods

		/// <summary>
		/// Parses the and execute script.
		/// </summary>
		/// <param name="autoScript">The auto script.</param>
		/// <param name="targetHwnd">The target HWND.</param>
		/// <param name="code">The code.</param>
		protected virtual void ParseAndExecuteScript(XDocument autoScript, IntPtr targetHwnd, AutomationDelegate code) {
			GCHandle automationEndCallback = GCHandle.Alloc(code);

			switch (AutoScriptVersion) {
				case "1.0":
					DispatchMessage(autoScript, targetHwnd, (AutomationDelegate)automationEndCallback.Target);
					break;
			}

			automationEndCallback.Free();
		}

		#endregion
	}
}