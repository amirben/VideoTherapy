using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using VideoTherapy.Objects;

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
                    TrainingList = (T.Descendants("Training").Select(EX => new Training() {
                        TrainingNumber = Int32.Parse(EX.Element("TrainingNumber").Value),
                        TrainingName = EX.Element("TrainingName").Value,
                        Repetitions = Int32.Parse(EX.Element("Repetitions").Value),
                        TrainingCompleted = Int32.Parse(EX.Element("TrainingCompleted").Value),
                        
                        TrainingThumbs = EX.Element("TrainingThumbs").Value

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
