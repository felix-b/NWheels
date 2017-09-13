﻿using System.Xml.Serialization;

namespace NWheels.Microservices.Api
{
    [XmlRoot(ElementName = "microservice", IsNullable = false)]
    public class MicroserviceConfig
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        [XmlElement("injection-adapter")]
        public InjectionAdapterElement InjectionAdapter { get; set; }

        [XmlArray("framework-modules")]
        [XmlArrayItem("module")]
        public ModuleConfig[] FrameworkModules { get; set; }
        
        [XmlArray("application-modules")]
        [XmlArrayItem("module")]
        public ModuleConfig[] ApplicationModules { get; set; }
        
        public class InjectionAdapterElement
        {
            [XmlAttribute(AttributeName = "assembly")]
            public string Assembly { get; set; }
        }

        public class ModuleConfig
        {
            [XmlAttribute(AttributeName = "assembly")]
            public string Assembly { get; set; }

            [XmlElement(ElementName = "feature")]
            public FeatureConfig[] Features { get; set; }

            public class FeatureConfig
            {
                [XmlAttribute(AttributeName = "name")]
                public string Name { get; set; }
            }
        }
    }
}
