﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoTherapy.Objects
{
    public class Training
    {
        public int TrainingId { set; get; }
        public int TrainingNumber { set; get; }
        public string TrainingName { set; get; }
        public bool isRecommended { set; get; }
        public int Repetitions { set; get; }
        public int TrainingCompleted { set; get; }
        public DateTime LastViewed { set; get; }
        public string TrainingThumbs { set; get; }
        public List<Exercise> Playlist { set; get; }
        
        public Dictionary<int, List<Exercise>> Playlist2 { set; get; }

        public Boolean Downloaded { set; get;}
        public Boolean SkipDemo { set; get; } 

        public int TrainingScore { set; get; }
        public int TrainingCompliance { set; get; }
    }
}
