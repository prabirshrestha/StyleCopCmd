using System;
using System.Collections.Generic;
using System.Linq;

namespace StyleCop
{
    /// <summary>
    /// Utility class to handle command line arguments
    /// </summary>
    public class ParameterHandler : IDisposable
    {
        #region Public Properties

        /// <summary>
        /// Gets a value indicating whether parameters were valid
        /// </summary>
        public bool Valid
        {
            get;
            private set;
        }

        #endregion
        #region Public Methods

        /// <summary>
        /// Constructor accepting the commandline arguments
        /// </summary>
        /// Create this from your programs Main() method, passing the 
        /// arguments that need to be parsed.
        /// <param name="arguments">Actual commandline arguments</param>
        public ParameterHandler(string[] arguments)
        {
            mArguments = arguments;
        }

        /// <summary>
        /// Configure the default action when an argument has no controlling 
        /// switch
        /// </summary>
        /// <param name="action">Action to take</param>
        public void Default(Action<string> action)
        {
            mDefault = action;
        }

        /// <summary>
        /// Configure an action for a boolean switch
        /// </summary>
        /// <param name="name">Name of switch (without any leading punctuation)</param>
        /// <param name="description">Description to display as guidance on this parameter.</param>
        /// <param name="action">Action to invoke</param>
        public void AddSwitch(string name, string description, Action action)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            mSwitchParameters.Add(new SwitchParameter(name, description, action));
        }

        /// <summary>
        /// Configure an action for a parameter
        /// </summary>
        /// <param name="name">Name of parameter (without any leading punctuation)</param>
        /// <param name="description">Description to display as guidance on this parameter.</param>
        /// <param name="action">Action to invoke</param>
        public void AddParameter(string name, string description, Action<string> action)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            if (string.IsNullOrEmpty(description))
            {
                throw new ArgumentNullException("description");
            }

            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            mArgumentParameters.Add(new ArgumentParameter(name, description, false, action));
        }

        /// <summary>
        /// Configure an action for a parameter
        /// </summary>
        /// <param name="name">Name of parameter (without any leading punctuation)</param>
        /// <param name="description">Description to display as guidance on this parameter.</param>
        /// <param name="action">Action to invoke</param>
        public void AddMandatoryParameter(string name, string description, Action<string> action)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            if (string.IsNullOrEmpty(description))
            {
                throw new ArgumentNullException("description");
            }

            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            mArgumentParameters.Add(new ArgumentParameter(name, description, true, action));
        }

        /// <summary>
        /// Display help for guidance of the user
        /// </summary>
        public void PrintHelp()
        {
            foreach (ArgumentParameter p in mArgumentParameters)
            {
                string s = String.Format("{2} -{0,-20} {1}", p.Name, p.Description, p.Mandatory ? "*" : " ");
                Console.WriteLine(s);
            }

            foreach (SwitchParameter p in mSwitchParameters)
            {
                string s = String.Format("  -{0,-20} {1}", p.Name, p.Description);
                Console.WriteLine(s);
            }
        }

        #endregion
        #region Events
        #region ParameterError Event

        /// <summary>
        /// Event triggered when an unexpected parameter is found
        /// </summary>
        public event EventHandler<ParameterErrorEventArgs> ParameterError
        {
            add { mParameterError += value; }
            remove { mParameterError -= value; }
        }

        /// <summary>
        /// Trigger method for the ParameterError event
        /// </summary>
        /// <param name="message">Message describing the error</param>
        protected void OnParameterError(string message)
        {
            EventHandler<ParameterErrorEventArgs> handlers = mParameterError;
            if (handlers != null)
            {
                ParameterErrorEventArgs args = new ParameterErrorEventArgs(message);
                mParameterError(this, args);
            }
        }

        /// <summary>
        /// Storage for the ParameterError event
        /// </summary>
        private EventHandler<ParameterErrorEventArgs> mParameterError;

        #endregion
        #endregion
        #region IDisposable Implementation

        /// <summary>
        /// Dispose of this ParameterHandler
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Implementation of the Dispose() pattern
        /// </summary>
        /// <param name="disposing">True if we are Disposing, false if we are Finalising</param>
        protected void Dispose(bool disposing)
        {
            if (!mDisposed)
            {
                if (disposing)
                {
                    DisposeManaged();
                }

                DisposeUnmanaged();
            }

            mDisposed = true;
        }

        /// <summary>
        /// Has this instance been disposed already?
        /// </summary>
        private bool mDisposed = false;

        /// <summary>
        /// Finaliser used to ensure disposal
        /// </summary>
        ~ParameterHandler()
        {
            Dispose(false);
        }

        /// <summary>
        /// Dispose any managed resources
        /// </summary>
        /// We use this as a hook point to do our parameter parsing
        protected virtual void DisposeManaged()
        {
            Parse();
        }

        /// <summary>
        /// Dispose any unmanaged resources
        /// </summary>
        protected virtual void DisposeUnmanaged()
        {
            // Dispose of unmanaged resources here
        }

        #endregion
        #region Private Methods

        /// <summary>
        /// Parse the commandline arguments for configured switches and 
        /// parameters.
        /// </summary>
        private void Parse()
        {
            int index = 0;
            while (index < mArguments.Length)
            {
                string parameter = mArguments[index];

                if (parameter.StartsWith("-") || parameter.StartsWith("/"))
                {
                    string name = parameter.Remove(0, 1).ToLowerInvariant();

                    SwitchParameter s = mSwitchParameters.FirstOrDefault(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
                    if (s != null)
                    {
                        s.Execute();
                    }
                    else if (index < mArguments.Length - 1)
                    {
                        ArgumentParameter a = mArgumentParameters.FirstOrDefault(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

                        if (a != null)
                        {
                            a.Execute(mArguments[index + 1]);
                            index++;
                        }
                        else
                        {
                            OnParameterError(String.Format("Parameter {0} not recognised", parameter));
                        }
                    }
                    else
                    {
                        OnParameterError(String.Format("Parameter {0} cannot appear at end of command line", parameter));
                    }
                }
                else
                {
                    mDefault(parameter);
                }

                index++;
            }

            foreach (ArgumentParameter a in mArgumentParameters)
            {
                string message;
                if (!a.Validate(out message))
                {
                    OnParameterError(message);
                }
            }

            foreach (SwitchParameter s in mSwitchParameters)
            {
                string message;
                if (!s.Validate(out message))
                {
                    OnParameterError(message);
                }
            }
        }

        #endregion
        #region Private Members

        /// <summary>
        /// Commandline arguments as passed to our Main() method
        /// </summary>
        private string[] mArguments;

        /// <summary>
        /// Default action to take for regular parameters
        /// </summary>
        private Action<string> mDefault;

        /// <summary>
        /// Storage for all switch parameters
        /// </summary>
        private List<SwitchParameter> mSwitchParameters = new List<SwitchParameter>();

        /// <summary>
        /// Storage for all argument parameters
        /// </summary>
        private List<ArgumentParameter> mArgumentParameters = new List<ArgumentParameter>();

        #endregion
    }
}