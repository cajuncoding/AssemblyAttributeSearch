using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyAttributeSearch.Tests
{
    public enum LightSaberColor
    {
        Blue,
        Green,
        Red,
        White
    }

    public class TheForceIsWithHimAttribute : Attribute
    {
        public TheForceIsWithHimAttribute(LightSaberColor lightSaberColor)
        {
            this.LightSaber = lightSaberColor;
        }

        public LightSaberColor LightSaber { get; protected set; }
        public bool IsDarkSide => LightSaber == LightSaberColor.Red;
    }

    public class JediAttribute : TheForceIsWithHimAttribute
    {
        public JediAttribute(LightSaberColor lightSaberColor) : base(lightSaberColor)
        {
        }
    }

    public class DarkSideAttribute : TheForceIsWithHimAttribute
    {
        public DarkSideAttribute(LightSaberColor lightSaberColor) : base(lightSaberColor)
        {
        }
    }
}
