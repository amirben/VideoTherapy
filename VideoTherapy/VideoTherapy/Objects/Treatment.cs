using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoTherapy.Objects
{
    public class Treatment
    {
        public int TreatmentNumber { set; get; }
        public List<Training> TrainingList { set; get; }
        public Training CurrentTraining { set; get; }
    }
}
