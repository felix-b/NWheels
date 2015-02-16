﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NUnit.Framework;
using NWheels.Concurrency;
using NWheels.Entities;
using NWheels.Extensions;
using NWheels.Puzzle.EntityFramework.Conventions;
using NWheels.Puzzle.EntityFramework.EFConventions;
using NWheels.Puzzle.EntityFramework.Impl;
using System.Data.Entity.ModelConfiguration;

namespace NWheels.Puzzle.EntityFramework.ComponentTests
{
    public static class HardCodedImplementations
    {
        public static class Repository1
        {
            public static void RegisterEntityFineTunings(ContainerBuilder builder)
            {
                builder.RegisterRelationalMappingFineTune<Interfaces.Repository1.IProduct>(ft => ft
                    .Table("MY_PRODUCTS")
                    .Column(p => p.Price, columnName: "MY_SPECIAL_PRICE_COLUMN", dataType: "MONEY"));

                builder.RegisterRelationalMappingFineTune<Interfaces.Repository1.IOrder>(ft => ft
                    .Table("MY_ORDERS")
                    .Column(o => o.Id, columnName: "MY_SPECIAL_ORDER_ID_COLUMN"));

                builder.RegisterRelationalMappingFineTune<Interfaces.Repository1.IOrderLine>(ft => ft
                    .Table("MY_ORDER_LINES")
                    .Column(ol => ol.Product, columnName: "MY_SPECIAL_PRODUCT_ID_COLUMN"));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public class DataRepositoryObject_DataRepository : EntityFrameworkDataRepositoryBase, Interfaces.Repository1.IOnlineStoreRepository
            {
                private IEntityRepository<Interfaces.Repository1.IOrder> m_Orders;
                private IEntityRepository<Interfaces.Repository1.IProduct> m_Products;

                public DataRepositoryObject_DataRepository(DbConnection connection, bool autoCommit)
                    : base(GetOrBuildDbCompoledModel(connection), connection, autoCommit)
                {
                    this.m_Products = new EntityFrameworkEntityRepository<Interfaces.Repository1.IProduct, EntityObject_Product>(this);
                    this.m_Orders = new EntityFrameworkEntityRepository<Interfaces.Repository1.IOrder, EntityObject_Order>(this);
                }

                public override sealed Type[] GetEntityTypesInRepository()
                {
                    return new Type[] { typeof(EntityObject_Product), typeof(EntityObject_Order) };
                }

                public Interfaces.Repository1.IOrderLine NewOrderLine(
                    Interfaces.Repository1.IOrder order, 
                    Interfaces.Repository1.IProduct product, 
                    int quantity)
                {
                    Interfaces.Repository1.IOrderLine orderLine = new EntityObject_OrderLine();
                    orderLine.Order = order;
                    orderLine.Product = product;
                    orderLine.Quantity = quantity;
                    return orderLine;
                }

                public IEntityRepository<Interfaces.Repository1.IOrder> Orders
                {
                    get
                    {
                        return this.m_Orders;
                    }
                }

                public IEntityRepository<Interfaces.Repository1.IProduct> Products
                {
                    get
                    {
                        return this.m_Products;
                    }
                }

                private static readonly object _s_compiledModelSyncRoot = new object();
                private static DbCompiledModel _s_compiledModel;

                private static DbCompiledModel GetOrBuildDbCompoledModel(DbConnection connection)
                {
                    if ( _s_compiledModel == null )
                    {
                        lock ( _s_compiledModelSyncRoot )
                        {
                            if ( _s_compiledModel == null )
                            {
                                var modelBuilder = new DbModelBuilder();

                                modelBuilder.Conventions.Add(new NoUnderscoreForeignKeyNamingConvention());

                                var objectSetProducts = modelBuilder.Entity<EntityObject_Product>().HasEntitySetName("Product").ToTable("Products");
                                var objectSetOrders = modelBuilder.Entity<EntityObject_Order>().HasEntitySetName("Order").ToTable("Orders");
                                var objectSetOrderLines = modelBuilder.Entity<EntityObject_OrderLine>().HasEntitySetName("OrderLine").ToTable("OrderLines");

                                var model = modelBuilder.Build(connection);
                                _s_compiledModel = model.Compile();
                            }
                        }
                    }

                    return _s_compiledModel;
                }
            }
            
            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public class DataRepositoryObject_CustomNames : EntityFrameworkDataRepositoryBase, Interfaces.Repository1.IOnlineStoreRepository
            {
                private IEntityRepository<Interfaces.Repository1.IOrder> m_Orders;
                private IEntityRepository<Interfaces.Repository1.IProduct> m_Products;

                public DataRepositoryObject_CustomNames(DbConnection connection, bool autoCommit)
                    : base(GetOrBuildDbCompiledModel(connection), connection, autoCommit)
                {
                    this.m_Products = new EntityFrameworkEntityRepository<Interfaces.Repository1.IProduct, EntityObject_Product>(this);
                    this.m_Orders = new EntityFrameworkEntityRepository<Interfaces.Repository1.IOrder, EntityObject_Order>(this);
                }

                public override sealed Type[] GetEntityTypesInRepository()
                {
                    return new Type[] { typeof(EntityObject_Product), typeof(EntityObject_Order) };
                }

                public Interfaces.Repository1.IOrderLine NewOrderLine(
                    Interfaces.Repository1.IOrder order,
                    Interfaces.Repository1.IProduct product,
                    int quantity)
                {
                    Interfaces.Repository1.IOrderLine orderLine = new EntityObject_OrderLine();
                    orderLine.Order = order;
                    orderLine.Product = product;
                    orderLine.Quantity = quantity;
                    return orderLine;
                }

                public IEntityRepository<Interfaces.Repository1.IOrder> Orders
                {
                    get
                    {
                        return this.m_Orders;
                    }
                }

                public IEntityRepository<Interfaces.Repository1.IProduct> Products
                {
                    get
                    {
                        return this.m_Products;
                    }
                }

                private static readonly object _s_compiledModelSyncRoot = new object();
                private static DbCompiledModel _s_compiledModel;

                private static DbCompiledModel GetOrBuildDbCompiledModel(DbConnection connection)
                {
                    if ( _s_compiledModel == null )
                    {
                        lock ( _s_compiledModelSyncRoot )
                        {
                            if ( _s_compiledModel == null )
                            {
                                var modelBuilder = new DbModelBuilder();

                                modelBuilder.Conventions.Add(new NoUnderscoreForeignKeyNamingConvention());

                                var objectSetProducts = modelBuilder.Entity<EntityObject_Product>().HasEntitySetName("Product").ToTable("MY_PRODUCTS");
                                var objectSetOrders = modelBuilder.Entity<EntityObject_Order>().HasEntitySetName("Order").ToTable("MY_ORDERS");
                                var objectSetOrderLines = modelBuilder.Entity<EntityObject_OrderLine>().HasEntitySetName("OrderLine").ToTable("MY_ORDER_LINES");

                                objectSetProducts.Property(p => p.Price).HasColumnName("MY_SPECIAL_PRICE_COLUMN").HasColumnType("MONEY");
                                objectSetOrderLines.HasRequired(p => p.Product).WithRequiredDependent().Map(m => m.MapKey("MY_SPECIAL_PRODUCT_ID_COLUMN"));
                                objectSetOrderLines.HasRequired(p => p.Order).WithMany(o => o.OrderLines).Map(m => m.MapKey("OrderId"));
                                objectSetOrders.Property(p => p.Id).HasColumnName("MY_SPECIAL_ORDER_ID_COLUMN");

                                var model = modelBuilder.Build(connection);
                                _s_compiledModel = model.Compile();
                            }
                        }
                    }

                    return _s_compiledModel;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public class EntityObject_Order : Interfaces.Repository1.IOrder
            {
                private int m_Id;
                private ICollection<EntityObject_OrderLine> m_OrderLines = new HashSet<EntityObject_OrderLine>();
                private EfEntityObjectFactory.CollectionAdapter<EntityObject_OrderLine, Interfaces.Repository1.IOrderLine> m_OrderLines_Adapter;
                private DateTime m_PlacedAt;
                private Interfaces.Repository1.OrderStatus m_Status;

                public EntityObject_Order()
                {
                    this.m_OrderLines_Adapter =
                        new EfEntityObjectFactory.CollectionAdapter<EntityObject_OrderLine, Interfaces.Repository1.IOrderLine>(this.m_OrderLines);
                    this.m_Status = Interfaces.Repository1.OrderStatus.New;
               }

                public int Id
                {
                    get { return this.m_Id; }
                    set { this.m_Id = value; }
                }

                ICollection<Interfaces.Repository1.IOrderLine> Interfaces.Repository1.IOrder.OrderLines
                {
                    get { return this.m_OrderLines_Adapter; }
                }

                public virtual ICollection<EntityObject_OrderLine> OrderLines
                {
                    get { return this.m_OrderLines; }
                    set
                    {
                        this.m_OrderLines_Adapter = new EfEntityObjectFactory.CollectionAdapter<EntityObject_OrderLine, Interfaces.Repository1.IOrderLine>(value);
                        this.m_OrderLines = value;
                    }
                }

                public DateTime PlacedAt
                {
                    get { return this.m_PlacedAt; }
                    set { this.m_PlacedAt = value; }
                }

                public Interfaces.Repository1.OrderStatus Status
                {
                    get { return m_Status; }
                    set { m_Status = value; }
                }
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public class EntityObject_OrderLine : Interfaces.Repository1.IOrderLine
            {
                private int m_Id;
                private EntityObject_Order m_Order;
                private EntityObject_Product m_Product;
                private int m_Quantity;

                public int Id
                {
                    get { return this.m_Id; }
                    set { this.m_Id = value; }
                }

                Interfaces.Repository1.IOrder Interfaces.Repository1.IOrderLine.Order
                {
                    get { return this.m_Order; }
                    set { this.m_Order = (EntityObject_Order)value; }
                }

                Interfaces.Repository1.IProduct Interfaces.Repository1.IOrderLine.Product
                {
                    get { return this.m_Product; }
                    set
                    {
                        this.m_Product = (EntityObject_Product)value;
                    }
                }

                [Column(name: "OrderId")]
                public virtual EntityObject_Order Order
                {
                    get { return this.m_Order; }
                    set { this.m_Order = value; }
                }

                [Column(name: "ProductId")]
                public virtual EntityObject_Product Product
                {
                    get { return this.m_Product; }
                    set { this.m_Product = value; }
                }

                public int Quantity
                {
                    get { return this.m_Quantity; }
                    set { this.m_Quantity = value; }
                }
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public class EntityObject_Product : Interfaces.Repository1.IProduct
            {
                private int m_Id;
                private string m_Name;
                private decimal m_Price;

                public int Id
                {
                    get { return this.m_Id; }
                    set { this.m_Id = value; }
                }

                public string Name
                {
                    get { return this.m_Name; }
                    set { this.m_Name = value; }
                }

                public decimal Price
                {
                    get { return this.m_Price; }
                    set { this.m_Price = value; }
                }
            }
        }
    }
}
