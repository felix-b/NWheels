﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;
using NWheels.Extensions;
using NWheels.UI.Toolbox;

namespace NWheels.Processing.Documents
{
    public class DocumentDesign
    {
        public DocumentDesign(Element contents)
        {
            this.Contents = contents;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Element Contents { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static EntityTableBuilder<TEntity> EntityTable<TEntity>(ITypeMetadataCache metadataCache)
        {
            return new EntityTableBuilder<TEntity>(metadataCache);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract class Element
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract class ContainerElement : Element, IEnumerable<Element>
        {
            protected ContainerElement(params Element[] contents)
            {
                this.Contents = new List<Element>(contents);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IList<Element> Contents { get; private set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            IEnumerator<Element> IEnumerable<Element>.GetEnumerator()
            {
                return Contents.GetEnumerator();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return Contents.GetEnumerator();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class TableElement : Element
        {
            public TableElement(params Column[] columns)
            {
                this.Columns = new List<Column>(columns);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string BoundEntityName { get; set; }
            public IList<Column> Columns { get; private set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public class Column
            {
                public Column(string title, int? width, Binding binding)
                {
                    Title = title;
                    Width = width;
                    Binding = binding;
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                public string Title { get; private set; }
                public int? Width { get; private set; }
                public Binding Binding { get; private set; }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class Binding
        {
            public Binding(string expression, string format, string fallback = null)
            {
                SpecialName = FieldSpecialName.None;
                Expression = expression;
                Format = format;
                Fallback = fallback;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Binding(FieldSpecialName specialName, string format, string fallback = null)
            {
                SpecialName = specialName;
                Expression = null;
                Format = format;
                Fallback = fallback;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public FieldSpecialName SpecialName { get; private set; }
            public string Expression { get; private set; }
            public string Format { get; private set; }
            public string Fallback { get; private set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class EntityTableBuilder<TEntity> 
        {
            private readonly ITypeMetadataCache _metadataCache;
            private readonly ITypeMetadata _metaType;
            private TableElement _element;
            private string _navigationPrefix;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public EntityTableBuilder(ITypeMetadataCache metadataCache)
            {
                _metadataCache = metadataCache;
                _metaType = metadataCache.GetTypeMetadata(typeof(TEntity));
                _element = new TableElement();
                _element.BoundEntityName = _metaType.QualifiedName;
                _navigationPrefix = "";
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public EntityTableBuilder<TEntity2> NavigateTo<TEntity2>(Expression<Func<TEntity, TEntity2>> navigation)
            {
                var nextNavigations = navigation.ToNormalizedNavigationStringArray();
                var nextNavigationPrefix = string.Join(".", nextNavigations) + ".";
                var newBuilder = new EntityTableBuilder<TEntity2>(_metadataCache);

                newBuilder._element = _element;
                newBuilder._navigationPrefix = _navigationPrefix + nextNavigationPrefix;
                
                return newBuilder;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public EntityTableBuilder<TEntity2> NavigateToCollection<TEntity2>(Expression<Func<TEntity, ICollection<TEntity2>>> navigation)
            {
                var nextNavigations = navigation.ToNormalizedNavigationStringArray();
                var nextNavigationPrefix = string.Join(".", nextNavigations) + ".";
                var newBuilder = new EntityTableBuilder<TEntity2>(_metadataCache);

                newBuilder._element = _element;
                newBuilder._navigationPrefix = _navigationPrefix + nextNavigationPrefix;

                return newBuilder;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public EntityTableBuilder<TEntity> Column<T>(
                Expression<Func<TEntity, T>> property, 
                string title = null, 
                int? width = null, 
                string format = null, 
                string fallback = null)
            {
                return InternalColumn(property, title, width, format, fallback);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public EntityTableBuilder<TEntity> InheritorColumn<TInheritorEntity>(
                Expression<Func<TInheritorEntity, object>> property,
                string title = null,
                int? width = null,
                string format = null,
                string fallback = null)
                where TInheritorEntity : TEntity
            {
                return InternalColumn(property, title, width, format, fallback);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public EntityTableBuilder<TEntity> TypeColumn(
                string title = null,
                int? width = null,
                string format = null,
                string fallback = null)
            {
                var column = new TableElement.Column(
                    title.OrDefaultIfNullOrWhitespace("Type"),
                    width,
                    new Binding(FieldSpecialName.Type, format, fallback));

                _element.Columns.Add(column);

                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public EntityTableBuilder<TEntity> IdColumn(
                string title = null,
                int? width = null,
                string format = null,
                string fallback = null)
            {
                var column = new TableElement.Column(
                    title.OrDefaultIfNullOrWhitespace("Id"),
                    width,
                    new Binding(FieldSpecialName.Id, format, fallback));

                _element.Columns.Add(column);

                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private EntityTableBuilder<TEntity> InternalColumn(
                LambdaExpression propertyExpression,
                string title = null,
                int? width = null,
                string format = null,
                string fallback = null)
            {
                var propertyInfo = propertyExpression.GetPropertyInfo();
                IPropertyMetadata metaProperty;

                if (propertyInfo.DeclaringType.IsAssignableFrom(_metaType.ContractType))
                {
                    metaProperty = _metaType.GetPropertyByDeclaration(propertyInfo);
                }
                else
                {
                    var inheritorMetaType = _metaType.DerivedTypes.First(t => propertyInfo.DeclaringType.IsAssignableFrom(t.ContractType));
                    metaProperty = inheritorMetaType.GetPropertyByDeclaration(propertyInfo);
                }

                var bindingExpression = _navigationPrefix + string.Join(".", propertyExpression.ToNormalizedNavigationStringArray());

                var column = new TableElement.Column(
                    title.OrDefaultIfNullOrWhitespace(metaProperty.Name),
                    width,
                    new Binding(bindingExpression, format, fallback));

                _element.Columns.Add(column);

                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static implicit operator TableElement(EntityTableBuilder<TEntity> builder)
            {
                return builder._element;
            }
        }
    }
}
