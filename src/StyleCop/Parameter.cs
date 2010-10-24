
namespace StyleCop
{
    /// <summary>
    /// Represents a command line parameter
    /// </summary>
    public class Parameter
    {
        #region Public Properties

        /// <summary>
        /// Gets the name of this parameter
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the description of this parameter
        /// </summary>
        public string Description
        {
            get;
            private set;
        }

        #endregion
        #region Public Methods

        /// <summary>
        /// Create a parameter
        /// </summary>
        /// <param name="name">Name - as expected on the commandline</param>
        /// <param name="description">Description of this parameter</param>
        public Parameter(string name, string description)
        {
            Name = name;
            Description = description;
        }

        /// <summary>
        /// Validate this parameter
        /// </summary>
        /// <param name="message">Message to display, if invalid</param>
        /// <returns>True if valid, false otherwise.</returns>
        public virtual bool Validate(out string message)
        {
            message = string.Empty;
            return true;
        }
        
        #endregion
    }
}
