using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoTherapy.Objects
{
    public class Patient : User
    {
        public string UserProfileLink { set; get; }
        public Therapist PhisicalTherapist { set; get; }
        public Treatment PatientTreatment { set; get; }
    }
}
