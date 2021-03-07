using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Omni.API.Models {
    public class GetUserStatusRequest {

        ///<summary>The user to retrieve the status for.</summary>
        public User User { get; set; }
    }
}
