using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sklep.Models.ViewModel
{
    public class ChangeDataInMyProfileViewModel
    {
        public ChangeEmailViewModel changeEmailViewModel { get; set; }
        public ChangePasswordViewModel changePasswordViewModel { get; set; }
        public DeliveryAddressViewModel deliveryAddressViewModel { get; set; }
        public ChangePersonalDataViewModel changePersonalDataViewModel { get; set; }
    }
}