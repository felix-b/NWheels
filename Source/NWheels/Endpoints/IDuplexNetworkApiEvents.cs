﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Endpoints
{
    public interface IDuplexNetworkApiEvents
    {
        void OnSessionClosed(IDuplexNetworkEndpointApiProxy proxy, SessionCloseReason reason);
    }
}