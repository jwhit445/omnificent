using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Omni.API.Models {
    public class ReportUserRequest {

        ///<summary>Username of the user that reported.</summary>
        public string Reporter { get; set; }

        ///<summary>Username of the user to report.</summary>
        public string Username { get; set; }
    }
}
