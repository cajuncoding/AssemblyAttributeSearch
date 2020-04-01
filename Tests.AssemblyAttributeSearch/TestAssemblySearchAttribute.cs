using AssemblyHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using Tests.ClassLibrary.Common;

namespace AssemblyAttributeSearch.Tests
{
    [TestClass]
    public class TestAssemblySearchAttribute
    {
        [TestMethod]
        public void RunTestsInSpecificSequence()
        {
            //FIRST:
            TestFindingClassesWithoutForcedEagerLoading();
            
            //SECOND:
            TestForceEagerLoadingOfReferencedClassLibraries();

            //REST:
            TestFindingClassesWithCustomAttribute();
            TestFindingClassesWithCustomAttributeAndInterfaceFilter();
        }

        //[TestMethod]
        public void TestFindingClassesWithoutForcedEagerLoading()
        {
            var thisAssembly = this.GetType().Assembly;
            var allJediClasses = AssemblyAttributeSearch<JediAttribute>.FindAllAttributedClasses(thisAssembly, null, false);
            var allDarkSideClasses = AssemblyAttributeSearch<DarkSideAttribute>.FindAllAttributedClasses(thisAssembly, null, false);

            Assert.AreEqual(allJediClasses.Count, 0, "There should be NO Jedi found if the Assembly hasn't been forced to load!");
            Assert.AreEqual(allDarkSideClasses.Count, 0, "There should be NO DarkSide found if the Assembly hasn't been forced to load!");
        }

        //[TestMethod]
        public void TestForceEagerLoadingOfReferencedClassLibraries()
        {
            const string MODELS_ASSEMBLY_NAME = "Tests.ClassLibrary.Models";
            var modelsAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName.Contains(MODELS_ASSEMBLY_NAME, StringComparison.OrdinalIgnoreCase));
            Assert.IsNull(modelsAssembly, "Models Assembly shouldln't be loaded yet!");

            AssemblyLoadHelper.ForceLoadClassLibraries(this.GetType().Assembly);

            var modelsAssemblyFound = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName.Contains(MODELS_ASSEMBLY_NAME, StringComparison.OrdinalIgnoreCase));
            Assert.IsNotNull(modelsAssemblyFound, "Models Assembly should now be loaded!");
        }

        //[TestMethod]
        public void TestFindingClassesWithCustomAttribute()
        {
            var thisAssembly = this.GetType().Assembly;
            var allJediClasses = AssemblyAttributeSearch<JediAttribute>.FindAllAttributedClasses(thisAssembly);
            var allDarkSideClasses = AssemblyAttributeSearch<DarkSideAttribute>.FindAllAttributedClasses(thisAssembly);

            Assert.AreEqual(allJediClasses.Count, 2, "There should be 2 Jedi!");
            Assert.AreEqual(allDarkSideClasses.Count, 2, "There should be 2 on the Dark Side!");

            var lukeSkywalkerMeta = allJediClasses.FirstOrDefault(j => j.InstantiationClassType.Name == nameof(LukeSykwalker));
            Assert.IsNotNull(lukeSkywalkerMeta, "Luke must be a Jedi!");
            Assert.AreEqual(lukeSkywalkerMeta.Attribute.LightSaber, LightSaberColor.Blue, "Lukes Lightsaber must be Blue!");
            Assert.AreEqual(lukeSkywalkerMeta.InstantiationClassType, typeof(LukeSykwalker));
            var jediLuke = lukeSkywalkerMeta.CreateInstance<IJedi>();
            Assert.IsTrue(jediLuke.IsJedi);

            var yodaMeta = allJediClasses.FirstOrDefault(j => j.InstantiationClassType.Name == nameof(Yoda));
            Assert.IsNotNull(yodaMeta, "Yoda must be a Jedi!");
            Assert.AreEqual(yodaMeta.Attribute.LightSaber, LightSaberColor.Green, "Yoda's Lightsaber must be Green!");
            Assert.AreEqual(yodaMeta.InstantiationClassType, typeof(Yoda));
            var jediMasterYoda = yodaMeta.CreateInstance<IJediMaster>();
            Assert.IsTrue(jediMasterYoda.IsJedi && jediMasterYoda.LivesForever);

            var darthVaderMeta = allDarkSideClasses.FirstOrDefault(j => j.InstantiationClassType.Name == nameof(DarthVader));
            Assert.IsNotNull(darthVaderMeta, "Darth Vader must on the Dark Side!");
            Assert.AreEqual(darthVaderMeta.Attribute.LightSaber, LightSaberColor.Red, "Darth Vader's Lightsaber must be Red!");
            Assert.AreEqual(darthVaderMeta.InstantiationClassType, typeof(DarthVader));
            var darthVader = darthVaderMeta.CreateInstance<IDarkSide>();
            Assert.IsTrue(darthVader.IsDarkSide);

            var emporerPalpatineMeta = allDarkSideClasses.FirstOrDefault(j => j.InstantiationClassType.Name == nameof(EmporerPalpatine));
            Assert.IsNotNull(emporerPalpatineMeta, "Emporer Palpatine must be on the Dark Side!");
            Assert.AreEqual(emporerPalpatineMeta.Attribute.LightSaber, LightSaberColor.Red, "Emporer Palpatine's Lightsaber must be Red!");
            Assert.AreEqual(emporerPalpatineMeta.InstantiationClassType, typeof(EmporerPalpatine));
            var sithLord = emporerPalpatineMeta.CreateInstance<IDarkSide>();
            Assert.IsTrue(sithLord.IsDarkSide);
        }

        //[TestMethod]
        public void TestFindingClassesWithCustomAttributeAndInterfaceFilter()
        {
            var thisAssembly = this.GetType().Assembly;
            var allJediClasses = AssemblyAttributeSearch<JediAttribute>.FindAllAttributedClasses(thisAssembly, typeof(IJedi));
            var allJediMasterClasses = AssemblyAttributeSearch<JediAttribute>.FindAllAttributedClasses(thisAssembly, typeof(IJediMaster));
            var allDarkSideClasses = AssemblyAttributeSearch<DarkSideAttribute>.FindAllAttributedClasses(thisAssembly, typeof(IDarkSide));
            var allMixedInvalidClasses = AssemblyAttributeSearch<JediAttribute>.FindAllAttributedClasses(thisAssembly, typeof(IDarkSide));

            Assert.AreEqual(allJediClasses.Count, 2, "There should be 2 Jedi!");
            Assert.AreEqual(allJediMasterClasses.Count, 1, "There should be only 1 Jedi Master (Yoda)!");
            Assert.AreEqual(allDarkSideClasses.Count, 2, "There should be 2 on the Dark Side!");
            Assert.AreEqual(allMixedInvalidClasses.Count, 0, "There should be No attributed Jedi that implement the IDarkSide interface!");
        }

    }
}
