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

            q1.LeftAnswer = new Answer() { AnswerNum = 1, AnswerTitle = "Too difficult" };
            q1.CenterAnswer = new Answer() { AnswerNum = 2, AnswerTitle = "Too easy" };
            q1.RightAnswer = new Answer() { AnswerNum = 3, AnswerTitle = "Just right" };
            q1.SelectedAnswer = 0;


            Question q2 = new Question();
            q2.QuestionNumber = 2;
            q2.QuestionString = "Did you feel safe?";
            q2.LeftAnswer = new Answer() { AnswerNum = 1, AnswerTitle = "No" };
            q2.CenterAnswer = new Answer() { AnswerNum = 2, AnswerTitle = "Moderately" };
            q2.RightAnswer = new Answer() { AnswerNum = 3, AnswerTitle = "Yes" };
            q2.SelectedAnswer = 0;

            Question q3 = new Question();
            q3.QuestionNumber = 3;
            q3.QuestionString = "Did you enjoy your training?";
            q3.LeftAnswer = new Answer() { AnswerNum = 1, AnswerTitle = "No"};
            q3.CenterAnswer = new Answer() { AnswerNum = 2, AnswerTitle = "It was ok" };
            q3.RightAnswer = new Answer() { AnswerNum = 3, AnswerTitle = "Yes" };
            q3.SelectedAnswer = 0;

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
            string json = JsonConvert.SerializeObject(questionList);

            string response = await ApiConnection.ReportTrainingFeedback(json, ExerciseView.CurrentTraining.CalGuid, ExerciseView.CurrentTraining.CalEventId);
        }



        private void LeftAnswer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            currentNode.Value.SelectedAnswer = currentNode.Value.LeftAnswer.AnswerNum;
            NextQuesiton();
        }

        private void CenterAnswer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            currentNode.Value.SelectedAnswer = currentNode.Value.CenterAnswer.AnswerNum;
            NextQuesiton();
        }

        private void RightAnswer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            currentNode.Value.SelectedAnswer = currentNode.Value.RightAnswer.AnswerNum;
            NextQuesiton();
        }

        private void CloseAndBackToTreatment_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ExerciseView.CloseQuestionnairePopUp(false);
        }

        private void QuestionAnsweredHandle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Answer currentAnswer = ((StackPanel)sender).DataContext as Answer;

            currentNode.Value.SelectedAnswer = currentAnswer.AnswerNum;
            NextQuesiton();
        }
    }
}
