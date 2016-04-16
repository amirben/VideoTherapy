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
        public Answer LeftAnswer { set; get; }
        public Answer CenterAnswer { set; get; }
        public Answer RightAnswer { set; get; }
        public int SelectedAnswer { set; get; }

    }
}
