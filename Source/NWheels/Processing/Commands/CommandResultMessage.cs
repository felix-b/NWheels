﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Authorization;
using NWheels.Extensions;
using NWheels.Processing.Messages;
using NWheels.UI;

namespace NWheels.Processing.Commands
{
    public class CommandResultMessage : AbstractSessionPushMessage, IPromiseFailureInfo
    {
        public CommandResultMessage(
            IFramework framework,
            ISession toSession,
            Guid commandMessageId,
            object result,
            bool success, 
            string newSessionId = null,
            string faultCode = null, 
            string faultSubCode = null, 
            string faultReason = null)
            : base(framework, toSession)
        {
            this.CommandMessageId = commandMessageId;
            this.Result = result;
            this.Success = success;
            this.NewSessionId = newSessionId;
            this.FaultCode = faultCode;
            this.FaultSubCode = faultSubCode;
            this.FaultReason = faultReason;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override object TakeSerializableSnapshot()
        {
            return new Snapshot(this);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Guid CommandMessageId { get; private set; }
        public object Result { get; private set; }
        public bool Success { get; private set; }
        public string NewSessionId { get; private set; }
        public string FaultCode { get; private set; }
        public string FaultSubCode { get; private set; }
        public string FaultReason { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class Snapshot
        {
            public Snapshot(CommandResultMessage source)
            {
                this.Type = source.GetType().SimpleQualifiedName();
                this.CommandMessageId = source.CommandMessageId;
                this.Result = source.Result;
                this.Success = source.Success;
                this.FaultCode = source.FaultCode;
                this.FaultSubCode = source.FaultSubCode;
                this.FaultReason = source.FaultReason;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string Type { get; private set; }
            public Guid CommandMessageId { get; private set; }
            public object Result { get; private set; }
            public bool Success { get; private set; }
            public string FaultCode { get; private set; }
            public string FaultSubCode { get; private set; }
            public string FaultReason { get; private set; }
        }
    }
}
