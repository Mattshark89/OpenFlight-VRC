using UnityEngine;

namespace Koyashiro.UdonList.Internal
{
    public static class ExceptionHelper
    {
        private const string TAG = "UdonList";
        private const string COLOR_TAG = "red";
        private const string COLOR_EXCEPTION = "lime";
        private const string COLOR_PARAMETER = "cyan";
        private const string COLOR_ACTUAL_VALUE = "magenta";

        public static void ThrowArgumentException()
        {
            LogErrorMessage(typeof(System.ArgumentException).FullName, "Value does not fall within the expected range.");
            Panic();
        }

        public static void ThrowArgumentNullException(string paramName)
        {
            LogErrorMessage(typeof(System.ArgumentNullException).FullName, "Value cannot be null.", paramName);
            Panic();
        }

        public static void ThrowArgumentOutOfRangeException()
        {
            LogErrorMessage(typeof(System.ArgumentOutOfRangeException).FullName, "Specified argument was out of the range of valid values.");
            Panic();
        }

        public static void ThrowIndexOutOfRangeException()
        {
            LogErrorMessage(typeof(System.IndexOutOfRangeException).FullName, "Index was outside the bounds of the array.");
            Panic();
        }

        private static void LogErrorMessage(string exception, string message)
        {
            Debug.LogError($"[<color={COLOR_TAG}>{TAG}</color>] <color={COLOR_EXCEPTION}>{exception}</color>: {message}");
        }

        private static void LogErrorMessage(string exception, string message, string paramName)
        {
            Debug.LogError($"[<color={COLOR_TAG}>{TAG}</color>] <color={COLOR_EXCEPTION}>{exception}</color>: {message} (Parameter '<color={COLOR_PARAMETER}>{paramName}</color>')");
        }

        private static void Panic()
        {
            // Raise runtime Exception
            ((object)null).ToString();
        }
    }
}
