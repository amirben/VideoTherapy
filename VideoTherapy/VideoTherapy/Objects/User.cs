using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoTherapy.Objects
{
    public abstract class User
    {
        public int AccountId { set; get; }
        public string FirstName { set; get; }
        public string LastName { set; get; }
        public string Email { set; get; }
        public string ImageThumb { set; get; }
        public string Gender { set; get; }
        public int Age { set; get; }
        public DateTime BirthDay { set; get; }
        public string UserProfileLink { set; get; }

        public string FullName
        {
            get
            {
                return FirstName + " " + LastName;
            }
        }
    }
}
