using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MVCManukauTech.ViewModels
{
    public class ConfirmPurchaseViewModel
    {
        public string Id { get; set; }
        [Required(ErrorMessage = "Required")]
        public string MembershipName { get; set; }
    }
}
