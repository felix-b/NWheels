﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using NWheels.Extensions;
using NWheels.Globalization.Core;
using NWheels.UI.Core;
using NWheels.UI.Uidl;

namespace NWheels.UI.Toolbox
{
    public class TabSet : WidgetBase<TabSet, Empty.Data, Empty.State>
    {
        public TabSet(string idName, ControlledUidlNode parent)
            : base(idName, parent)
        {
            this.Tabs = new List<WidgetUidlNode>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Add(WidgetUidlNode content)
        {
            this.Tabs.Add(content);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddTabs(params WidgetUidlNode[] tabContents)
        {
            this.Tabs.AddRange(tabContents);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override IEnumerable<WidgetUidlNode> GetNestedWidgets()
        {
            return Tabs;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public List<WidgetUidlNode> Tabs { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<TabSet, Empty.Data, Empty.State> presenter)
        {
        }
    }
}