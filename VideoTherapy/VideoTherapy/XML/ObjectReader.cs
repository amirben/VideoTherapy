using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using VideoTherapy_Objects;

namespace VideoTherapy.XML
{
    // Read data from XML (or Server) and using LINQ to create objects
    public static class ObjectReader
    {
        public static List<Treatment> GetAllTreatments()
        {
            try
            {
                //Load XML document
                XDocument Xdoc = XDocument.Load(@"XML\TreatmentsList.xml");

                var Query = Xdoc.Descendants("Treatment").Select(T => new Treatment()
                {
                    TreatmentNumber = Int32.Parse(T.Attribute("num").Value),
                    ExerciseList = (T.Descendants("Exercise").Select(EX => new Exercise() {
                        ExerciseNumber = Int32.Parse(EX.Element("ExerciseNumber").Value),
                        ExerciseName = EX.Element("ExerciseName").Value,
                        Repetitions = Int32.Parse(EX.Element("Repetitions").Value),
                        ExerciseCompleted = Int32.Parse(EX.Element("ExerciseCompleted").Value),
                        LastViewed = EX.Element("LastViewed").Value,
                        IsPlayed = bool.Parse(EX.Element("IsPlayed").Value),
                        ExerciseThumbs = EX.Element("ExerciseThumbs").Value

                    })).ToList()
                    

                }).ToList();

                return Query;
            }catch(Exception ex)
            {
                throw new Exception(ex.Message);
                
            }

           
        }
    }
}
