/**
 * @ Maintainer: Happyrobot33
 */

using UnityEngine;
using UdonSharp;
using OpenFlightVRC.UI;

namespace OpenFlightVRC
{
	/// <summary>
	/// The type of log to write
	/// </summary>
	enum LogType
	{
		Log,
		Warning,
		Error
	};

	/// <summary>
	/// A simple logger that prefixes all messages with [OpenFlight]
	/// </summary>
	[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
	public class Logger : UdonSharpBehaviour
	{
		const int MaxLogLength = 50;

		const string PackageColor = "orange";

		private static string Prefix()
		{
			return "[" + ColorText("OpenFlight", PackageColor) + "]";
		}

		private static void WriteToUILog(string text, LogType lT, LoggableUdonSharpBehaviour self)
		{
			LoggerProxy logProxy = self._logProxy;

			if (logProxy == null)
			{
				const string log = "OpenFlightLogObject";

				GameObject logObject = GameObject.Find(log);

				if (logObject == null)
				{
					return;
				}

				LoggerProxy logUdon = logObject.GetComponent<LoggerProxy>();

				self._logProxy = logUdon;
				logProxy = logUdon;
			}

			switch (lT)
			{
				case LogType.Log:
					text = "[" + ColorText("Log", "white") + "] " + text;
					break;
				case LogType.Warning:
					text = "[" + ColorText("Warning", "yellow") + "] " + text;
					break;
				case LogType.Error:
					text = "[" + ColorText("Error", "red") + "] " + text;
					break;
			}

			string logString = logProxy.log;

			logString += text + "\n";

			//trim text if too many lines
			if (logString.Split('\n').Length > MaxLogLength)
			{
				logString = logString.Substring(logString.IndexOf('\n') + 1);
			}

			logProxy.log = logString;
		}

		/// <summary>
		/// Logs a message to the console
		/// </summary>
		/// <param name="text">The text to print to the console</param>
		/// <param name="self">The UdonSharpBehaviour that is logging the text</param>
		internal static void Log(string text, LoggableUdonSharpBehaviour self)
		{
			Debug.Log(Format(text, self));
			WriteToUILog(Format(text, self, false), LogType.Log, self);
		}

		/// <summary>
		/// Logs a warning to the console
		/// </summary>
		/// <param name="text">The text to print to the console</param>
		/// <param name="self">The UdonSharpBehaviour that is logging the text</param>
		internal static void LogWarning(string text, LoggableUdonSharpBehaviour self)
		{
			Debug.LogWarning(Format(text, self));
			WriteToUILog(Format(text, self, false), LogType.Warning, self);
		}

		/// <summary>
		/// Logs an error to the console
		/// </summary>
		/// <param name="text">The text to print to the console</param>
		/// <param name="self">The UdonSharpBehaviour that is logging the text</param>
		internal static void LogError(string text, LoggableUdonSharpBehaviour self)
		{
			Debug.LogError(Format(text, self));
			WriteToUILog(Format(text, self, false), LogType.Error, self);
		}

		/// <summary>
		/// Formats the text to be logged
		/// </summary>
		/// <param name="text">The text to format</param>
		/// <param name="self">The UdonSharpBehaviour that is logging the text</param>
		/// <param name="includePrefix">Whether or not to include the prefix</param>
		/// <returns>The formatted text</returns>
		private static string Format(string text, UdonSharpBehaviour self, bool includePrefix = true)
		{
			string prefix = includePrefix ? Prefix() + " " : "";
			return prefix + "[" + ColorizeScript(self) + "] " + text;
			//return (includePrefix ? Prefix() + " " : "") + ColorizeScript(self) + " " + text;
		}

		/// <summary>
		/// Returns a colored string of the UdonSharpBehaviour's name
		/// </summary>
		/// <param name="script">The UdonSharpBehaviour to colorize</param>
		/// <returns>The colored name</returns>
		public static string ColorizeScript(UdonSharpBehaviour script)
		{
			return ColorText(script.name, ChooseColor(script));
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
			return "<i>" + colorized + "</i>";
		}

		/// <summary>
		/// Colors a string
		/// </summary>
		/// <param name="text">The text to color</param>
		/// <param name="color">The color to color the text</param>
		/// <returns>The colored text</returns>
		private static string ColorText(string text, string color)
		{
			return "<color=" + color + ">" + text + "</color>";
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
				Random.InitState(self.name.GetHashCode());
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
	}
}
