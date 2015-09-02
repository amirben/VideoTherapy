using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoTherapy_Objects
{
    public class Treatment
    {
        public int TreatmentNumber { set; get; }
        public List<Exercise> ExerciseList { set; get; }
    }
}
