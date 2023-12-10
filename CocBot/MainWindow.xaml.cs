using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CocBot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int row = 0;
        private List<UpgradeTask> tasks = new List<UpgradeTask>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void AddClick(object sender, RoutedEventArgs e)
        {
            var upgradeTask = new UpgradeTask
            {
                Id = row,
                Time = -1,
            };
            tasks.Add(upgradeTask);
            var textboxHours = new TextBox
            {
                Text = "hours",
                Name = "hours" + row
            };

            textboxHours.SetValue(Grid.RowProperty, row);
            textboxHours.SetValue(Grid.ColumnProperty, 0);

            pnlInputs.Children.Add(textboxHours);


            var textboxMinutes = new TextBox
            {
                Text = "minutes",
                Name = "minutes" + row

            };

            textboxMinutes.SetValue(Grid.RowProperty, row);
            textboxMinutes.SetValue(Grid.ColumnProperty, 1);

            pnlInputs.Children.Add(textboxMinutes);



            var button = new Button
            {
                Content = "Set Position 1",
                Name = "pos1_" + row,
            };

            var button2 = new Button
            {
                Content = "Set Position 2",
                Name = "pos2_" + row,
            };

            var button3 = new Button
            {
                Content = "Set Position 3",
                Name = "pos3_" + row,
            };

            button3.Click += Pos3Click;


            button3.SetValue(Grid.RowProperty, row);
            button3.SetValue(Grid.ColumnProperty, 4);

            button.SetValue(Grid.RowProperty, row);
            button.SetValue(Grid.ColumnProperty, 2);

            button2.SetValue(Grid.RowProperty, row);
            button2.SetValue(Grid.ColumnProperty, 3);

            button.Click += Pos1Click;
            button2.Click += Pos2Click;

            var buttonSave = new Button
            {
                Content = "Save",
                Name = "save_" + row
            };

            buttonSave.Click += SaveTimer;

            buttonSave.SetValue(Grid.RowProperty, row);
            buttonSave.SetValue(Grid.ColumnProperty, 5);

            pnlInputs.Children.Add(button);
            pnlInputs.Children.Add(button2);
            pnlInputs.Children.Add(button3);

            pnlInputs.Children.Add(buttonSave);



            row++;
        }

       

        private void SaveTimer(object sender, RoutedEventArgs e)
        {
            var butt = sender as Button;
            int positionNr = int.Parse(butt!.Name.Split("_")[1]);

            int hours = 0;
            int minutes = 0;

            foreach (var item in pnlInputs.Children)
            {
                if (item is TextBox)
                {
                    var textBox = item as TextBox;
                    if (textBox!.Name == "minutes" + positionNr)
                    {
                        minutes = int.Parse(textBox.Text);
                    }
                    if (textBox.Name == "hours" + positionNr)
                    {
                        hours = int.Parse(textBox.Text);
                    }
                }
            }
            var task = tasks.ElementAt(positionNr);
            task.Time = hours * 3600 + minutes * 60;

            Console.WriteLine($"Upgrade starting in {task.Time} seconds");
            Console.WriteLine($"Position 1: x: {task.X1} y: {task.Y1}");
            Console.WriteLine($"Position 2: x: {task.X2} y: {task.Y2}");

        }

        private int pos2Index = -1;
        private int pos1Index = -1;
        private int pos3Index = -1;

        private void Pos2Click(object sender, RoutedEventArgs e)
        {
            var butt = sender as Button;
            int index = int.Parse(butt!.Name.Split("_")[1]);
            pos2Index = index;
            pos1Index = -1;
            pos3Index = -1;
        }

        private void Pos1Click(object sender, RoutedEventArgs e)
        {
            var butt = sender as Button;
            int index = int.Parse(butt!.Name.Split("_")[1]);
            pos2Index = -1;
            pos3Index = -1;
            pos1Index = index;
        }

        private void Pos3Click(object sender, RoutedEventArgs e)
        {
            var butt = sender as Button;
            int index = int.Parse(butt!.Name.Split("_")[1]);
            pos2Index = -1;
            pos1Index = -1;
            pos3Index = index;
        }

        [DllImport("user32.dll")]
        static extern void mouse_event(int dwFlags, int dx, int dy,
                      int dwData, int dwExtraInfo);

        [Flags]
        public enum MouseEventFlags
        {
            LEFTDOWN = 0x00000002,
            LEFTUP = 0x00000004,
            MIDDLEDOWN = 0x00000020,
            MIDDLEUP = 0x00000040,
            MOVE = 0x00000001,
            ABSOLUTE = 0x00008000,
            RIGHTDOWN = 0x00000008,
            RIGHTUP = 0x00000010
        }

        public static void LeftClick(int x, int y)
        {
            Console.WriteLine("Derf nd");
            SetCursorPos(x, y);
            mouse_event((int)(MouseEventFlags.LEFTDOWN), 0, 0, 0, 0);
            mouse_event((int)(MouseEventFlags.LEFTUP), 0, 0, 0, 0);
        }

        [DllImport("user32.dll", EntryPoint = "SetCursorPos")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetCursorPos(int x, int y);


        public Timer timer;
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            timer = new Timer(CheckUpgradceDo, null, 0, 1000);
        }


        internal static class CursorPosition
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct PointInter
            {
                public int X;
                public int Y;
                public static explicit operator Point(PointInter point) => new Point(point.X, point.Y);
            }

            [DllImport("user32.dll")]
            public static extern bool GetCursorPos(out PointInter lpPoint);

        }
        private void GetMousePosition()
        {
            var point = new CursorPosition.PointInter();
            CursorPosition.GetCursorPos(out point);





            if (pos1Index != -1)
            {
                tasks.ElementAt(pos1Index).X1 = point.X;
                tasks.ElementAt(pos1Index).Y1 = point.Y;
            }
            else if (pos2Index != -1)
            {
                tasks.ElementAt(pos2Index).X2 = point.X;
                tasks.ElementAt(pos2Index).Y2 = point.Y;
            }
            else if(pos3Index != -1) {
                tasks.ElementAt(pos3Index).X3 = point.X;
                tasks.ElementAt(pos3Index).Y3 = point.Y;
            }

            //var ut = new UpgradeTask(point.X, point.Y, point.X, point.Y, 3);
            //tasks.Add(ut);
            Console.WriteLine("SetPos");
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            Console.WriteLine("Key Down");
            if (e.Key == Key.OemPeriod)
                GetMousePosition();
        }

        private void CheckUpgradceDo(object? state)
        {
            //Console.WriteLine(tasks.Count());

            foreach (var ut in tasks.Where(x => x.Time >= 0))
            {
                ut.Tick();
                if (ut.Time > 0) continue;


                LeftClick(ut.X1, ut.Y1);
                Thread.Sleep(300);
                LeftClick(ut.X2, ut.Y2);
                Thread.Sleep(300);
                LeftClick(ut.X3, ut.Y3);
                Console.WriteLine("Execute");
            }

            //tasks = tasks.Where(x => x.Time != 0).ToList();

        }

        class UpgradeTask
        {
            public int Id { get; set; }
            public int X1 { get;  set; }
            public int Y1 { get;  set; }
            public int X2 { get;  set; }
            public int Y2 { get;  set; }
            public int X3 { get; set; }
            public int Y3 { get; set; }
            public int Time { get;  set; }


            public void Tick() => Time--;

            public UpgradeTask()
            {
            }
        }

    }

}