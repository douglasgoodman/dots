using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AiTest
{
    public class Dot
    {
        private bool DrawLine = false;
        private const int BrainDirectionCount = 100;
        private const int MinSpeed = 50;
        private const int MaxSpeed = 200;
        private const double DotRadius = 5;
        private const int DirectionChangeMs = 100;

        private static readonly Brush AliveColor = Brushes.Green;
        private static readonly Brush DeadColor = Brushes.DarkGray;
        private static readonly Brush WinnerColor = Brushes.HotPink;
        private static readonly Brush BestColor = Brushes.Red;
        private static readonly Random random = new Random();

        private readonly Brain brain;
        private readonly Canvas canvas;
        private readonly Point goalCenter;
        private readonly int goalRadius;

        private DateTime lastMove = DateTime.MaxValue;
        private DateTime lastDirectionChange = DateTime.MaxValue;

        public Point Position { get; set; }
        public Point Center => new Point(Position.X + (DotRadius / 2), Position.Y + (DotRadius / 2));
        public VelocityVector Velocity { get; set; }
        public Ellipse Ellipse { get; set; }
        public Line Line { get; set; }
        public double Fitness { get; set; }

        private bool dead = false;
        public bool Dead
        {
            get => dead;
            set
            {
                dead = value;
                SetColor();
            }
        }

        private bool isBest = false;
        public bool IsBest
        {
            get => isBest;
            set
            {
                isBest = value;
                SetColor();
            }
        }


        private bool reachedGoal = false;
        public bool ReachedGoal
        {
            get => reachedGoal;
            set
            {
                reachedGoal = value;
                SetColor();
            }
        }

        public Dot(Canvas canvas, Point goalCenter, int goalRadius)
            : this(canvas, goalCenter, goalRadius, new Brain(BrainDirectionCount))
        {
        }

        private Dot(Canvas canvas, Point goalCenter, int goalRadius, Brain brain)
        {
            this.canvas = canvas;
            this.goalCenter = goalCenter;
            this.goalRadius = goalRadius;
            this.brain = brain;

            var x = (canvas.ActualWidth / 2.0d) - (DotRadius / 2.0d);
            var y = (canvas.ActualHeight / 10.0) * 9.0d;
            Position = new Point(x, y);

            Velocity = new VelocityVector
            {
                //Speed = (random.NextDouble() * (MaxSpeed - MinSpeed)) + MinSpeed,
                Speed = MaxSpeed,
                Direction = 90
            };

            canvas.Dispatcher.BeginInvoke(new Action(() =>
            {
                Ellipse = new Ellipse
                {
                    Height = DotRadius,
                    Width = DotRadius,
                };

                canvas.Children.Add(Ellipse);

                if (DrawLine)
                {
                    Line = new Line()
                    {
                        StrokeThickness = 1,
                        X1 = Position.X,
                        X2 = Position.X + Velocity.Speed * Math.Cos(Velocity.DirectionInRadians),
                        Y1 = Position.Y,
                        Y2 = Position.Y - Velocity.Speed * Math.Sin(Velocity.DirectionInRadians)
                    };

                    canvas.Children.Add(Line);
                }

                SetColor();
            }));
        }

        private void SetColor()
        {
            canvas.Dispatcher.BeginInvoke(new Action(() =>
            {
                var color = IsBest ? BestColor : ReachedGoal ? WinnerColor : Dead ? DeadColor : AliveColor;

                Ellipse.Fill = color;
                Ellipse.Stroke = color;

                if (DrawLine)
                {
                    Line.Stroke = color;
                }
            }));
        }

        public void Move()
        {
            if (Dead)
            {
                return;
            }

            if (lastMove == DateTime.MaxValue)
            {
                lastMove = DateTime.Now;
                return;
            }

            var timeSinceLastMove = DateTime.Now - lastMove;
            lastMove = DateTime.Now;

            var distance = Velocity.Speed * timeSinceLastMove.TotalSeconds;

            var x = distance * Math.Cos(Velocity.DirectionInRadians);
            var y = -distance * Math.Sin(Velocity.DirectionInRadians);
            Position = new Point(Position.X + x, Position.Y + y);

            if (Position.X < 0 ||
                Position.X > canvas.ActualWidth - 5 ||
                Position.Y < 0 ||
                Position.Y > canvas.ActualHeight - 5)
            {
                Dead = true;
                return;
            }

            var distanceFromGoal = goalCenter - Center;
            if (distanceFromGoal.Length < (goalRadius / 2d))
            {
                Dead = true;
                ReachedGoal = true;
                SetColor();
            }

            if (lastDirectionChange == DateTime.MaxValue)
            {
                lastDirectionChange = DateTime.Now;
                return;
            }

            var timeSinceLastDirectionChange = DateTime.Now - lastDirectionChange;
            if (timeSinceLastDirectionChange.TotalMilliseconds > DirectionChangeMs)
            {
                var direction = brain.NextDirection();
                if (direction.HasValue)
                {
                    Velocity.Direction += direction.Value;
                    lastDirectionChange = DateTime.Now;
                }
                else
                {
                    Dead = true;
                    return;
                }
            }
        }

        public void Draw()
        {
            if (!Dead)
            {
                canvas.Dispatcher.BeginInvoke(new Action(() =>
                {
                    Canvas.SetLeft(Ellipse, Position.X);
                    Canvas.SetTop(Ellipse, Position.Y);

                    if (DrawLine)
                    {
                        Line.X1 = Position.X + (DotRadius / 2);
                        Line.X2 = Position.X + Velocity.Speed * Math.Cos(Velocity.DirectionInRadians);
                        Line.Y1 = Position.Y + (DotRadius / 2);
                        Line.Y2 = Position.Y - Velocity.Speed * Math.Sin(Velocity.DirectionInRadians);
                    }
                }));
            }
        }

        public void CalculateFitness()
        {
            if (ReachedGoal)
            {
                Fitness = (1.0d / 16.0d) + (10000.0d / (brain.Step * brain.Step));
            }
            else
            {
                var distanceFromGoal = goalCenter - Center;
                Fitness = 1.0d / distanceFromGoal.LengthSquared;
            }
        }

        public Dot Clone()
        {
            return new Dot(canvas, goalCenter, goalRadius, brain.Clone());
        }

        public void Mutate() => brain.Mutate();
    }
}
