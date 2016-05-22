using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Newtonsoft.Json;
using VideoTherapy.Objects;
using VideoTherapy.ServerConnections;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;

namespace VideoTherapy.Views.ExerciseScreen
{
    /// <summary>
    /// Interaction logic for QuestionnairePopUp.xaml
    /// </summary>
    public partial class QuestionnairePopUp : UserControl
    {
        private LinkedList<Question> questionList = new LinkedList<Question>();
        private LinkedListNode<Question> currentNode;

        public ExerciseView ExerciseView;

        private List<string> propertiesToSerialize = new List<string>(new string[]
        {
            "AnswerKey"
        });

        public QuestionnairePopUp()
        {
            InitializeComponent();
            this.Loaded += QuestionnairePopUp_Loaded;
        }

        private void QuestionnairePopUp_Loaded(object sender, RoutedEventArgs e)
        {
            CreateQuestions();
            currentNode = questionList.First;

            NumOfQuestion.Text = Convert.ToString(questionList.Count);

            UpdateBindingText();
        }

        private void UpdateBindingText()
        {
            LeftAnswer.DataContext = currentNode.Value.LeftAnswer;
            CenterAnswer.DataContext = currentNode.Value.CenterAnswer;
            RightAnswer.DataContext = currentNode.Value.RightAnswer;
            QuestionNumberTB.DataContext = currentNode.Value;
            QuestionText.DataContext = currentNode.Value;
        }

        //todo - get from server
        private void CreateQuestions()
        {
            Question q1 = new Question();
            q1.QuestionNumber = 1;
            q1.QuestionString = "How was your training?";

            q1.LeftAnswer = new Answer() { AnswerNum = 1, AnswerTitle = "Too difficult", AnswerKey = 'b'};
            q1.CenterAnswer = new Answer() { AnswerNum = 2, AnswerTitle = "Too easy", AnswerKey = 'c' };
            q1.RightAnswer = new Answer() { AnswerNum = 3, AnswerTitle = "Just right", AnswerKey = 'a' };
            q1.SelectedAnswer = ' ';


            Question q2 = new Question();
            q2.QuestionNumber = 2;
            q2.QuestionString = "Did you feel safe?";
            q2.LeftAnswer = new Answer() { AnswerNum = 1, AnswerTitle = "No", AnswerKey = 'b' };
            q2.CenterAnswer = new Answer() { AnswerNum = 2, AnswerTitle = "Moderately", AnswerKey = 'c' };
            q2.RightAnswer = new Answer() { AnswerNum = 3, AnswerTitle = "Yes", AnswerKey = 'a' };
            q2.SelectedAnswer = ' ';

            Question q3 = new Question();
            q3.QuestionNumber = 3;
            q3.QuestionString = "Did you enjoy your training?";
            q3.LeftAnswer = new Answer() { AnswerNum = 1, AnswerTitle = "No", AnswerKey = 'b' };
            q3.CenterAnswer = new Answer() { AnswerNum = 2, AnswerTitle = "It was ok", AnswerKey = 'c' };
            q3.RightAnswer = new Answer() { AnswerNum = 3, AnswerTitle = "Yes", AnswerKey = 'a' };
            q3.SelectedAnswer = ' ';

            questionList.AddLast(q1);
            questionList.AddLast(q2);
            questionList.AddLast(q3);
            
        }

        public void SetSize(int height, int width)
        {
            QuestionPopUpStackpanel.Width = width;
            QuestionPopUpStackpanel.Height = height;
        }

        public void NextQuesiton()
        {
            if (currentNode.Next != null)
            {
                currentNode = currentNode.Next;
                UpdateBindingText();
            }
            else
            {
                //update server the answers:
                SendAnswersToServer();
                ExerciseView.CloseQuestionnairePopUp(true);
            }
        }

        private async void SendAnswersToServer()
        {
            List<Question> list = questionList.ToList();


            JObject answers = new JObject();
            answers["10"] = list[0].SelectedAnswer.ToString();
            answers["20"] = list[1].SelectedAnswer.ToString();
            answers["30"] = list[2].SelectedAnswer.ToString();

            string json = JsonConvert.SerializeObject(questionList);
            //string json = answers.ToString();
            
            string response = await ApiConnection.ReportTrainingFeedback(json, ExerciseView.CurrentTraining.CalGuid, ExerciseView.CurrentTraining.CalEventId);
            Console.WriteLine(response);
        }

        private void LeftAnswer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            currentNode.Value.SelectedAnswer = currentNode.Value.LeftAnswer.AnswerKey;
            NextQuesiton();
        }

        private void CenterAnswer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            currentNode.Value.SelectedAnswer = currentNode.Value.CenterAnswer.AnswerKey;
            NextQuesiton();
        }

        private void RightAnswer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            currentNode.Value.SelectedAnswer = currentNode.Value.RightAnswer.AnswerKey;
            NextQuesiton();
        }

        private void CloseAndBackToTreatment_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ExerciseView.CloseQuestionnairePopUp(false);
        }

        private void QuestionAnsweredHandle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Answer currentAnswer = ((StackPanel)sender).DataContext as Answer;

            currentNode.Value.SelectedAnswer = currentAnswer.AnswerKey;
            NextQuesiton();
        }
    }

}
