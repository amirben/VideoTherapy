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
        private Barrier _barrier;
        private Patient _patient;
        private string dir;

        private Object theLock = new Object();

        public DownloadCache(Patient _patient)
        {
            this._patient = _patient;

            dir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        }

        /// <summary>
        /// Download videos and images of the current training
        /// <param name="_patient">Current user patient</param>
        /// </summary>
        public void DownloadTraining(Patient _patient, int _trainingIndex)
        {
            _barrier = new Barrier(1);

            string newDir = CreateDir(dir, _patient.AccountId.ToString());
            //create treatment dir (it if needed)
            newDir = CreateDir(newDir, _patient.PatientTreatment.TreatmentNumber.ToString());
            //create training dir (it if needed)
            newDir = CreateDir(newDir, _patient.PatientTreatment.TrainingList[_trainingIndex].TrainingId.ToString());

            int size = _patient.PatientTreatment.TrainingList[_trainingIndex].Playlist.Count;
            for (int i = 0; i < size; i++)
            {
                int temp = i;

                Exercise exercise = _patient.PatientTreatment.TrainingList[_trainingIndex].Playlist[temp];

                string exercisePath = newDir + "\\" + exercise.ExerciseId;

                //string videoPath = exercisePath;
                string imagePath = exercisePath;

                if (exercise.isDemo)
                {
                    //videoPath += "_demo";
                    imagePath += "_demo";
                }

                //videoPath += ".mp4";
                imagePath += ".png";

                //To download once the file
                if (!exercise.isDuplicate)
                {
                    //download video
                    //string _from1 = exercise.VideoPath;
                    //Thread temp1 = new Thread(() => DownloadFile(_from1, videoPath));

                    //download image
                    string _from2 = exercise.ExerciseThumbs;
                    Thread temp2 = new Thread(() => DownloadFile(_from2, imagePath));

                    _barrier.AddParticipants(1);
                    //_barrier.AddParticipants(2);

                    //temp1.Start();
                    temp2.Start();
                }

                //Duplicate the path to the rest of them
                //_patient.PatientTreatment.TrainingList[_trainingIndex].Playlist[temp].VideoPath = videoPath;
                _patient.PatientTreatment.TrainingList[_trainingIndex].Playlist[temp].ExerciseThumbs = imagePath;
            }

            _barrier.SignalAndWait();
            Console.WriteLine("-----> finished download");
        }

        /// <summary>
        /// Download treatment images and current training Id
        /// <param name="_patient">Current user patient</param>
        /// </summary>
        public void DownloadTreatment()
        {
            //crearte dir for user (if not already created)
            string newDir = CreateDir(dir, _patient.AccountId.ToString());
            newDir = CreateDir(newDir, _patient.PatientTreatment.TreatmentNumber.ToString());

            //initilaize the barrier
            int size = _patient.PatientTreatment.TrainingList.Count;
            _barrier = new Barrier(size + 1);

            for (int i = 0; i < size; i++)
            {
                int temp = i;
                //Thread tempThread = new Thread(() =>
                //                _patient.PatientTreatment.TrainingList[temp].TrainingThumbs = DownloadImageFile(_patient, newDir, ref temp));

                string _from = _patient.PatientTreatment.TrainingList[temp].TrainingThumbs;
                string _to = newDir + "\\" + _patient.PatientTreatment.TrainingList[temp].TrainingId + ".png";
                _patient.PatientTreatment.TrainingList[temp].TrainingThumbs = _to;

                Thread tempThread = new Thread(() => DownloadFile(_from, _to));
                tempThread.Start();
            }

            _barrier.SignalAndWait();
            
        }

        public void DownloadAllTreatmentImages()
        {
            //Downloading all thumbs in treatment
            DownloadTreatment();
            
            //download all thumbs in each training

            foreach(Training t in _patient.PatientTreatment.TrainingList)
            {
                DownloadTraining(_patient, _patient.PatientTreatment.TrainingList.IndexOf(t));
            }

        }

        public void DownloadGDBfile(Exercise _exercise)
        {
            _barrier = new Barrier(1);

            string newDir = CreateDir(dir, _patient.AccountId.ToString());
            newDir = CreateDir(newDir, _patient.PatientTreatment.TreatmentNumber.ToString());
            newDir = CreateDir(newDir, _patient.PatientTreatment.CurrentTraining.TrainingId.ToString());
            newDir = CreateDir(newDir, _exercise.ExerciseId.ToString());

            newDir += "\\" + _exercise.ExerciseId + ".gdb";

            DownloadFile(_exercise.DBUrl, newDir);
            _exercise.DBPath = newDir;
        }

        private void DownloadFile(string _from, string _to)
        {
            try
            {
                //lock (theLock)
                //{
                //    Console.WriteLine("From : =>");
                //    Console.WriteLine(_from);
                //    Console.WriteLine("To : =>");
                //    Console.WriteLine(_to);
                //    Console.WriteLine();
                //}

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

            _barrier.SignalAndWait();
            Console.WriteLine("Num of participate {0}", _barrier.ParticipantsRemaining);
        }

        private string DownloadImageFile(Patient _patient, string _path, ref int i)
        {
            Training tempTraining = _patient.PatientTreatment.TrainingList[i];
            string path = String.Format("{0}\\{1}.png", _path, tempTraining.TrainingId);

            using (WebClient webClient = new WebClient())
            {
                try
                {  
                    //string path = String.Format("{0}\\{1}\\{2}\\{3}.png", dir, _patient.AccountId, _patient.PatientTreatment.TreatmentNumber, tempTraining.TrainingId);                

                    webClient.DownloadFile(tempTraining.TrainingThumbs, path);

                    _patient.PatientTreatment.TrainingList[i].TrainingThumbs = path;

                }

                catch (Exception ex)
                {
                    while (ex != null)
                    {
                        Console.WriteLine(ex.Message);
                        ex = ex.InnerException;
                    }
                }

                _barrier.SignalAndWait();

                return path;
            }
        }

        /// <summary>
        /// Delete user temporery dir after exiting the program
        /// <param name="_patientId">Current user patient Id, use to know the path</param>
        /// </summary>
        public bool DeleteTempDir(int _patientId)
        {

            return false;
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
            _barrier.Dispose();
        }
    }
}
