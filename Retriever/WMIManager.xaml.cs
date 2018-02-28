using System;
using System.Collections.Generic;
using System.Globalization;
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
using System.Windows.Shapes;
using DataTypes;

namespace Retriever
{
    /// <summary>
    /// Interaction logic for WMIManager.xaml
    /// </summary>
    public partial class WmiManager
    {
        public WmiManager()
        {
            InitializeComponent();
        }

        internal class Win32HardwareToInt : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value == null)
                    throw new NullReferenceException("Błąd konwertera: argument był wartości null.");
                if (targetType != typeof(Win32Hardware))
                    throw new InvalidOperationException("Błąd konwertera: celem powinien być typ Win32Hardware.");
                return (int)(Win32Hardware)value;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
}
}
