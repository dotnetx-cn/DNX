using DNX.Core;
using System;

namespace DNX.Data
{
    /// <summary>
    /// Data execution error type
    /// </summary>
    public enum ErrorType
    {
        /// <summary>
        /// Data Validate Failure
        /// </summary>
        DataValidateFailure = 110,
        /// <summary>
        /// Data Design Error
        /// </summary>
        DataDesignError = 0x7D0,
        /// <summary>
        /// None
        /// </summary>
        None = 0,
        /// <summary>
        /// Operation failure
        /// </summary>
        OperationFailure = 300,
        /// <summary>
        /// Operation Timeout
        /// </summary>
        OperationTimeout = 350,
        /// <summary>
        /// Operation Warning
        /// </summary>
        OperationWarning = 290,
        /// <summary>
        /// Support Failed
        /// </summary>
        SupportFailed = 400
    }

    [Serializable]
    public class Result
    {
        #region constructor
        public Result()
        {
            this.Error = ErrorType.None;
            this.ErrorItem = string.Empty;
            this.ErrorExplanation = string.Empty;
            this.Succeed = true;
        }

        public Result(object data)
        {
            this.Error = ErrorType.None;
            this.ErrorItem = string.Empty;
            this.ErrorExplanation = string.Empty;

            if (data is ErrorType)
            {
                this.Succeed = false;
                this.Error = (ErrorType)data;
            }
            else
            {
                this.Succeed = true;
                this.Data = data;
            }
        }

        public Result(ErrorType errorType, string errItem, string explanation)
            : this(errorType)
        {
            this.ErrorItem = errItem;
            this.ErrorExplanation = explanation;
        }
        #endregion

        /// <summary>
        /// Execution result data
        /// </summary>
        public object Data
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets data for the specified data type
        /// </summary>
        /// <typeparam name="T">The data type returned</typeparam>
        /// <returns></returns>
        public T GetData<T>()
        {
            return DataConvertHelper.ChangeType<object, T>(this.Data);
        }

        /// <summary>
        /// Error
        /// </summary>
        public ErrorType Error
        {
            get;
            private set;
        }
        /// <summary>
        /// ErrorExplanation
        /// </summary>
        public string ErrorExplanation
        {
            get;
            private set;
        }

        /// <summary>
        /// ErrorItem
        /// </summary>
        public string ErrorItem
        {
            get;
            private set;
        }
        /// <summary>
        /// Indicates that the current operation is executed successfully
        /// </summary>
        public bool Succeed
        {
            get;
            private set;
        }
    }

    /// <summary>
    /// Results
    /// </summary>
    public class Results
    {
        /// <summary>
        /// Indicates that the current operation is executed successfully
        /// </summary>
        public static Result Succeed
        {
            get
            {
                return new Result();
            }
        }
    }
}
