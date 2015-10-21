using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoTherapy_Objects
{
    public class Exercise
    {
        public int ExerciseId { set; get; }
        public int ExerciseNum { set; get; }
        public String ExerciseName { set; get; }
        public int Repetitions { set; get; }
        public string ExerciseThumbs { set; get; }
        public string VideoPath { set; get; }
    }
}
