using DBTOOL.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BigFileSplit
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //string lujing = "F:\\vs2010\\Excel\\测试.xls";
            string Openlujing = OpenFileDialog(openFileDialog);

            //判断路径是否为空
            if (Openlujing == null || Openlujing.Equals(null))
            {
                MessageBox.Show("没有选择文件，无法拆分");
            }
            else
            {
                outputLog("获取到的文件路径为" + Openlujing);
                int KBlen = 0;
                string lentxtBoxStr = lentxtBox.Text;
                int len = Int32.Parse(lentxtBoxStr);

                KBlen = 1024 * len;

                fileSplit(Openlujing, KBlen, false, 0);
            }
         }


        /// <summary>
        /// 单个文件分割函数,
        /// 可将任意文件fileIn分割为若干个子文件， 单个子文件最大为 len KB
        /// delet标识文件分割完成后是否删除原文件, change为加密密匙
        /// fileIn = "D:\\file.rar", 子文件名形如"D:\\file.rar@_1.split"
        /// </summary>
        public void fileSplit(String fileIn, int KBlen, bool delet, int change)
        {
            //输入文件校验
            if (fileIn == null || !System.IO.File.Exists(fileIn))
            {
                MessageBox.Show("文件" + fileIn + "不存在！");
                return;
            }

            //加密初始化
            short sign = 1;
            int num = 0, tmp;
            if (change < 0) { sign = -1; change = -change; }

            //取文件名和后缀, fileIn = "D:\\1.rar"

            //从文件创建输入流
            FileStream FileIn = new FileStream(fileIn, FileMode.Open);
            byte[] data = new byte[1024];   //流读取,缓存空间
            int len = 0, I = 1;             //记录子文件累积读取的KB大小, 分割的子文件序号

            FileStream FileOut = null;      //输出流
            int readLen = 0;                //每次实际读取的字节大小
            while (readLen > 0 || (readLen = FileIn.Read(data, 0, data.Length)) > 0) //读取数据
            {
                //创建分割后的子文件，已有则覆盖，子文件"D:\\1.rar@_1.split"

                
                if (len == 0)
                {
                    String splitFileName = fileIn + "@_" + I++ + ".split";
                    outputLog("即将拆分文件" + splitFileName);
                    FileOut = new FileStream(splitFileName, FileMode.Create);
                }

                //加密逻辑,对data的首字节进行逻辑偏移加密
                if (num == 0) num = change;
                tmp = data[0] + sign * (num % 3 + 3);
                if (tmp > 255) tmp -= 255;
                else if (tmp < 0) tmp += 255;
                data[0] = (byte)tmp;
                num /= 3;

                //输出，缓存数据写入子文件
                FileOut.Write(data, 0, readLen);
                FileOut.Flush();

                //预读下一轮缓存数据
                readLen = FileIn.Read(data, 0, data.Length);
                if (++len >= KBlen || readLen == 0)     //子文件达到指定大小，或文件已读完
                {
                    FileOut.Close();                    //关闭当前输出流
                    len = 0;
                }
            }

            FileIn.Close();                             //关闭输入流
            if (delet) System.IO.File.Delete(fileIn);   //删除源文件
        }





        /// <summary>
        /// 根据用户选择的文件获取文件的路径。
        /// 如果获取文件路径成功则返回路径，如果失败则返回mull
        /// /// <param name="openFileDialog">OpenFileDialog</param>
        /// </summary>
        /// <returns>如果获取文件路径成功则返回路径，如果失败则返回mull</returns>
        private static String OpenFileDialog(OpenFileDialog openFileDialog)
        {
            //展示提示语言
            openFileDialog.Title = "仅支持.txt 或者.log文件";
            //规定用户可以选择文件的类型，用后缀名控制，只支持【.xlsx】和【.xls】
            openFileDialog.Filter = "Excel文件(*.txt;*.log)|*.txt;*.log";
            //控制用户是否可以选择多个文件，ture为可以选择多个，false为不可以选择多个；
            openFileDialog.Multiselect = false;

            openFileDialog.ValidateNames = true;     //文件有效性验证ValidateNames，验证用户输入是否是一个有效的Windows文件名
            openFileDialog.CheckFileExists = true;  //验证路径有效性
            openFileDialog.CheckPathExists = true; //验证文件有效性


            //判断用户是否点击了Ok按钮
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                //获取文件的的名字
                String FileName = openFileDialog.FileNames[0];
                //判断文件名是否为空
                if (FileName == "" || FileName.Equals(null))
                {
                    return null;

                }
                else
                {
                    return FileName;
                }
            }
            else
            {
                return null;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            lentxtBox.Text = "20";
            tixingLab.Text = "拆分后的数据将放在文件的同级目录，格式为  原文件名@_序号.split";
            tixingLab.ForeColor = Color.Red;
        }




        //日志记录
        public void outputLog(String msg)
        {
            outputLog(msg, Color.Black, false);
        }


        //日志记录
        public void outputLog(String msg, Color color, Boolean isBold)
        {
            //让文本框获取焦点，不过注释这行也能达到效果
            this.richTextBoxLog.Focus();
            //设置光标的位置到文本尾   
            this.richTextBoxLog.Select(this.richTextBoxLog.TextLength, 0);
            //滚动到控件光标处   
            this.richTextBoxLog.ScrollToCaret();
            //设置字体颜色
            this.richTextBoxLog.SelectionColor = color;
            if (isBold)
            {
                this.richTextBoxLog.SelectionFont = new Font(Font, FontStyle.Bold);
            }



            System.DateTime currentTime = new System.DateTime();
            currentTime = System.DateTime.Now; //获取当前时间
            msg = currentTime.ToString("yyyy-MM-dd HH:mm:ss") + "  " + msg;
            this.richTextBoxLog.AppendText(msg);//输出到界面
            this.richTextBoxLog.AppendText(Environment.NewLine);

            //LogHelper.SetLog(msg);//写入日志文件

        }
    }
}
