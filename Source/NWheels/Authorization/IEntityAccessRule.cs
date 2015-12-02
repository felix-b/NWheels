﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Authorization
{
    public interface IEntityAccessRule
    {
        void BuildAccessControl(IEntityAccessControlBuilder access);
    }
}
