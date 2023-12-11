using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.RightsManagement;
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
        private static List<UpgradeTask> tasks = new List<UpgradeTask>();
        private static List<Vector2> positionsRandomClick = [];
        private static MainWindow _instance;

        private static int pos2Index = -1;
        private static int pos1Index = -1;
        private static int pos3Index = -1;
        private static int pos4Index = -1;
        private static bool addRandom = false;
        private static bool hasStartedFlashing = false;
        private static int posBefore = -1;
        private static int index = -1;
        private static bool isRecording = true;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, KBDLLHOOKSTRUCT lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, KBDLLHOOKSTRUCT lParam);
        [StructLayout(LayoutKind.Sequential)]
        public struct KBDLLHOOKSTRUCT
        {
            public uint vkCode;
            public uint scanCode;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }
        private static IntPtr KeyboardProc(int nCode, IntPtr wParam, KBDLLHOOKSTRUCT lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                switch (lParam.vkCode)
                {
                    case 0x70:
                        hasStartedFlashing = !hasStartedFlashing;
                        break;
                    case 0xBE:
                        if (pos1Index != -1)
                        {
                            pos1Index = -1;
                            pos2Index = index;
                        }
                        else if (pos2Index != -1)
                        {
                            pos2Index = -1;
                            pos3Index = index;
                        }
                        else if (pos3Index != -1)
                        {
                            pos3Index = -1;
                            pos4Index = index;
                        }
                        else if (pos1Index == -1 && pos2Index == -1 && pos3Index == -1 && pos4Index == -1 && index != -1)
                        {
                            pos1Index = index;
                        }
                        else if (pos4Index != -1)
                        {
                            pos4Index = -1;
                            index = -1;
                        }

                        _instance.GetMousePosition();
                        break;

                    case 0x2E:
                        Application.Current.Shutdown();
                        Environment.Exit(0);
                        break;
                }
            }

            return CallNextHookEx(_hook, nCode, wParam, lParam);
        }

        private static IntPtr _hook = IntPtr.Zero;
        private static LowLevelKeyboardProc _proc = KeyboardProc;


        public MainWindow()
        {
            _instance = this;
            InitializeComponent();
            using (var process = Process.GetCurrentProcess())
            using (var module = process.MainModule)
            {
                _hook = SetWindowsHookEx(WH_KEYBOARD_LL, _proc,
                    GetModuleHandle(module.ModuleName), 0);
            }
        }

        private void AddClick(object sender, RoutedEventArgs e)
        {
            var upgradeTask = new UpgradeTask
            {
                Id = row,
                Time = -1,
            };
            tasks.Add(upgradeTask);
            tasks.ElementAt(row).positions = new();

            var textboxName = new TextBox
            {
                Name = "name" + row,
                Text = "",
            };

            textboxName.SetValue(Grid.RowProperty, row + 1);
            textboxName.SetValue(Grid.ColumnProperty, 0);

            pnlInputs.Children.Add(textboxName);

            var textboxHours = new TextBox
            {
                Name = "hours" + row,
                Text = "0",
            };

            textboxHours.SetValue(Grid.RowProperty, row + 1);
            textboxHours.SetValue(Grid.ColumnProperty, 1);

            pnlInputs.Children.Add(textboxHours);


            var textboxMinutes = new TextBox
            {
                Name = "minutes" + row,
                Text = "0",
            };

            textboxMinutes.SetValue(Grid.RowProperty, row + 1);
            textboxMinutes.SetValue(Grid.ColumnProperty, 2);

            pnlInputs.Children.Add(textboxMinutes);



            var button = new Button
            {
                Content = "Start Recording Positions",
                Name = "pos1_" + row,
            };

            button.SetValue(Grid.RowProperty, row + 1);
            button.SetValue(Grid.ColumnProperty, 3);

            button.Click += Pos1Click;

            var buttonSave = new Button
            {
                Content = "Save",
                Name = "save_" + row
            };

            buttonSave.Click += SaveTimer;

            var checkbox1 = new CheckBox
            {
                IsChecked = false,
                IsEnabled = false,
                Name = "pos1_" + row,
            };

            checkbox1.SetValue(Grid.RowProperty, row + 1);
            checkbox1.SetValue(Grid.ColumnProperty, 4);

            pnlInputs.Children.Add(checkbox1);

            var checkbox2 = new CheckBox
            {
                IsChecked = false,
                IsEnabled = false,
                Name = "pos2_" + row,
            };

            checkbox2.SetValue(Grid.RowProperty, row + 1);
            checkbox2.SetValue(Grid.ColumnProperty, 5);

            pnlInputs.Children.Add(checkbox2);

            var checkbox3 = new CheckBox
            {
                IsChecked = false,
                IsEnabled = false,
                Name = "pos3_" + row,
            };

            checkbox3.SetValue(Grid.RowProperty, row + 1);
            checkbox3.SetValue(Grid.ColumnProperty, 6);

            pnlInputs.Children.Add(checkbox3);

            var checkbox4 = new CheckBox
            {
                IsChecked = false,
                IsEnabled = false,
                Name = "pos4_" + row,
            };

            checkbox4.SetValue(Grid.RowProperty, row + 1);
            checkbox4.SetValue(Grid.ColumnProperty, 7);

            pnlInputs.Children.Add(checkbox4);



            buttonSave.SetValue(Grid.RowProperty, row + 1);
            buttonSave.SetValue(Grid.ColumnProperty, 8);

            pnlInputs.Children.Add(button);

            pnlInputs.Children.Add(buttonSave);



            row++;
        }



        private void SaveTimer(object sender, RoutedEventArgs e)
        {
            var butt = sender as Button;
            int positionNr = int.Parse(butt!.Name.Split("_")[1]);

            int hours = 0;
            int minutes = 0;
            string name = "";

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
                    if (textBox.Name == "name" + positionNr)
                    {
                        name = textBox.Text;
                    }
                }
            }
            var task = tasks.ElementAt(positionNr);
            task.Time = hours * 3600 + minutes * 60;
            task.Name = name;

            Console.WriteLine($"Upgrade starting in {task.Time} seconds");
            for (int i = 0; i < task.positions.Count; i++)
            {
                Console.WriteLine($"Position {i + 1}: x: {task.positions.ElementAt(i).X} y: {task.positions.ElementAt(i).Y}");
            }

            lstTasks.Items.Add(task);


        }



        private void Pos1Click(object sender, RoutedEventArgs e)
        {
            var butt = sender as Button;

            if (butt.Content == "Start Recording Positions")
            {
                isRecording = true;
                int index = int.Parse(butt!.Name.Split("_")[1]);
                MainWindow.index = index;
                butt.Content = "Stop Recording Positions";
            }
            else
            {
                isRecording = false;
                index = -1;
                pos1Index = -1;
                pos2Index = -1;
                pos3Index = -1;
                pos4Index = -1;
                butt.Content = "Start Recording Positions";
            }
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

            var label = new Label
            {
                Content = "Name",
                FontWeight = FontWeights.Bold,
            };

            label.SetValue(Grid.RowProperty, 0);
            label.SetValue(Grid.ColumnProperty, 0);

            pnlInputs.Children.Add(label);

            var label1 = new Label
            {
                Content = "Set hours",
                FontWeight = FontWeights.Bold,
            };

            label1.SetValue(Grid.RowProperty, 0);
            label1.SetValue(Grid.ColumnProperty, 1);

            pnlInputs.Children.Add(label1);


            var label2 = new Label
            {
                Content = "Set minutes",
                FontWeight = FontWeights.Bold,
            };

            label2.SetValue(Grid.RowProperty, 0);
            label2.SetValue(Grid.ColumnProperty, 2);

            pnlInputs.Children.Add(label2);

            var label3 = new Label
            {
                Content = "Start Recording",
                FontWeight = FontWeights.Bold,
            };

            label3.SetValue(Grid.RowProperty, 0);
            label3.SetValue(Grid.ColumnProperty, 3);

            pnlInputs.Children.Add(label3);

            var label4 = new Label
            {
                Content = "Pos1",
                FontWeight = FontWeights.Bold,
            };

            label4.SetValue(Grid.RowProperty, 0);
            label4.SetValue(Grid.ColumnProperty, 4);

            pnlInputs.Children.Add(label4);

            var label5 = new Label
            {
                Content = "Pos2",
                FontWeight = FontWeights.Bold,
            };

            label5.SetValue(Grid.RowProperty, 0);
            label5.SetValue(Grid.ColumnProperty, 5);

            pnlInputs.Children.Add(label5);

            var label6 = new Label
            {
                Content = "Pos3",
                FontWeight = FontWeights.Bold,
            };

            label6.SetValue(Grid.RowProperty, 0);
            label6.SetValue(Grid.ColumnProperty, 6);

            pnlInputs.Children.Add(label6);

            var label7 = new Label
            {
                Content = "Pos4",
                FontWeight = FontWeights.Bold,
            };

            label7.SetValue(Grid.RowProperty, 0);
            label7.SetValue(Grid.ColumnProperty, 7);

            pnlInputs.Children.Add(label7);


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




            if (addRandom)
            {
                var vec = new Vector2
                {
                    X = point.X,
                    Y = point.Y
                };


                positionsRandomClick.Add(vec);
            }
            else
            {

                var vec = new Vector2
                {
                    X = point.X,
                    Y = point.Y
                };
                if (pos1Index != -1)
                {

                    tasks.ElementAt(index).positions.Add(vec);
                    foreach (var item in pnlInputs.Children)
                    {
                        if (item is CheckBox)
                        {
                            var checkbox = item as CheckBox;
                            if (checkbox!.Name == "pos1_" + pos1Index)
                            {
                                checkbox.IsChecked = true;
                            }
                        }
                    }
                }
                else if (pos2Index != -1)
                {
                    tasks.ElementAt(index).positions.Add(vec);
                    foreach (var item in pnlInputs.Children)
                    {
                        if (item is CheckBox)
                        {
                            var checkbox = item as CheckBox;
                            if (checkbox!.Name == "pos2_" + pos2Index)
                            {
                                checkbox.IsChecked = true;
                            }
                        }
                    }
                }
                else if (pos3Index != -1)
                {
                    tasks.ElementAt(index).positions.Add(vec);
                    foreach (var item in pnlInputs.Children)
                    {
                        if (item is CheckBox)
                        {
                            var checkbox = item as CheckBox;
                            if (checkbox!.Name == "pos3_" + pos3Index)
                            {
                                checkbox.IsChecked = true;
                            }
                        }
                    }
                }
                else if (pos4Index != -1)
                {
                    tasks.ElementAt(index).positions.Add(vec);
                    isRecording = false;
                    foreach (var item in pnlInputs.Children)
                    {
                        if (item is CheckBox)
                        {
                            var checkbox = item as CheckBox;
                            if (checkbox!.Name == "pos4_" + pos4Index)
                            {
                                checkbox.IsChecked = true;
                            }
                        }
                    }
                    foreach (var item in pnlInputs.Children)
                    {
                        if (item is Button)
                        {
                            var button = item as Button;
                            if (button.Content.ToString().Contains("Recording"))
                            {
                                button!.Content = "Start Recording Positions";
                            }

                            isRecording = false;
                        }
                    }

                    index = -1;
                    pos4Index = -1;
                }
            }

            //var ut = new UpgradeTask(point.X, point.Y, point.X, point.Y, 3);
            //tasks.Add(ut);
            Console.WriteLine($"Set Position to x: {point.X} y: {point.Y}");
        }



        private void Window_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void CheckUpgradceDo(object? state)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                lstTasks.Items.Refresh();
            });
            if (positionsRandomClick.Count() >= 2 && hasStartedFlashing)
            {
                int rand = -1;
                do
                {
                    rand = new Random().Next(positionsRandomClick.Count());
                } while (rand == posBefore);
                posBefore = rand;
                var element = positionsRandomClick.ElementAt(rand);
                LeftClick((int)element.X, (int)element.Y);
            }

            foreach (var ut in tasks.Where(x => x.Time > 0))
            {
                ut.Tick();
                if (ut.Time > 0) continue;

                int i = 0;
                foreach (var item in ut.positions)
                {
                    LeftClick((int)item.X, (int)item.Y);
                    i++;
                    if (i < ut.positions.Count) { 
                        Thread.Sleep(400);
                    }
                }

                Console.WriteLine($"Executeing Upgrade {ut.Name}");
            }

            //tasks = tasks.Where(x => x.Time != 0).ToList();

        }

        class UpgradeTask
        {
            public int Id { get; set; }
            public List<Vector2> positions { get; set; }
            public int Time { get; set; }

            public string Name { get; set; }


            public void Tick() => Time--;

            public UpgradeTask()
            {
            }

            public override string ToString()
            {
                TimeSpan time = TimeSpan.FromSeconds(Time);

                string str = time.ToString(@"hh\:mm\:ss\:fff");
                string name = Name == "" ? "" : $" {Name}";
                return $"Upgrade{name}: Time left: {time}";
            }
        }

        private void AddRandomClicks(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            addRandom = !addRandom;
            if (addRandom)
            {
                button.Content = "Stop record random clicks";
            }
            else {
                button.Content = "Start record random clicks";

            }

        }


    }

}