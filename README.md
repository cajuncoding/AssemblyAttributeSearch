# AssemblyAttributeSearch
This is a small small .Net Standard library/helper to provide greatly simplified convenience methods for dynamically finding, filtering, 
and initializing classes based on Custom Attributes. It provides a simpel convenience methods to allow searching 
for all instances of a Custom Attribute type. 

In addition, you may specify an optional Class/Interface filter to only return types that implement the given class/interface.

The results will have the details of the Attribute found, as well as the class type for the calling code to use (e.g. process, instantiate dynamically, etc.).1

The default behavior assumes that there may be instances where Interfaces, and classes are defined in separate class Librareis and may
in which the Assembly may not yet be loaded. In which case the local assemblies -- in the same root/bin folder as the specified assembly --
are eagerly loaded (e.g. force loaded) so that all libraries are available to be searched.

This was important for my use case whereby I wanted to have a generic Factory that could create instances of any class that implmented
my interface and had appropriate metadata defined by my Custom Attribute.  This allowed develoeprs to create many implementations easily
without any impact to the core framework that could instantiate and implement the use of those classes.

NOTE: In the context of Azure Functions, the Assembly.GetExecutingAssembly() method results in a dynamic `func` and not the original
project assembly, therefore the local class libraries could not be found and loaded as expected.  The solution is to simply use 
the Assembly property of a specific type (e.g. this.GetType().Assembly) that exists in the main project assembly and everythign 
will resolve as expected; as noted in the samples below.

## Project Goals & Benefits (Why did I share this?)
* Provide a very simple and lightweight library for searching/filtering all assemblies for Classes and their associated Attributes.
* Implement internal static caching for performance so that reflection calls do not need to be made for the same searches more than once.
* Return all related data for both the Attribute & Class Types in the response so that no additional Reflection processing is needed.
** I originally used `FluentAssemblyScanner` (a great looking project) however did not want to have to scan assemblies and then do additional lookups to get my Attribute data again.
** In addition, much of the `FluentAssemblyScanner` library was unused so something even lighter weight could meet my needs (and many others I believe).

## Usage
To get all classes & attribute info. for all classes denoted with a given `JediAttribute` Custom Attribute:
```
var thisAssembly = this.GetType().Assembly;

//This gets the list of all search results...
var allJediClassInfoList = AssemblyAttributeSearch<JediAttribute>.FindAllAttributedClasses(thisAssembly);

//This convenience method lets us instanticate an instance (via Relfection)
var dynamicallyCreatedJedi = allJediClassInfoList.FirstOrDefault()?.CreateInstanc<IJedi>();
```

To get only the classes & attribute info. with a given `JediAttribute` and that also implements the IJediMaster Interface:
```
var thisAssembly = this.GetType().Assembly;

//This gets the list of all search results...
var jediMasterClassInfoList = AssemblyAttributeSearch<JediAttribute>.FindAllAttributedClasses(thisAssembly, typeof(IJediMaster));

//This convenience method lets us instanticate an instance (via Relfection)
var dynamicallyCreatedJediMaster = jediMasterClassInfoList.FirstOrDefault()?.CreateInstanc<IJediMaster>();
```

Or if you just want to force load the local class libraries via the Helper and then process with your own Reflection Logic:
```
//Eagerly load all local assemblies...
AssemblyLoadHelper.ForceLoadClassLibraries(this.GetType().Assembly);

//Now you can retrieve and process all types in all assemblies even if they have not yet been lazy initialized by .Net Framework...
var allTypesInAllLocalAssemblies = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes());
```

```
/*
MIT License

Copyright (c) 2018

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
```
