/**
 * @ Maintainer: Happyrobot33
 */

using UnityEngine;
using UdonSharp;
using OpenFlightVRC.UI;
using VRC.SDK3.Data;
using TMPro;
using UnityEngine.PlayerLoop;
using VRC.SDKBase;

namespace OpenFlightVRC
{
	/// <summary>
	/// The type of log to write
	/// </summary>
	[System.Flags]
	public enum LogLevel
	{
		Info = 1 << 0,
		/// <summary>
		/// Used for logging where a callback is induced or setup, like <see cref="Info"/> but specifically for callbacks
		/// This should only be used for info level like messages
		/// Actual Callback errors or warnings should be logged as <see cref="Error"/> or <see cref="Warning"/> respectively
		/// </summary>
		Callback = 1 << 1,
		Warning = 1 << 2,
		Error = 1 << 3
	};

	public enum LogEntryKeys
	{
		Text,
		Time,
		Script,
		Level
	}

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
		//public string log = "";
		public DataDictionary logDictionary = new DataDictionary();
		public TextMeshProUGUI text;

		void Start()
		{
			//set our name just to be sure its correct
			gameObject.name = logObjectName;

			//TODO: Remove this, but this is testing calls for toggles
			//SetControlMatrix("PlayerSettings", Util.OrEnums(LogLevel.Info, LogLevel.Warning, LogLevel.Error));
			//SetControlMatrix("PlayerMetrics", Util.OrEnums(LogLevel.Info, LogLevel.Callback, LogLevel.Error));
		}

		public void TestMethod()
		{
			SetControlMatrix("PlayerMetrics", Util.OrEnums(LogLevel.Info, LogLevel.Warning, LogLevel.Error));
		}

		private const string LogLevelKey = "logLevelFlags";

		public void OnEnable()
		{
			//when we become visible again, we should make sure we have the latest text visible
			AttemptToPopulateText();
		}

		/// <summary>
		/// Updates the log text
		/// </summary>
		private void AttemptToPopulateText()
		{
			//determine if we are visible, and if we are not, then dont do anything
			if (gameObject.activeInHierarchy == false)
			{
				return;
			}

			text.text = "";

			//will contain all filtered logs
			DataList FilteredLogs = new DataList();

			//Filter the logs by the categorys
			DataList categorys = logDictionary.GetKeys();
			DataToken[] categoryArray = categorys.ToArray();
			foreach (DataToken category in categoryArray)
			{
				//get the dictionary of the category
				DataDictionary categoryDict = logDictionary[category.String].DataDictionary;

				//get the matrix
				LogLevel levels = (LogLevel)categoryDict[LogLevelKey].Reference;
				DataList flags = new DataList();

				//convert to flags by checking each bit
				for (int i = 1; i < int.MaxValue && i > 0; i *= 2)
				{
					if (Util.AndEnums(levels, (LogLevel)i) != 0)
					{
						flags.Add(new DataToken(LogLevelToString((LogLevel)i)));
					}
				}

				//Filter the logs by the flags
				DataList logLevels = categoryDict.GetKeys();
				DataToken[] logLevelArray = logLevels.ToArray();
				foreach (DataToken logLevel in logLevelArray)
				{
					//see if we should even be checking this log level
					if (flags.Contains(logLevel.String))
					{
						//get the list of log entries
						DataList logList = categoryDict[logLevel.String].DataList;

						//get the log entries
						FilteredLogs.AddRange(logList);
					}
				}
			}

			//we now need to just sort the logs by time
			//TODO: Implement the max messages limiter. I am leaving it out for now as I want to experiement without it and I would also like to 
			//possible make it configurable at runtime
			DataToken[] logArray = FilteredLogs.ToArray();
			//insertion sort
			for (int i = 1; i < logArray.Length; i++)
			{
				DataToken tempref = logArray[i];
				long temp = logArray[i].DataDictionary[(long)LogEntryKeys.Time].Long;

				int j = i - 1;
				while (j >= 0 && logArray[j].DataDictionary[(long)LogEntryKeys.Time].Long > temp)
				{
					logArray[j + 1] = logArray[j];
					j--;
				}
				logArray[j + 1] = tempref;
			}

			//print out in order
			foreach (DataToken logEntry in logArray)
			{
				DataDictionary logEntryDict = logEntry.DataDictionary;
				//get the text
				string logText = logEntryDict[(long)LogEntryKeys.Text].String;

				//get the time
				long time = logEntryDict[(long)LogEntryKeys.Time].Long;

				//get the level
				LogLevel level = (LogLevel)logEntryDict[(long)LogEntryKeys.Level].Reference;

				//get the script
				UdonSharpBehaviour script = (UdonSharpBehaviour)logEntryDict[(long)LogEntryKeys.Script].Reference;

				//add the log to the text
				text.text += Format(logText, time, level, script, false) + "\n";
			}
		}
		#endregion

		#region Public Logging API
		const string PackageColor = "orange";
		const string PackageName = "OpenFlight";
		/// <summary>
		/// The max number of log messages to store in the internal log data sets
		/// </summary>
		const int MaxInternalLogMessages = 200;

		public void SetControlMatrix(string category, LogLevel levels)
		{
			DataDictionary categoryDict = GetLogCategory(this, category);

			//if the category does not exist, create it
			categoryDict.SetValue(LogLevelKey, new DataToken(levels));

			//make sure the dictionary has the key
			logDictionary.SetValue(category, categoryDict);

			AttemptToPopulateText();
		}

		internal static void LogToUI(string text, LogLevel level, LoggableUdonSharpBehaviour self)
		{
			Logger logProxy = null;
			if (!SetupLogProxy(self, ref logProxy))
			{
				return;
			}

			//Setup our log entry, which is functionally a struct here
			DataDictionary logEntry = new DataDictionary();
			logEntry.SetValue((long)LogEntryKeys.Text, text);
			logEntry.SetValue((long)LogEntryKeys.Time, System.DateTime.Now.Ticks);
			logEntry.SetValue((long)LogEntryKeys.Script, self);
			logEntry.SetValue((long)LogEntryKeys.Level, new DataToken(level));

			//if self is null, then use a static string
			string logCategory = "Utility Methods";
			if (self != null)
			{
				logCategory = self._logCategory;
			}

			DataList logList = GetLogList(level, logProxy, logCategory);

			//add the entry to the list
			logList.Add(logEntry);

			logProxy.AttemptToPopulateText();
		}

		private static DataList GetLogList(LogLevel level, Logger logProxy, string logCategory)
		{
			DataList logList = new DataList();
			DataDictionary categoryDict = GetLogCategory(logProxy, logCategory);

			if (categoryDict.TryGetValue(LogLevelToString(level), out DataToken logToken))
			{
				logList = logToken.DataList;
			}

			categoryDict.SetValue(LogLevelToString(level), logList);

			return logList;
		}

		private static DataDictionary GetLogCategory(Logger logProxy, string logCategory)
		{
			DataDictionary categoryDict = new DataDictionary();
			//get the value
			if (logProxy.logDictionary.TryGetValue(logCategory, out DataToken levelToken))
			{
				//token is a dictionary of the different log levels
				categoryDict = levelToken.DataDictionary;

			}
			else
			{
				//if the category does not exist, create it
				logProxy.logDictionary.SetValue(logCategory, categoryDict);

				//setup the log level flags
				categoryDict.SetValue(LogLevelKey, new DataToken(Util.OrEnums(LogLevel.Info, LogLevel.Callback, LogLevel.Warning, LogLevel.Error)));
			}

			return categoryDict;
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
				case LogLevel.Callback:
					return ColorText(nameof(LogLevel.Callback), "#00FFFF");
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
		private static string LogLevelToString(LogLevel LT)
		{
			switch (LT)
			{
				case LogLevel.Info:
					return nameof(LogLevel.Info);
				case LogLevel.Callback:
					return nameof(LogLevel.Callback);
				case LogLevel.Warning:
					return nameof(LogLevel.Warning);
				case LogLevel.Error:
					return nameof(LogLevel.Error);
				default:
					return "";
			}
		}

		/// <summary>
		/// Logs a message to the console
		/// </summary>
		/// <param name="text"></param>
		/// <param name="LT"></param>
		private static void LogToConsole(string text, LogLevel LT, LoggableUdonSharpBehaviour self)
		{
			string formatted = Format(text, System.DateTime.Now.Ticks, LT, self, true);
			switch (LT)
			{
				case LogLevel.Info:
					Debug.Log(formatted, self);
					break;
				case LogLevel.Callback:
					Debug.Log(formatted, self);
					break;
				case LogLevel.Warning:
					Debug.LogWarning(formatted, self);
					break;
				case LogLevel.Error:
					Debug.LogError(formatted, self);
					break;
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
		/// <param name="level">The level of the log</param>
		/// <param name="text">The text to print to the console</param>
		/// <param name="once">Whether or not to only log the message once and ignore future calls until the latest message is different</param>
		/// <param name="self">The UdonSharpBehaviour that is logging the text</param>
		internal static void Log(LogLevel level, string text, bool once = false, LoggableUdonSharpBehaviour self = null)
		{
			//check if the message has already been logged
			if (once && CheckIfLogged(text, self))
			{
				return;
			}

			LogToConsole(text, level, self);
			//WriteToUILog(Format(text, LogLevel.Info, self, false), self);
			LogToUI(text, level, self);
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

			//TODO: Repair this to work with the new system
			//string logString = logProxy.log;
			Debug.Assert(false, string.Format("{0} has been un-implemented!", nameof(CheckIfLogged)));

			//check if the latest message is the same as the text
			//return logString.EndsWith(text + "\n");
			return false;
		}

		/// <summary>
		/// Gets the current timestamp
		/// </summary>
		/// <returns></returns>
		private static string GetTimeStampString(long ticks)
		{
			string time = new System.DateTime(ticks).ToString("h:mm:ss.ffff");
			return ColorText(time, "white");
		}

		/// <summary>
		/// Formats the text to be logged
		/// </summary>
		/// <param name="text">The text to format</param>
		/// <param name="self">The UdonSharpBehaviour that is logging the text</param>
		/// <param name="includePrefix">Whether or not to include the prefix</param>
		/// <returns>The formatted text</returns>
		internal static string Format(string text, long ticks, LogLevel LT, UdonSharpBehaviour self, bool includePrefix = true)
		{
			string prefix = includePrefix ? string.Format("[{0}]", ColorText(PackageName, PackageColor)) : "";
			return string.Format("{0} [{1}] [{2}] [{3}] {4}", prefix, GetLogTypeString(LT), GetTimeStampString(ticks), ColorizeScript(self), text);
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
