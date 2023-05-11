using System.Data.SqlClient;
using DNX.Core;

namespace DNX.Data.DbAccess
{
    /// <summary>
    /// SQL exception handling help class
    /// </summary>
    internal class SQLExceptionHelper
    {
        /// <summary>
        /// SQL exception analysis processing
        /// </summary>
        /// <param name="err"></param>
        /// <returns></returns>
        public static Result SQLExceptionAnalyze(SqlException err)
        {
            if (err == null)
                return new Result(ErrorType.OperationFailure, "Data execution exception", "Exceptions that the framework failed to handle.");

            switch (err.Number)
            {
                case 50000:
                    string msg = string.Empty;
                    int dwIndex = ExceptionHelper.GetRealException(err).Message.IndexOf("\r\n");
                    msg = (dwIndex == -1) ? ExceptionHelper.GetRealException(err).Message :
                        ExceptionHelper.GetRealException(err).Message.Substring(0, dwIndex);

                    return new Result(ErrorType.OperationFailure, "The data engine is abnormal. Procedure", msg);
                case 547:
                    if (ExceptionHelper.GetRealException(err).Message.StartsWith("DELETE"))
                        return new Result(ErrorType.OperationFailure, "The data engine is abnormal. Procedure", "Because the execution statement does not meet the operation conditions, the data to be deleted must be isolated data items!");

                    if (ExceptionHelper.GetRealException(err).Message.StartsWith("INSERT"))
                        return new Result(ErrorType.OperationFailure, "The data engine is abnormal. Procedure", "Because the statement does not meet operation conditions, the unique attribute item must be unique.");

                    if (ExceptionHelper.GetRealException(err).Message.StartsWith("UPDATE"))
                        return new Result(ErrorType.OperationFailure, "The data engine is abnormal. Procedure", "The execution statement did not meet the operation conditions");

                    return new Result(ErrorType.OperationFailure, "The data engine is abnormal. Procedure", ExceptionHelper.GetRealException(err).Message);
                case 2627:
                case 2601:
                    return new Result(ErrorType.OperationFailure, "The data engine is abnormal. Procedure", "The only property that is currently added is the same as some existing data, which is not allowed.");
                default:
                    {
                        if (err.Message.Contains("Timeout Expired") || err.Message.Contains("超时"))
                            return new Result(ErrorType.OperationFailure, "The data engine is abnormal. Procedure", "The database operation timed out");
                        else
                            return new Result(ErrorType.OperationFailure, "The data engine is abnormal. Procedure", ExceptionHelper.GetRealException(err).Message);
                    }
            }
        }
    }
}
