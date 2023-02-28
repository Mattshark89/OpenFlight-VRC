using UnityEngine;

namespace Koyashiro.UdonDictionary.Internal
{
    public static class ExceptionHelper
    {
        private const string TAG = "UdonDictionary";
        private const string COLOR_TAG = "red";
        private const string COLOR_EXCEPTION = "lime";
        private const string COLOR_PARAMETER = "cyan";
        private const string COLOR_ACTUAL_VALUE = "magenta";

        public static void ThrowArgumentException(string message)
        {
            LogErrorMessage(typeof(System.ArgumentException).FullName, message);
            Panic();
        }

        public static void ThrowKeyNotFoundException()
        {
            LogErrorMessage(typeof(System.Collections.Generic.KeyNotFoundException).FullName, "The given key was not present in the dictionary.");
            Panic();
        }

        private static void LogErrorMessage(string exception, string message)
        {
            Debug.LogError($"[<color={COLOR_TAG}>{TAG}</color>] <color={COLOR_EXCEPTION}>{exception}</color>: {message}");
        }

        private static void Panic()
        {
            // Raise runtime Exception
            ((object)null).ToString();
        }
    }
}
