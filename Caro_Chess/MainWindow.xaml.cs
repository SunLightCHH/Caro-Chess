using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.IO;
using Microsoft.Win32;

namespace Caro_Chess
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            time.Text = countTime.ToString();
        }

        // Biến kiểm tra xem đã đi hết bàn cờ chưa
        int SumCount = 0;

        #region Biến toàn cục

        int[,] a;
        Button[,] Buttons;
        int[,] test;
        int Cols = 4;
        int Rows = 4;

        bool Xturn = true;
        bool MayTurn = false;

        // Tạo 1 luồng đếm giờ song song với chương trình
        DispatcherTimer dt = new DispatcherTimer();
        int countTime = 10; // thời gian suy nghĩ là 10 giây

        #endregion
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dt.Interval = TimeSpan.FromSeconds(1);
            dt.Tick += dtTicker;

            int btnWidth = 40;
            int btnHeight = 40;

            a = new int[Rows, Cols];
            Buttons = new Button[Rows, Cols];

            // Đặt các button vào canvas
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Cols; j++)
                {
                    // Tạo 1 button
                    Buttons[i, j] = new Button();
                    Buttons[i, j].Height = btnHeight;
                    Buttons[i, j].Width = btnWidth;
                    Buttons[i, j].Background = Brushes.White;
                    Buttons[i, j].FontSize = 20;

                    Buttons[i, j].Tag = new Tuple<int, int>(i, j);
                    Buttons[i, j].Click += BtnClick;

                    // Đưa button ra màn hình
                    Canvas.Children.Add(Buttons[i, j]);
                    Canvas.SetLeft(Buttons[i, j], 160 + j * btnWidth);
                    Canvas.SetTop(Buttons[i, j], 160 + i * btnHeight);
                }
            }
        }
        #region Giao diện, hành động
        private void BtnClick(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var tuple = btn.Tag as Tuple<int, int>;

            int i = tuple.Item1;
            int j = tuple.Item2;

            //MessageBox.Show($"{i}, {j}");

            // Thay đổi hình ảnh, trạng thái khi nhấp chuột vào 1 ô
            if(a[i, j] == 0)
            {
                SumCount++;

                // Lượt của X
                if (Xturn)
                {
                    btn.Content = "X";
                    btn.Foreground = Brushes.Red;
                    a[i, j] = 1;
                }

                // Lượt của O
                else
                {
                    btn.Content = "O";
                    btn.Foreground = Brushes.Blue;
                    a[i, j] = 2;
                }

                // Chuyển lượt đánh dấu cho người còn lại
                Xturn = !Xturn;
                MayTurn = !MayTurn;

                // Kiểm tra thắng, thua và hòa
                var kt = checkWin(a, i, j);
                if(kt == 2)
                {
                    dt.Stop();
                    MessageBox.Show("O Won!");               
                    Reset();
                }
                if (kt == 1)
                {
                    dt.Stop();
                    MessageBox.Show("X Won!");                   
                    Reset();
                }
                if(SumCount == Rows * Cols)
                {
                    dt.Stop();
                    MessageBox.Show("Hòa");                  
                    Reset();
                }

                countTime = 10;
                SetTurn();
            }
        }

        // Chuyển đổi kí tự X, O trong label turn
        public void SetTurn()
        {
            if (Xturn)
            {
                turn.Content = "X";
                turn.Foreground = Brushes.Red;
            }
            else
            {
                turn.Content = "O";
                turn.Foreground = Brushes.Blue;
            }
        }

        // Nhấn vào nút Save
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var screen = new FileName();
            dt.Stop();
            StartTimeOut.Content = "Continute";
            StreamWriter Writer = null;
            if(screen.ShowDialog() == true)
            {
                if (!screen.filename.ToString().Equals(""))
                {
                    Writer = new StreamWriter(screen.filename);
                    // Lưu lại lượt đi hiện tại
                    // Xturn = true Lưu X không thì lưu O
                    Writer.WriteLine(Xturn ? "X" : "O");

                    // Lưu SumCount
                    Writer.WriteLine($"{SumCount}");

                    // Lưu lại thời gian
                    Writer.WriteLine($"{time.Text.ToString()}");

                    // Lưu ma trận hiện tại
                    for (int i = 0; i < Rows; i++)
                    {
                        for (int j = 0; j < Cols; j++)
                        {
                            Writer.Write($"{a[i, j]}");
                            if(j < Cols - 1)
                            {
                                Writer.Write(" ");
                            }
                        }
                        Writer.WriteLine("");
                    }
                    Writer.Close();
                    MessageBox.Show("Lưu thành công");
                }
            }
        }

        // Nhấn vào nút Load

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            var screen = new OpenFileDialog();
            if (screen.ShowDialog() == true)
            {
                var FileName = screen.FileName;

                var Reader = new StreamReader(FileName);

                // Đọc dòng đầu (lượt đi hiện tại)
                var firstLine = Reader.ReadLine();
                Xturn = firstLine == "X";
                SetTurn();

                // Đọc dòng tiếp theo Sumcount biến kiểm tra đã đi hết bàn cờ chưa
                SumCount = int.Parse(Reader.ReadLine());

                // Ghi lại thời gian
                countTime = int.Parse(Reader.ReadLine());
                time.Text = countTime.ToString();
                StartTimeOut.Content = "Continute";

                // Xuất lại bàn cờ
                for (int i = 0; i < Rows; i++)
                {
                    var token = Reader.ReadLine().Split(new string[] { " " }, StringSplitOptions.None);
                    for (int j = 0; j < Cols; j++)
                    {
                        a[i, j] = int.Parse(token[j]);
                        if(a[i, j] == 1)
                        {
                            Buttons[i, j].Content = "X";
                            Buttons[i, j].Foreground = Brushes.Red;
                        }
                        if(a[i, j] == 2)
                        {
                            Buttons[i, j].Content = "O";
                            Buttons[i, j].Foreground = Brushes.Blue;
                        }
                        if(a[i, j] == 0)
                        {
                            Buttons[i,j].Content = "";
                        }
                    }
                }
            }
        }

        // Nhấn nứt Reset
        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            Reset();
        }

        // Nhấn nứt Start With Time Out
        private void TimeOut_Click(object sender, RoutedEventArgs e)
        {
            StartTimeOut.Content = "Start With Time Out";
            dt.Start();
        }

        // Nhấn nút Computer
        private void Computer_Click(object sender, RoutedEventArgs e)
        {
            Reset();
            if (MayTurn)
            {
                // Kiểm tra xem có nước nào để máy thắng hay là người chơi thắng không
                for (int i = 0; i < Rows; i++)
                {
                    for (int j = 0; j < Cols; j++)
                    {
                        if(a[i, j] == 0)
                        {
                            test[i, j] = 2;
                            if(checkWin(test, i, j) == 2)
                            {
                                Buttons[i, j].Content = "O";
                                Buttons[i, j].Foreground = Brushes.Blue;
                                a[i, j] = 2;
                                return;
                            }
                            test[i, j] = 1;
                            if (checkWin(test, i, j) == 1)
                            {
                                Buttons[i, j].Content = "O";
                                Buttons[i, j].Foreground = Brushes.Blue;
                                a[i, j] = 2;
                                return;
                            }
                        }
                    }
                }

                // Tìm con đường dẫn đến chiến thắng

            }
        }
        #endregion

        #region Phương thức

        //Thiết lập trạng thái theo thời gian
        private void dtTicker(object sender, EventArgs e)
        {
            countTime--;
            if (countTime < 5)
            {
                time.Foreground = Brushes.Red;
            }
            else
            {
                time.Foreground = Brushes.Blue;
            }
            time.Text = (countTime).ToString();

            // Trường hợp 1 bên nghĩ quá lâu
            if (countTime == 0)
            {
                // X nghĩ quá lâu
                if (Xturn)
                {
                    MessageBox.Show("O won!");
                    Reset();
                }

                // O nghĩ quá lâu
                else
                {
                    MessageBox.Show("X won!");
                    Reset();
                }
                dt.Stop(); // Dừng bộ đếm giờ
            }
        }

        private int checkWin(int[,] a, int i, int j)
        {
            const int conditionWin = 4;
            int count;
            int di, dj;
            //----------Kiểm tra thắng theo chiều ngang---------------
            count = 1;
            // Kiểm tra phía bên trái tọa độ a[i,j] có bn phần tử trùng nhau
            di = 0;
            dj = -1;
            count += sameCharInLine(di, dj, i, j);

            // Kiểm tra phía bên phỉa tọa độ a[i, j] có bn phần tử trùng nhau
            di = 0;
            dj = 1;
            count += sameCharInLine(di, dj, i, j);

            if (count >= conditionWin)
            {
                return a[i, j];
            }

            //------------Kiểm tra thắng theo chiều dọc-------------
            count = 1;
            // Kiểm tra phía trên tọa độ a[i,j] có bn phần tử giống nhau
            di = -1;
            dj = 0;
            count += sameCharInLine(di, dj, i, j);

            // Kiểm tra phía dưới tọa độ a[i, j] có bn phần tử giống nhau
            di = 1;
            dj = 0;
            count += sameCharInLine(di, dj, i, j);

            if(count >= conditionWin)
            {
                return a[i, j];
            }

            //----------Kiểm tra theo đường chéo chính----------
            count = 1;
            // Kiểm tra ô chéo phía trên bên trái của a[i, j] có bn phần tử giống nhau
            di = -1;
            dj = -1;
            count += sameCharInLine(di, dj, i, j);

            // Kiểm tra ô chéo phía dưới bên phải của a[i, j] có bn phần tử giống nhau
            di = 1;
            dj = 1;
            count += sameCharInLine(di, dj, i, j);

            if (count >= conditionWin)
            {
                return a[i, j];
            }

            //----------------Kiểm tra đường chéo phụ-------------
            count = 1;
            // Kiểm tra ô chéo phía trên bên phải của a[i, j] có bn phần tử giống nhau
            di = -1;
            dj = 1;
            count += sameCharInLine(di, dj, i, j);

            // Kiểm tra ô chéo phía dưới bên trái của a[i, j] có bn phần tử giống nhau
            di = 1;
            dj = -1;
            count += sameCharInLine(di, dj, i, j);

            if(count >= conditionWin)
            {
                return a[i, j];
            }
            return 0;
        }

        int sameCharInLine(int di, int dj, int i, int j)
        {
            int count = 0;
            int StartJ = j;
            int StartI = i;

            while (true)
            {
                j += dj;
                i += di;
                if (i > 3 || i < 0 || j > 3 || j < 0)
                    break;
                if(a[StartI, StartJ] != a[i, j])
                {
                    break;
                }
                else
                {
                    count++;
                }
            }
            return count;
        }

        // Tạo trò chơi mới
        private void Reset()
        {
            for (int i = 0; i < Cols; i++)
            {
                for (int j = 0; j < Rows; j++)
                {
                    a[i, j] = 0;
                    Buttons[i, j].Content = "";
                }
            }
            Xturn = true;
            MayTurn = false;
            SetTurn();
            countTime = 10;
            time.Text = "10";
            SumCount = 0;
            time.Foreground = Brushes.Blue;
            dt.Stop();
        }

        #endregion
    }
}
