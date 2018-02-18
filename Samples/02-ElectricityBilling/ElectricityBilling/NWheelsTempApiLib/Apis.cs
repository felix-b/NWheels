﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using NWheels.Ddd;
using NWheels.I18n;

namespace NWheels
{
    public static class TypeContract
    {
        public static class Presentation
        {
            public class DefaultFormatAttribute : Attribute
            {
                public DefaultFormatAttribute(string formatPattern)
                {
                }
            }
        }
    }

    public static class MemberContract
    {
        public class TransientAttribute : Attribute
        {
        }
        public class PersistentAttribute : Attribute
        {
        }
        public class InjectedDependencyAttribute : Attribute
        {
        }
        public class AutoGeneratedAttribute : Attribute
        {
        }
        public class EntityIdAttribute : Attribute
        {
        }
        public class RequiredAttribute : Attribute
        {
            public RequiredAttribute(bool allowEmpty = false)
            {
            }
        }

        public static class Semantics
        {
            public class EmailAddressAttribute : Attribute
            {
            }
            public class PhoneNumberAttribute : Attribute
            {
            }
            public class CurrencyAttribute : Attribute
            {
            }
        }

        public static class Validation
        {
            public class NonNegativeAttribute : Attribute
            {
            }
        }

        public static class Presentation
        {
            public class DefaultObjectDisplayAttribute : Attribute
            {
                public DefaultObjectDisplayAttribute()
                {
                }
                public DefaultObjectDisplayAttribute(string formatPattern)
                {
                }
            }
        }
    }

    namespace Authorization
    {
        public static class SecurityContract
        {
            public class AllowAnonymousAttribute : Attribute
            {
            }

            public class Require : Attribute
            {
                public Require(object claim)
                {
                    this.Claim = claim;
                }

                public object Claim { get; }
            }
        }
    }

    namespace I18n
    {
        public interface ILocalizationService
        {
            string GetLocalDisplayString<T>(T value, string formatPattern);
            string GetLocalDisplayString<T>(T value, Attribute memberContract);
        }

        public static class TypeContract
        {
            public class LocalizablesAttribute : Attribute
            {
            }
        }
    }

    namespace Transactions
    {
        public interface ITransactionFactory
        {
            IUnitOfWork NewUnitOfWork();
        }

        public interface IUnitOfWork : IDisposable
        {
            Task Commit();
            Task Discard();
        }
    }

    namespace RestApi
    {
        using NWheels.Microservices;

        public class AspNetCoreSwaggerStack
        {
        }

        public class ResourceCatalogBuilder
        {
        }

        public static class MicroserviceHostBuilderExtensions
        {
            public static MicroserviceHostBuilder UseRestApiResources(
                this MicroserviceHostBuilder hostBuilder,
                Action<ResourceCatalogBuilder> buildCatalog)
            {
                return hostBuilder;
            }
        }

        public static class ComponentContainerBuilderExtensions
        {
            public static IComponentContainerBuilder RegisterRestApiResources(
                this IComponentContainerBuilder containerBuilder, 
                Action<ResourceCatalogBuilder> buildCatalog)
            {
                return containerBuilder;
            }
        }
    }

    namespace DB
    {
        public static class TypeContract
        {
            public class ViewAttribute : Attribute
            {
            }
        }

        public static class MemberContract
        {
            public class MapToMemberAttribute : Attribute
            {
                public MapToMemberAttribute(Type type, string memberName)
                {
                }
            }
        }

        public interface IAsyncQuery<T>
        {
            Task<bool> AnyAsync();
            Task<long> CountAsync();
            Task<T> FirstAsync();
            Task<T> FirstOrDefaultAsync();
            Task<List<T>> ToListAsync();
            Task<T[]> ToArrayAsync();
            Task<IAsyncQuery<T>> TakeAsync(long count);
            Task<IAsyncQuery<T>> WhileAsync(Func<T, bool> predicate);
        }

        public interface IRepository<T>
        {
            IAsyncQuery<T> Query(Func<IQueryable<T>, IQueryable<T>> query);
            T New(Func<T> constructor);
            Task<T> UpsertAsync(Func<IQueryable<T>, IQueryable<T>> query, Func<T> constructor);
            void Delete(T obj);
        }

        public interface IView<T>
        {
            IAsyncQuery<T> Query(Func<IQueryable<T>, IQueryable<T>> query);
        }

        public class EFCoreStack
        {
        }
    }

    namespace Ddd
    {
        using  NWheels.RestApi;

        public interface IDomainObjectValidator
        {
            void Report<TDomainObject>(
                Expression<Func<TDomainObject, object>> member, 
                ValidationErrorType errorType, 
                string errorMessage);
        }

        public interface IDomainObjectValidator<T> : IDomainObjectValidator
        {
            void InvalidValue(Expression<Func<T, object>> member, string message = null);
            void NullValue(Expression<Func<T, object>> member, string message = null);
            void EmptyValue(Expression<Func<T, object>> member, string message = null);
            void ValueOutOfRange(Expression<Func<T, object>> member, string message = null);
            void ValueDoesNotMatchPattern(Expression<Func<T, object>> member, string message = null);
        }

        public enum ValidationErrorType
        {
            NotSpecified,
            ValueIsNull,
            ValueIsEmpty,
            ValueIsOutOfRange,
            ValueDoesNotMatchPattern,
            ValueIsInvalid
        }

        public static class EntityRef<TEntity>
            where TEntity : class
        {
            public static EntityRef<TId, TEntity> FromId<TId>(TId id)
            {
                return new EntityRef<TId, TEntity>(id);
            }
        }

        public struct ValueObject<T>
        {
            public bool IsLoaded { get; }
            public bool CanLoad { get; }
            public T Value { get; set; }
        }

        public struct EntityRef<TId, TEntity>
            where TEntity : class
        {
            public EntityRef(TId id)
            {
                this.Id = id;
                this.IsLoaded = false;
                this.CanLoad = false;
                this.Entity = null;
            }

            public TId Id { get; }
            public bool IsLoaded { get; }
            public bool CanLoad { get; }
            public TEntity Entity { get; }
        }

        public struct EntitySet<TId, TEntity> : IQueryable<TEntity>
            where TEntity : class
        {
            private Type _elementType;
            private Expression _expression;
            private IQueryProvider _provider;
            public bool IsLoaded { get; }
            public bool CanLoad { get; }
            public ISet<EntityRef<TId, TEntity>> Set { get; }

            IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator()
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }

            Type IQueryable.ElementType
            {
                get { return _elementType; }
            }

            Expression IQueryable.Expression
            {
                get { return _expression; }
            }

            IQueryProvider IQueryable.Provider
            {
                get { return _provider; }
            }
        }

        public struct ValueObjectList<TValueObject>
        {
            public bool IsLoaded { get; }
            public bool CanLoad { get; }
            public IList<TValueObject> List { get; }
        }

        public interface IThisDomainObjectServices
        {
            TContext GetContext<TContext>() where TContext : class;
            string FormatDisplayString(ILocalizationService localization);
            string DisplayStringFormatPattern { get; }
        }

        public static class TypeContract
        {
            public class BoundedContextAttribute : Attribute
            {
            }
        }

        public static class ResourceCatalogBuilderExtensions
        {
            public static ResourceCatalogBuilder AddDomainContextTx<TContext>(
                this ResourceCatalogBuilder catalogBuilder, 
                Expression<Func<TContext, Task>> tx)
            {
                return catalogBuilder;
            }
        }
    }

    namespace Logging
    {
        public class ElasticStack
        {
        }
    }

    namespace UI
    {
        public class WebReactReduxStack
        {
        }
    }

    namespace Microservices
    {
        public interface IComponentContainer
        {
        }

        public interface IComponentContainerBuilder
        {
            IComponentContainerBuilder RegisterComponentType<T>();
            IComponentContainerBuilder InstancePerDependency();
            IComponentContainerBuilder ForService<T>();
        }

        public abstract class LifecycleComponentBase
        {
            public abstract void Activate();
        }

        public static class Microservice
        {
            public static int RunCli(string name, string[] arguments, Action<MicroserviceHostBuilder> builder)
            {
                var host = BuildHost(name, builder);
                return host.RunCli(arguments);
            }

            public static MicroserviceHost BuildHost(string name, Action<MicroserviceHostBuilder> builder)
            {
                return new MicroserviceHost();
            }
        }

        public class MicroserviceHostBuilder
        {
            public MicroserviceHostBuilder UseLogging<T>()
            {
                return this;
            }

            public MicroserviceHostBuilder UseDB<T>()
            {
                return this;
            }

            public MicroserviceHostBuilder UseDdd()
            {
                return this;
            }

            public MicroserviceHostBuilder UseApplicationFeature<T>()
            {
                return this;
            }

            public MicroserviceHostBuilder UseMicroserviceXml(string path)
            {
                return this;
            }

            public MicroserviceHostBuilder UseLifecycleComponent<T>()
            {
                return this;
            }

            public MicroserviceHostBuilder UseRestApi<T>()
            {
                return this;
            }

            public MicroserviceHostBuilder UseUidl<T>()
            {
                return this;
            }

            public MicroserviceHostBuilder UseComponents(Action<IComponentContainer, IComponentContainerBuilder> action)
            {
                throw new NotImplementedException();
            }
        }

        public class DefaultFeatureLoaderAttribute : Attribute
        {        }

        public abstract class FeatureLoaderBase
        {
            public abstract void ContributeComponents(
                IComponentContainer existingComponents,
                IComponentContainerBuilder newComponents);
        }

        public class AutoDiscoverAssemblyOf<T> : FeatureLoaderBase
        {
            public override void ContributeComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents)
            {
                throw new NotImplementedException();
            }
        }

        public class MicroserviceHost
        {
            public int RunCli(string[] arguments)
            {
                return 0;
            }

            public async Task<int> RunCliAsync(string[] arguments)
            {
                return 0;
            }
        }
    }
}
