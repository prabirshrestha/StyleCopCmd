using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SourceAnalysis;
using System.Xml.Linq;
using System.IO;
using System.Xml.Xsl;
using System.Xml;

namespace StyleCop
{
    /// <summary>
    /// Helper class used to drive StyleCop from the commandline
    /// </summary>
    public class StyleCopDriver
    {
        #region Public Properties
        #region SettingsFile Property

        /// <summary>
        /// Gets or sets a FilePath to a Settings.CodeAnalysis file.
        /// </summary>
        public string SettingsFile
        {
            get
            {
                return mOverrideSettingsFile;
            }

            set
            {
                if (!Object.Equals(mOverrideSettingsFile, value))
                {
                    mOverrideSettingsFile = value;
                }
            }
        }

        /// <summary>
        /// Storage for the SettingsFile property
        /// </summary>
        private string mOverrideSettingsFile = string.Empty;

        #endregion
        #region CacheResults Property

        /// <summary>
        /// Gets or sets a value indicating whether to generate and use cached results
        /// </summary>
        public bool CacheResults
        {
            get
            {
                return mCacheResults;
            }

            set
            {
                if (!Object.Equals(mCacheResults, value))
                {
                    mCacheResults = value;
                }
            }
        }

        /// <summary>
        /// Storage for the CacheResults property
        /// </summary>
        private bool mCacheResults = false;

        #endregion
        #region AdditionalAddInPaths Property

        /// <summary>
        /// Gets a list of Paths to search for additional AddIns
        /// </summary>
        public IList<string> AdditionalAddInPaths
        {
            get { return mAdditionalAddInPaths; }
        }

        /// <summary>
        /// Storage for the AdditionalAddInPaths property
        /// </summary>
        private List<string> mAdditionalAddInPaths = new List<string>();

        #endregion
        #region DefineConstants Property

        /// <summary>
        /// Gets a list of defined constants
        /// </summary>
        public IList<string> DefineConstants
        {
            get { return mDefineConstants; }
        }

        /// <summary>
        /// Storage for the DefineConstants property
        /// </summary>
        private IList<string> mDefineConstants = new List<string>();

        #endregion
        #region ProjectFile Property

        /// <summary>
        /// Gets or sets the file path to a project file
        /// </summary>
        public string ProjectFile
        {
            get
            {
                return mProjectFile;
            }

            set
            {
                if (!Object.Equals(mProjectFile, value))
                {
                    mProjectFile = value;
                }
            }
        }

        /// <summary>
        /// Storage for the ProjectFile poperty
        /// </summary>
        private string mProjectFile = string.Empty;

        #endregion
        #region SourceFiles Property

        /// <summary>
        /// Gets a list of Source files to be analysed
        /// </summary>
        public IList<string> SourceFiles
        {
            get { return mSourceFiles; }
        }

        /// <summary>
        /// Storage for the SourceFiles property
        /// </summary>
        private List<string> mSourceFiles = new List<string>();

        #endregion
        #region SaveXml Property

        /// <summary>
        /// Gets or sets the file path to save analysis results as Xml.
        /// </summary>
        public string SaveXml
        {
            get
            {
                return mSaveXml;
            }

            set
            {
                if (!Object.Equals(mSaveXml, value))
                {
                    mSaveXml = value;
                }
            }
        }

        /// <summary>
        /// Storage for the SaveXml directory.
        /// </summary>
        private string mSaveXml = string.Empty;

        #endregion
        #region SaveXml Property

        /// <summary>
        /// Gets or sets the file path for an Xsl file to transform the output
        /// </summary>
        public string Xsl
        {
            get
            {
                return mXsl;
            }

            set
            {
                if (!Object.Equals(mXsl, value))
                {
                    mXsl = value;
                }
            }
        }

        /// <summary>
        /// Storage for the Xsl file
        /// </summary>
        private string mXsl = string.Empty;

        #endregion
        #region ParameterErrors Property

        /// <summary>
        /// Gets or sets a value indicating whether any problems occurred 
        /// during parsing of parameters.
        /// </summary>
        public bool ParameterErrors
        {
            get
            {
                return mParameterErrors;
            }

            set
            {
                if (!Object.Equals(mParameterErrors, value))
                {
                    mParameterErrors = value;
                }
            }
        }

        /// <summary>
        /// Storage for the ParameterErrors method.
        /// </summary>
        private bool mParameterErrors = false;

        #endregion
        #endregion
        #region Public Methods

        /// <summary>
        /// Create a new Driver
        /// </summary>
        public StyleCopDriver()
        {
            // Nothing
        }

        /// <summary>
        /// Execute StyleCop as configured by this Helper instance
        /// </summary>
        public void Execute()
        {
            SourceAnalysisConsole console = new SourceAnalysisConsole(SettingsFile, CacheResults, null, mAdditionalAddInPaths, true);
            Configuration configuration = new Configuration(DefineConstants.ToArray());

            List<CodeProject> projects = new List<CodeProject>();

            CodeProject defaultProject = new CodeProject(ProjectFile.GetHashCode(), ProjectFile, configuration);
            projects.Add(defaultProject);

            foreach (string s in SourceFiles)
            {
                string file = Path.GetFullPath(s);
                string extension = Path.GetExtension(file);

                if (extension.Equals(".csproj", StringComparison.InvariantCultureIgnoreCase))
                {
                    CodeProject p = CreateProject(file, console, configuration);
                    projects.Add(p);
                }
                else
                {
                    console.Core.Environment.AddSourceCode(defaultProject, s, null);
                }
            }

            try
            {
                console.OutputGenerated += new EventHandler<OutputEventArgs>(ConsoleOutput);
                console.ViolationEncountered += new EventHandler<ViolationEventArgs>(ViolationEncountered);
                console.Start(projects.ToArray(), true);
            }
            finally
            {
                console.OutputGenerated -= new EventHandler<OutputEventArgs>(ConsoleOutput);
                console.ViolationEncountered -= new EventHandler<ViolationEventArgs>(ViolationEncountered);
            }

            SaveToXml();
        }

        #endregion
        #region Private Methods

        /// <summary>
        /// Create a separate CodeProject for a specified .csproj file
        /// </summary>
        /// <param name="projectFilePath">File path to a .csproj file</param>
        /// <param name="console">Reference to our master console</param>
        /// <param name="configuration">Configuration from the commandline</param>
        /// <returns>Newly constructed CodeProject instance</returns>
        private CodeProject CreateProject(string projectFilePath, SourceAnalysisConsole console, Configuration configuration)
        {
            string directory = Path.GetDirectoryName(projectFilePath);

            CodeProject project = new CodeProject(projectFilePath.GetHashCode(), projectFilePath, configuration);
            XDocument projectFile = XDocument.Load(projectFilePath);

            foreach (XElement x in projectFile.Descendants().Where(x => x.Name.LocalName.Equals("Compile")))
            {
                string file = x.Attribute("Include").Value;
                string filePath = Path.Combine(directory, file);
                console.Core.Environment.AddSourceCode(project, filePath, null);
            }

            return project;
        }

        /// <summary>
        /// Save log result to Xml if requested.
        /// </summary>
        private void SaveToXml()
        {
            if (!String.IsNullOrEmpty(mSaveXml))
            {
                string saveTo = mSaveXml;
                if (!Path.IsPathRooted(saveTo))
                {
                    string dir = Path.GetFullPath(Path.GetDirectoryName(SettingsFile));
                    saveTo = Path.Combine(dir, mSaveXml);
                }

                IEnumerable<Rule> rules = mViolations
                    .Select(violations => violations.Rule)
                    .Distinct()
                    .OrderBy(r => r.CheckId);

                var fileViolations =                     
                    from v in mViolations
                    group v by v.SourceCode.Name into f
                    select new XElement(
                        "File",
                        new XAttribute("Name", f.Key),
                        from v in f
                        orderby v.Line
                        select new XElement(
                            "Violation",
                            new XAttribute("CheckId", v.Rule.CheckId),
                            new XAttribute("message", v.Message),
                            new XAttribute("line", v.Line)));

                XElement filesElement = new XElement("Files", fileViolations);

                var ruleSummary = 
                    from r in rules
                    select new XElement(
                        "Rule",
                        new XAttribute("CheckId", r.CheckId ?? string.Empty),
                        new XAttribute("Name", r.Name ?? string.Empty),
                        new XAttribute("Description", r.Description ?? string.Empty),
                        String.IsNullOrEmpty(r.RuleGroup) ? null : new XAttribute("Group", r.RuleGroup));

                XElement rulesElement = new XElement("Rules", ruleSummary);

                XDocument xmlLog = new XDocument(new XElement("StyleCop", filesElement, rulesElement));

                if (!string.IsNullOrEmpty(Xsl))
                {
                    if (!File.Exists(Xsl))
                    {
                        Console.WriteLine("Xsl File {0} not found, saving raw Xml to {1}", Xsl, saveTo);
                    }
                    else
                    {
                        using (FileStream log = new FileStream(saveTo, FileMode.Create))
                        {
                            Console.WriteLine("Loading Stylesheet from " + Xsl);
                            XslCompiledTransform xslTransformer = new XslCompiledTransform();
                            xslTransformer.Load(Xsl);
                            Console.WriteLine("Applying transformation");
                            xslTransformer.Transform(xmlLog.CreateReader(), null, log);
                        }
                    }
                }
                else
                {
                    xmlLog.Save(saveTo);
                }
            }
        }

        #endregion
        #region Private Event Handlers

        /// <summary>
        /// Handle Console output from StyleCop
        /// </summary>
        /// <param name="sender">Event source</param>
        /// <param name="e">Arguments supplied</param>
        private void ConsoleOutput(object sender, OutputEventArgs e)
        {
            Console.Write(e.Output);
        }

        /// <summary>
        /// Handle Violation reporting from StyleCop
        /// </summary>
        /// <param name="sender">Event source</param>
        /// <param name="e">Arguments supplied</param>
        private void ViolationEncountered(object sender, ViolationEventArgs e)
        {
            mViolations.Add(e.Violation);
        }

        #endregion
        #region Private Members

        /// <summary>
        /// Storage list for violations found.
        /// </summary>
        private List<Violation> mViolations = new List<Violation>();

        #endregion
    }
}