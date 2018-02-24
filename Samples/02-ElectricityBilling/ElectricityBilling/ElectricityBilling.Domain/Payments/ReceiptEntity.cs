﻿using System;
using System.Collections.Generic;
using System.Text;
using ElectricityBilling.Domain.Basics;
using ElectricityBilling.Domain.Customers;
using NWheels;
using NWheels.Ddd;

namespace ElectricityBilling.Domain.Payments
{
    [NWheels.TypeContract.Presentation.DefaultFormat("{MoneyPaid:C} on {DateTime:D} ({ReferenceNumber})")]
    public class ReceiptEntity
    {
        [MemberContract.AutoGenerated]
        private readonly long _id;

        [MemberContract.AutoGenerated(Option = GeneratedValueOption.Database)]
        private readonly string _referenceNumber;

        private readonly CustomerEntity.Ref _customer;

        private readonly PaymentMethodEntity.Ref _paymentMethod;

        // amount the customer was charged, in the exact currency the customer paid with
        private readonly MoneyValueObject _amountPaid;

        // this is an internal value for the operator; it isn't displayed to the customer
        // this is amount converted to operator's currency, with payment processing fees deducted
        private readonly MoneyValueObject _amountReceived;

        [MemberContract.Semantics.Utc]
        private readonly DateTime _dateTime;

        [MemberContract.Validation.MaxLength(512)]
        private string _memo;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public long Id => _id;
        public string ReferenceNumber => _referenceNumber;
        public CustomerEntity.Ref Customer => _customer;
        public PaymentMethodEntity.Ref PaymentMethod => _paymentMethod;
        public MoneyValueObject AmountPaid => _amountPaid;
        public MoneyValueObject AmountReceived => _amountReceived;
        public DateTime DateTime => _dateTime;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string Memo
        {
            get => _memo;
            set => _memo = value;
        }
    }
}
