/**
 * @ Maintainer: Happyrobot33
 */

using UnityEngine;
using UdonSharp;
using OpenFlightVRC.UI;
using VRC.SDK3.Data;
using TMPro;

namespace OpenFlightVRC
{
	/// <summary>
	/// The type of log to write
	/// </summary>
	enum LogLevel
	{
		Info,
		Warning,
		Error
	};

	/// <summary>
	/// A simple logger that prefixes all messages with [OpenFlight]
	/// </summary>
	[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
	[DefaultExecutionOrder(-1000)] //Ensure this runs before any other scripts so its name is correct
	public class Logger : UdonSharpBehaviour
	{
		/// <summary>
		/// The name of the log object
		/// </summary>
		const string logObjectName = "OpenFlightLogObject";

		#region In-Client Log Visualisation
		public string log = "";
		public TextMeshProUGUI text;

		void Start()
		{
			//set our name just to be sure its correct
			gameObject.name = logObjectName;
		}

		/// <summary>
		/// Updates the log text
		/// </summary>
		private void UpdateLog()
		{
			text.text = log;
		}
		#endregion

		#region Public Logging API
		const string PackageColor = "orange";
		const string PackageName = "OpenFlight";
		/// <summary>
		/// The max number of log messages to display
		/// </summary>
		const int MaxLogMessages = 200;

		internal static void WriteToUILog(string text, LoggableUdonSharpBehaviour self)
        {
            Logger logProxy = null;
            if (!SetupLogProxy(self, ref logProxy))
            {
                return;
            }

            //add the text to the log
			logProxy.log += text + "\n";

			//split into lines
			//trim the log if it is too long
			string[] lines = logProxy.log.Split('\n');
			if (lines.Length > MaxLogMessages)
			{
				logProxy.log = string.Join("\n", lines, lines.Length - MaxLogMessages, MaxLogMessages);
			}

            logProxy.UpdateLog();
        }

		/// <summary>
		/// Gets the log type string
		/// </summary>
		/// <param name="lT"></param>
		/// <returns></returns>
        private static string GetLogTypeString(LogLevel lT)
        {
            switch (lT)
            {
                case LogLevel.Info:
					return ColorText(nameof(LogLevel.Info), "white");
                case LogLevel.Warning:
					return ColorText(nameof(LogLevel.Warning), "yellow");
                case LogLevel.Error:
					return ColorText(nameof(LogLevel.Error), "red");
				default:
					return "";
            }
        }

		/// <summary>
		/// Converts a LogType to a string
		/// </summary>
		/// <param name="LT"></param>
		/// <returns></returns>
		private static string LogTypeToString(LogLevel LT)
		{
			switch (LT)
			{
				case LogLevel.Info:
					return nameof(LogLevel.Info);
				case LogLevel.Warning:
					return nameof(LogLevel.Warning);
				case LogLevel.Error:
					return nameof(LogLevel.Error);
				default:
					return "";
			}
		}

        /// <summary>
        /// Sets up the log proxy system
        /// </summary>
        /// <param name="self"></param>
        /// <param name="Logger"></param>
        /// <returns> Whether or not the setup was successful </returns>
        private static bool SetupLogProxy(LoggableUdonSharpBehaviour self, ref Logger Logger)
        {
            //check if self is null
            //if it isnt, we can check for and setup the logproxy cache system
            if (self != null)
            {
                Logger = self._logProxy;

                if (Logger == null)
                {

                    GameObject logObject = GameObject.Find(logObjectName);

                    if (logObject == null)
                    {
                        return false;
                    }

                    Logger logUdon = logObject.GetComponent<Logger>();

                    self._logProxy = logUdon;
                    Logger = logUdon;
                }
            }
            else
            {
                //if it *is* null, we need to do the more expensive gameobject.find every time
                GameObject logObject = GameObject.Find(logObjectName);

                if (logObject == null)
                {
                    return false;
                }

                Logger logUdon = logObject.GetComponent<Logger>();

                Logger = logUdon;
            }

			return true;
        }

        /// <summary>
        /// Logs a message to the console
        /// </summary>
        /// <param name="text">The text to print to the console</param>
        /// <param name="self">The UdonSharpBehaviour that is logging the text</param>
        internal static void Log(string text, LoggableUdonSharpBehaviour self)
		{
			Debug.Log(Format(text, LogLevel.Info, self));
			WriteToUILog(Format(text, LogLevel.Info, self, false), self);
		}

		/// <inheritdoc cref="Log(string, LoggableUdonSharpBehaviour)"/>
		/// <remarks> This version of Log will only log the message once </remarks>
		internal static void LogOnce(string text, LoggableUdonSharpBehaviour self)
		{
			//check if the message has already been logged
			if (CheckIfLogged(text, self))
			{
				return;
			}

			Debug.Log(Format(text, LogLevel.Info, self));
			WriteToUILog(Format(text, LogLevel.Info, self, false), self);
		}

		/// <summary>
		/// Logs a warning to the console
		/// </summary>
		/// <param name="text">The text to print to the console</param>
		/// <param name="self">The UdonSharpBehaviour that is logging the text</param>
		internal static void LogWarning(string text, LoggableUdonSharpBehaviour self)
		{
			Debug.LogWarning(Format(text, LogLevel.Warning, self));
			WriteToUILog(Format(text, LogLevel.Warning, self, false), self);
		}

		/// <inheritdoc cref="LogWarning(string, LoggableUdonSharpBehaviour)"/>
		/// <remarks> This version of LogWarning will only log the warning once </remarks>
		internal static void LogWarningOnce(string text, LoggableUdonSharpBehaviour self)
		{
			//check if the warning has already been logged
			if (CheckIfLogged(text, self))
			{
				return;
			}

			Debug.LogWarning(Format(text, LogLevel.Warning, self));
			WriteToUILog(Format(text, LogLevel.Warning, self, false), self);
		}

		/// <summary>
		/// Logs an error to the console
		/// </summary>
		/// <param name="text">The text to print to the console</param>
		/// <param name="self">The UdonSharpBehaviour that is logging the text</param>
		internal static void LogError(string text, LoggableUdonSharpBehaviour self)
		{
			Debug.LogError(Format(text, LogLevel.Error, self));
			WriteToUILog(Format(text, LogLevel.Error, self, false), self);
		}

		/// <inheritdoc cref="LogError(string, LoggableUdonSharpBehaviour)"/>
		/// <remarks> This version of LogError will only log the error once </remarks>
		internal static void LogErrorOnce(string text, LoggableUdonSharpBehaviour self)
		{
			//check if the error has already been logged
			if (CheckIfLogged(text, self))
			{
				return;
			}

			Debug.LogError(Format(text, LogLevel.Error, self));
			WriteToUILog(Format(text, LogLevel.Error, self, false), self);
		}

		/// <summary>
		/// Checks if a specific text has been logged already, as the latest message
		/// </summary>
		/// <param name="text"></param>
		/// <param name="self"></param>
		/// <returns> Whether or not the text has been logged as the latest message </returns>
		internal static bool CheckIfLogged(string text, LoggableUdonSharpBehaviour self)
		{
			Logger logProxy = null;
			if (!SetupLogProxy(self, ref logProxy))
			{
				return false;
			}

			string logString = logProxy.log;

			//check if the latest message is the same as the text
			return logString.EndsWith(text + "\n");
		}

		/// <summary>
		/// Gets the current timestamp
		/// </summary>
		/// <returns></returns>
		private static string GetTimeStampString()
		{
			string time = System.DateTime.Now.ToString("T");
			return ColorText(time, "white");
		}

		/// <summary>
		/// Formats the text to be logged
		/// </summary>
		/// <param name="text">The text to format</param>
		/// <param name="self">The UdonSharpBehaviour that is logging the text</param>
		/// <param name="includePrefix">Whether or not to include the prefix</param>
		/// <returns>The formatted text</returns>
		internal static string Format(string text, LogLevel LT, UdonSharpBehaviour self, bool includePrefix = true)
		{
			string prefix = includePrefix ? string.Format("[{0}]", ColorText(PackageName, PackageColor)) : "";
			return string.Format("{0} [{1}] [{2}] [{3}] {4}", prefix, GetLogTypeString(LT), GetTimeStampString(), ColorizeScript(self), text);
			//return string.Format("{0} [{1}] {2}", prefix, ColorizeScript(self), text);
			//return (includePrefix ? Prefix() + " " : "") + ColorizeScript(self) + " " + text;
		}

		/// <summary>
		/// Returns a colored string of the UdonSharpBehaviour's name
		/// </summary>
		/// <param name="script">The UdonSharpBehaviour to colorize</param>
		/// <returns>The colored name</returns>
		public static string ColorizeScript(UdonSharpBehaviour script)
		{
			return ColorText(GetScriptName(script), ChooseColor(script));
		}

		/// <summary>
		/// Returns a colored string of the UdonSharpBehaviour's function
		/// </summary>
		/// <param name="script">The UdonSharpBehaviour to colorize</param>
		/// <param name="function">The function to colorize</param>
		/// <returns>The colored function</returns>
		public static string ColorizeFunction(UdonSharpBehaviour script, string function)
		{
			string colorized = ColorText(function, ChooseColor(script));

			//italicise it to denote that it is a function
			return string.Format("<i>{0}</i>", colorized);
		}

		/// <summary>
		/// Colors a string
		/// </summary>
		/// <param name="text">The text to color</param>
		/// <param name="color">The color to color the text</param>
		/// <returns>The colored text</returns>
		private static string ColorText(string text, string color)
		{
			return string.Format("<color={0}>{1}</color>", color, text);
		}

		/// <summary>
		/// Chooses a color based on the name of the UdonSharpBehaviour
		/// </summary>
		/// <param name="self">The UdonSharpBehaviour to choose a color for</param>
		/// <returns>The color to use</returns>
		private static string ChooseColor(UdonSharpBehaviour self)
		{
			//if the script is null, init to a constant
			if (self == null)
			{
				Random.InitState(0);
			}
			else
			{
				//set random seed to hash of name
				Random.InitState(GetScriptName(self).GetHashCode());
			}

			float Saturation = 1f;
			float Brightness = 1f;

			float hue = Random.Range(0.0f, 1.0f);

			Color color = Color.HSVToRGB(hue, Saturation, Brightness);

			return ColorToHTML(color);
		}

		/// <summary>
		/// Converts RGB to HTML
		/// </summary>
		/// <param name="color">The color to convert</param>
		/// <returns>The HTML color</returns>
		private static string ColorToHTML(Color color)
		{
			string RHex = ((int)(color.r * 255)).ToString("X2");
			string GHex = ((int)(color.g * 255)).ToString("X2");
			string BHex = ((int)(color.b * 255)).ToString("X2");

			return "#" + RHex + GHex + BHex;
		}

		/// <summary>
		/// Gets the name of the UdonSharpBehaviour. If null, returns "Untraceable Static Function Call"
		/// </summary>
		/// <param name="script"></param>
		/// <returns></returns>
		private static string GetScriptName(UdonSharpBehaviour script)
		{
			//check if null
			if (script == null)
			{
				return "Untraceable Static Function Call";
			}

			return script.name;
		}
		#endregion
	}
}
