namespace Stream_Music_MAUI
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            //MainPage = new AppShell();

        }
        protected override Window CreateWindow(IActivationState activationState) =>
        new Window(new AppShell())
        {
            Width = 700,
            Height = 700,
            X = 100,
            Y = 100
        };

    }
}
