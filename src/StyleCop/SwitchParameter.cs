using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StyleCop
{
    /// <summary>
    /// A Command line switch parameter 
    /// </summary>
    public class SwitchParameter : Parameter
    {
        #region Public Methods

        /// <summary>
        /// Create a parameter switch
        /// </summary>
        /// <param name="name">Name expected on the commandline</param>
        /// <param name="description">Description to guide the user</param>
        /// <param name="action">Action to trigger when the parameter is found</param>
        public SwitchParameter(string name, string description, Action action)
            : base(name, description)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            mAction = action;
        }

        /// <summary>
        /// Execute the action configured on for this parameter
        /// </summary>
        public void Execute()
        {
            mAction();
        }

        #endregion
        #region Private Members

        /// <summary>
        /// Storage for the action we invoke when this parameter is found
        /// </summary>
        private Action mAction;

        #endregion
    }
}