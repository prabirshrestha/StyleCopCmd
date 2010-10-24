using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.StyleCop;
using System.IO;
using System.Xml.Linq;

namespace StyleCop
{
    /// <summary>
    /// Program entry point for the StyleCop commandline
    /// </summary>
    public class Program
    {
        #region Public Static Methods

        /// <summary>
        /// Actual entry point
        /// </summary>
        /// <param name="args">Commandline parameters</param>
        public static void Main(string[] args)
        {
            StyleCopDriver d = new StyleCopDriver();
            if (!Configure(d, args))
            {
                return;
            }

            if (!ValidateConfiguration(d))
            {
                return;
            }

            d.Execute();
        }

        /// <summary>
        /// Configure our Driver from the commandline
        /// </summary>
        /// <param name="driver">StyleCopDriver to configure</param>
        /// <param name="arguments">Commandline Arguments</param>
        /// <returns>True if configuration works, false otherwise.</returns>
        public static bool Configure(StyleCopDriver driver, string[] arguments)
        {
            bool result = true;

            using (ParameterHandler p = new ParameterHandler(arguments))
            {
                p.AddSwitch("help", "Display parameter help", p.PrintHelp);
                p.AddSwitch("?", "Display parameter help", p.PrintHelp);

                p.AddMandatoryParameter("settings", "Specify the settings file to load", s => driver.SettingsFile = s);

                p.AddSwitch("cache", "Turn on caching", () => driver.CacheResults = true);
                p.AddSwitch("nocache", "Turn off caching", () => driver.CacheResults = false);

                p.AddParameter("path", "Add path to load addins", s => driver.AdditionalAddInPaths.Add(s));
                p.AddParameter("define", "Define constant", s => driver.DefineConstants.Add(s));

                p.AddParameter("project", "Stylecop project file", s => driver.ProjectFile = s);

                p.AddParameter("xml", "Save to xml file", s => driver.SaveXml = s);
                p.AddParameter("xsl", "Specify xsl file to transform xml", s => driver.Xsl = s);

                p.Default(s => driver.SourceFiles.Add(s));

                p.ParameterError +=
                    (s, a) =>
                    {
                        Console.WriteLine("Parameter Error: " + a.Message);
                        result = false;
                    };
            }

            return result;
        }

        /// <summary>
        /// Check that our configuration is valid
        /// </summary>
        /// <param name="driver">Driver to validate</param>
        /// <returns>True if valid, false otherwise.</returns>
        public static bool ValidateConfiguration(StyleCopDriver driver)
        {
            if (String.IsNullOrEmpty(driver.SettingsFile))
            {
                Console.WriteLine("No settings file specified (-settings)");
                return false;
            }

            return true;
        }

        #endregion
    }
}
