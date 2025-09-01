using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using XrmToolBox.Extensibility;
using XrmToolBox.Extensibility.Interfaces;

namespace DataverseUserSecurity
{
    // Do not forget to update version number and author (company attribute) in AssemblyInfo.cs class
    // To generate Base64 string for Images below, you can use https://www.base64-image.de/
    [Export(typeof(IXrmToolBoxPlugin)),
        ExportMetadata("Name", "Dataverse Users, Security roles, Teams and Teams security roles"),
        ExportMetadata("Description", "This tool is used to retrieve and search for the Users Details, User security roles, User Teams and User Teams security roles"),
        // Please specify the base64 content of a 32x32 pixels image
        ExportMetadata("SmallImageBase64", "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"), 
        // Please specify the base64 content of a 80x80 pixels image
        ExportMetadata("BigImageBase64", "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"),
        ExportMetadata("BackgroundColor", "Purple"),
        ExportMetadata("PrimaryFontColor", "White"),
        ExportMetadata("SecondaryFontColor", "Gray")]

    public class MyPlugin : PluginBase
    {
        public override IXrmToolBoxPluginControl GetControl()
        {
            return new MyPluginControl();
        }

        /// <summary>
        /// Constructor 
        /// </summary>
        public MyPlugin()
        {
            // If you have external assemblies that you need to load, uncomment the following to 
            // hook into the event that will fire when an Assembly fails to resolve
            // AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(AssemblyResolveEventHandler);
        }

        /// <summary>
        /// Event fired by CLR when an assembly reference fails to load
        /// Assumes that related assemblies will be loaded from a subfolder named the same as the Plugin
        /// For example, a folder named Sample.XrmToolBox.MyPlugin 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private Assembly AssemblyResolveEventHandler(object sender, ResolveEventArgs args)
        {
            Assembly loadAssembly = null;
            Assembly currAssembly = Assembly.GetExecutingAssembly();

            // base name of the assembly that failed to resolve
            var argName = args.Name.Substring(0, args.Name.IndexOf(","));

            // check to see if the failing assembly is one that we reference.
            List<AssemblyName> refAssemblies = currAssembly.GetReferencedAssemblies().ToList();
            var refAssembly = refAssemblies.Where(a => a.Name == argName).FirstOrDefault();

            // if the current unresolved assembly is referenced by our plugin, attempt to load
            if (refAssembly != null)
            {
                // load from the path to this plugin assembly, not host executable
                string dir = Path.GetDirectoryName(currAssembly.Location).ToLower();
                string folder = Path.GetFileNameWithoutExtension(currAssembly.Location);
                dir = Path.Combine(dir, folder);

                var assmbPath = Path.Combine(dir, $"{argName}.dll");

                if (File.Exists(assmbPath))
                {
                    loadAssembly = Assembly.LoadFrom(assmbPath);
                }
                else
                {
                    throw new FileNotFoundException($"Unable to locate dependency: {assmbPath}");
                }
            }

            return loadAssembly;
        }
    }
}