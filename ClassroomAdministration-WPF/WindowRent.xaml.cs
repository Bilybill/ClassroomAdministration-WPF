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
using System.Windows.Shapes;

namespace ClassroomAdministration_WPF
{
    /// <summary>
    /// WindowRent.xaml 的交互逻辑
    /// </summary>
    public partial class WindowRent : Window
    {
        Rent rent;
        WindowIndex father;

        public WindowRent(Rent r, WindowIndex fatherWindow)
        {

            InitializeComponent();
            rent = r;
            father = fatherWindow;

            if (father.WindowState == WindowState.Maximized)
            {
                this.Width = 450;
                this.Height = 450;
                BorderBackground.Width = 450;
                BorderBackground.Height = 450;
                TBinfo.FontSize = 40;

                TBhost.FontSize =
                TBrentTime.FontSize =
                TBtakepartinInfo.FontSize =
                TBclassroom.FontSize =
                TBclassroom_Copy.FontSize = 
                TBChoose.FontSize =
                TBexit.FontSize =
                TBOK.FontSize =
                TBDecline.FontSize = 22;
            }

        }

        private void BorderBackground_Loaded(object sender, RoutedEventArgs e)
        {
            switch (WindowIndex.currSkin)
            {
                case WindowIndex.skin.Starry:
                    BorderBackground.Background = new ImageBrush(WindowIndex.ChangeBitmapToImageSource(Properties.Resources.rentback));
                    break;
                case WindowIndex.skin.ColorBox:
                    BorderBackground.Background = new ImageBrush(WindowIndex.ChangeBitmapToImageSource(Properties.Resources.Color1));
                    break;
            }
        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            if (rent == null) return;

            TBinfo.Text = rent.Info;
            if (!rent.Approved) TBinfo.Text += " (未审核)";
            TBinfo.Background = new SolidColorBrush(MyColor.NameColor(rent.Info, 0.2));
            TBinfo.Foreground = new SolidColorBrush(WindowIndex.textColor);

            TBhost.Content = "申请人: " + DatabaseLinker.GetName(rent.pId);

            Classroom c = Building.GetClassroom(rent.cId);
            if (c != null) TBclassroom.Content = "教室: " + c.Name; else TBclassroom.Visibility = Visibility.Collapsed;

            TBrentTime.Content = "时间: "+rent.Time.Display();

            List<int> listPId = DatabaseLinker.GetPIdList(rent.rId);
            TBtakepartinInfo.Content = "参加人数: "+listPId.Count;

            if (father.personRentTable.Contains(rent.rId))
                TBChoose.Content = "从我的课程表中删除";
            else
                TBChoose.Content = "加入我的课程表";

            if (rent.Approved || father.Peron is User)
            {
                TBOK.Visibility = Visibility.Collapsed;
                TBDecline.Visibility = Visibility.Collapsed;
            }
        }

        private void TBclassroom_MouseDown(object sender, MouseButtonEventArgs e)
        {
            father.SetClassroom(rent.cId);
            if (rent.Time.OnceActivity)
            {
                Console.WriteLine("ONE ACTIVITY");
                father.GotoDateClass(rent.Time.StartDate, rent.Time.StartClass);
            }
            this.Close();
        }
        private void TB_MouseEnter(object sender, MouseEventArgs e)
        {
            Label tb = (Label)sender;
            tb.Background = new SolidColorBrush(MyColor.NameColor(rent.Info, 0.2));
        }
        private void TB_MouseLeave(object sender, MouseEventArgs e)
        {
            Label tb = (Label)sender;
            tb.Background = null;
        }

        private void TBexit_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void TBChoose_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (father.personRentTable.Contains(rent.rId))
            {
                if (MessageBox.Show("确定删除 "+rent.Info+"？", "删除课程", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    DatabaseLinker.DeleteTakepartin(father.Peron.pId, rent.rId);
                    father.personRentTable.Remove(rent);
                    father.RefreshSchedule();
                    this.Close();
                }
            }
            else
            {
                Rent rr = father.personRentTable.Add(rent.rId);
                if (rr == null)
                    DatabaseLinker.AddTakepartin(father.Peron.pId, rent.rId);
                else
                {
                    MessageBox.Show("添加失败。此课程同您的课程 " + rr.Info + " 存在冲突。");
                    if (!rent.Time.OnceActivity) father.GotoDateClass(rr.Time.StartDate, rr.Time.StartClass);
                }

                father.RefreshSchedule();

                this.Close();
            }
        }

        private void TBOK_MouseDown(object sender, MouseButtonEventArgs e)
        {

            if (DatabaseLinker.ApproveRent(rent))
            {
                rent.GetApproved();

                SysMsg msg = new SysMsg(0, rent.pId, DateTime.Now, "您申请的课程 " + rent.rId + ", " + rent.Info + " 已经通过审核.");
                DatabaseLinker.SendSysMsg(msg);

                MessageBox.Show("审核已通过.");
                this.Close();
            }

        }

        private void TBDecline_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (DatabaseLinker.DeleteRent(rent))
            {
                rent.GetApproved();

                SysMsg msg = new SysMsg(0, rent.pId, DateTime.Now, "对不起, 您申请的课程 " + rent.rId + ", " + rent.Info + " 没有通过审核, 已被管理员删除.");
                DatabaseLinker.SendSysMsg(msg);

                MessageBox.Show("已删除课程.");
                this.Close();
            }
        }

        private void TBtakepartinInfo_MouseDown(object sender, MouseButtonEventArgs e)
        {
            string s = "";

            List<int> listPId = DatabaseLinker.GetPIdList(rent.rId);
            foreach (int pId in listPId)
                s += DatabaseLinker.GetName(pId) + " ";

            if (listPId.Count == 0) s = "暂时没有人参加。";
            MessageBox.Show(s, "参加同学名单");
        }


        private void Window_PreviewKeyDown_1(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    this.Close();
                    break;
                case Key.Q:
                    TBChoose_MouseDown(null, null);
                    break;
                case Key.C:
                    TBclassroom_MouseDown(null, null);
                    break;
            }
        }

        private void Border_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        //Close Button
        private void CloseBorder_MouseEnter_1(object sender, MouseEventArgs e)
        {
            CloseBorder.Background = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255));
        }
        private void CloseBorder_MouseLeave(object sender, MouseEventArgs e)
        {
            CloseLabel.Margin = new Thickness(4, 0, 0.333, 0);
            CloseBorder.Background = null;
        }
        private void CloseBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CloseLabel.Margin = new Thickness(6, 0, 0.333, 0);
        }
        private void CloseBorder_MouseUp(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }


    }
}
