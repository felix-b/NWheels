//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace NWheels.Samples.RestService
{
    using System;
    using System.Collections.Generic;
    
    public partial class Order
    {
        public Order()
        {
            this.OrderLines = new HashSet<OrderLine>();
        }
    
        public int Id { get; set; }

        public System.DateTime DateTime
        {
            get
            {
                return _utc;
            }
            set
            {
                _utc = value;
            }
        }
        public string CustomerEmail { get; set; }
    
        public virtual ICollection<OrderLine> OrderLines { get; set; }
    }
}
