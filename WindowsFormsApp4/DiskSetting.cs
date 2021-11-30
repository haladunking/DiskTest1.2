using DiskTestLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DiskTest11
{
    public delegate void WriteBlockCompleteHandler(double speed);
    public delegate void ReadBlockComleteHandler(double speed);
    public delegate void NotifyEventHandler(int i,double speed,double written_MB,string now_time);//进度条
    public delegate void SwitchEventHandler(int i);//切换页面
    public delegate void LogEventHandler(string s);//日志
    public delegate void StartTimeEventHandler(string s);//日志
    public partial class DiskSetting : Sunny.UI.UIPage
    {
        private const int DEAFAUT_BLOCKSIZE = 512;

        /// <summary>
        /// 委托事件的定义
        /// </summary>
        public NotifyEventHandler NotifyEvent;//进度条,速度等信息
        public SwitchEventHandler SwitchEvent;//切换界面
        public LogEventHandler LogEvent;//日志打印
        public StartTimeEventHandler StartTimeEvent;//传递开始时间

        public Disk[] Ed = new Disk[3]; //创建用户控件，显示硬盘的控件

        ArrayList Disk_Informaion_List = new ArrayList();
        ArrayList Disk_Driver_List = new ArrayList();
        ArrayList Disk_Choose_Information = new ArrayList();
        public WriteBlockCompleteHandler GetWriteSpeed;
        public ReadBlockComleteHandler GetReadSpeed;
        /// <summary>
        /// 测试数组
        /// </summary>
        private byte[] TestArray;
        private byte[] CompareArray;

        public ArrayList Disk_Information_List = new ArrayList();

        void Get_Disk_Information_Event(ArrayList value)
        {
            Disk_Information_List = value;
        }
        public DiskSetting()
        {
            InitializeComponent();
            Init_Disk_Information();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;            
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(134)));
            dataGridView1.Rows.Add();
            dataGridView1.Rows.Add();
            Ed[0] = new Disk();
            Ed[1] = new Disk();
            Ed[2] = new Disk();
            Ed[0].TransfEvent += Get_Disk_Information_Event;
            Ed[0].Show();
            panel1.Controls.Add(Ed[0]);
        }
        //增添Notify观察者对象
        public void AddNotifyObserver(NotifyEventHandler observer)
        {
            NotifyEvent += observer;
        }
        public void RemoteNotifyObserver(NotifyEventHandler observer)
        {
            NotifyEvent -= observer;
        }
        //增添Switch观察者对象
        public void AddSwitchObserver(SwitchEventHandler observer)
        {
            SwitchEvent += observer;
        }
        public void RemoteSwitchObserver(SwitchEventHandler observer)
        {
            SwitchEvent -= observer;
        }

        public void AddLogObserver(LogEventHandler observer)
        {
            LogEvent += observer;
        }
        public void RemoteLogObserver(LogEventHandler observer)
        {
            LogEvent -= observer;
        }

        public void AddStartTimeObserver(StartTimeEventHandler observer)
        {
            StartTimeEvent += observer;
        }
        public void RemoteStartTimeObserver(StartTimeEventHandler observer)
        {
            StartTimeEvent -= observer;
        }

        /// <summary>
        /// 广播速度，已读写量等信息，事件的具体实现，将这个组件的信息传给所有的观察者，让观察者执行相应的函数
        /// </summary>
        /// <param name="i"></param>
        /// <param name="speed"></param>
        /// <param name="wirtten_MB"></param>
        /// <param name="now_time"></param>
        public void PublishNotify(int i,double speed,double wirtten_MB,string now_time)
        {
            if (NotifyEvent != null)
            {
                NotifyEvent(i,speed,wirtten_MB,now_time);
            }
        }
        public void SwitchPage(int i)
        {
            if (SwitchEvent != null)
            {
                SwitchEvent(i);
            }
        }
        public void PrintLog(string s)
        {
            if(LogEvent!=null)
            {
                LogEvent(s);
            }
        }
        public void PublishStartTime(string s)
        {
            if(StartTimeEvent!=null)
            {
                StartTimeEvent(s);
            }
        }

        private void Start_Test(Object obj)
        {
            if (Disk_Choose_Information.Count <= 0)
            {
                MessageBox.Show("测试信息数组为空");
            }
            for (int i = 0; i < Disk_Choose_Information.Count; i++)
            {
                ChooseInformation chooseInformation = (ChooseInformation)Disk_Choose_Information[i];
                DriverLoader driverLoader = (DriverLoader)Disk_Driver_List[i];
                if (chooseInformation.TestOrNot == true)
                {
                    if (chooseInformation.TestMode == 0)//随机读写验证
                    {
                        RandomWriteAndVerify(i, chooseInformation.TestNum, chooseInformation.TestTime);
                    }
                    else if (chooseInformation.TestMode == 1)//随机只读
                    {
                        RandomOnlyRead(i, chooseInformation.TestNum, chooseInformation.TestTime);
                    }
                    else if (chooseInformation.TestMode == 2)//随机只写
                    {
                        RandomOnlyWrite(i, chooseInformation.TestNum, chooseInformation.TestTime);
                    }
                    else if (chooseInformation.TestMode == 3)//顺序读写验证
                    {
                        OrderWriteAndVerify(i, chooseInformation.TestPercent, chooseInformation.TestDataMode, chooseInformation.BlockSize, chooseInformation.TestCircle);
                    }
                    else if (chooseInformation.TestMode == 4)//顺序只读
                    {
                        OrderOnlyRead(i, chooseInformation.TestPercent, chooseInformation.TestDataMode, chooseInformation.BlockSize, chooseInformation.TestCircle);
                    }
                    else if (chooseInformation.TestMode == 5)//顺序只写
                    {
                        OrderOnlyWrite(i, chooseInformation.TestPercent, chooseInformation.TestDataMode, chooseInformation.BlockSize, chooseInformation.TestCircle);
                    }
                    else
                    {
                        MessageBox.Show("测试模式错误！");
                    }
                }
                else
                {
                    MessageBox.Show("该磁盘无法进行测试，请检查选项");
                }
            }
        }
        public void Init_Disk_Driver()
        {
            if (Disk_Informaion_List.Count <= 0)
            {
                MessageBox.Show("未检测到设备！");
                return;
            }
            for (int i = 0; i < Disk_Informaion_List.Count; i++)
            {
                DiskInformation information = (DiskInformation)Disk_Informaion_List[i];
                DriverLoader driver = new DriverLoader(information);
                Disk_Driver_List.Add(driver);
            }
        }
        private void Init_Test_Parameters()
        {
            for (int i = 0; i < Disk_Driver_List.Count; i++)
            {
                if ((bool)Disk_Information_List[0])
                {
                    if ((int)this.Disk_Information_List[1] == 0 || (int)this.Disk_Information_List[1] == 1 || (int)this.Disk_Information_List[1] == 2)
                    {

                        bool testornot = (bool)this.Disk_Information_List[0];
                        int testmode = (int)this.Disk_Information_List[1];
                        long testtime = (long)this.Disk_Information_List[5];
                        long testnum = (long)this.Disk_Information_List[7];
                        ChooseInformation choose = new ChooseInformation();
                        choose.SetRandomParameters(testornot, testmode, testtime, testnum);
                        Disk_Choose_Information.Add(choose);
                    }
                    else
                    {
                        bool testornot = (bool)this.Disk_Information_List[0];
                        int testmode = (int)this.Disk_Information_List[1];
                        int testdatamode = (int)this.Disk_Information_List[2];
                        long testtime = (long)this.Disk_Information_List[5];
                        int testcircle = (int)this.Disk_Information_List[6];
                        long testnum = (long)this.Disk_Information_List[7];
                        int testpercent = (int)this.Disk_Information_List[3];
                        int blocksize = (int)this.Disk_Information_List[4];
                        ChooseInformation choose = new ChooseInformation();
                        choose.SetOrderParameters(testornot, testmode, testdatamode, testpercent, blocksize, testtime, testnum, testcircle);
                        Disk_Choose_Information.Add(choose);
                    }
                }
            }
        }
        public void OutWriteSpeed(double speed)
        {
            Console.WriteLine(speed);
        }
        public void ReturnPercent(int percent)
        {

        }
        public void addColumn(string name, decimal size, long sectorsize)
        {
            int index = this.dataGridView1.Rows.Add();
            dataGridView1.Rows[index].Cells[0].Value = name;
            dataGridView1.Rows[index].Cells[1].Value = size;
            dataGridView1.Rows[index].Cells[2].Value = "";
            dataGridView1.Rows[index].Cells[3].Value = sectorsize;
            dataGridView1.Rows[index].Cells[4].Value = "512B";

        }
        public void Init_Disk_Information()
        {
            ManagementClass Diskobject = new ManagementClass("Win32_DiskDrive");//获取一个磁盘实例对象
            ManagementObjectCollection moc = Diskobject.GetInstances();//获取对象信息的集合            
            int id = 0;
            int i = 1;
            foreach (ManagementObject mo in moc)
            {
                if (mo.Properties["InterfaceType"].Value.ToString() == "USB")
                {
                    try
                    {
                        //产品名称
                        string name = mo.Properties["Name"].Value.ToString();
                        string sector_size_s = mo.Properties["TotalSectors"].Value.ToString();
                        long sector_size = Convert.ToInt64(sector_size_s);

                        string size_s = mo.Properties["Size"].Value.ToString();
                        double size_d = Convert.ToDouble(size_s) / (1024 * 1024 * 1024);
                        decimal size = decimal.Round(decimal.Parse("" + size_d), 2);
                        //long size = Convert.ToInt64(size_s);
                        DiskInformation d = new DiskInformation(name, sector_size, size, id++);
                        this.addColumn(name, size, sector_size);
                        Disk_Informaion_List.Add(d);

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
            if (id == 0)
            {
                MessageBox.Show("未检测到设备！");
            }
        }
        public void OrderWriteAndVerify(int driver_index, int percent = 100, int test_data_mode = 0, int block_size = 1, int circle = 1)
        {
            if (Disk_Driver_List.Count <= 0)
            {
                MessageBox.Show("未检测到设备！");
                return;
            }
            DriverLoader driver = (DriverLoader)Disk_Driver_List[driver_index];
            TestArray = new byte[DEAFAUT_BLOCKSIZE * block_size];
            CompareArray = new byte[DEAFAUT_BLOCKSIZE * block_size];
            //long actual_size = ((driver.DiskInformation.DiskSectorSize / block_size)*percent)/100;
            long actual_size = 10;
            if (test_data_mode == 0 || test_data_mode == 1)
            {
                int error_num = 0;
                Init_TestArray(block_size, test_data_mode);
                for (long i = 0; i < actual_size; i++)
                {
                    driver.WritSector(TestArray, i, block_size);
                    CompareArray = driver.ReadSector(i, block_size);
                    error_num += VerifyArray(TestArray, CompareArray);
                    Console.WriteLine("向" + i + "扇区写入了数据");
                }

                if (error_num == 0)
                    Console.WriteLine("顺序读写测试完成，测试了" + actual_size + "次未发生错误！");
            }
            else if (test_data_mode == 2)
            {
                int error_num = 0;
                for (long i = 0; i < actual_size; i++)
                {
                    Init_TestArray(block_size, test_data_mode);
                    driver.WritSector(TestArray, i, block_size);
                    CompareArray = driver.ReadSector(i, block_size);
                    error_num += VerifyArray(TestArray, CompareArray);
                }
                if (error_num == 0)
                    Console.WriteLine("顺序读写测试完成，测试了" + actual_size + "次未发生错误！");
            }
            else
            {
                Console.WriteLine("测试模式不存在，请重新选择!");
            }
        }
        public void OrderOnlyWrite(int driver_index, int percent = 100, int test_data_mode = 0, int block_size = 1, int circle = 1)
        {
            if (Disk_Driver_List.Count <= 0)
            {
                MessageBox.Show("未检测到设备！");
                return;
            }
            block_size = 2 * 1024 * 50;
            DriverLoader driver = (DriverLoader)Disk_Driver_List[driver_index];
            TestArray = new byte[DEAFAUT_BLOCKSIZE * block_size];
            CompareArray = new byte[DEAFAUT_BLOCKSIZE * block_size];
            //long actual_size = ((driver.DiskInformation.DiskSectorSize / block_size)*percent)/100;
            long actual_size = driver.DiskInformation.DiskSectorSize * percent / 100;
            long start_time = Environment.TickCount;
            long speed_start = start_time;//测试读写速度
            long speed_end;
            long _MB_num = 0;
            
            if (test_data_mode == 0 || test_data_mode == 1)
            {
                Init_TestArray(block_size, test_data_mode);
                for (long i = 0; i < actual_size;)
                {
                    //由于最后一个块比较大，最后一个块判断不能执行之后，将会有大量的块无法执行。

                    driver.WritSector(TestArray, i, block_size);
                    speed_end = Environment.TickCount;
                    _MB_num += DEAFAUT_BLOCKSIZE * block_size;
                    if (_MB_num % (1048576 * 50) == 0)//
                    {
                        double now_speed = ((double)(1000 * 50) / (double)(speed_end - speed_start));
                        double now_MB = _MB_num / 1048576;
                        string now_time = DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss");
                        GetWriteSpeed?.Invoke(now_speed);
                        speed_start = speed_end;
                        this.PublishNotify((int)(i*100/actual_size),now_speed,now_MB,now_time);
                        //Console.WriteLine("顺序只写测试完成，测试了" + actual_size + "次未发生错误！");
                    }
                    i += block_size;
                    //这个i就是当前测得容量
                }
                this.PublishNotify(100,0,0, DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss"));
                Console.WriteLine("顺序只写测试完成，测试了" + actual_size + "次未发生错误！");
                this.PrintLog("顺序只写测试完成，测试了" +actual_size + "次未发生错误！");
            }
            else if (test_data_mode == 2)
            {
                int error_num = 0;
                for (long i = 0; i < actual_size; i++)
                {
                    Init_TestArray(block_size, test_data_mode);
                    driver.WritSector(TestArray, i, block_size);
                }
                Console.WriteLine("顺序只写测试完成，测试了" + actual_size + "次未发生错误！");
            }
            else
            {
                Console.WriteLine("测试模式不存在，请重新选择!");
            }
            //driver.Close();
            TestArray = null;
            CompareArray = null;
        }
        public void OrderOnlyRead(int driver_index, int percent = 100, int test_data_mode = 0, int block_size = 1, int circle = 1)
        {
            if (Disk_Driver_List.Count <= 0)
            {
                MessageBox.Show("未检测到设备！");
                return;
            }
            DriverLoader driver = (DriverLoader)Disk_Driver_List[driver_index];
            CompareArray = new byte[DEAFAUT_BLOCKSIZE * block_size];
            //long actual_size = ((driver.DiskInformation.DiskSectorSize / block_size)*percent)/100;
            long actual_size = 10;
            for (long i = 0; i < actual_size; i++)
            {
                CompareArray = driver.ReadSector(i, block_size);
            }
            Console.WriteLine("顺序只读测试完成，测试了" + actual_size + "次未发生错误！");

        }
        public void RandomWriteAndVerify(int driver_index, long test_num = 0, long test_time = 0, int test_mode = 2)
        {
            if (Disk_Driver_List.Count <= 0)
            {
                MessageBox.Show("未检测到设备！");
                return;
            }
            Random R = new Random();
            DriverLoader driver = (DriverLoader)Disk_Driver_List[driver_index];
            long _MB_num = 0;
            if (test_num == 0)
            {
                long start_time = Environment.TickCount;
                long speed_start = start_time;//测试读写速度
                long speed_end;
                long error_num = 0;
                while (true)
                {
                    int temp_block = R.Next(1, 5);
                    int actual_block_size = DEAFAUT_BLOCKSIZE * temp_block;
                    TestArray = new byte[actual_block_size];
                    CompareArray = new byte[actual_block_size];
                    Init_TestArray(temp_block, test_mode);
                    long pos = NextLong(0, driver.DiskInformation.DiskSectorSize - temp_block);
                    //Console.WriteLine("写入" + pos + "扇区");

                    driver.WritSector(TestArray, pos, temp_block);
                    speed_end = Environment.TickCount;//测试读写速度

                    CompareArray = driver.ReadSector(pos, temp_block);
                    error_num += VerifyArray(TestArray, CompareArray);
                    long end_time = Environment.TickCount;
                    _MB_num += actual_block_size;
                    if (_MB_num % 1048576 == 0)//100KB/ms
                    {
                        GetWriteSpeed?.Invoke(((double)1000 / (double)(speed_end - speed_start)));
                        speed_start = speed_end;
                    }
                    if (end_time - start_time >= test_time)
                        break;
                }
                if (error_num == 0)
                    Console.WriteLine("随机读写验证测试完成，测试了" + test_time + "毫秒未发生错误！");
            }
            else if (test_time == 0)
            {
                long temp_num = 0;
                int error_num = 0;
                long start_time = Environment.TickCount;
                long speed_start = start_time;//测试读写速度
                long speed_end;//算读写速度的
                while (true)
                {
                    if (temp_num >= test_num)
                        break;
                    int temp_block = R.Next(1, 5);
                    int actual_block_size = DEAFAUT_BLOCKSIZE * temp_block;
                    TestArray = new byte[actual_block_size];
                    CompareArray = new byte[actual_block_size];
                    Init_TestArray(temp_block, test_mode);
                    long pos = NextLong(0, driver.DiskInformation.DiskSectorSize - temp_block);
                    Console.WriteLine("写入" + pos + "扇区");
                    driver.WritSector(TestArray, pos, temp_block);
                    speed_end = Environment.TickCount;//测试读写速度
                    CompareArray = driver.ReadSector(pos, temp_block);
                    error_num += VerifyArray(TestArray, CompareArray);
                    temp_num++;
                    _MB_num += actual_block_size;
                    if (_MB_num % 1048576 == 0)//100KB/ms
                    {
                        GetWriteSpeed?.Invoke(((double)1000 / (double)(speed_end - speed_start)));
                        speed_start = speed_end;
                    }
                }
                if (error_num == 0)
                    Console.WriteLine("随机读写验证测试完成，测试了" + test_num + "次未发生错误！");
            }
        }
        public void RandomOnlyRead(int driver_index, long test_num = 0, long test_time = 0)
        {
            if (Disk_Driver_List.Count <= 0)
            {
                MessageBox.Show("未检测到设备！");
                return;
            }
            Random R = new Random();
            DriverLoader driver = (DriverLoader)Disk_Driver_List[driver_index];
            if (test_num == 0)
            {
                long start_time = Environment.TickCount;
                while (true)
                {
                    int temp_block = R.Next(1, 5);
                    int actual_block_size = DEAFAUT_BLOCKSIZE * temp_block;
                    CompareArray = new byte[actual_block_size];
                    long pos = NextLong(0, driver.DiskInformation.DiskSectorSize - temp_block);
                    CompareArray = driver.ReadSector(pos, temp_block);
                    long end_time = Environment.TickCount;
                    if (end_time - start_time >= test_time)
                        break;
                }
                Console.WriteLine("随机只读测试完成，测试了" + test_time + "毫秒未发生错误！");
            }
            else if (test_time == 0)
            {
                long temp_num = 0;
                while (true)
                {
                    if (temp_num >= test_num)
                        break;
                    int temp_block = R.Next(1, 5);
                    int actual_block_size = DEAFAUT_BLOCKSIZE * temp_block;
                    CompareArray = new byte[actual_block_size];
                    long pos = NextLong(0, driver.DiskInformation.DiskSectorSize - temp_block);
                    CompareArray = driver.ReadSector(pos, temp_block);
                    temp_num++;
                }
                Console.WriteLine("随机只读测试完成，测试了" + test_num + "次未发生错误！");
            }
        }
        public void RandomOnlyWrite(int driver_index, long test_num = 0, long test_time = 0, int test_mode = 2)
        {
            if (Disk_Driver_List.Count <= 0)
            {
                MessageBox.Show("未检测到设备！");
                return;
            }
            Random R = new Random();
            DriverLoader driver = (DriverLoader)Disk_Driver_List[driver_index];
            if (test_num == 0)
            {
                long start_time = Environment.TickCount;
                while (true)
                {
                    int temp_block = R.Next(1, 5);
                    int actual_block_size = DEAFAUT_BLOCKSIZE * temp_block;
                    TestArray = new byte[actual_block_size];
                    Init_TestArray(temp_block, test_mode);
                    long pos = NextLong(0, driver.DiskInformation.DiskSectorSize - temp_block);
                    driver.WritSector(TestArray, pos, temp_block);
                    long end_time = Environment.TickCount;
                    if (end_time - start_time >= test_time)
                        break;
                }
                Console.WriteLine("随机只写测试完成，测试了" + test_time + "毫秒！");
            }
            else if (test_time == 0)
            {
                long temp_num = 0;
                while (true)
                {
                    if (temp_num >= test_num)
                        break;
                    int temp_block = R.Next(1, 5);
                    int actual_block_size = DEAFAUT_BLOCKSIZE * temp_block;
                    TestArray = new byte[actual_block_size];
                    Init_TestArray(temp_block, test_mode);
                    long pos = NextLong(0, driver.DiskInformation.DiskSectorSize - temp_block);
                    driver.WritSector(TestArray, pos, temp_block);
                    temp_num++;
                }
                Console.WriteLine("随机只写测试完成，测试了" + test_num + "次！");
            }
        }
        public void Init_TestArray(int block_size, int mode)
        {
            if (block_size == 0)
            {
                MessageBox.Show("块大小不能为0");
                return;
            }
            if (mode == 0)
            {
                for (int i = 0; i < block_size * DEAFAUT_BLOCKSIZE; i++)
                {
                    TestArray[i] = 0;
                }
            }
            else if (mode == 1)
            {
                for (int i = 0; i < block_size * DEAFAUT_BLOCKSIZE; i++)
                {
                    TestArray[i] = 255;
                }
            }
            else if (mode == 2)
            {
                Random R = new Random();
                for (int i = 0; i < block_size * DEAFAUT_BLOCKSIZE; i++)
                {

                    TestArray[i] = (byte)R.Next(0, 255);
                }
            }

        }
        public int VerifyArray(byte[] testarray, byte[] comparearray)
        {
            if (testarray.Length != comparearray.Length)
            {
                Console.WriteLine("数组长度不匹配！");
            }
            int error_num = 0;
            for (int i = 0; i < testarray.Length; i++)
            {
                if (testarray[i] != comparearray[i])
                {
                    Console.WriteLine("当前位置" + i + "出错，正确数据为" + testarray[i] + "错误数据为：" + comparearray[i]);
                    error_num++;
                }
            }
            return error_num;

        }
        public long NextLong(long A, long B)
        {
            Random R = new Random((int)DateTime.Now.Ticks);
            long myResult = A;
            //-----
            long Max = B, Min = A;
            if (A > B)
            {
                Max = A;
                Min = B;
            }
            double Key = R.NextDouble();
            myResult = Min + (long)((Max - Min) * Key);
            //-----
            return myResult;
        }
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            switch (e.RowIndex)
            {
                case 0:
                    Ed[0].Show();
                    Ed[0].TransfEvent += Get_Disk_Information_Event;
                    panel1.Controls.Clear();
                    panel1.Controls.Add(Ed[0]);
                    break;
                case 1:
                    Ed[1].Show();
                    Ed[1].TransfEvent += Get_Disk_Information_Event;
                    panel1.Controls.Clear();
                    panel1.Controls.Add(Ed[1]);
                    break;
                case 2:
                    Ed[2].Show();
                    Ed[1].TransfEvent += Get_Disk_Information_Event;
                    panel1.Controls.Clear();
                    panel1.Controls.Add(Ed[2]);
                    break;
            }
        }

        private void StartTest_Click(object sender, EventArgs e)
        {

            Init_Disk_Driver();
            Init_Test_Parameters();
            GetWriteSpeed = OutWriteSpeed;
            System.Threading.Thread thr = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(Start_Test));
            thr.Start();
            this.SwitchPage(201);
            this.PublishStartTime(DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss"));
        }
    }
}
