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
        public int TreatmentId { set; get; }
        public Therapist TreatmentTherapist { set; get; }
        public List<Training> TrainingList { set; get; }
        public Training CurrentTraining { set; get; }
        public string StartDate { set; get; }

        public Training NextTraining()
        {
            int next = TrainingList.IndexOf(CurrentTraining);
            if (next + 1 < TrainingList.Count)
            {
                CurrentTraining = TrainingList[next + 1];
            }

            return CurrentTraining;
        }

        public Training PrevTraining()
        {
            int prev = TrainingList.IndexOf(CurrentTraining);
            if (prev - 1 >= 0)
            {
                CurrentTraining = TrainingList[prev - 1];
            }

            return CurrentTraining;
        }
    }

    
}
