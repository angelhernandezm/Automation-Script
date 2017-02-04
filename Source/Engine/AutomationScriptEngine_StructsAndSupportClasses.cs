using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Reflection;

// AutomationScriptEngine
namespace AutomationScript {
    public partial class Engine : IDisposable {
		#region Classes

		/// <summary>
		/// Describes a step in an automation script 
		/// </summary>
		public class AutomationStep {
			#region Contructors

			/// <summary>
			/// Initializes a new instance of the <see cref="AutomationStep"/> class.
			/// </summary>
			public AutomationStep() {

			}

			#endregion

			#region Properties

			/// <summary>
			/// Gets or sets the h WND.
			/// </summary>
			/// <value>The h WND.</value>
			public IntPtr hWnd {
				get;
				set;
			}

			/// <summary>
			/// Gets or sets the MSG.
			/// </summary>
			/// <value>The MSG.</value>
			public uint Msg {
				get;
				set;
			}

			/// <summary>
			/// Gets or sets the w param.
			/// </summary>
			/// <value>The w param.</value>
			public uint wParam {
				get;
				set;
			}

			/// <summary>
			/// Gets or sets the l param.
			/// </summary>
			/// <value>The l param.</value>
			public int lParam {
				get;
				set;
			}

			/// <summary>
			/// Gets or sets the delay.
			/// </summary>
			/// <value>The delay.</value>
			public int Delay {
				get;
				set;
			}

			/// <summary>
			/// Gets or sets a value indicating whether this instance is disabled.
			/// </summary>
			/// <value>
			/// 	<c>true</c> if this instance is disabled; otherwise, <c>false</c>.
			/// </value>
			public bool IsDisabled {
				get;
				set;
			}

			/// <summary>
			/// Gets or sets the path or title.
			/// </summary>
			/// <value>The path or title.</value>
			public string PathOrTitle {
				get;
				set;
			}

			/// <summary>
			/// Gets or sets the child element.
			/// </summary>
			/// <value>The child element.</value>
			public string ChildElement {
				get;
				set;
			}

			/// <summary>
			/// Gets or sets the clipboard data.
			/// </summary>
			/// <value>The clipboard data.</value>
			public string ClipboardData {
				get;
				set;
			}

			/// <summary>
			/// Gets or sets the target class.
			/// </summary>
			/// <value>The target class.</value>
			public string TargetClass {
				get;
				set;
			}

			/// <summary>
			/// Gets or sets a value indicating whether [exit automation on failure].
			/// </summary>
			/// <value>
			/// 	<c>true</c> if [exit automation on failure]; otherwise, <c>false</c>.
			/// </value>
			public bool ExitAutomationOnFailure {
				get;
				set;
			}

			/// <summary>
			/// Gets or sets the step as XML.
			/// </summary>
			/// <value>The step as XML.</value>
			public XElement StepAsXml {
				get;
				private set;
			}

			/// <summary>
			/// Gets or sets the action.
			/// </summary>
			/// <value>The action.</value>
			public StepAction Action {
				get;
				private set;
			}

			/// <summary>
			/// Gets or sets the key code.
			/// </summary>
			/// <value>The key code.</value>
			public VirtualKeyCode KeyCode {
				get;
				private set;
			}

			/// <summary>
			/// Gets or sets the send message to.
			/// </summary>
			/// <value>The send message to.</value>
			public string SendMessageTo {
				get;
				set;
			}

			/// <summary>
			/// Empty instance
			/// </summary>
			public static readonly AutomationStep Empty = new AutomationStep() {
				Msg = 0,
				Delay = 0,
				lParam = 0,
				wParam = 0,
				StepAsXml = null,
				IsDisabled = false,
				hWnd = IntPtr.Zero,
				Action = StepAction.None,
				PathOrTitle = string.Empty,
				TargetClass = string.Empty,
				ChildElement = string.Empty,
				ClipboardData = string.Empty,
				SendMessageTo = string.Empty,
				KeyCode = VirtualKeyCode.None,
				ExitAutomationOnFailure = false
			};

			#endregion

			#region Support methods

			/// <summary>
			/// Gets the script steps.
			/// </summary>
			/// <param name="autoScript">The auto script.</param>
			/// <returns></returns>
			public static IEnumerable<AutomationStep> GetScriptSteps(XDocument autoScript) {
				XElement script = null;
				Type currentType = null;
				XAttribute selected = null;
				AutomationStep step = null;
				object propertyValue = null;
				PropertyInfo propInfo = null;
				IEnumerable<XElement> steps = null;
				List<AutomationStep> retval = new List<AutomationStep>();

				BindingFlags flags = BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.NonPublic |
									 BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty;

				Dictionary<string, string> mapping = new Dictionary<string, string>()
                {
                    {"hWnd","hWnd"}, {"Msg","Msg"}, {"wParam","wParam"}, {"lParam","lParam"},
                    {"delay","Delay"}, {"isDisabled","IsDisabled"}, {"exitAutomationOnFailure","ExitAutomationOnFailure"},
                    {"pathOrTitle","PathOrTitle"}, {"childElement","ChildElement"}, {"clipBoardData","ClipboardData"},
                    {"targetClass", "TargetClass"}, {"stepAction","Action"}, {"virtualKeyCode","KeyCode"}, {"sendMessageTo","SendMessageTo"}
                };

				if ((script = autoScript.Element("autoscript")) != null &&
					(steps = script.Elements()) != null && steps.Count() > 0) {
					steps.ToList().ForEach(stepDetail => {
						step = new AutomationStep();
						currentType = step.GetType();
						mapping.ToList().ForEach(item => {
							if ((selected = stepDetail.Attribute(item.Key)) != null &&
								!string.IsNullOrEmpty(selected.Value)) {
								propInfo = currentType.GetProperty(item.Value, flags);
								propertyValue = GetSafePropertyValue(propInfo, selected.Value);
								propInfo.SetValue(step, propertyValue, null);
							}
						});
						step.StepAsXml = stepDetail;
						retval.Add(step);
					});
				}
				return retval;
			}


			/// <summary>
			/// Gets the safe property value.
			/// </summary>
			/// <param name="info">The info.</param>
			/// <param name="value">The value.</param>
			/// <returns></returns>
			public static object GetSafePropertyValue(PropertyInfo info, string value) {
				object retval = null;
				int parseTestInt = 0;
				uint parseTestUint = 0;
				bool parseTestBool = false;

				if (info.PropertyType.Equals(typeof(int)))
					retval = int.TryParse(value, out parseTestInt) ? parseTestInt : 0;
				else if (info.PropertyType.Equals(typeof(uint)))
					retval = uint.TryParse(value, out parseTestUint) ? parseTestUint : 0;
				else if (info.PropertyType.Equals(typeof(bool)))
					retval = bool.TryParse(value, out parseTestBool) ? parseTestBool : false;
				else if (info.PropertyType.Equals(typeof(IntPtr)))
					retval = Engine.GetHwndForStep(value);
				else if (info.PropertyType.Equals(typeof(StepAction)))
					retval = Enum.Parse(typeof(StepAction), value, true);
				else if (info.PropertyType.Equals(typeof(VirtualKeyCode)))
					retval = Enum.Parse(typeof(VirtualKeyCode), value, true);
				else if (info.PropertyType.Equals(typeof(BehaviorType)))
					retval = Enum.Parse(typeof(BehaviorType), value, true);
				else if (info.PropertyType.Equals(typeof(ConditionType)))
					retval = Enum.Parse(typeof(ConditionType), value, true);
				else
					retval = value;

				return retval;
			}


			#endregion

			#region Overloads & overrides

			/// <summary>
			/// Implements the operator ==.
			/// </summary>
			/// <param name="leftOp">The left op.</param>
			/// <param name="rightOp">The right op.</param>
			/// <returns>The result of the operator.</returns>
			public static bool operator ==(AutomationStep leftOp, AutomationStep rightOp) {
				bool retval = object.ReferenceEquals(leftOp, rightOp);

				if (!retval) {
					if (!(retval = (((object)leftOp == null) || ((object)rightOp == null)))) {
						retval = leftOp.ChildElement.Equals(rightOp.ChildElement) &&
								 leftOp.ClipboardData.Equals(rightOp.ClipboardData) &&
								 leftOp.Delay.Equals(rightOp.Delay) &&
								 leftOp.hWnd.ToInt32().Equals(rightOp.hWnd.ToInt32()) &&
								 leftOp.IsDisabled.Equals(rightOp.IsDisabled) &&
								 leftOp.lParam.Equals(rightOp.lParam) &&
								 leftOp.PathOrTitle.Equals(rightOp.PathOrTitle) &&
								 leftOp.TargetClass.Equals(rightOp.TargetClass) &&
								 leftOp.wParam.Equals(rightOp.wParam) &&
								 leftOp.ExitAutomationOnFailure.Equals(rightOp.ExitAutomationOnFailure) &&
								 leftOp.KeyCode.Equals(rightOp.KeyCode) && leftOp.Action.Equals(rightOp.Action) &&
								 leftOp.Msg.Equals(rightOp.Msg) && leftOp.SendMessageTo.Equals(rightOp.SendMessageTo) &&
								 ((leftOp.StepAsXml != null && rightOp.StepAsXml != null) &&
								  leftOp.StepAsXml.ToString().Equals(rightOp.StepAsXml.ToString()));
					}
				}

				return retval;
			}


			/// <summary>
			/// Implements the operator !=.
			/// </summary>
			/// <param name="leftOp">The left op.</param>
			/// <param name="rightOp">The right op.</param>
			/// <returns>The result of the operator.</returns>
			public static bool operator !=(AutomationStep leftOp, AutomationStep rightOp) {
				return !(leftOp == rightOp);
			}

			/// <summary>
			/// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
			/// </summary>
			/// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
			/// <returns>
			/// 	<c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
			/// </returns>
			/// <exception cref="T:System.NullReferenceException">
			/// The <paramref name="obj"/> parameter is null.
			/// </exception>
			public override bool Equals(object obj) {
				return ((obj != null && obj is AutomationStep) &&
								(this == (AutomationStep)obj));
			}

			/// <summary>
			/// Returns a <see cref="System.String"/> that represents this instance.
			/// </summary>
			/// <returns>
			/// A <see cref="System.String"/> that represents this instance.
			/// </returns>
			public override string ToString() {
				return base.ToString();
			}


			/// <summary>
			/// Returns a hash code for this instance.
			/// </summary>
			/// <returns>
			/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
			/// </returns>
			public override int GetHashCode() {
				return base.GetHashCode();
			}

			#endregion
		}

		/// <summary>
		/// Translates a predefined step into Win32 messages
		/// </summary>
		public class ActionMessageTranslation {
			#region Members

			Dictionary<StepAction, List<int>> mapping = new Dictionary<StepAction, List<int>>()
            {
                {StepAction.KeyStroke, new List<int>() {256, 258, 257}} // WM_KEYDOWN, WM_CHAR & WM_KEYUP
            };

			#endregion

			#region Indexers

			/// <summary>
			/// Gets the <see cref="System.Collections.Generic.List&lt;System.Int32&gt;"/> with the specified action.
			/// </summary>
			/// <value></value>
			public List<int> this[StepAction action] {
				get {
					List<int> retval = null;

					if (mapping.ContainsKey(action))
						retval = mapping[action];

					return retval;
				}
			}

			#endregion

			#region Contructors

			/// <summary>
			/// Initializes a new instance of the <see cref="ActionMessageTranslation"/> class.
			/// </summary>
			protected ActionMessageTranslation() {

			}

			#endregion

			#region Public static methods

			/// <summary>
			/// Gets the message mapping.
			/// </summary>
			/// <param name="action">The action.</param>
			/// <returns></returns>
			public static List<int> GetMessageMapping(StepAction action) {
				return (new ActionMessageTranslation())[action];
			}

			#endregion
		}

		/// <summary>
		/// Describes a for loop (statement) in automation script
		/// </summary>
		public class For {
			#region Properties

			/// <summary>
			/// Gets or sets the step count.
			/// </summary>
			/// <value>The step count.</value>
			public int StepCount {
				get;
				set;
			}

			/// <summary>
			/// Gets or sets the action.
			/// </summary>
			/// <value>The action.</value>
			public StepAction Action {
				get;
				set;
			}

			/// <summary>
			/// Gets or sets the key code.
			/// </summary>
			/// <value>The key code.</value>
			public VirtualKeyCode KeyCode {
				get;
				set;
			}

			/// <summary>
			/// Empty instance
			/// </summary>
			public readonly static For Empty = new For() {
				StepCount = 0,
				Action = StepAction.None,
				KeyCode = VirtualKeyCode.None
			};

			#endregion

			#region Overloads & overrides

			/// <summary>
			/// Implements the operator ==.
			/// </summary>
			/// <param name="leftOp">The left op.</param>
			/// <param name="rightOp">The right op.</param>
			/// <returns>The result of the operator.</returns>
			public static bool operator ==(For leftOp, For rightOp) {
				bool retval = object.ReferenceEquals(leftOp, rightOp);

				if (!retval) {
					if (!(retval = (((object)leftOp == null) || ((object)rightOp == null)))) {
						retval = leftOp.Action.Equals(rightOp.Action) &&
							leftOp.KeyCode.Equals(rightOp.KeyCode) &&
							leftOp.StepCount.Equals(rightOp.StepCount);
					}
				}
				return retval;
			}

			/// <summary>
			/// Implements the operator !=.
			/// </summary>
			/// <param name="leftOp">The left op.</param>
			/// <param name="rightOp">The right op.</param>
			/// <returns>The result of the operator.</returns>
			public static bool operator !=(For leftOp, For rightOp) {
				return !(leftOp == rightOp);
			}

			/// <summary>
			/// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
			/// </summary>
			/// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
			/// <returns>
			/// 	<c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
			/// </returns>
			/// <exception cref="T:System.NullReferenceException">
			/// The <paramref name="obj"/> parameter is null.
			/// </exception>
			public override bool Equals(object obj) {
				return ((obj != null && obj is For) &&
								(this == (For)obj));
			}

			/// <summary>
			/// Returns a <see cref="System.String"/> that represents this instance.
			/// </summary>
			/// <returns>
			/// A <see cref="System.String"/> that represents this instance.
			/// </returns>
			public override string ToString() {
				return base.ToString();
			}


			/// <summary>
			/// Returns a hash code for this instance.
			/// </summary>
			/// <returns>
			/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
			/// </returns>
			public override int GetHashCode() {
				return base.GetHashCode();
			}

			#endregion

		}

		/// <summary>
		/// Describes a condition in automation script
		/// </summary>
		public class Condition {
			#region Properties

			/// <summary>
			/// Gets or sets the type.
			/// </summary>
			/// <value>The type.</value>
			public ConditionType Type {
				get;
				set;
			}

			/// <summary>
			/// Gets or sets the behavior.
			/// </summary>
			/// <value>The behavior.</value>
			public BehaviorType Behavior {
				get;
				set;
			}

			/// <summary>
			/// Gets or sets the maximum retries.
			/// </summary>
			/// <value>The maximum retries.</value>
			public int MaximumRetries {
				get;
				set;
			}

			/// <summary>
			/// Gets or sets the time interval.
			/// </summary>
			/// <value>The time interval.</value>
			public int TimeInterval {
				get;
				set;
			}

			/// <summary>
			/// Empty instance
			/// </summary>
			public readonly static Condition Empty = new Condition() {
				Behavior = BehaviorType.None,
				MaximumRetries = 0,
				TimeInterval = 0,
				Type = ConditionType.None
			};

			#endregion

			#region Overloads & overrides

			/// <summary>
			/// Implements the operator ==.
			/// </summary>
			/// <param name="leftOp">The left op.</param>
			/// <param name="rightOp">The right op.</param>
			/// <returns>The result of the operator.</returns>
			public static bool operator ==(Condition leftOp, Condition rightOp) {
				bool retval = object.ReferenceEquals(leftOp, rightOp);

				if (!retval) {
					if (!(retval = (((object)leftOp == null) || ((object)rightOp == null)))) {
						retval = leftOp.Behavior.Equals(rightOp.Behavior) &&
							leftOp.MaximumRetries.Equals(rightOp.MaximumRetries) &&
							leftOp.TimeInterval.Equals(rightOp.TimeInterval) &&
							leftOp.Type.Equals(rightOp.Type);
					}
				}

				return retval;
			}

			/// <summary>
			/// Implements the operator !=.
			/// </summary>
			/// <param name="leftOp">The left op.</param>
			/// <param name="rightOp">The right op.</param>
			/// <returns>The result of the operator.</returns>
			public static bool operator !=(Condition leftOp, Condition rightOp) {
				return !(leftOp == rightOp);
			}

			/// <summary>
			/// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
			/// </summary>
			/// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
			/// <returns>
			/// 	<c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
			/// </returns>
			/// <exception cref="T:System.NullReferenceException">
			/// The <paramref name="obj"/> parameter is null.
			/// </exception>
			public override bool Equals(object obj) {
				return ((obj != null && obj is Condition) &&
								(this == (Condition)obj));
			}

			/// <summary>
			/// Returns a <see cref="System.String"/> that represents this instance.
			/// </summary>
			/// <returns>
			/// A <see cref="System.String"/> that represents this instance.
			/// </returns>
			public override string ToString() {
				return base.ToString();
			}

			/// <summary>
			/// Returns a hash code for this instance.
			/// </summary>
			/// <returns>
			/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
			/// </returns>
			public override int GetHashCode() {
				return base.GetHashCode();
			}

			#endregion
		}

		#endregion
	}
}