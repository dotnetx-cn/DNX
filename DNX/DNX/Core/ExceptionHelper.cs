using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Web.Services.Protocols;

namespace DNX.Core
{
    /// <summary>
    /// The TrueThrow method determines if its Boolean parameter is true and throws an Exception if it is; 
    /// The FalseThrow method determines whether its Boolean argument value is false, and if so, throws an exception. 
    /// </summary>
    public static class ExceptionHelper
    {
        #region Microsoft Windows Win32 API Extern
        [DllImport("Kernel32.dll", CharSet = CharSet.Ansi)]
        private static extern uint FormatMessage(uint dwFlags, IntPtr lpSource, uint dwMessageId, uint dwLanguageId, ref IntPtr lpBuffer, uint nSize, IntPtr Arguments);
        [DllImport("Kernel32.dll")]
        private static extern IntPtr LocalFree(IntPtr hMem);
        #endregion

        /// <summary>
        /// Determines whether the value of the specified conditional result is false, and if so throws a specific type of exception.
        /// </summary>
        /// <typeparam name="T">Specific exception types</typeparam>
        /// <param name="parseExpressionResult">expression</param>
        /// <param name="message">error message</param>
        /// <param name="messageParams">exception parmameters</param>
        public static void FalseThrow<T>(bool parseExpressionResult, string message, params object[] messageParams) where T : Exception
        {
            TrueThrow<T>(!parseExpressionResult, message, messageParams);
        }

        /// <summary>
        /// Determines whether the value of the specified conditional result is false, and if so throws a specific type of exception.
        /// </summary>
        /// <typeparam name="T">Specific exception types</typeparam>
        /// <param name="parseExpressionResult">expression</param>
        /// <param name="message">error message</param>
        /// <param name="messageParams">exception parmameters</param>
        public static void TrueThrow<T>(bool parseExpressionResult, string message, params object[] messageParams) where T : Exception
        {
            if (parseExpressionResult == true)
            {
                if (message == null)
                    throw new ArgumentNullException("message");

                object obj = Activator.CreateInstance(typeof(T));

                Type[] parameter = new Type[] { typeof(string) };

                ConstructorInfo constructorInfo = typeof(T).GetConstructor(
                    BindingFlags.Instance | BindingFlags.Public, null, CallingConventions.HasThis, parameter, null);

                object[] args = new object[] { string.Format(message, messageParams) };

                constructorInfo.Invoke(obj, args);

                throw (Exception)obj;
            }
        }

        /// <summary>
        /// From the Exception object, get the exception object where the exception actually occurred.
        /// </summary>
        /// <param name="err">Exception</param>
        /// <returns></returns>
        public static Exception GetRealException(Exception err)
        {
            Exception lastestEx = err;

            if (err is SoapException)
            {
                lastestEx = new Exception(GetSoapExceptionMessage(err), err);
            }
            else if (err is Win32Exception)
            {
                lastestEx = new Exception(GetWindowsExceptionMessage(err as Win32Exception), err);
            }
            else
            {
                while (err != null &&
                    (err is System.Web.HttpUnhandledException || err is System.Web.HttpException || err is TargetInvocationException))
                {
                    lastestEx = (err.InnerException != null) ? err.InnerException : err;

                    err = err.InnerException;
                }

            }

            return lastestEx;
        }

        /// <summary>
        /// Gets exception information in SoapException
        /// </summary>
        /// <param name="ex">SoapException</param>
        /// <returns></returns>
        public static string GetSoapExceptionMessage(Exception ex)
        {
            string strNewMsg = ex.Message;

            if (ex is SoapException)
            {
                int i = strNewMsg.LastIndexOf("--> ");

                if (i > 0)
                {
                    strNewMsg = strNewMsg.Substring(i + 4);
                    i = strNewMsg.IndexOf(": ");

                    if (i > 0)
                    {
                        strNewMsg = strNewMsg.Substring(i + 2);

                        i = strNewMsg.IndexOf("\n   ");

                        strNewMsg = strNewMsg.Substring(0, i);
                    }
                }
            }

            return strNewMsg;
        }

        /// <summary>
        /// Obtain the Win32 exception description of Windows
        /// </summary>
        /// <param name="err">The exception to Win32Exception to get</param>
        /// <returns></returns>
        public static string GetWindowsExceptionMessage(Win32Exception err)
        {
            return GetWindowsExceptionMessage((uint)err.ErrorCode);
        }

        /// <summary>
        /// Gets the specified Windows exception description
        /// </summary>
        /// <param name="exceptionCode">Windows Error Code</param>
        /// <returns></returns>
        public static string GetWindowsExceptionMessage(uint exceptionCode)
        {
            IntPtr result = IntPtr.Zero;

            if (FormatMessage((uint)0x1300, IntPtr.Zero, exceptionCode, 0, ref result, 0, IntPtr.Zero) == uint.Parse("0"))
                return string.Empty;

            string strResult = Marshal.PtrToStringAnsi(result);

            result = LocalFree(result);

            return strResult;
        }
    }
}
