using System;
using System.Web;

namespace DNX.Core
{
    /// <summary>
    /// Application environment handles operation classes
    /// </summary>
    public static class EnvironmentHelper
    {
        public enum InstanceMode
        {
            /// <summary>
            /// Windows
            /// </summary>
            Windows,
            /// <summary>
            /// Web
            /// </summary>
            Web
        }

        /// <summary>
        /// Get the properties of whether the current application is a Web application (Windows/Web)
        /// </summary>
        public static InstanceMode Mode
        {
            get
            {
                if (EnvironmentHelper.CheckIsWebApplication())
                    return InstanceMode.Web;
                else
                    return InstanceMode.Windows;
            }
        }

        /// <summary>
        /// Gets the version number of the currently executed code set
        /// </summary>
        /// <returns></returns>
        public static string GetAssemblyVersion()
        {
            return GetAssemblyVersion(System.Reflection.Assembly.GetExecutingAssembly().FullName);
        }

        /// <summary>
        /// Gets the specified code set version number
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static string GetAssemblyVersion(string assembly)
        {
            try
            {
                return System.Reflection.Assembly.Load(assembly).GetName().Version.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        #region Private
        private static bool CheckIsWebApplication()
        {
            bool isWebApplication = false;

            AppDomain domain = AppDomain.CurrentDomain;

            try
            {
                if (domain.ShadowCopyFiles)
                    isWebApplication = (HttpContext.Current != null);
            }
            catch
            {
            }

            return isWebApplication;
        }
        #endregion
    }
}
