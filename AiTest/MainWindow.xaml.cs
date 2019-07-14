using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AiTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private const int DotCount = 100;
        private DateTime startTime = DateTime.Now;

        private Game brain;

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public string FrameRateText => $"{brain?.FrameRate ?? 0:F2} fps";
        public string TimeText => $"Elapsed {(DateTime.Now - startTime).TotalSeconds:F2}";
        public string DeadText => $"Dead {brain?.DeadDotCount ?? 0}/{DotCount}";
        public string MaxFitnessText => $"Max Fitness {brain?.MaxFitness ?? 0:F5}";
        public bool IsRestartButtonEnabled { get; set; } = true;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.KeyDown += MainWindow_KeyDown;
        }

        private void MainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                if (IsRestartButtonEnabled)
                {
                    Start();
                }
            }
        }

        private void Start()
        {
            Canvas.Children.Clear();
            IsRestartButtonEnabled = false;
            RaisePropertyChanged(nameof(IsRestartButtonEnabled));

            var startingLine = new Line
            {
                X1 = 0,
                X2 = Canvas.ActualWidth,
                Y1 = Canvas.ActualHeight * 0.75d,
                Y2 = Canvas.ActualHeight * 0.75d,
                Stroke = Brushes.Gray,
                StrokeThickness = 2,
                StrokeDashArray = new DoubleCollection(new[] { 2.0d, 2.0d })
            };

            Canvas.Children.Add(startingLine);

            brain = new Game(DotCount, Canvas);
            brain.FrameHappened += Brain_FrameHappened;
            brain.Finished += Brain_Finished;
            brain.Start();
            startTime = DateTime.Now;
        }

        private void Brain_Finished(object sender, EventArgs e)
        {
            brain.FrameHappened -= Brain_FrameHappened;
            brain.Finished -= Brain_Finished;
            IsRestartButtonEnabled = true;
            RaisePropertyChanged(nameof(IsRestartButtonEnabled));
        }

        private void Brain_FrameHappened(object sender, EventArgs e)
        {
            RaisePropertyChanged(nameof(FrameRateText));
            RaisePropertyChanged(nameof(TimeText));
            RaisePropertyChanged(nameof(DeadText));
            RaisePropertyChanged(nameof(MaxFitnessText));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Start();
        }
    }
}
