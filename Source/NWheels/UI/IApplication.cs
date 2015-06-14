﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI
{
    /// <summary>
    /// Defines a UI application. This is UIDL root object.
    /// An Application declares its Screens and Screen Parts.
    /// </summary>
    public interface IApplication : IUIElement, IUIElementContainer
    {
    }
}