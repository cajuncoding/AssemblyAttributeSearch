using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AssemblyHelpers
{
    /// <summary>
    /// BBernard
    /// Generic helper to provide greatly simplified convenience methods for dynamically finding, filtering, 
    ///     and initialize classes based on Custom Attributes. You can search for all instances of an Attribute type
    ///     and specify an optional Class/Interface filter also to only return types that implement the given class/interface.
    ///     The results will have the details of the Attribute found, as well as the class type for the calling code to use
    ///     (e.g. process, instantiate dynamically, etc.).
    /// It also provides an internal static caching mechanism to ensure that multiple calls for the same search do not result
    ///     unnecessary Reflection processing becuase the results cannot change after compilation; this will meet 99% of the use-cases
    ///     that most developers will use Custom Attributes for.
    /// NOTE: This supports the forced eager loading of local class libraries in the same folder as the 
    ///         specified Assembly to ensure that related assemblies are loaded and can be available to be searched if needed.
    /// NOTE: This helper has no coupling to any other business logic, and is intended ONLY to encapsulate the work
    ///         needed to work with the .Net reflection and processing of attributes and related assemblies.
    /// </summary>
    public static class AssemblyAttributeSearch<TAttribute> where TAttribute : Attribute
    {
        private static readonly ConcurrentDictionary<string, Lazy<List<AttributedClassInfo<TAttribute>>>> attributedClassCache = new ConcurrentDictionary<string, Lazy<List<AttributedClassInfo<TAttribute>>>>();

        /// <summary>
        /// Convenience Method for searching through all Types in the Application Domain for the specified Class Type 
        /// & Attribute generic type arguments.
        /// </summary>
        /// <typeparam name="TClassFilter"></typeparam>
        /// <typeparam name="TAttributeFilter"></typeparam>
        /// <returns></returns>
        public static List<AttributedClassInfo<TAttribute>> FindAllAttributedClasses(Assembly rootAssembly, Type classFilterType = null, bool forceLoadLocalClassLibraries = true)
        {
            var rootFolder = new FileInfo(rootAssembly.Location).Directory;
            return FindAllAttributedClasses(rootFolder, classFilterType, forceLoadLocalClassLibraries);
        }


        /// <summary>
        /// Convenience Method for searching through all Types in the Application Domain for the specified Class Type 
        /// & Attribute generic type arguments.
        /// </summary>
        /// <typeparam name="TClassFilter"></typeparam>
        /// <typeparam name="TAttributeFilter"></typeparam>
        /// <returns></returns>
        public static List<AttributedClassInfo<TAttribute>> FindAllAttributedClasses(DirectoryInfo rootFolder, Type classFilterType = null, bool forceLoadLocalClassLibraries = true)
        {
            var attributeFilterType = typeof(TAttribute);
            var key = $"Attribute [{attributeFilterType.Name}] :: Class [{classFilterType?.Name}] :: ForceLoadLibraries [{forceLoadLocalClassLibraries}]";

            //BBernard
            //Use the internal Lazy cache to ensure that we don't process this more than once; because we assume for our use-cases that 
            //  the Reflection results will NOT change.
            var lazyResult = attributedClassCache.GetOrAdd(key, new Lazy<List<AttributedClassInfo<TAttribute>>>(() =>
            {
                //FIRST we must force all local (i.e. likely referenced assemblies) to be loaded...
                if (forceLoadLocalClassLibraries) AssemblyLoadHelper.ForceLoadClassLibraries(rootFolder);

                //SECOND we can now access & retrieve all Types from all loaded Assemblies in the current App Domain!
                //NOTE: Dynamic assemblies cannot be processeed (exceptions may occur) therefore we filter them out!
                var allReferencedAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic);
                var allReferendedTypes = allReferencedAssemblies.SelectMany(a => a.GetTypes());

                // Please note that this query is a bit simplistic. It doesn't
                // handle error reporting.
                var resultsList = (
                    from type in allReferendedTypes
                    //BBernard - If specified then we Filter by the Class/Interface type specified; all resulting Types must be 
                    //              instantiable (e.g. cannot be Abstract, Interface, etc.)
                    where (classFilterType == null || classFilterType.IsAssignableFrom(type)) && !type.IsAbstract && !type.IsInterface
                    //BBernard - Filter by the Attribute filter type specified
                    let attribute = type.GetCustomAttributes(attributeFilterType, false)?.FirstOrDefault() as TAttribute
                    where attribute != null
                    select new AttributedClassInfo<TAttribute>(attribute, type)
                ).ToList();

                return resultsList;
            }));

            return lazyResult.Value;
        }
    }


    /// <summary>
    /// BBernard
    /// Wrapper class to encapsulate the results of a unique combination of Custom Attribute & Class,
    /// and provide convenience methods for that particular combination (e.g. CreateInstance(optional args)).
    /// </summary>
    /// <typeparam name="TAttribute"></typeparam>
    public class AttributedClassInfo<TAttribute> where TAttribute : class
    {
        public AttributedClassInfo(TAttribute attribute, Type instantiationClassType)
        {
            this.InstantiationClassType = instantiationClassType;
            this.Attribute = attribute;
        }

        public TAttribute Attribute { get; protected set; }
        public Type InstantiationClassType { get; protected set; }

        public TClassCast CreateInstance<TClassCast>(params object[] args) where TClassCast: class
        {
            var instance = args.Length == 0 
                            ? Activator.CreateInstance(this.InstantiationClassType) as TClassCast
                            : Activator.CreateInstance(this.InstantiationClassType, args) as TClassCast;
            
            return instance;
        }

        public override string ToString()
        {
            return $"Attribute [{this.Attribute?.ToString()}]; Class [{this.InstantiationClassType?.Name}]";
        }
    }


}

