using System.Windows;

namespace AutoRenamer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {

        }


        public void Init()
        {
            Initializer.Init();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Init();
        }
    }
}
