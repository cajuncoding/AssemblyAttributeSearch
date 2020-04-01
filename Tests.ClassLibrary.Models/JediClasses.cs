using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tests.ClassLibrary.Common;

namespace AssemblyAttributeSearch.Tests
{


    [Jedi(LightSaberColor.Blue)]
    public class LukeSykwalker : IUsesTheForce, IJedi
    {
        public bool IsJedi => this.GetType().GetCustomAttributes(typeof(JediAttribute), false).FirstOrDefault() != null;
        public string Name => nameof(LukeSykwalker);
    }

    [Jedi(LightSaberColor.Green)]
    public class Yoda : IUsesTheForce, IJediMaster
    {
        public bool IsJedi => this.GetType().GetCustomAttributes(typeof(JediAttribute), false).FirstOrDefault() != null;
        public string Name => nameof(Yoda);
        public bool LivesForever => true;
    }

    [DarkSide(LightSaberColor.Red)]
    public class DarthVader : IUsesTheForce, IDarkSide
    {
        public bool IsDarkSide => this.GetType().GetCustomAttributes(typeof(DarkSideAttribute), false).FirstOrDefault() != null;
        public string Name => nameof(DarthVader);
    }

    [DarkSide(LightSaberColor.Red)]
    public class EmporerPalpatine : IUsesTheForce, IDarkSide
    {
        public bool IsDarkSide => this.GetType().GetCustomAttributes(typeof(DarkSideAttribute), false).FirstOrDefault() != null;
        public string Name => nameof(EmporerPalpatine);
    }

}



