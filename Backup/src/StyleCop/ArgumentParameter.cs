using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StyleCop
{
    /// <summary>
    /// Helper class used to define and parse commandline parameters that take 
    /// an argument.
    /// </summary>
    public class ArgumentParameter : Parameter
    {
        #region Public Properties

        /// <summary>
        /// Gets a value indicating whether the parameter is required
        /// </summary>
        public bool Mandatory
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether the conditions for this parameter have been satisfied
        /// </summary>
        public bool Used
        {
            get;
            private set;
        }

        #endregion
        #region Public Methods
        
        /// <summary>
        /// Create an ArgumentParameter to parse the commandline
        /// </summary>
        /// <param name="name">Expected commandline parameter</param>
        /// <param name="description">Description for users</param>
        /// <param name="mandatory">Is this a mandatory parameter?</param>
        /// <param name="action">Action to take when the parameter is found</param>
        public ArgumentParameter(string name, string description, bool mandatory, Action<string> action)
            : base(name, description)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            mAction = action;
            Mandatory = mandatory;
        }

        /// <summary>
        /// Execute the action configured on this action
        /// </summary>
        /// <param name="argument">Value supplied on the commandline</param>
        public void Execute(string argument)
        {
            mAction(argument);
            Used = true;
        }

        /// <summary>
        /// Validate the state of this parameter
        /// </summary>
        /// <param name="message">Message to display if invalid</param>
        /// <returns>True if valid, false if not.</returns>
        public override bool Validate(out string message)
        {
            if (Mandatory && !Used)
            {
                message = String.Format("Mandatory parameter {0} not specified", Name);
                return false;
            }

            message = string.Empty;
            return true;
        }

        #endregion
        #region Private Members

        /// <summary>
        /// Storage for the action to invoke when this parameter is found.
        /// </summary>
        private Action<string> mAction;

        #endregion
    }
}
