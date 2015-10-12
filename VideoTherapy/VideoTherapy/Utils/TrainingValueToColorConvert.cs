using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;
using VideoTherapy_Objects;

namespace VideoTherapy.Utils
{
    class TrainingValueToColorConvert : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
           Training training = (Training)System.Convert.ChangeType(value, typeof(Training));

            if (training.TrainingCompleted == 0)
            {
                return 0;
            }

            if (training.Repetitions == training.TrainingCompleted)
            {
                return 1;
            }

            if (training.Repetitions > training.TrainingCompleted)
            {
                return -1;
            }

            return -1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
