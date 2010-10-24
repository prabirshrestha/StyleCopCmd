using System;
using System.Collections.Generic;
using System.Text;

namespace StyleCop
{
    /// <summary>
    /// Cribbed from Oren Eini (ayende.com), provides an action to execute at the end of a block. 
    /// </summary>
    /// See http://www.ayende.com/Blog/archive/8065.aspx for details.
    /// 
    public class DisposableAction : IDisposable
    {
        /// <summary>
        /// Create an action ready to execute
        /// </summary>
        /// <param name="action">Action method</param>
        public DisposableAction(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            mAction = action;
        }

        /// <summary>
        /// Fire the contained action.
        /// </summary>
        public void Dispose()
        {
            mAction();
        }

        /// <summary>
        /// The action to trigger when this container is Disposed
        /// </summary>
        private Action mAction;
    }
}