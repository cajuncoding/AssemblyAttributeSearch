using System;
using System.Collections.Generic;
using System.Text;

namespace Tests.ClassLibrary.Common
{
    public interface IJedi
    {
        bool IsJedi { get; }
    }

    public interface IJediMaster : IJedi
    {
        bool LivesForever { get; }
    }

    public interface IDarkSide
    {
        bool IsDarkSide { get; }
    }

    public interface IUsesTheForce
    {
        string Name { get; }
    }
}
