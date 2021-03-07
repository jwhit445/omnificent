using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Omni.API.Models {
    public class GetUserStatusResponse {

        ///<summary>The user status of the requested user.</summary>
        public UserStatus Status { get; set; }
    }
}
