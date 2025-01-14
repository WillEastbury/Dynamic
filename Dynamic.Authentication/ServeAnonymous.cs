using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamic.Core.Authentication.Levels
{
    public class ServeAuthenticatedOnly : Attribute
    {
       
        
    }

    public class ServeAuthenticatedThisApplicationOnly : Attribute
    { 
        public string TargetApplication { get; set; }
    }

    public class ServeAuthorizedThisApplicationRoleOnly : Attribute
    {
        public string TargetApplication { get; set; }
        public string TargetRoleAccess { get; set; } 
    }

    public class ServeAuthorizedThisApplicationSpecialClaimOnly : Attribute
    {
        public string TargetApplication { get; set; }
        public string TargetRoleAccess { get; set; }
        public KeyValuePair<string, object> SpecialClaim { get; set; }
    }
}
