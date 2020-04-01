using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AssemblyHelpers
{
    /// <summary>
    /// BBernard
    /// Generic helper to provide simplified convenience methods to support the forced eager loading of 
    ///         local class libraries in the same folder as the specified Assembly to ensure that related 
    ///         assemblies are loaded and can be available to be searched if needed.
    /// </summary>
    public class AssemblyLoadHelper
    {
        public const string CLASS_LIBRARY_FILE_EXTENSION = "dll";

        /// <summary>
        /// Force load of all local class library files that exist in the same folder as the assembly specified!
        /// </summary>
        /// <param name="rootAssembly"></param>
        /// <returns></returns>
        public static List<Assembly> ForceLoadClassLibraries(Assembly rootAssembly, bool searchFileSystemAssembliesRecursively = false)
        {
            var rootFolder = new FileInfo(rootAssembly.Location).Directory;
            return ForceLoadClassLibraries(rootFolder, searchFileSystemAssembliesRecursively);
        }

        /// <summary>
        /// Force load of all local class library files that exist in the root folder specified!
        /// </summary>
        /// <param name="rootFolder"></param>
        /// <returns></returns>
        public static List<Assembly> ForceLoadClassLibraries(DirectoryInfo rootFolder, bool searchFoldersRecursively = false)
        {
            var searchOption = searchFoldersRecursively ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var localClassLibraryFilePaths = rootFolder.GetFiles($"*.{CLASS_LIBRARY_FILE_EXTENSION}", searchOption).ToList();
            return ForceLoadClassLibraries(localClassLibraryFilePaths);
        }

        /// <summary>
        /// Force load of all local class library files in the list specified. This overload puts the control of creating the list
        /// of assembly file paths in control of the caller with custom logic!
        /// </summary>
        /// <param name="rootAssembly"></param>
        /// <returns></returns>
        public static List<Assembly> ForceLoadClassLibraries(List<FileInfo> filePathsList)
        {
            var loadedAssemblyList = new List<Assembly>();

            if (filePathsList == null || !filePathsList.Any()) return loadedAssemblyList;

            //BBernard
            //Eager/Greedily load all local class library references; the default .Net behaviour is to Lazy load on-demand only.
            //NOTE: We must force them to load so that we can access their types via Reflection!  Not all assemblies will be visible/avialable 
            //      to Reflection API's until they are first loaded up-front here...
            //NOTE: Adapted and optimized from code found in the best viable info. on doing this at the StackOverflow post here:
            //      https://stackoverflow.com/a/2384679/7293142
            //NOTE: Dynamic assemblies cannot be force loaded (exceptions will occur) therefore we must filter them out!
            var appDomainLoadedAssemblyPaths = AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic).Select(a => a.Location);
            var assemblyNamesToLoad = filePathsList
                                        .Where(f => f.Extension.Equals($".{CLASS_LIBRARY_FILE_EXTENSION}", StringComparison.OrdinalIgnoreCase) && f.Exists)
                                        .Select(f => f.FullName)
                                        .Except(appDomainLoadedAssemblyPaths, StringComparer.OrdinalIgnoreCase)
                                        .Select(p => AssemblyName.GetAssemblyName(p));

            foreach (var assemblyName in assemblyNamesToLoad)
            {
                var newAssembly = AppDomain.CurrentDomain.Load(assemblyName);
                loadedAssemblyList.Add(newAssembly);
            }

            //Return the list of all assemblies loaded by this process...
            return loadedAssemblyList;
        }
    }
}
