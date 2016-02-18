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
        public Training RecommendedTraining { set; get; }
        public DateTime StartDate { set; get; }
        public DateTime EndDate { set; get; }

        public int TreatmentProgress
        {

            set;
            get;

        }

        public Training NextTraining()
        {
            if (!TrainingList.Last().Equals(CurrentTraining))
            {
                CurrentTraining = TrainingList[TrainingList.IndexOf(CurrentTraining) + 1];
            }

            //int next = TrainingList.IndexOf(CurrentTraining);
            //if (next + 1 < TrainingList.Count)
            //{
            //    CurrentTraining = TrainingList[next + 1];
            //}

            return CurrentTraining;
        }

        public Training PrevTraining()
        {
            if (!TrainingList.First().Equals(CurrentTraining))
            {
                CurrentTraining = TrainingList[TrainingList.IndexOf(CurrentTraining) - 1];
            }

            //int prev = TrainingList.IndexOf(CurrentTraining);
            //if (prev - 1 >= 0)
            //{
            //    CurrentTraining = TrainingList[prev - 1];
            //}

            return CurrentTraining;
        }
    }

    
}
