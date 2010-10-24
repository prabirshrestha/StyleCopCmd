using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StyleCop
{
    /// <summary>
    /// Arguments for the ParameterError event
    /// </summary>
    public class ParameterErrorEventArgs : EventArgs
    {
        #region Public Properties

        /// <summary>
        /// Gets Message describing the parameter error
        /// </summary>
        public string Message
        {
            get;
            private set;
        }

        #endregion
        #region Public Methods

        /// <summary>
        /// Constructor for this EventArgs
        /// </summary>
        /// <param name="message">Content for Message property</param>
        public ParameterErrorEventArgs(string message)
        {
            Message = message;
        }

        #endregion
    }
}
