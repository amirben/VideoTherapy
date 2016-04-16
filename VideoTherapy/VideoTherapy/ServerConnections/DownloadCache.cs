using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

using VideoTherapy.Objects;
using System.IO;
using System.Threading;

namespace VideoTherapy.ServerConnections
{
    /// <summary>
    /// Class used for download temporery images and videos
    /// </summary>
    public class DownloadCache : IDisposable
    {
        #region members
        /// <summary>
        /// The current patient on the application
        /// </summary>
        private Patient _patient;

        /// <summary>
        /// The directory of the files
        /// </summary>
        private string dir;

        #endregion

        #region constractor
        /// <summary>
        /// The constractor
        /// <param name="_patient">Current user patient</param>
        /// </summary>
        public DownloadCache(Patient _patient)
        {
            this._patient = _patient;

            dir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        }
        #endregion

        #region methods
        /// <summary>
        /// Use for downloading all images for preview
        /// <param name = "_trainingIndex">current training index</param>
        /// </summary>
        public void DownloadTraining(int _trainingIndex)
        {
            Barrier barrier = new Barrier(1);

            //string newDir = CreateDir(dir, _patient.AccountId.ToString());
            
            ////create treatment dir (it if needed)
            //newDir = CreateDir(newDir, _patient.PatientTreatment.TreatmentNumber.ToString());
            
            ////create training dir (it if needed)
            //newDir = CreateDir(newDir, _patient.PatientTreatment.TrainingList[_trainingIndex].TrainingId.ToString());


            string newDir = CreateDir(dir, "trainingThumb");

            foreach (int key in _patient.PatientTreatment.TrainingList[_trainingIndex].Playlist.Keys)
            {

                Task downloadThread = new Task(() => {
                    int keyThread = key;

                    Exercise exercise = _patient.PatientTreatment.TrainingList[_trainingIndex].Playlist[keyThread][0];
                    string imagePath = newDir + "\\" + exercise.ExerciseId + ".png";

                    string _from = exercise.ExerciseThumbs;

                    if (!File.Exists(imagePath))
                    {
                        barrier.AddParticipants(1);
                        DownloadFile(_from, imagePath);

                        barrier.SignalAndWait();
                    }

                    foreach (Exercise _exercise in _patient.PatientTreatment.TrainingList[_trainingIndex].Playlist[keyThread])
                    {
                        _exercise.ExerciseThumbs = imagePath;
                    }

                    
                });
                downloadThread.Start();
            }

            barrier.SignalAndWait();

            //barrier.Dispose();
        }

        /// <summary>
        /// Download training preview images for the treatment screen
        /// </summary>
        public void DownloadTreatment()
        {
            //crearte dir for user (if not already created)
            //string newDir = CreateDir(dir, _patient.AccountId.ToString());
            //newDir = CreateDir(newDir, _patient.PatientTreatment.TreatmentNumber.ToString());
            string newDir = CreateDir(dir, "treatmentThumb");

            //initilaize the barrier
            int size = _patient.PatientTreatment.TrainingList.Count;
            //Barrier _barrier = new Barrier(size + 1);
            Barrier _barrier = new Barrier(1);

            for (int i = 0; i < size; i++)
            {
                int temp = i;

                string _from = _patient.PatientTreatment.TrainingList[temp].TrainingThumbs;
                string _to = newDir + "\\" + _patient.PatientTreatment.TrainingList[temp].TrainingId + ".png";
                _patient.PatientTreatment.TrainingList[temp].TrainingThumbs = _to;

                if (!File.Exists(_to))
                {
                    _barrier.AddParticipant();
                    Task tempThread = new Task(() =>
                    {
                        DownloadFile(_from, _to);
                        _barrier.SignalAndWait();
                    });
                    tempThread.Start();
                }
                
            }

            _barrier.SignalAndWait();

            _barrier.Dispose();  
        }

        /// <summary>
        /// Download all the images for the current session
        /// </summary>
        public void DownloadAllTreatmentImages()
        {
            Barrier barrier = new Barrier(_patient.PatientTreatment.TrainingList.Count + 2);

            Task treatmentThread = new Task(() =>
            {
                //Downloading all thumbs in treatment
                DownloadTreatment();

                barrier.SignalAndWait();
            });
            treatmentThread.Start();

            foreach(Training t in _patient.PatientTreatment.TrainingList)
            {
                Task tt = new Task(() =>
                {
                    DownloadTraining(_patient.PatientTreatment.TrainingList.IndexOf(t));
                    barrier.SignalAndWait();
                });
                tt.Start();     
                         
            }

            barrier.SignalAndWait();
            barrier.Dispose();
        }

        /// <summary>
        /// Download the gdb file for the gesture detection
        /// <param name="_exercise">Current user patient</param>
        /// </summary>
        public void DownloadGDBfile(Exercise _exercise)
        {
            Barrier _barrier = new Barrier(1);

            string newDir = CreateDir(dir, "DB");
            newDir += "\\" + _exercise.ExerciseId + ".gdb";

            if (!File.Exists(newDir))
            {
                DownloadFile(_exercise.DBUrl, newDir);
                _barrier.SignalAndWait();
            }

            _exercise.DBPath = newDir;
        }

        /// <summary>
        /// Download the file and save it in the path the pass.
        /// <param name="_from">path from to download</param>
        /// <param name="_to">path to download</param>
        /// </summary>
        private void DownloadFile(string _from, string _to)
        {
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    webClient.DownloadFile(_from, _to);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("<---------- error -------->");

                while (ex != null)
                {
                    Console.WriteLine(ex.Message);
                    ex = ex.InnerException;
                }
            }
        }


        /// <summary>
        /// Create temporery dir for the user
        /// <param name="_path">The current path of the application</param>
        /// <param name="_dirName">The dir name to create</param>
        /// </summary>
        private string CreateDir(string _path, string _dirName)
        {
            string _newPath = _path + "\\" + _dirName; 
            try
            {
                if (!Directory.Exists(_newPath))
                {
                    Directory.CreateDirectory(_newPath);
                    return _newPath;
                }
                else
                {
                    return _newPath;
                }
            }
            catch(IOException ex)
            {
                Console.WriteLine(ex.Message);
            }

            return "";
        }

        public void Dispose()
        {
        }

        #endregion 
    }
}
