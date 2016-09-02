using System;

namespace NWheels.Api.Ddd
{
    public static class DomainModel
    {
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = true, AllowMultiple = false)]
        public class BoundedContextAttribute : System.Attribute
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = true, AllowMultiple = false)]
        public abstract class AbstractDomainObjectAttribute : System.Attribute
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = true, AllowMultiple = false)]
        public class EntityAttribute : AbstractDomainObjectAttribute
        {
            public bool IsAggregateRoot { get; set; }
            public bool IsTreeStructure { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = true, AllowMultiple = false)]
        public class ValueObjectAttribute : AbstractDomainObjectAttribute
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
        public class PersistedValueAttribute : System.Attribute
        {
            public bool AutoGenerated { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
        public class EntityIdAttribute : PersistedValueAttribute
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
        public class CalculatedValueAttribute : System.Attribute
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
        public class ThisEntityReferenceAttribute : System.Attribute
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
        public class InjectedDependencyAttribute : System.Attribute
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
        public class ConstructorAttribute : System.Attribute
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
        public class DestructorAttribute : System.Attribute
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
        public class DomainEventFilterAttribute : System.Attribute
        {
            public string SelectOneMethod { get; set; }
            public string SelectManyMethod { get; set; }
        }
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static class Invariant
        {
            [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
            public class RequiredAttribute : System.Attribute
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
            public class UniqueAttribute : System.Attribute
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
            public class UniquePerParentsAttribute : System.Attribute
            {
                public UniquePerParentsAttribute(params string[] propertyNames)
                {
                    this.ParentPropertyNames = propertyNames;
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                public string[] ParentPropertyNames { get; private set; }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static class Relation
        {
            [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
            public class CompositionAttribute : System.Attribute
            {
            }
            
            //-------------------------------------------------------------------------------------------------------------------------------------------------

            [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
            public class CompositionParentAttribute : System.Attribute
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
            public class AggregationAttribute : System.Attribute
            {
            }
            
            //-------------------------------------------------------------------------------------------------------------------------------------------------

            [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
            public class AggregationParentAttribute : System.Attribute
            {
            }
        }
    }
}