using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoTherapy.Objects
{
    public class Question
    {
        public int QuestionNumber { set; get; }
        public string QuestionString{ set; get; }
        public string LeftAnswer { set; get; }
        public string CenterAnswer { set; get; }
        public string RightAnswer { set; get; }
        public string SelectedAnswer { set; get; }

    }
}
