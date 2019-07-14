using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AiTest
{
    public class Game
    {
        private const int GoalRadius = 30;

        private static readonly Brush GoalColor = Brushes.Yellow;
        private static readonly Random random = new Random();

        private readonly int dotCount;

        private Canvas canvas;
        private List<Dot> dots = new List<Dot>();
        private Ellipse goalEllipse;
        private Point goalPosition;
        private Point goalCenter;
        private double fitnessSum;

        public event EventHandler FrameHappened;
        public event EventHandler NextGeneration;

        public int DeadDotCount => dots.Count(d => d.Dead);
        public int ReachedGoalCount => dots.Count(d => d.ReachedGoal);
        public double FrameRate { get; private set; }
        public double MaxFitness { get; private set; }
        public int Generation { get; private set; }
        public int Best { get; private set; }

        public Game(int count, Canvas canvas)
        {
            dotCount = count;
            this.canvas = canvas;

            var x = canvas.ActualWidth / 2.0d;
            var y = canvas.ActualHeight / 10.0d;
            goalCenter = new Point(x, y);

            goalPosition = new Point(goalCenter.X - (GoalRadius / 2d), goalCenter.Y - (GoalRadius / 2d));

            for (int i = 0; i < dotCount; i++)
            {
                dots.Add(new Dot(canvas, goalCenter, GoalRadius));
            }
        }

        public void Start()
        {
            PlaceGoal();
            Task.Run(() => Loop());
        }

        private void PlaceGoal()
        {
            goalEllipse = new Ellipse
            {
                Stroke = GoalColor,
                Fill = GoalColor,
                Height = GoalRadius,
                Width = GoalRadius,
            };

            canvas.Children.Add(goalEllipse);

            Canvas.SetLeft(goalEllipse, goalPosition.X);
            Canvas.SetTop(goalEllipse, goalPosition.Y);
        }

        public async Task Loop()
        {
            while (DeadDotCount < dotCount)
            {
                var frameStart = DateTime.Now;

                MoveDots();
                DrawDots();

                await Task.Delay(1);

                var frameTime = DateTime.Now - frameStart;
                FrameRate = 1000 / frameTime.TotalMilliseconds;
                FrameHappened?.Invoke(this, EventArgs.Empty);
            }

            Best = Math.Max(Best, ReachedGoalCount);
            BuildNextGeneration();

            NextGeneration?.Invoke(this, EventArgs.Empty);
        }

        private void BuildNextGeneration()
        {
            Generation++;
            canvas.Dispatcher.BeginInvoke(new Action(() =>
            {
                canvas.Children.Clear();
            }));
            CalculateFitness();
            SetBestDot();
            NaturalSelection();
            MutateDots();
        }

        private void MoveDots() => dots.ForEach(d => d.Move());

        private void DrawDots() => dots.ForEach(d => d.Draw());

        private void CalculateFitness() => dots.ForEach(d => d.CalculateFitness());

        private void SetBestDot()
        {
            fitnessSum = 0;
            var maxFitness = 0d;
            Dot maxFitnessDot = null;

            foreach (var d in dots)
            {
                fitnessSum += d.Fitness;

                if (d.Fitness > maxFitness)
                {
                    maxFitnessDot = d;
                    maxFitness = d.Fitness;
                }
            }

            maxFitnessDot.IsBest = true;
        }

        private void NaturalSelection()
        {
            //var bestDot = dots.First(d => d.IsBest).Clone();
            //bestDot.IsBest = true;

            //var newDots = new List<Dot> { bestDot };

            var newDots = new List<Dot>();
            var winningDots = dots.Where(d => d.ReachedGoal).Select(d => d.Clone());
            newDots.AddRange(winningDots);

            for (var i = 0; i < dotCount - winningDots.Count(); i++)
            {
                var parent = SelectParent();
                var clone = parent.Clone();
                //clone.Mutate();
                newDots.Add(clone);
            }

            dots = newDots;
        }

        private Dot SelectParent()
        {
            var next = random.NextDouble() * fitnessSum;
            var runningSum = 0d;

            foreach (var d in dots)
            {
                runningSum += d.Fitness;
                if (runningSum > next)
                {
                    return d;
                }
            }

            return null;
        }

        private void MutateDots() => dots.ForEach(d => d.Mutate());
    }
}
