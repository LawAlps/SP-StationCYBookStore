using System;
using System.Collections.Generic;

namespace MVCManukauTech.Models.DB
{
    public partial class MembershipType
    {
        public MembershipType()
        {
            AspNetUsers = new HashSet<AspNetUsers>();
        }

        public int MembershipTypeId { get; set; }
        public string MembershipTypeName { get; set; }
        public double Discount { get; set; }

        public virtual ICollection<AspNetUsers> AspNetUsers { get; set; }
    }
}
