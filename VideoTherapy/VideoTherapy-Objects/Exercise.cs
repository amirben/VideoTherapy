using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoTherapy_Objects
{
    public class Exercise
    {
        public int ExerciseNumber { set; get; }
        public string ExerciseName { set; get; }
        public int Repetitions { set; get; }
        public int ExerciseCompleted { set; get; }
        public string LastViewed { set; get; }
        public bool IsPlayed { set; get; }
        public string ExerciseThumbs { set; get; }

    }
}
