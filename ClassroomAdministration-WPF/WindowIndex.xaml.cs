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
using System.Collections.ObjectModel;

namespace ClassroomAdministration_WPF
{
    /// <summary>
    /// WindowIndex.xaml 的交互逻辑
    /// </summary>
    public partial class WindowIndex : Window
    {        
        public WindowIndex(Person p)
        {
            InitializeComponent();
            person = p;

            EnsureSkins();
            ApplySkin(StarrySkin);

            if (person.pId == 0) SetStatus(status.Message);
            else SetStatus(status.Table);
        }

        enum status { Info, Table, Message }
        status currStatus = status.Table;

        #region 换肤

        public enum skin { Starry, ColorBox }
        static public skin currSkin = skin.Starry;
        static public Color textColor = Colors.White;

        static ResourceDictionary ColorBoxSkin = null, StarrySkin = null;

        //初始化ResourceDictionary
        void EnsureSkins()
        {
            if (ColorBoxSkin == null)
            {
                ColorBoxSkin = new ResourceDictionary();
                ColorBoxSkin.Source = new Uri("StyleColorBox.xaml", UriKind.Relative);

                StarrySkin = new ResourceDictionary();
                StarrySkin.Source = new Uri("StyleStarry.xaml", UriKind.Relative);
            }
        }
        //应用ResourceDictionary
        void ApplySkin(ResourceDictionary newSkin)
        {
            Collection<ResourceDictionary> appMergedDictionaries = Application.Current.Resources.MergedDictionaries;

            if (appMergedDictionaries.Count != 0)
                appMergedDictionaries.Remove(appMergedDictionaries[0]);

            appMergedDictionaries.Add(newSkin);

        }

        //设置皮肤
        void SetSkin(skin newSkin)
        {
            if (currSkin == newSkin) return;

            currSkin = newSkin;

            switch (currSkin)
            {
                case skin.Starry:
                    
                    ApplySkin(StarrySkin);
                    BorderMain.Background = new ImageBrush(ChangeBitmapToImageSource(Properties.Resources.tableback2));
                    textColor = Colors.White;
                    
                    break;
                case skin.ColorBox:
                    
                    ApplySkin(ColorBoxSkin);
                    BorderMain.Background = new ImageBrush(ChangeBitmapToImageSource(Properties.Resources.Color3));
                    textColor = Colors.Black;
                    break;
            }

            TextBlockChosenDate.Foreground = new SolidColorBrush(textColor);
            TextBoxCId.Foreground = new SolidColorBrush(textColor);

            RefreshSchedule();

        }

        //从资源中抓取图片
        public static ImageSource ChangeBitmapToImageSource(System.Drawing.Bitmap bitmap)
        {

            IntPtr hBitmap = bitmap.GetHbitmap();
            ImageSource wpfBitmap = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            return wpfBitmap;

        }

        #endregion

        #region 个人信息逻辑
        private void GridInfo_Loaded(object sender, RoutedEventArgs e)
        {
            textBlockPersonName.Text = person.Name;
            textBlockPId.Text = "学号：" + person.pId;

            if (person is Administrator)
                textBlockDepartment.Text = "管理员用户";
            else textBlockDepartment.Text = "专业：" + (person as User).Department;

            List<Rent> listRent = DatabaseLinker.GetMyApplyingRents(person.pId);
            foreach (Rent r in listRent)
            {
                TextBlock tb = new TextBlock();
                MyTextBlockInitialize(tb, r);
                if (r.Approved) wrapPanelRents.Children.Add(tb);
                else wrapPanelRentsUnapproved.Children.Add(tb);
            }
        }

        private void MyTextBlockInitialize(TextBlock tb, Rent r)
        {
            tb.Tag = r;
            tb.Height = 125; tb.Width = 125;
            tb.Margin = new Thickness(10);

            tb.Background = MyColor.NameBrush(r.Info);
            tb.Text = r.Info; if (!r.Approved) tb.Text += "(未审核)";
            Classroom c = Building.GetClassroom(r.cId); if (c != null) tb.Text += ("@" + c.Name);
            tb.FontSize = 16;

          //  tb.Foreground = new SolidColorBrush(textColor);
            tb.TextWrapping = TextWrapping.Wrap;

            tb.MouseDown += tb_MouseDown;

            tb.MouseEnter += tb_MouseEnter;
            tb.MouseLeave += tb_MouseLeave;

        }
        void tb_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Rent r = (sender as TextBlock).Tag as Rent;
         
            new WindowRent(r, this).ShowDialog();

            e.Handled = false;
        }
        void tb_MouseLeave(object sender, MouseEventArgs e)
        {
            TextBlock tb = (TextBlock)sender;
            Rent r = (Rent)(tb.Tag);
            tb.Background = MyColor.NameBrush(r.Info);
        }
        void tb_MouseEnter(object sender, MouseEventArgs e)
        {
            TextBlock tb = (TextBlock)sender;
            Rent r = (Rent)(tb.Tag);
            tb.Background = MyColor.NameBrush(r.Info, 0.8);
        }
        #endregion

        #region 系统消息逻辑

        List<SysMsg> listSysMsg;

        private void GridMessage_Loaded(object sender, RoutedEventArgs e)
        {
            listSysMsg = DatabaseLinker.GetPersonSysMsgList(person.pId);

            for (int i = listSysMsg.Count - 1; i >= 0; --i)
            {
                SysMsg msg = listSysMsg[i];
                TextBlock tb = new TextBlock();
                TextBlockMessageInitialize(tb, msg);
                stackPanelMessage.Children.Add(tb);
            }

            if (person.pId == 0)
            {
                List<Rent> listRent = DatabaseLinker.GetUnapprovedRentTable();

                foreach (Rent r in listRent)
                {
                    TextBlock tb = new TextBlock();
                    TextBlockUnapprovedRentInitialize(tb, r);
                    stackPanelMessage.Children.Add(tb);
                }
            }

        }
        
       // 系统消息初始化
        private void TextBlockMessageInitialize(TextBlock tb, SysMsg msg)
        {
            string sendName = DatabaseLinker.GetName(msg.SendId);

            tb.FontSize = 24;
            tb.Padding = new Thickness(16);
            tb.Inlines.Add(new Bold(new Run(sendName + ": ("+msg.Time.ToString("yyyy/MM/dd")+")\r\n")));
            tb.Inlines.Add(new Run("  " + msg.Info));
            tb.TextWrapping = TextWrapping.Wrap;

          //  tb.MouseEnter += tbRent_MouseEnter;
          //  tb.MouseLeave += tbRent_MouseLeave;

            tb.Tag = msg;
        }

        //(管理员功能)查看未审核课程
        private void TextBlockUnapprovedRentInitialize(TextBlock tb, Rent r)
        {
            string applicantName = DatabaseLinker.GetName(r.pId);
           string s = r.Info;
            Classroom c = Building.GetClassroom(r.cId); if (c != null) s += ("@" + c.Name);

            tb.Inlines.Add(new Bold(new Run(applicantName + ":\r\n")));
            tb.Inlines.Add(new Run("  " + s));

            tb.FontSize = 24;
            tb.Padding = new Thickness(16);

            tb.MouseDown += tbRent_MouseDown;
            tb.MouseEnter += tbRent_MouseEnter;
            tb.MouseLeave += tbRent_MouseLeave;

            tb.Tag = r;
        }

        void tbRent_MouseEnter(object sender, MouseEventArgs e)
        {
            TextBlock tb = sender as TextBlock;
            tb.Background = new SolidColorBrush(MyColor.NameColor(tb.Text, 0.2));
        }
        void tbRent_MouseLeave(object sender, MouseEventArgs e)
        {
            TextBlock tb = sender as TextBlock;
            tb.Background = null;
        }
        void tbRent_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Rent r = (sender as TextBlock).Tag as Rent;

           
            new WindowRent(r, this).ShowDialog();
         
        }
        

        #endregion

        #region 课程表逻辑

        //课表尺寸
        const int cntCol = 7, cntRow = 14;

        //左侧: 个人课程表
        Person person;
        Classroom classroom = null;

        Schedule sch1, sch2;


        private void GridTable_Loaded(object sender, RoutedEventArgs e)
        {
            Building.Initialize();

            sch1 = new Schedule(person, GridSchedule1, RectangleChosonClass1, GridScheduleHead, this);
            sch2 = new Schedule(null, GridSchedule2, RectangleChosonClass2, GridScheduleHead2, this);

            if (sch1.ChosenRent != null)
            {
                SetCId(sch1.ChosenRent.cId);
            }

            if (DateTime.Now > RentTime.FirstDate && DateTime.Now < RentTime.LastDate)
            {
                Schedule.CurrDate = DateTime.Now.Date;
            }


            Schedule.CurrClass = 1;
            string t = DateTime.Now.ToString("HH:mm~HH:mm"); Console.WriteLine(t);
            while (Schedule.CurrClass < 14 && string.Compare(t, RentTime.StringClassTime[Schedule.CurrClass]) > 0) ++Schedule.CurrClass;
            SetDateClass(Schedule.CurrDate, Schedule.CurrClass);

            checkoutWeek();
        }

        //选择日期时间
        private void SetDateClass(DateTime date, int cc)
        {

            if (cc < 1 || cc > 14) return;

            if (date < RentTime.FirstDate)
            {
                MessageBox.Show("您选择的日期不在本学期内。");
                if (Schedule.CurrDate < RentTime.FirstDate) date = RentTime.FirstDate; else return;
            }

            Schedule.CurrDate = date;
            Schedule.CurrClass = cc;

            int days = (Schedule.CurrDate - RentTime.FirstDate).Days;
            SetWeeks(days / 7 + 1);
            Schedule.CurrWeekDay = days % 7;

            DateChosen.SelectedDate = date;
            TextBlockChosenDate.Text = date.ToString("yyyy/MM/dd");

            sch1.SetDateClass(date, cc);
            sch2.SetDateClass(date, cc);

            ChosenRentControl();
        }
        private void SetDateClass(int weekDay, int cc)
        {

            Schedule.CurrWeekDay = weekDay;
            Schedule.CurrDate = RentTime.FirstDate + new TimeSpan(7 * (Schedule.CurrWeek - 1) + weekDay, 0, 0, 0);

            SetDateClass(Schedule.CurrDate, cc);
        }

        //设置周数
        private void checkoutWeek()
        {
            LabelWeek.Content = person.Name + "的第" + Schedule.CurrWeek + "周";
            if (classroom != null) LabelClassroom.Content = classroom.Name + "的第" + Schedule.CurrWeek + "周";

            sch1.checkoutWeek();
            sch2.checkoutWeek();
        }
        private void SetWeeks(int w)
        {
            if (w != Schedule.CurrWeek)
            {
                Schedule.CurrWeek = w;
                checkoutWeek();
            }
            
        }
 
  
        //检查选中的时间点的课程
        private void ChosenRentControl()
        {

            if (CouldApply())
            {
                RectangleChosonClass1.Visibility = Visibility.Visible;
                RectangleChosonClass2.Visibility = Visibility.Visible;

                RectangleChosonClass1.Content = "+申请活动";
                RectangleChosonClass2.Content = "+申请活动";
            }
            else
            {
                RectangleChosonClass1.Content = "";
                RectangleChosonClass2.Content = "";

                RectangleChosonClass1.Visibility = sch1.ChosenRent == null ? Visibility.Visible : Visibility.Collapsed;
                RectangleChosonClass2.Visibility = (sch2.ChosenRent == null && classroom != null) ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        private bool CouldApply()
        {

            if (classroom == null) return false;

            if (sch1.ChosenRent != null || sch2.ChosenRent != null) return false;

            return sch1.RentTable.QuiteFreeTime(Schedule.CurrDate, Schedule.CurrClass) && sch2.RentTable.QuiteFreeTime(Schedule.CurrDate, Schedule.CurrClass);

        }


        //设置教室。在教室发生变化的时候切换UI
        private void SetCId(int cId)
        {
            Classroom C = Building.GetClassroom(cId);
            if (null == C) return;
            if (classroom != null && classroom.cId == C.cId) return;

            TextBoxCId.Text = cId.ToString();

            classroom = C;
          
            LabelClassroom.Content = classroom.Name + "的第" + Schedule.CurrWeek + "周";


            sch2.ChangeOwner(classroom);

            checkoutWeek();
            ChosenRentControl();
        }

        #endregion      
        #region 课程表交互
        //通过Calendar选择date
        private void DateChosen_CalendarClosed(object sender, RoutedEventArgs e)
        {
            DateTime date = (DateTime)DateChosen.SelectedDate;
            SetDateClass(date, Schedule.CurrClass);
        }

        //通过textBox选择教室
        private void TextBoxCId_TextChanged(object sender, TextChangedEventArgs e)
        {
            int cId;
            if (int.TryParse(TextBoxCId.Text, out cId))
            {
                SetCId(cId);
            }
        }

        //单击选定框
        private void RectangleChosonRent1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (CouldApply())
            {
                 new WindowApplyRent(person, classroom, Schedule.CurrDate, Schedule.CurrClass, this).ShowDialog();
            }
        }

        //进入教室列表
        private void LabelClassroom_MouseEnter(object sender, MouseEventArgs e)
        {
            LabelClassroom.Content = "进入教室列表";
            LabelClassroom.Background = new SolidColorBrush(Color.FromArgb(51, 255, 255, 255));
            LabelClassroom.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 86, 157, 229));
        }
        private void LabelClassroom_MouseLeave(object sender, MouseEventArgs e)
        {
            if (classroom != null) LabelClassroom.Content = classroom.Name + "的第" + Schedule.CurrWeek + "周";
            LabelClassroom.Background = null;
            LabelClassroom.BorderBrush = null;
        }
        private void LabelClassroom_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RentTable rt = new RentTable(DatabaseLinker.GetDateRentTable(Schedule.CurrDate).GetFromDateClass(Schedule.CurrDate, Schedule.CurrClass));
            new WindowClassroomList(rt, this).ShowDialog();
        }
        #endregion

        #region 键盘 & 鼠标滚轮
        //全体键盘托管
        private void Window_PreviewKeyDown_1(object sender, KeyEventArgs e)
        {


            //换肤
            switch (e.Key)
            {
                case Key.F1: SetSkin(skin.Starry); break;
                case Key.F2: SetSkin(skin.ColorBox); break;
                case Key.D1: SetStatus(status.Info); break;
                case Key.D2: SetStatus(status.Table); break;
                case Key.D3: 
                    RentTable rt = new RentTable(DatabaseLinker.GetDateRentTable(Schedule.CurrDate).GetFromDateClass(Schedule.CurrDate, Schedule.CurrClass));
                    new WindowClassroomList(rt, this).ShowDialog();
                    break;
                case Key.D4: SetStatus(status.Message); break;
            }

            switch (currStatus)
            {
                case status.Table:
                    //课程表控制
                    if (!TextBoxCId.IsKeyboardFocused)
                    {
                        switch (e.Key)
                        {
                            case Key.Up: if (Schedule.CurrClass > 1) --Schedule.CurrClass; break;
                            case Key.Down: if (Schedule.CurrClass < cntRow) ++Schedule.CurrClass; break;
                            case Key.Left: if (Schedule.CurrDate > RentTime.FirstDate) Schedule.CurrDate -= new TimeSpan(1, 0, 0, 0); break;
                            case Key.Right: if (Schedule.CurrDate < RentTime.LastDate) Schedule.CurrDate += new TimeSpan(1, 0, 0, 0); break;
                            case Key.Home: Schedule.CurrClass = 1; break;
                            case Key.End: Schedule.CurrClass = cntRow; break;
                            case Key.PageUp: if (Schedule.CurrWeek > 1) Schedule.CurrDate -= new TimeSpan(7, 0, 0, 0); break;
                            case Key.PageDown: if (Schedule.CurrWeek < 23) Schedule.CurrDate += new TimeSpan(7, 0, 0, 0); break;
                            case Key.Enter:
                                Rent r = sch1.ChosenRent;
                                if (r == null) break;
                                new WindowRent(r, this).ShowDialog();
                                break;
                        }
                        SetDateClass(Schedule.CurrDate, Schedule.CurrClass);
                    }
                    else
                    //教室控制
                    {
                        int b, c;
                        if (classroom == null) { b = 0; c = 0; }
                        else { b = classroom.Building.bId; c = classroom.cId; }

                        switch (e.Key)
                        {
                            case Key.Up:
                                while (c < Classroom.MaxCId)
                                {
                                    ++c;
                                    if (Building.GetClassroom(c) != null) break;
                                }
                                break;
                            case Key.Down:
                                while (c > Classroom.MinCId)
                                {
                                    --c;
                                    if (Building.GetClassroom(c) != null) break;
                                }
                                break;
                            case Key.PageUp:
                                while (b < Building.MaxBId)
                                {
                                    ++b;
                                    if (Building.GetBuilding(b) != null)
                                    {
                                        c = Building.GetBuilding(b).Classrooms[0].cId;
                                        break;
                                    }
                                }
                                break;
                            case Key.PageDown:
                                while (b > Building.MinBId)
                                {
                                    --b;
                                    if (Building.GetBuilding(b) != null)
                                    {
                                        c = Building.GetBuilding(b).Classrooms[0].cId;
                                        break;
                                    }
                                }
                                break;
                        }
                        SetCId(c);
                    }
                    break;
            }
        }
        //鼠标滚轮托管
        private void Window_PreviewMouseWheel_1(object sender, MouseWheelEventArgs e)
        {
            switch (currStatus)
            {
                case status.Table:
                    int d = e.Delta / 120;
                    if (d > 0)
                        if (Schedule.CurrWeek > 1) Schedule.CurrDate -= new TimeSpan(7, 0, 0, 0);
                    if (d < 0)
                        if (Schedule.CurrWeek < 23) Schedule.CurrDate += new TimeSpan(7, 0, 0, 0);
                    SetDateClass(Schedule.CurrDate, Schedule.CurrClass);
                    break;
            }
        }
        #endregion

        #region 各个public函数
        public void SetClassroom(int cId)
        {
            SetStatus(status.Table);
            SetCId(cId);
            checkoutWeek();
        }
        public void RefreshSchedule()
        {

            sch1.Reset();
            sch2.Reset();

            checkoutWeek();
        }
        public void GotoDateClass(DateTime date, int cc)
        {
            SetStatus(status.Table);

            SetDateClass(date, cc);
            checkoutWeek();
        }
        public void GoToDateClass(int weekDay, int cc)
        {
            SetStatus(status.Table);

            SetDateClass(weekDay, cc);
            checkoutWeek();
        }
        public void GridFocus()
        {
            TextBoxCId_Copy.Focus();
        }
        public Person Peron { get { return person; } }
        public RentTable personRentTable { get { return person.RentTable; } }
        #endregion

        #region 窗口必备功能

        //窗口拖动
        private void Border_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
           this.DragMove();
        }
        
        //最大化
        private void MaxBorder_MouseEnter(object sender, MouseEventArgs e)
        {
            MaxBorder.Background = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255));
        }
        private void MaxBorder_MouseLeave(object sender, MouseEventArgs e)
        {
            MaxLabel.Margin = new Thickness(0, 0, 5, 0);
            MaxBorder.Background = null;
        }
        private void MaxBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MaxLabel.Margin = new Thickness(0, 0, 3, 0);
        }
        private void MaxBorder_MouseUp(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Maximized;
            
            MaxLabel.Margin = new Thickness(0, 0, 5, 0);
            

        }
        //还原
        private void NormalBorder_MouseEnter(object sender, MouseEventArgs e)
        {
            NormalBorder.Background = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255));
        }
        private void NormalBorder_MouseLeave(object sender, MouseEventArgs e)
        {
            canvas_normalborder.Margin = new Thickness(0);
            NormalBorder.Background = null;
        }
        private void NormalBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            canvas_normalborder.Margin = new Thickness(2, 0, 0, 0);
        }
        private void NormalBorder_MouseUp(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Normal;

            MaxLabel.Margin = new Thickness(0,0,5,0);
            
        }
        //最小化
        private void MinBorder_MouseEnter(object sender, MouseEventArgs e)
        {
            MinBorder.Background = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255));
        }
        private void MinBorder_MouseLeave(object sender, MouseEventArgs e)
        {
            MinLabel.Margin = new Thickness(0, 0, 3, 0);
            MinBorder.Background = null;
        }
        private void MinBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MinLabel.Margin = new Thickness(0, 0, 1, 0);
        }
        private void MinBorder_MouseUp(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        //关闭
        private void CloseBorder_MouseEnter_1(object sender, MouseEventArgs e)
        {
            CloseBorder.Background = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255));
        }
        private void CloseBorder_MouseLeave(object sender, MouseEventArgs e)
        {
            CloseLabel.Margin = new Thickness(0, 0, -3, 0);
            CloseBorder.Background = null;
        }
        private void CloseBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CloseLabel.Margin = new Thickness(0, 0, -5, 0);
        }
        private void CloseBorder_MouseUp(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        //换肤
        private void SkinBorder_MouseEnter(object sender, MouseEventArgs e)
        {
            SkinBorder.Background = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255));
        }
        private void SkinBorder_MouseLeave(object sender, MouseEventArgs e)
        {
            SkinBorder.Background = null;
            Canvas.SetLeft(imageSkin, 5);
        }
        private void SkinBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Canvas.SetLeft(imageSkin, 7);
        }
        private void SkinBorder_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Canvas.SetLeft(imageSkin, 5);

            switch (currSkin)
            {
                case skin.Starry: SetSkin(skin.ColorBox); break;
                case skin.ColorBox: SetSkin(skin.Starry); break;
            }
        }


        //帮助
        private void HelpBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            HelpLabel.Margin = new Thickness(8, 0, 0, 0);
        }
        private void HelpBorder_MouseEnter(object sender, MouseEventArgs e)
        {
            HelpBorder.Background = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255));
        }
        private void HelpBorder_MouseLeave(object sender, MouseEventArgs e)
        {
            HelpLabel.Margin = new Thickness(6, 0, 0, 0);
            HelpBorder.Background = null;
        }
        private void HelpBorder_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show(
@"
滚轮：
    切换周数

键盘操作提示：
键盘的焦点可能在课程表或教师号输入框中。使用Tab键切换焦点。

通用：
    F1,F2：切换皮肤
    1,2,3,4：分别是四个导航按钮

焦点在课程表：
    方向键：调整选定的格子
    回车： 打开左侧课程表选定的课程
    Home,End：第一节课/最后一节课
    PageUp,PageDown：切换周数
    
焦点在课程表：
    上下键：切换教室
    PageUp,PageDown：切换教学楼

另外，在课程信息页面：
    回车： 关闭窗口
    Q：选课或退课
    C：进入教室

");
        }

        //最大化和最小化控制
        WindowState lastWindowState = WindowState.Normal;
        private void Window_SizeChanged_1(object sender, SizeChangedEventArgs e)
        {
            if (this.WindowState != lastWindowState)
            {
                RefreshSchedule();
            }

            switch (this.WindowState)
            {
                case WindowState.Maximized:
                    MaxBorder.Visibility = Visibility.Hidden;
                    MaxBorder.IsEnabled = false;
                    NormalBorder.Visibility = Visibility.Visible;
                    NormalBorder.IsEnabled = true;
                    break;
                case WindowState.Normal:
                    MaxBorder.Visibility = Visibility.Visible;
                    MaxBorder.IsEnabled = true;
                    NormalBorder.Visibility = Visibility.Hidden;
                    NormalBorder.IsEnabled = false;
                    break;
            }

            lastWindowState = this.WindowState;
        }
        

        #endregion

        #region 上侧导航按钮
        private void BorderButton_MouseEnter(object sender, MouseEventArgs e)
        {
            Border b = sender as Border;
            b.BorderThickness = new Thickness(3);
        }
        private void BorderButton_MouseLeave(object sender, MouseEventArgs e)
        {
            Border b = sender as Border;
            b.BorderThickness = new Thickness(0);
        }

        private void SetStatus(status S)
        {
            currStatus = S;
            GridInfo.Visibility = currStatus == status.Info ? Visibility.Visible : Visibility.Collapsed;
            GridTable.Visibility = currStatus == status.Table ? Visibility.Visible : Visibility.Collapsed;
            GridMessage.Visibility = currStatus == status.Message ? Visibility.Visible : Visibility.Collapsed;
        }

        private void BorderInfo_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SetStatus(status.Info);
        }

        private void BorderTable_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SetStatus(status.Table);
        }

        private void BorderMessage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SetStatus(status.Message);
        }


        

        #endregion

    }
}
