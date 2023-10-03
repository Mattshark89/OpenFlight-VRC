using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UdonSharp;
using VRC.Udon;

namespace OpenFlightVRC
{
    /// <summary>
    /// A simple logger that prefixes all messages with [OpenFlight]
    /// </summary>
	public class Logger : UdonSharpBehaviour
	{
		void Start()
		{
            Log("Logging started", this);
		}

        const string packageColor = "orange";

        private static string Prefix()
        {
            return "[" + ColorText("OpenFlight", packageColor) + "]";
        }

        /// <summary>
        /// Logs a message to the console
        /// </summary>
        /// <param name="text">The text to print to the console</param>
        /// <param name="self">The UdonSharpBehaviour that is logging the text</param>
		internal static void Log(string text, UdonSharpBehaviour self)
		{
            Debug.Log(Format(text, self));
		}

        /// <summary>
        /// Logs a warning to the console
        /// </summary>
        /// <param name="text">The text to print to the console</param>
        /// <param name="self">The UdonSharpBehaviour that is logging the text</param>
        internal static void LogWarning(string text, UdonSharpBehaviour self)
        {
            Debug.LogWarning(Format(text, self));
        }

        /// <summary>
        /// Logs an error to the console
        /// </summary>
        /// <param name="text">The text to print to the console</param>
        /// <param name="self">The UdonSharpBehaviour that is logging the text</param>
        internal static void LogError(string text, UdonSharpBehaviour self)
        {
            Debug.LogError(Format(text, self));
        }

        /// <summary>
        /// Formats the text to be logged
        /// </summary>
        /// <param name="text">The text to format</param>
        /// <param name="self">The UdonSharpBehaviour that is logging the text</param>
        /// <returns>The formatted text</returns>
        private static string Format(string text, UdonSharpBehaviour self)
        {
            return Prefix() + " [" + ColorizeScript(self) + "] " + text;
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
            //set random seed to hash of name
            Random.InitState(self.name.GetHashCode());

            float Saturation = 1f;
            float Brightness = 1f;

            Random random = new Random();
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
