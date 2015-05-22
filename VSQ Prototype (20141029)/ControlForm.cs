using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.IO;

namespace Prototype
{
    public partial class ControlForm : Form
    {
        public static int testvariable = 0;

        // 실험에 사용되는 Text 집합
        public static ArrayList m_alTargetTextSet = null;

        public ControlForm()
        {
            InitializeComponent();
        }

        private void buttonDoExperiment_Click(object sender, EventArgs e)
        {
            // 넘길 변수 초기화
            string strUserName = "";
            int iButtonSize = 0;
            int iCDGain = 0;

            // Form에 선택된 값에 따라 설정
            // User name 설정
            if (textBoxUserName.Text == "")
            {
                MessageBox.Show("사용자 이름을 입력하세요.");
                textBoxUserName.Focus();
                return;
            }
            else
                strUserName = textBoxUserName.Text;

            // 만일 이름이 없다면 다시 입력 필요
            

            // Button size 설정
            if (radioButtonButtonSize22.Checked == true)
            {
                iButtonSize = 22;
            }
            else if (radioButtonButtonSize33.Checked == true)
            {
                iButtonSize = 33;
            }
            else if (radioButtonButtonSize44.Checked == true)
            {
                iButtonSize = 44;
            }
            else if (radioButtonButtonSize55.Checked == true)
            {
                iButtonSize = 55;
            }
            else if (radioButtonButtonSize66.Checked == true)
            {
                iButtonSize = 66;
            }
            else if (radioButtonButtonSize77.Checked == true)
            {
                iButtonSize = 77;
            }

            // CD gain 설정
            if (radioButtonCDGain1x.Checked == true)
            {
                iCDGain = 1;
            }
            else if (radioButtonCDGain2x.Checked == true)
            {
                iCDGain = 2;
            }
            else if (radioButtonCDGain3x.Checked == true)
            {
                iCDGain = 3;
            }
            else if (radioButtonCDGain4x.Checked == true)
            {
                iCDGain = 4;
            }                                 

            
            // ExperimentForm의 전역변수에 저장
            //ExperimentForm frm = new ExperimentForm();
            ExperimentForm frm = new ExperimentForm(strUserName, iButtonSize, iCDGain);            
            frm.ShowDialog();            
        }

        private void ControlForm_Load(object sender, EventArgs e)
        {
            //Console.WriteLine("ControlForm_Load");
            
            // Button Size 기본값 설정
            radioButtonButtonSize33.Checked = true;

            // CD Gain 기본값 설정
            radioButtonCDGain1x.Checked = true;
            
            // 실험을 위한 택스트 집합 초기화
            InitializeTargetTextSet();
        }

        private void buttonExitExperiment_Click(object sender, EventArgs e)
        {
            this.Close();
            this.Dispose();
            Application.Exit();
        }

        // 실험 글자 초기화
        // 실행 경로의 phrases.txt 파일 읽어서 m_alTargetTextSet에 순차적으로 저장
        private void InitializeTargetTextSet()
        {
            // Array 초기화
            ControlForm.m_alTargetTextSet = new ArrayList();

            // phrases.text 불러오기
            StreamReader sr = new StreamReader(@"phrases.txt", Encoding.Default);
            while (sr.Peek() >= 0)
            {
                ControlForm.m_alTargetTextSet.Add(sr.ReadLine());
            }
            sr.Close();
        }
    }
}
