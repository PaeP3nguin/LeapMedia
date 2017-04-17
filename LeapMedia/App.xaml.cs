using System.Windows;
using WindowsInput;

namespace LeapMedia {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        public static readonly InputSimulator InputSimulator = new InputSimulator();
    }
}
