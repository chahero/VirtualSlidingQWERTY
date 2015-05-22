using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Runtime.InteropServices;
using System.Diagnostics;

using System.IO;


namespace Prototype
{
    public partial class ExperimentForm : Form
    {
        // 마우스가 Down되었는지에 대한 flag
        private bool m_bMouseDownFlag = false;
        
        // 마우스가 움직였는지에대한 flag
        private bool m_bMouseMoveFlag = false;
      
        // Button size
        // 2x2, 3x3, 4x4, 5x5, 6x6, 7x7이 있으며, 각각 22, 33, 44, 55, 66, 77로 표현
        private int m_iButtonSize = 22;
        private int m_iButtonSizePixel = 0; // pixel로 변환 된 규격

        // 버튼과 버튼 간의 Gap
        // 기본 값은 2pixe = 0.4mm (원래는 0.5mm를 하려고 하였으나, 2.5pixel 안되므로, 2pixel로 설정)
        private const int m_iGapSizeBetweenButtion = 2;

        // CD Gain
        // 1x, 2x, 3x, 4x가 있으며, 각각 1, 2, 3, 4로 표현
        private int m_iCDGain = 1;


        // TextBox를 Touch 하였을 때의 위치
        private int m_iTextBoxTouchDownX = 0;
        private int m_iTextBoxTouchUpX = 0;
        private const int m_iTextBoxTouchMax = 15;

        // TextBox를 Touch 하였을 때의 Caret 위치
        private int m_iCaretPosition = 0;

        // TextBox를 Touch하였을 때의 기능 처리를 위한 API
        [DllImport("User32.DLL", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        private static extern int SendMessage(IntPtr hWnd, uint Msg, int wParam, ref Point lParam);
        private const int EM_CHARFROMPOS = 0x00D7;
            
        // 버튼 이동을 위한 변수
        private Point m_startPoint;
        private Point m_endPoint;
     
        // 파일 저장을 위한 변수
        private FileStream m_fileStream;
        private StreamWriter m_streamWriter;
        private DateTime m_eventTime;

        private DateTime m_dtStartTrialTime;            // Trial이 시작되었을 때의 시간
        private DateTime m_dtEndTrialTime;              // Trial이 시작되었을 때의 시간
        private TimeSpan m_tsTotalTrialTime;            // Trial이 시작된 시간과 끝난 시간과의 차이
        private double m_dTotalCharacterPerMinute;      // 1분당 걸린 시간
        private int m_iTotalButtonCountExceptBackSpace; // BackSpace 제외 버튼 눌린 횟수    
        private int m_iTotalButtonBackSpaceCount;       // BackSpace 버튼 눌린 횟수
        private int m_iTotalGestureSpaceCount;          // Space 제스처 눌린 횟수
        private int m_iTotalGestureBackSpaceCount;      // BackSpace 제스처 눌린 횟수
        private int m_iTotalButtonMoveCount;            // 총 키보드 이동을 위한 횟수
        
        // 피실험자 이름
        private string m_strUserName;

        // 실험 횟수 번호
        private int m_iTrialNo = 0;

        // 한 실험 조건 당 Trial 횟수
        private const int m_iTrialNoMax = 5;

        // 대상 텍스트 집합에서 랜덤으로 추출하기 위한 번호
        private int iRandomTargetTextNo = 0;
                
 
        
        // Constructor
        public ExperimentForm()
        {
            InitializeComponent();
        }

        // Constructor
        public ExperimentForm(String strUsername, int iButtonSize, int iCDGain)
        {
            InitializeComponent();

            // 변수 설정
            m_strUserName = strUsername;
            m_iButtonSize = iButtonSize;
            m_iCDGain = iCDGain;

            // m_iButtonSize의 mm를 pixel단위에 맞도록 변환
            // 5pixel 간격이 약 1mm          
            if (m_iButtonSize == 22)
            {
                m_iButtonSizePixel = 13;
            }
            else if (m_iButtonSize == 33)
            {
                m_iButtonSizePixel = 18;
            }
            else if (m_iButtonSize == 44)
            {
                m_iButtonSizePixel = 23;
            }
            else if (m_iButtonSize == 55)
            {
                m_iButtonSizePixel = 28;
            }
            else if (m_iButtonSize == 66)
            {
                m_iButtonSizePixel = 33;
            }
            else if (m_iButtonSize == 77)
            {
                m_iButtonSizePixel = 38;
            }

            // 폴더 확인하고 폴더 생성
            string strFolderName = m_strUserName + "_" + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString();

            DirectoryInfo dir = new DirectoryInfo(strFolderName);

            // 동일 폴더가 없다면 폴더 생성
            if (dir.Exists == false)
            {
                dir.Create();
            }

            // 파일 저장을 위한 파일 생성
            string strFileName = strFolderName + "/" + m_iButtonSize.ToString() + "_" + m_iCDGain.ToString() + ".txt";

            m_fileStream = new FileStream(strFileName, FileMode.Create, FileAccess.Write);
            m_streamWriter = new StreamWriter(m_fileStream, System.Text.Encoding.Default);

            // 기록할 변수 초기화
            m_dTotalCharacterPerMinute = 0;                 // 1분당 걸린 시간
            m_iTotalButtonCountExceptBackSpace = 0;         // BackSpace 제외 버튼 눌린 횟수
            m_iTotalButtonBackSpaceCount = 0;               // BackSpace 버튼 눌린 횟수
            m_iTotalGestureSpaceCount = 0;                  // Space 제스처 눌린 횟수
            m_iTotalGestureBackSpaceCount = 0;              // BackSpace 제스처 눌린 횟수
            m_iTotalButtonMoveCount = 0;                    // 총 키보드 이동을 위한 횟수
        }             
        
        private void Form1_Load(object sender, EventArgs e)
        {
            // 실험 횟수 설정
            m_iTrialNo = 1;
            labelTrialNo.Text = "trial " + m_iTrialNo.ToString();

            // 실험에 다른 대상 문자 집합 업데이트
            // 랜덥화 하여 추출
            labelTargetText.Text = ReadRandomTargetTextSet();

            // 대상 텍스트 집합에서 삭제
            //Console.WriteLine("지울 번호는" + iRandomTargetTextNo);
            ControlForm.m_alTargetTextSet.RemoveAt(iRandomTargetTextNo);
            //Console.WriteLine("지워진 이후" + ControlForm.m_alTargetTextSet.Count);

           
            // 버튼 사이즈 초기화
            InitializeButtonSize();

            // 버튼을 포함하고 있는 패널 사이즈 초기화
            InitializeButtonGroupPanelSize();

            // 패널 사이즈에 맞도록 버튼 위치 초기화
            InitializeButtonLocation();

            // 버튼의 폰트 사이즈 초기화
            InitializeButtonFontSize();

            // 패널 위치 초기화
            InitializeButtonGroupPanelLocation();

            // 전송 버튼 초기화
            buttonDoExperiment.Enabled = true;
            buttonSend.Enabled = false;

            // 윈도우를 보이지 않도록 함
            pictureBoxWatchTop.Visible = false;
            pictureBoxWatchBottom.Visible = false;
            pictureBoxWatchLeft.Visible = false;
            pictureBoxWatchRight.Visible = false;
            pictureBoxWatchCenter.Visible = false;
            textBoxMessage.Visible = false;
            panelButtonGroup.Visible = false;

            // 화면 업데이트
            this.Refresh();
        }
        
        // 버튼 사이즈 초기화
        private void InitializeButtonSize()
        {
            // 버튼 크기 조절
            button1.Size = new Size(m_iButtonSizePixel, m_iButtonSizePixel);
            button2.Size = new Size(m_iButtonSizePixel, m_iButtonSizePixel);
            button3.Size = new Size(m_iButtonSizePixel, m_iButtonSizePixel);
            button4.Size = new Size(m_iButtonSizePixel, m_iButtonSizePixel);
            button5.Size = new Size(m_iButtonSizePixel, m_iButtonSizePixel);
            button6.Size = new Size(m_iButtonSizePixel, m_iButtonSizePixel);
            button7.Size = new Size(m_iButtonSizePixel, m_iButtonSizePixel);
            button8.Size = new Size(m_iButtonSizePixel, m_iButtonSizePixel);
            button9.Size = new Size(m_iButtonSizePixel, m_iButtonSizePixel);
            button0.Size = new Size(m_iButtonSizePixel, m_iButtonSizePixel);

            buttonQ.Size = new Size(m_iButtonSizePixel, m_iButtonSizePixel);
            buttonW.Size = new Size(m_iButtonSizePixel, m_iButtonSizePixel);
            buttonE.Size = new Size(m_iButtonSizePixel, m_iButtonSizePixel);
            buttonR.Size = new Size(m_iButtonSizePixel, m_iButtonSizePixel);
            buttonT.Size = new Size(m_iButtonSizePixel, m_iButtonSizePixel);
            buttonY.Size = new Size(m_iButtonSizePixel, m_iButtonSizePixel);
            buttonU.Size = new Size(m_iButtonSizePixel, m_iButtonSizePixel);
            buttonI.Size = new Size(m_iButtonSizePixel, m_iButtonSizePixel);
            buttonO.Size = new Size(m_iButtonSizePixel, m_iButtonSizePixel);
            buttonP.Size = new Size(m_iButtonSizePixel, m_iButtonSizePixel);

            buttonA.Size = new Size(m_iButtonSizePixel, m_iButtonSizePixel);
            buttonS.Size = new Size(m_iButtonSizePixel, m_iButtonSizePixel);
            buttonD.Size = new Size(m_iButtonSizePixel, m_iButtonSizePixel);
            buttonF.Size = new Size(m_iButtonSizePixel, m_iButtonSizePixel);
            buttonG.Size = new Size(m_iButtonSizePixel, m_iButtonSizePixel);
            buttonH.Size = new Size(m_iButtonSizePixel, m_iButtonSizePixel);
            buttonJ.Size = new Size(m_iButtonSizePixel, m_iButtonSizePixel);
            buttonK.Size = new Size(m_iButtonSizePixel, m_iButtonSizePixel);
            buttonL.Size = new Size(m_iButtonSizePixel, m_iButtonSizePixel);

            buttonZ.Size = new Size(m_iButtonSizePixel, m_iButtonSizePixel);
            buttonX.Size = new Size(m_iButtonSizePixel, m_iButtonSizePixel);
            buttonC.Size = new Size(m_iButtonSizePixel, m_iButtonSizePixel);
            buttonV.Size = new Size(m_iButtonSizePixel, m_iButtonSizePixel);
            buttonB.Size = new Size(m_iButtonSizePixel, m_iButtonSizePixel);
            buttonN.Size = new Size(m_iButtonSizePixel, m_iButtonSizePixel);
            buttonM.Size = new Size(m_iButtonSizePixel, m_iButtonSizePixel);

            buttonSpace.Size = new Size(m_iButtonSizePixel * 2 + m_iGapSizeBetweenButtion, m_iButtonSizePixel);

            // 26x28, 36x38, 46x48, 56x58, 66x68, 76x78
            buttonBackSpace.Size = new Size(m_iButtonSizePixel * 2 + m_iGapSizeBetweenButtion, m_iButtonSizePixel);
        }

        // 버튼을 포함하고 있는 그룹박스 사이즈 초기화
        private void InitializeButtonGroupPanelSize()
        {
            // 버튼 그룹박스 크기 계산
            // 가로로 10버튼과 간격이 최대 넓이
            // 세로로 5버튼과 간격이 최대 넓이

            int iGroupBoxWidth = m_iButtonSizePixel * 10 + m_iGapSizeBetweenButtion * 12;
            int iGroupBoxHeight = m_iButtonSizePixel * 5 + m_iGapSizeBetweenButtion * 10;

            panelButtonGroup.Size = new Size(iGroupBoxWidth, iGroupBoxHeight);
        }

        // 버튼 위치 초기화
        private void InitializeButtonLocation()
        {
            // GroupBox의 중심에 G버튼의 위치를 설정하고, 같은 줄의 버튼을 설정
            // T 버튼과 Y 버튼의 위치를 설정하고, 같은 줄의 버튼을 설정
            // 5 버튼과 6 버튼의 위치를 설정하고, 같은 줄의 버튼을 설정
            // V 버튼의 위치를 설정하고, 같은 줄의 버튼을 설정
            // Space 버튼의 위치를 설정
                // 버튼 사이즈의 절반 크기를 왼쪽/위쪽으로 삭제
                // 각 버튼간 간격을 띄우고 (2pixel), 버튼 사이즈에 맞추어 간격 배치

            // ButtonGroupPanel의 중심이 PictureBoxWaterCenter의 중심에 올 수 있도록 설정
            int iButtonGroupPanelCenterLocationX = panelButtonGroup.Size.Width / 2;
            int iButtonGroupPanelCenterLocationY = panelButtonGroup.Size.Height / 2;
                                    
            // 3번째 줄 설정
            /// G 버튼 설정
            int iButtonGLocationX = iButtonGroupPanelCenterLocationX - (m_iButtonSizePixel / 2);
            int iButtonGLocationY = iButtonGroupPanelCenterLocationY - (m_iButtonSizePixel / 2);  
            buttonG.Location = new Point(iButtonGLocationX, iButtonGLocationY); // QWERTY의 중심

            // G 버튼의 왼쪽 버튼 (FDSA) 설정
            buttonF.Location = new Point(iButtonGLocationX - m_iGapSizeBetweenButtion - m_iButtonSizePixel, iButtonGLocationY);
            buttonD.Location = new Point(iButtonGLocationX - m_iGapSizeBetweenButtion * 2 - m_iButtonSizePixel * 2, iButtonGLocationY);
            buttonS.Location = new Point(iButtonGLocationX - m_iGapSizeBetweenButtion * 3 - m_iButtonSizePixel * 3, iButtonGLocationY);
            buttonA.Location = new Point(iButtonGLocationX - m_iGapSizeBetweenButtion * 4 - m_iButtonSizePixel * 4, iButtonGLocationY);

            // G 버튼의 오른쪽 버튼 (HJKL) 설정            
            buttonH.Location = new Point(iButtonGLocationX + m_iButtonSizePixel + m_iGapSizeBetweenButtion, iButtonGLocationY);
            buttonJ.Location = new Point(iButtonGLocationX + m_iButtonSizePixel * 2 + m_iGapSizeBetweenButtion * 2, iButtonGLocationY);
            buttonK.Location = new Point(iButtonGLocationX + m_iButtonSizePixel * 3 + m_iGapSizeBetweenButtion * 3, iButtonGLocationY);
            buttonL.Location = new Point(iButtonGLocationX + m_iButtonSizePixel * 4 + m_iGapSizeBetweenButtion * 4, iButtonGLocationY);

            // 2번째 줄 설정
            // T 버튼 설정
            int iButtonTLocationX = iButtonGroupPanelCenterLocationX - (m_iButtonSizePixel / 2) * 2 - (m_iGapSizeBetweenButtion / 2);
            int iButtonTLocationY = iButtonGroupPanelCenterLocationY - (m_iButtonSizePixel / 2) - m_iButtonSizePixel - m_iGapSizeBetweenButtion;  
            buttonT.Location = new Point(iButtonTLocationX, iButtonTLocationY);

            // T 버튼의 왼쪽 버튼 (REWQ) 설정)
            buttonR.Location = new Point(iButtonTLocationX - m_iGapSizeBetweenButtion - m_iButtonSizePixel, iButtonTLocationY);
            buttonE.Location = new Point(iButtonTLocationX - m_iGapSizeBetweenButtion * 2 - m_iButtonSizePixel * 2, iButtonTLocationY);
            buttonW.Location = new Point(iButtonTLocationX - m_iGapSizeBetweenButtion * 3 - m_iButtonSizePixel * 3, iButtonTLocationY);
            buttonQ.Location = new Point(iButtonTLocationX - m_iGapSizeBetweenButtion * 4 - m_iButtonSizePixel * 4, iButtonTLocationY);
            
            // Y 버튼 설정
            int iButtonYLocationX = iButtonGroupPanelCenterLocationX + (m_iGapSizeBetweenButtion / 2);
            int iButtonYLocationY = iButtonGroupPanelCenterLocationY - (m_iButtonSizePixel / 2) - m_iButtonSizePixel - m_iGapSizeBetweenButtion;
            buttonY.Location = new Point(iButtonYLocationX, iButtonYLocationY);

            // Y버튼의 오른쪽 버튼 (UIOP) 설정)            
            buttonU.Location = new Point(iButtonYLocationX + m_iGapSizeBetweenButtion + m_iButtonSizePixel, iButtonYLocationY);
            buttonI.Location = new Point(iButtonYLocationX + m_iGapSizeBetweenButtion * 2 + m_iButtonSizePixel * 2, iButtonYLocationY);
            buttonO.Location = new Point(iButtonYLocationX + m_iGapSizeBetweenButtion * 3 + m_iButtonSizePixel * 3, iButtonYLocationY);
            buttonP.Location = new Point(iButtonYLocationX + m_iGapSizeBetweenButtion * 4 + m_iButtonSizePixel * 4, iButtonYLocationY);
            
            // 1번째 줄 설정
            // 5 버튼 설정
            int iButton5LocationX = iButtonGroupPanelCenterLocationX - (m_iButtonSizePixel / 2) * 2 - (m_iGapSizeBetweenButtion / 2);
            int iButton5LocationY = iButtonGroupPanelCenterLocationY - (m_iButtonSizePixel / 2) - m_iButtonSizePixel * 2 - m_iGapSizeBetweenButtion * 2;
            button5.Location = new Point(iButton5LocationX, iButton5LocationY);            

            // 5 버튼의 왼족 버튼 (4321) 설정            
            button4.Location = new Point(iButton5LocationX - m_iGapSizeBetweenButtion - m_iButtonSizePixel, iButton5LocationY);
            button3.Location = new Point(iButton5LocationX - m_iGapSizeBetweenButtion * 2 - m_iButtonSizePixel * 2, iButton5LocationY);
            button2.Location = new Point(iButton5LocationX - m_iGapSizeBetweenButtion * 3 - m_iButtonSizePixel * 3, iButton5LocationY);
            button1.Location = new Point(iButton5LocationX - m_iGapSizeBetweenButtion * 4 - m_iButtonSizePixel * 4, iButton5LocationY);
            
            // 6버튼 설정            
            int iButton6LocationX = iButtonGroupPanelCenterLocationX + (m_iGapSizeBetweenButtion / 2);
            int iButton6LocationY = iButtonGroupPanelCenterLocationY - (m_iButtonSizePixel / 2) - m_iButtonSizePixel * 2 - m_iGapSizeBetweenButtion * 2;
            button6.Location = new Point(iButton6LocationX, iButton6LocationY);

            // 6버튼의 오른쪽 버튼 (7890) 설정
            button7.Location = new Point(iButton6LocationX + m_iGapSizeBetweenButtion + m_iButtonSizePixel, iButton6LocationY);
            button8.Location = new Point(iButton6LocationX + m_iGapSizeBetweenButtion * 2 + m_iButtonSizePixel * 2, iButton6LocationY);
            button9.Location = new Point(iButton6LocationX + m_iGapSizeBetweenButtion * 3 + m_iButtonSizePixel * 3, iButton6LocationY);
            button0.Location = new Point(iButton6LocationX + m_iGapSizeBetweenButtion * 4 + m_iButtonSizePixel * 4, iButton6LocationY);

            // 4번째 줄 설정
            // V 버튼 설정
            int iButtonVLocationX = iButtonGroupPanelCenterLocationX - (m_iButtonSizePixel / 2);
            int iButtonVLocationY = iButtonGroupPanelCenterLocationY - (m_iButtonSizePixel / 2) + m_iButtonSizePixel + m_iGapSizeBetweenButtion;
            buttonV.Location = new Point(iButtonVLocationX, iButtonVLocationY);

            // V 버튼의 왼족 버튼 (CXZ) 설정
            buttonC.Location = new Point(iButtonVLocationX - m_iGapSizeBetweenButtion - m_iButtonSizePixel, iButtonVLocationY);
            buttonX.Location = new Point(iButtonVLocationX - m_iGapSizeBetweenButtion * 2 - m_iButtonSizePixel * 2, iButtonVLocationY);
            buttonZ.Location = new Point(iButtonVLocationX - m_iGapSizeBetweenButtion * 3 - m_iButtonSizePixel * 3, iButtonVLocationY);
            
            // V 버튼의 오른쪽 버튼 (BNM) 설정
            buttonB.Location = new Point(iButtonVLocationX + m_iGapSizeBetweenButtion + m_iButtonSizePixel, iButtonVLocationY);
            buttonN.Location = new Point(iButtonVLocationX + m_iGapSizeBetweenButtion * 2 + m_iButtonSizePixel * 2, iButtonVLocationY);
            buttonM.Location = new Point(iButtonVLocationX + m_iGapSizeBetweenButtion * 3 + m_iButtonSizePixel * 3, iButtonVLocationY);

            // 5번째 줄 설정
            // Space 버튼 설정
            int iButtonSpaceLocationX = iButtonGroupPanelCenterLocationX - buttonSpace.Size.Width / 2;
            int iButtonSpaceLocationY = iButtonGroupPanelCenterLocationY - (buttonSpace.Size.Height / 2) + buttonSpace.Size.Height * 2 + m_iGapSizeBetweenButtion * 4;
            buttonSpace.Location = new Point(iButtonSpaceLocationX, iButtonSpaceLocationY);
            buttonBackSpace.Location = new Point(iButtonSpaceLocationX + m_iButtonSizePixel * 4 + m_iGapSizeBetweenButtion * 3, iButtonSpaceLocationY);
         }

        private void InitializeButtonFontSize()
        {
            // 버튼의 크기에 따라 글자가 보이도록 폰트 사이즈 설정

            if (m_iButtonSize == 22)
            {
                Font font =new Font ("Times New Roman", 4f);
                button1.Font = font; button2.Font = font; button3.Font = font; button4.Font = font;
                button5.Font = font; button6.Font = font; button7.Font = font; button8.Font = font;
                button9.Font = font; button0.Font = font;
                buttonQ.Font = font; buttonW.Font = font; buttonE.Font = font; buttonR.Font = font;
                buttonT.Font = font; buttonY.Font = font; buttonU.Font = font; buttonI.Font = font;
                buttonO.Font = font; buttonP.Font = font;
                buttonA.Font = font; buttonS.Font = font; buttonD.Font = font; buttonF.Font = font;
                buttonG.Font = font; buttonH.Font = font; buttonJ.Font = font; buttonK.Font = font;
                buttonL.Font = font;
                buttonZ.Font = font; buttonX.Font = font; buttonC.Font = font; buttonV.Font = font;
                buttonB.Font = font; buttonN.Font = font; buttonM.Font = font;
                buttonSpace.Font = font;
                buttonBackSpace.Font = font;
            }
            else if (m_iButtonSize == 33)
            {
                Font font = new Font("Times New Roman", 6.75f);
                button1.Font = font; button2.Font = font; button3.Font = font; button4.Font = font;
                button5.Font = font; button6.Font = font; button7.Font = font; button8.Font = font;
                button9.Font = font; button0.Font = font;
                buttonQ.Font = font; buttonW.Font = font; buttonE.Font = font; buttonR.Font = font;
                buttonT.Font = font; buttonY.Font = font; buttonU.Font = font; buttonI.Font = font;
                buttonO.Font = font; buttonP.Font = font;
                buttonA.Font = font; buttonS.Font = font; buttonD.Font = font; buttonF.Font = font;
                buttonG.Font = font; buttonH.Font = font; buttonJ.Font = font; buttonK.Font = font;
                buttonL.Font = font;
                buttonZ.Font = font; buttonX.Font = font; buttonC.Font = font; buttonV.Font = font;
                buttonB.Font = font; buttonN.Font = font; buttonM.Font = font;
                buttonSpace.Font = font;
                buttonBackSpace.Font = font;
            }
            else if (m_iButtonSize == 44)
            {
                Font font = new Font("Times New Roman", 9.75f);
                button1.Font = font; button2.Font = font; button3.Font = font; button4.Font = font;
                button5.Font = font; button6.Font = font; button7.Font = font; button8.Font = font;
                button9.Font = font; button0.Font = font;
                buttonQ.Font = font; buttonW.Font = font; buttonE.Font = font; buttonR.Font = font;
                buttonT.Font = font; buttonY.Font = font; buttonU.Font = font; buttonI.Font = font;
                buttonO.Font = font; buttonP.Font = font;
                buttonA.Font = font; buttonS.Font = font; buttonD.Font = font; buttonF.Font = font;
                buttonG.Font = font; buttonH.Font = font; buttonJ.Font = font; buttonK.Font = font;
                buttonL.Font = font;
                buttonZ.Font = font; buttonX.Font = font; buttonC.Font = font; buttonV.Font = font;
                buttonB.Font = font; buttonN.Font = font; buttonM.Font = font;
                buttonSpace.Font = font;
                buttonBackSpace.Font = font;
            }
            else if (m_iButtonSize == 55)
            {
                Font font = new Font("Times New Roman", 12.75f);
                button1.Font = font; button2.Font = font; button3.Font = font; button4.Font = font;
                button5.Font = font; button6.Font = font; button7.Font = font; button8.Font = font;
                button9.Font = font; button0.Font = font;
                buttonQ.Font = font; buttonW.Font = font; buttonE.Font = font; buttonR.Font = font;
                buttonT.Font = font; buttonY.Font = font; buttonU.Font = font; buttonI.Font = font;
                buttonO.Font = font; buttonP.Font = font;
                buttonA.Font = font; buttonS.Font = font; buttonD.Font = font; buttonF.Font = font;
                buttonG.Font = font; buttonH.Font = font; buttonJ.Font = font; buttonK.Font = font;
                buttonL.Font = font;
                buttonZ.Font = font; buttonX.Font = font; buttonC.Font = font; buttonV.Font = font;
                buttonB.Font = font; buttonN.Font = font; buttonM.Font = font;
                buttonSpace.Font = font;
                buttonBackSpace.Font = font;
            }
            else if (m_iButtonSize == 66)
            {
                Font font = new Font("Times New Roman", 15.75f);
                button1.Font = font; button2.Font = font; button3.Font = font; button4.Font = font;
                button5.Font = font; button6.Font = font; button7.Font = font; button8.Font = font;
                button9.Font = font; button0.Font = font;
                buttonQ.Font = font; buttonW.Font = font; buttonE.Font = font; buttonR.Font = font;
                buttonT.Font = font; buttonY.Font = font; buttonU.Font = font; buttonI.Font = font;
                buttonO.Font = font; buttonP.Font = font;
                buttonA.Font = font; buttonS.Font = font; buttonD.Font = font; buttonF.Font = font;
                buttonG.Font = font; buttonH.Font = font; buttonJ.Font = font; buttonK.Font = font;
                buttonL.Font = font;
                buttonZ.Font = font; buttonX.Font = font; buttonC.Font = font; buttonV.Font = font;
                buttonB.Font = font; buttonN.Font = font; buttonM.Font = font;
                buttonSpace.Font = font;
                buttonBackSpace.Font = font;
            }
            else if (m_iButtonSize == 77)
            {
                Font font = new Font("Times New Roman", 18.75f);
                button1.Font = font;    button2.Font = font;    button3.Font = font;    button4.Font = font;
                button5.Font = font;    button6.Font = font;    button7.Font = font;    button8.Font = font;
                button9.Font = font;    button0.Font = font;
                buttonQ.Font = font;    buttonW.Font = font;    buttonE.Font = font;    buttonR.Font = font;
                buttonT.Font = font;    buttonY.Font = font;    buttonU.Font = font;    buttonI.Font = font;
                buttonO.Font = font;    buttonP.Font = font;
                buttonA.Font = font;    buttonS.Font = font;    buttonD.Font = font;    buttonF.Font = font;
                buttonG.Font = font;    buttonH.Font = font;    buttonJ.Font = font;    buttonK.Font = font;
                buttonL.Font = font;
                buttonZ.Font = font;    buttonX.Font = font;    buttonC.Font = font;    buttonV.Font = font;
                buttonB.Font = font;    buttonN.Font = font;    buttonM.Font = font;
                buttonSpace.Font = font;
                buttonBackSpace.Font = font;
            }
        }

        //
        private void InitializeButtonGroupPanelLocation()
        {
            // 버튼을 포함하고 있는 패널의 위치를 설정
            // 패널의 중심을 시계 화면의중심에 위치하도록 이동

            // 시계 화면 중심 위치 계산
            int iWatchScreenCenterX = pictureBoxWatchCenter.Location.X + (pictureBoxWatchCenter.Width / 2);
            int iWatchScreenCenterY = pictureBoxWatchCenter.Location.Y + (pictureBoxWatchCenter.Height / 2);

            //Console.WriteLine("iWatchScreenCetner {0}, {1}", iWatchScreenCenterX, iWatchScreenCenterY);
            //Console.WriteLine("Panel {0}, {1}", panelButtonGroup.Location.X, panelButtonGroup.Location.Y);

            // 패널의 왼쪽 위를 시계 화면 중심에 오도록 계산
            // 패널 절반 크기만큼 가로/세로로 왼쪽/위로 이동
            Point panelPos = new Point(iWatchScreenCenterX - panelButtonGroup.Width / 2, iWatchScreenCenterY - panelButtonGroup.Height / 2);

            panelButtonGroup.Location = panelPos;
        }

        //
        private void textBoxMessage_MouseDown(object sender, MouseEventArgs e)
        {            
            // 터치down 되었을 때 텍스트박스에서의 위치 반환 
            m_iTextBoxTouchDownX = e.Location.X;

            // 기록 저장 (이벤트, 시간, 시간차 등)
            //m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "textBox", "down", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
            //m_eventTime = DateTime.Now;
        }

        private void textBoxMessage_MouseMove(object sender, MouseEventArgs e)
        {
        }
        
        private void textBoxMessage_MouseUp(object sender, MouseEventArgs e)
        {            
            if (textBoxMessage.TextLength > 0)
            {
                // TextBox의 글자수가 1개 이상일때는 왼쪽 오른쪽 제스처 모두 동작

                // 터치up 되었을 때 텍스트박스에서의 위치 반환 
                m_iTextBoxTouchUpX = e.Location.X;

                if (m_iTextBoxTouchDownX - m_iTextBoxTouchUpX >= m_iTextBoxTouchMax)
                {
                    // 터치 Up 되었을 때의 위치가 Down 되었을 때의 위치보다 왼쪽이면
                    // 즉, 왼쪽으로의 Gesture면,

                    // 마지막 글자 제거
                    string strModifiedText = textBoxMessage.Text.Remove(textBoxMessage.Text.Length - 1, 1);
                    textBoxMessage.Text = strModifiedText;

                    // 입력된 글자와 입력해야 하는 글자가 다르면 빨갛게 표시
                    CheckCharacterBetweenInputAndTarget();

                    // Caret 위치
                    m_iCaretPosition = textBoxMessage.Text.Length;

                    textBoxMessage.SelectionStart = m_iCaretPosition;  

                    // OK 버튼으로 초첨 변경
                    textBoxMessage.Focus();

                    // BackSpace 제스처 기록
                    m_iTotalGestureBackSpaceCount = m_iTotalGestureBackSpaceCount + 1;

                    // 기록 저장 (이벤트, 시간, 시간차 등)
                    m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "gesture", "backspace", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                    m_eventTime = DateTime.Now;
                }
                else if (m_iTextBoxTouchDownX - m_iTextBoxTouchUpX <= -m_iTextBoxTouchMax)
                {
                    // 터치 Up 되었을 때의 위치가 Down 되었을 때의 위치보다 오른쪽이면
                    // 즉, 오른쪽으로의 Gesture면,

                    // 마지막 글자에 빈칸 추가
                    string strModifiedText = textBoxMessage.Text + " ";
                    textBoxMessage.Text = strModifiedText;

                    // 입력된 글자와 입력해야 하는 글자가 다르면 빨갛게 표시
                    CheckCharacterBetweenInputAndTarget();

                    // Caret 위치
                    m_iCaretPosition = textBoxMessage.Text.Length;

                    textBoxMessage.SelectionStart = m_iCaretPosition;

                    // OK 버튼으로 초첨 변경
                    textBoxMessage.Focus();

                    // Space 제스처 기록
                    m_iTotalGestureSpaceCount = m_iTotalGestureSpaceCount + 1;

                    // 기록 저장 (이벤트, 시간, 시간차 등)
                    m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "gesture", "space", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                    m_eventTime = DateTime.Now;
                }
                
                // Touch의 위치 초기화
                m_iTextBoxTouchDownX = 0;
                m_iTextBoxTouchUpX = 0;
            }            
        }

        private void textBoxMessage_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //Console.WriteLine("textBoxMessage_MouseDoubleClick");

            m_iCaretPosition = textBoxMessage.TextLength;

            textBoxMessage.SelectionStart = m_iCaretPosition; 
            //Console.WriteLine(m_iCaretPosition);
        }

        private void textBoxMessage_MouseClick(object sender, MouseEventArgs e)
        {
            //Console.WriteLine("textBoxMessage_MouseClick");

            // TextBox를 선택한 곳에서의 위치 처리 함수
            // TextBox Control의 GetCharIndexFromPosition 함수가 마지막 글자의 Index를 제대로 가져오지 못하는 Bug가 있어서 다음과 같이 해결
            // TextBox Control의 경우는 이 함수도 적용되지 않으며, RichTextBox만 적용 가능함
            int X = Math.Min(Math.Max(e.X, 0), ((RichTextBox)sender).ClientSize.Width);
            int Y = Math.Min(Math.Max(e.Y, 0), ((RichTextBox)sender).ClientSize.Height);

            Point vPoint = new Point(X, Y);

            m_iCaretPosition = (int)SendMessage(((RichTextBox)sender).Handle, EM_CHARFROMPOS, 0, ref vPoint);
            //Console.WriteLine(m_iCaretPosition + " " + vPoint);

            // 기록 저장 (이벤트, 시간, 시간차 등)
            //m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "textBox", "click", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
            //m_eventTime = DateTime.Now;
        }


        private void textBoxMessage_SelectionChanged(object sender, EventArgs e)
        {
           
        }


        // 입력된 글자와 입력해야 하는 글자가 다르면 빨갛게 표시 
        private void CheckCharacterBetweenInputAndTarget()
        {
            for (int i = 0; i < textBoxMessage.Text.Length; i++)
            {
                string strInput = textBoxMessage.Text[i].ToString();
                string strTarget = "";

                //Console.WriteLine(textBoxMessage.Text + "  " + labelTargetText.Text);

                if (i >= labelTargetText.Text.Length)
                {
                    // 만일 텍스트 박스에 입력된 글자가 대상 텍스트 글자보다 길면, 
                    // strTarget을 빈 글자로 처리
                    strTarget = "";
                }
                else
                {
                    // 같거나 작으면
                    strTarget = labelTargetText.Text[i].ToString();
                }


                if (strInput == strTarget)
                {
                    textBoxMessage.Select(i, i + 1);
                    textBoxMessage.SelectionColor = Color.Black;
                    //Console.WriteLine("black " + i + " " + (i + 1) + " " + strInput + " " + strTarget);
                }
                else
                {
                    textBoxMessage.Select(i, i + 1);
                    textBoxMessage.SelectionColor = Color.Red;
                    //Console.WriteLine("Red " + i + " " + (i + 1) + " " + strInput + " " + strTarget);
                }
            }
        }

        // 
        private int CheckButtonGroupPanelOutsideBoundary()
        {
            // 새로운 마우스 위치가 버튼 제한 영역 밖을 벗어나는지 확인
            // 가장 왼쪽 위에 있는 버튼인 1 버튼이 시계 화면 중심보다 오른쪽 아래로 가지 못하도록 막음
            // 가장 아래쪽에 있는 버튼인 space 버튼이 시계 화면 중심보다 위쪽으로 가지 못하도록 막음
            // 가장 오른쪽 위에 있는 버튼인 0 버튼이 시계 화면 중심보다 왼쪽 아래로 가지 못하도록 막음

            int iWatchScreenLeftQuarterX = pictureBoxWatchCenter.Location.X + pictureBoxWatchCenter.Width / 8;
            int iWatchScreenRightQuarterX = pictureBoxWatchCenter.Location.X + pictureBoxWatchCenter.Width * 7 / 8;
            int iWatchScreenTopQuarterY = pictureBoxWatchCenter.Location.Y + pictureBoxWatchCenter.Height / 8;
            int iWatchScreenBottomQuarterY = pictureBoxWatchCenter.Location.Y + pictureBoxWatchCenter.Height * 7 / 8;

            if (panelButtonGroup.Location.X > iWatchScreenLeftQuarterX)
            {
                panelButtonGroup.Location = new Point(iWatchScreenLeftQuarterX, panelButtonGroup.Location.Y);
                return 1;
            }

            if (panelButtonGroup.Location.Y > iWatchScreenTopQuarterY)
            {
                panelButtonGroup.Location = new Point(panelButtonGroup.Location.X, iWatchScreenTopQuarterY);
                return 2;
            }

            if ((panelButtonGroup.Location.X + panelButtonGroup.Size.Width) < iWatchScreenRightQuarterX)
            {
                panelButtonGroup.Location = new Point(iWatchScreenRightQuarterX - panelButtonGroup.Width, panelButtonGroup.Location.Y);
                return 3;
            }

            if ((panelButtonGroup.Location.Y + panelButtonGroup.Size.Height) < iWatchScreenBottomQuarterY)
            {
                panelButtonGroup.Location = new Point(panelButtonGroup.Location.X, iWatchScreenBottomQuarterY - panelButtonGroup.Height);
                return 4;
            }

            return 0;
        }
        
        
        // button1
        private void button1_MouseDown(object sender, MouseEventArgs e)
        {
            m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));
                        
            m_bMouseDownFlag = true;

            // 기록 저장 (이벤트, 시간, 시간차 등)
            m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "button1", "down", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
            m_eventTime = DateTime.Now;
        }
        
        private void button1_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true)
            {
                // 버튼을 포함하는 패널이 영역을 벗어나는지 확인
                int iRet = CheckButtonGroupPanelOutsideBoundary();
                if (iRet == 1 || iRet == 2 || iRet == 3 || iRet == 4)
                    return;                

                m_endPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                int iDiffX = (m_endPoint.X - m_startPoint.X) * m_iCDGain;
                int iDiffY = (m_endPoint.Y - m_startPoint.Y) * m_iCDGain;

                // 새로운 마우스 위치 전역변수에 계산
                Point newPanelPos = new Point(panelButtonGroup.Location.X + iDiffX, panelButtonGroup.Location.Y + iDiffY);
                
                // 새로운 마우스 시작 좌표 저장
                m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                // 새로운 마우스 위치를 통해 버튼 그룹 이동
                panelButtonGroup.Location = newPanelPos;

                Invalidate();

                m_bMouseMoveFlag = true;
            }
        }

        private void button1_MouseUp(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true && m_bMouseMoveFlag == true)
            {
                m_iTotalButtonMoveCount = m_iTotalButtonMoveCount + 1;

                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "button1", "click during move", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }

            m_bMouseDownFlag = false;
            m_bMouseMoveFlag = false;

            // 초점을 항상 OK 버튼으로 고정            
            textBoxMessage.Focus();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (m_bMouseMoveFlag == false)
            {
                // Caret이 있는 위치 사이에 글자를 추가 처리
                textBoxMessage.Select(m_iCaretPosition, textBoxMessage.TextLength);
                string strTextAfter = textBoxMessage.SelectedText;

                textBoxMessage.Select(0, m_iCaretPosition);
                string strTextBefore = textBoxMessage.SelectedText;
                strTextBefore = strTextBefore + button1.Text;

                string strModifiedText = strTextBefore + strTextAfter;

                textBoxMessage.Text = strModifiedText;

                // 입력된 글자와 입력해야 하는 글자가 다르면 빨갛게 표시
                CheckCharacterBetweenInputAndTarget();   

                m_iCaretPosition = m_iCaretPosition + 1;

                textBoxMessage.SelectionStart = m_iCaretPosition;

                // 버튼 클릭 횟수 기록
                m_iTotalButtonCountExceptBackSpace = m_iTotalButtonCountExceptBackSpace + 1;

                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "button1", "click", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }
        }
        
        private void button2_MouseDown(object sender, MouseEventArgs e)
        {
            m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

            m_bMouseDownFlag = true;

            // 기록 저장 (이벤트, 시간, 시간차 등)
            m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "button2", "down", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
            m_eventTime = DateTime.Now;
        }

        private void button2_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true)
            {
                // 버튼을 포함하는 패널이 영역을 벗어나는지 확인
                int iRet = CheckButtonGroupPanelOutsideBoundary();
                if (iRet == 1 || iRet == 2 || iRet == 3 || iRet == 4)
                    return;

                m_endPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                int iDiffX = (m_endPoint.X - m_startPoint.X) * m_iCDGain;
                int iDiffY = (m_endPoint.Y - m_startPoint.Y) * m_iCDGain;

                // 새로운 마우스 위치 전역변수에 계산
                Point newPanelPos = new Point(panelButtonGroup.Location.X + iDiffX, panelButtonGroup.Location.Y + iDiffY);

                // 새로운 마우스 시작 좌표 저장
                m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                // 새로운 마우스 위치를 통해 버튼 그룹 이동
                panelButtonGroup.Location = newPanelPos;

                Invalidate();

                m_bMouseMoveFlag = true;
            }
        }

        private void button2_MouseUp(object sender, MouseEventArgs e)
        {            
            if (m_bMouseDownFlag == true && m_bMouseMoveFlag == true)
            {
                m_iTotalButtonMoveCount = m_iTotalButtonMoveCount + 1;

                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "button2", "move", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }

            m_bMouseDownFlag = false;
            m_bMouseMoveFlag = false;

            // 초점을 항상 OK 버튼으로 고정            
            textBoxMessage.Focus();
        }


        private void button2_Click(object sender, EventArgs e)
        {
            if (m_bMouseMoveFlag == false)
            {
                // Caret이 있는 위치 사이에 글자를 추가 처리
                textBoxMessage.Select(m_iCaretPosition, textBoxMessage.TextLength);
                string strTextAfter = textBoxMessage.SelectedText;

                textBoxMessage.Select(0, m_iCaretPosition);
                string strTextBefore = textBoxMessage.SelectedText;
                strTextBefore = strTextBefore + button2.Text;

                string strModifiedText = strTextBefore + strTextAfter;

                textBoxMessage.Text = strModifiedText;

                // 입력된 글자와 입력해야 하는 글자가 다르면 빨갛게 표시
                CheckCharacterBetweenInputAndTarget(); 

                m_iCaretPosition = m_iCaretPosition + 1;

                textBoxMessage.SelectionStart = m_iCaretPosition;

                // 버튼 클릭 횟수 기록
                m_iTotalButtonCountExceptBackSpace = m_iTotalButtonCountExceptBackSpace + 1;

                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "button2", "click", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }
        }

        private void button3_MouseDown(object sender, MouseEventArgs e)
        {
            m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

            m_bMouseDownFlag = true;

            // 기록 저장 (이벤트, 시간, 시간차 등)
            m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "button3", "down", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
            m_eventTime = DateTime.Now;
        }

        private void button3_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true)
            {
                // 버튼을 포함하는 패널이 영역을 벗어나는지 확인
                int iRet = CheckButtonGroupPanelOutsideBoundary();
                if (iRet == 1 || iRet == 2 || iRet == 3 || iRet == 4)
                    return;

                m_endPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                int iDiffX = (m_endPoint.X - m_startPoint.X) * m_iCDGain;
                int iDiffY = (m_endPoint.Y - m_startPoint.Y) * m_iCDGain;

                // 새로운 마우스 위치 전역변수에 계산
                Point newPanelPos = new Point(panelButtonGroup.Location.X + iDiffX, panelButtonGroup.Location.Y + iDiffY);

                // 새로운 마우스 시작 좌표 저장
                m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                // 새로운 마우스 위치를 통해 버튼 그룹 이동
                panelButtonGroup.Location = newPanelPos;

                Invalidate();

                m_bMouseMoveFlag = true;
            }
        }

        private void button3_MouseUp(object sender, MouseEventArgs e)
        {            
            if (m_bMouseDownFlag == true && m_bMouseMoveFlag == true)
            {
                m_iTotalButtonMoveCount = m_iTotalButtonMoveCount + 1;

                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "button3", "move", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }

            m_bMouseDownFlag = false;
            m_bMouseMoveFlag = false;

            // 초점을 항상 OK 버튼으로 고정            
            textBoxMessage.Focus();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (m_bMouseMoveFlag == false)
            {
                // Caret이 있는 위치 사이에 글자를 추가 처리
                textBoxMessage.Select(m_iCaretPosition, textBoxMessage.TextLength);
                string strTextAfter = textBoxMessage.SelectedText;

                textBoxMessage.Select(0, m_iCaretPosition);
                string strTextBefore = textBoxMessage.SelectedText;
                strTextBefore = strTextBefore + button3.Text;

                string strModifiedText = strTextBefore + strTextAfter;

                textBoxMessage.Text = strModifiedText;

                // 입력된 글자와 입력해야 하는 글자가 다르면 빨갛게 표시
                CheckCharacterBetweenInputAndTarget(); 

                m_iCaretPosition = m_iCaretPosition + 1;

                textBoxMessage.SelectionStart = m_iCaretPosition;

                // 버튼 클릭 횟수 기록
                m_iTotalButtonCountExceptBackSpace = m_iTotalButtonCountExceptBackSpace + 1;

                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "button3", "click", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }
        }

        private void button4_MouseDown(object sender, MouseEventArgs e)
        {
            m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

            m_bMouseDownFlag = true;

            // 기록 저장 (이벤트, 시간, 시간차 등)
            m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "button4", "down", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
            m_eventTime = DateTime.Now;
        }

        private void button4_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true)
            {
                // 버튼을 포함하는 패널이 영역을 벗어나는지 확인
                int iRet = CheckButtonGroupPanelOutsideBoundary();
                if (iRet == 1 || iRet == 2 || iRet == 3 || iRet == 4)
                    return;

                m_endPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                int iDiffX = (m_endPoint.X - m_startPoint.X) * m_iCDGain;
                int iDiffY = (m_endPoint.Y - m_startPoint.Y) * m_iCDGain;

                // 새로운 마우스 위치 전역변수에 계산
                Point newPanelPos = new Point(panelButtonGroup.Location.X + iDiffX, panelButtonGroup.Location.Y + iDiffY);

                // 새로운 마우스 시작 좌표 저장
                m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                // 새로운 마우스 위치를 통해 버튼 그룹 이동
                panelButtonGroup.Location = newPanelPos;

                Invalidate();

                m_bMouseMoveFlag = true;
            }
        }
        
        private void button4_MouseUp(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true && m_bMouseMoveFlag == true)
            {
                m_iTotalButtonMoveCount = m_iTotalButtonMoveCount + 1;
                
                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "button4", "move", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }

            m_bMouseDownFlag = false;
            m_bMouseMoveFlag = false;

            // 초점을 항상 OK 버튼으로 고정            
            textBoxMessage.Focus();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (m_bMouseMoveFlag == false)
            {
                // Caret이 있는 위치 사이에 글자를 추가 처리
                textBoxMessage.Select(m_iCaretPosition, textBoxMessage.TextLength);
                string strTextAfter = textBoxMessage.SelectedText;

                textBoxMessage.Select(0, m_iCaretPosition);
                string strTextBefore = textBoxMessage.SelectedText;
                strTextBefore = strTextBefore + button4.Text;

                string strModifiedText = strTextBefore + strTextAfter;

                textBoxMessage.Text = strModifiedText;

                // 입력된 글자와 입력해야 하는 글자가 다르면 빨갛게 표시
                CheckCharacterBetweenInputAndTarget(); 

                m_iCaretPosition = m_iCaretPosition + 1;

                textBoxMessage.SelectionStart = m_iCaretPosition;

                // 버튼 클릭 횟수 기록
                m_iTotalButtonCountExceptBackSpace = m_iTotalButtonCountExceptBackSpace + 1;

                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "button4", "click", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }
        }

        private void button5_MouseDown(object sender, MouseEventArgs e)
        {
            m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

            m_bMouseDownFlag = true;

            // 기록 저장 (이벤트, 시간, 시간차 등)
            m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "button5", "down", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
            m_eventTime = DateTime.Now;
        }

        private void button5_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true)
            {
                // 버튼을 포함하는 패널이 영역을 벗어나는지 확인
                int iRet = CheckButtonGroupPanelOutsideBoundary();
                if (iRet == 1 || iRet == 2 || iRet == 3 || iRet == 4)
                    return;

                m_endPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                int iDiffX = (m_endPoint.X - m_startPoint.X) * m_iCDGain;
                int iDiffY = (m_endPoint.Y - m_startPoint.Y) * m_iCDGain;

                // 새로운 마우스 위치 전역변수에 계산
                Point newPanelPos = new Point(panelButtonGroup.Location.X + iDiffX, panelButtonGroup.Location.Y + iDiffY);

                // 새로운 마우스 시작 좌표 저장
                m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                // 새로운 마우스 위치를 통해 버튼 그룹 이동
                panelButtonGroup.Location = newPanelPos;

                Invalidate();

                m_bMouseMoveFlag = true;
            }
        }

        private void button5_MouseUp(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true && m_bMouseMoveFlag == true)
            {
                m_iTotalButtonMoveCount = m_iTotalButtonMoveCount + 1;
                
                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "button5", "move", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }
            
            m_bMouseDownFlag = false;
            m_bMouseMoveFlag = false;

            // 초점을 항상 OK 버튼으로 고정            
            textBoxMessage.Focus();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (m_bMouseMoveFlag == false)
            {
                // Caret이 있는 위치 사이에 글자를 추가 처리
                textBoxMessage.Select(m_iCaretPosition, textBoxMessage.TextLength);
                string strTextAfter = textBoxMessage.SelectedText;

                textBoxMessage.Select(0, m_iCaretPosition);
                string strTextBefore = textBoxMessage.SelectedText;
                strTextBefore = strTextBefore + button5.Text;

                string strModifiedText = strTextBefore + strTextAfter;

                textBoxMessage.Text = strModifiedText;

                // 입력된 글자와 입력해야 하는 글자가 다르면 빨갛게 표시
                CheckCharacterBetweenInputAndTarget();

                m_iCaretPosition = m_iCaretPosition + 1;

                textBoxMessage.SelectionStart = m_iCaretPosition;

                // 버튼 클릭 횟수 기록
                m_iTotalButtonCountExceptBackSpace = m_iTotalButtonCountExceptBackSpace + 1;

                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "button5", "click", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }
        }

        private void button6_MouseDown(object sender, MouseEventArgs e)
        {
            m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

            m_bMouseDownFlag = true;

            // 기록 저장 (이벤트, 시간, 시간차 등)
            m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "button6", "down", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
            m_eventTime = DateTime.Now;
        }

        private void button6_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true)
            {
                // 버튼을 포함하는 패널이 영역을 벗어나는지 확인
                int iRet = CheckButtonGroupPanelOutsideBoundary();
                if (iRet == 1 || iRet == 2 || iRet == 3 || iRet == 4)
                    return;

                m_endPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                int iDiffX = (m_endPoint.X - m_startPoint.X) * m_iCDGain;
                int iDiffY = (m_endPoint.Y - m_startPoint.Y) * m_iCDGain;

                // 새로운 마우스 위치 전역변수에 계산
                Point newPanelPos = new Point(panelButtonGroup.Location.X + iDiffX, panelButtonGroup.Location.Y + iDiffY);

                // 새로운 마우스 시작 좌표 저장
                m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                // 새로운 마우스 위치를 통해 버튼 그룹 이동
                panelButtonGroup.Location = newPanelPos;

                Invalidate();

                m_bMouseMoveFlag = true;
            }
        }

        private void button6_MouseUp(object sender, MouseEventArgs e)
        {            
            if (m_bMouseDownFlag == true && m_bMouseMoveFlag == true)
            {
                m_iTotalButtonMoveCount = m_iTotalButtonMoveCount + 1;
                
                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "button6", "move", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }

            m_bMouseDownFlag = false;
            m_bMouseMoveFlag = false;

            // 초점을 항상 OK 버튼으로 고정            
            textBoxMessage.Focus();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (m_bMouseMoveFlag == false)
            {
                // Caret이 있는 위치 사이에 글자를 추가 처리
                textBoxMessage.Select(m_iCaretPosition, textBoxMessage.TextLength);
                string strTextAfter = textBoxMessage.SelectedText;

                textBoxMessage.Select(0, m_iCaretPosition);
                string strTextBefore = textBoxMessage.SelectedText;
                strTextBefore = strTextBefore + button6.Text;

                string strModifiedText = strTextBefore + strTextAfter;

                textBoxMessage.Text = strModifiedText;

                // 입력된 글자와 입력해야 하는 글자가 다르면 빨갛게 표시
                CheckCharacterBetweenInputAndTarget(); 

                m_iCaretPosition = m_iCaretPosition + 1;

                textBoxMessage.SelectionStart = m_iCaretPosition;

                // 버튼 클릭 횟수 기록
                m_iTotalButtonCountExceptBackSpace = m_iTotalButtonCountExceptBackSpace + 1;

                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "button6", "click", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }
        }              

        private void button7_MouseDown(object sender, MouseEventArgs e)
        {
            m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

            m_bMouseDownFlag = true;

            // 기록 저장 (이벤트, 시간, 시간차 등)
            m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "button7", "down", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
            m_eventTime = DateTime.Now;
        }

        private void button7_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true)
            {
                // 버튼을 포함하는 패널이 영역을 벗어나는지 확인
                int iRet = CheckButtonGroupPanelOutsideBoundary();
                if (iRet == 1 || iRet == 2 || iRet == 3 || iRet == 4)
                    return;

                m_endPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                int iDiffX = (m_endPoint.X - m_startPoint.X) * m_iCDGain;
                int iDiffY = (m_endPoint.Y - m_startPoint.Y) * m_iCDGain;

                // 새로운 마우스 위치 전역변수에 계산
                Point newPanelPos = new Point(panelButtonGroup.Location.X + iDiffX, panelButtonGroup.Location.Y + iDiffY);

                // 새로운 마우스 시작 좌표 저장
                m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                // 새로운 마우스 위치를 통해 버튼 그룹 이동
                panelButtonGroup.Location = newPanelPos;

                Invalidate();

                m_bMouseMoveFlag = true;
            }
        }

        private void button7_MouseUp(object sender, MouseEventArgs e)
        {            
            if (m_bMouseDownFlag == true && m_bMouseMoveFlag == true)
            {
                m_iTotalButtonMoveCount = m_iTotalButtonMoveCount + 1;
                
                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "button7", "move", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }

            m_bMouseDownFlag = false;
            m_bMouseMoveFlag = false;

            // 초점을 항상 OK 버튼으로 고정            
            textBoxMessage.Focus();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (m_bMouseMoveFlag == false)
            {
                // Caret이 있는 위치 사이에 글자를 추가 처리
                textBoxMessage.Select(m_iCaretPosition, textBoxMessage.TextLength);
                string strTextAfter = textBoxMessage.SelectedText;

                textBoxMessage.Select(0, m_iCaretPosition);
                string strTextBefore = textBoxMessage.SelectedText;
                strTextBefore = strTextBefore + button7.Text;
                
                string strModifiedText = strTextBefore + strTextAfter;

                textBoxMessage.Text = strModifiedText;

                // 입력된 글자와 입력해야 하는 글자가 다르면 빨갛게 표시
                CheckCharacterBetweenInputAndTarget(); 

                m_iCaretPosition = m_iCaretPosition + 1;

                textBoxMessage.SelectionStart = m_iCaretPosition;

                // 버튼 클릭 횟수 기록
                m_iTotalButtonCountExceptBackSpace = m_iTotalButtonCountExceptBackSpace + 1;

                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "button7", "click", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }
        }

        private void button8_MouseDown(object sender, MouseEventArgs e)
        {
            m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

            m_bMouseDownFlag = true;

            // 기록 저장 (이벤트, 시간, 시간차 등)
            m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "button8", "down", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
            m_eventTime = DateTime.Now;
        }

        private void button8_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true)
            {
                // 버튼을 포함하는 패널이 영역을 벗어나는지 확인
                int iRet = CheckButtonGroupPanelOutsideBoundary();
                if (iRet == 1 || iRet == 2 || iRet == 3 || iRet == 4)
                    return;

                m_endPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                int iDiffX = (m_endPoint.X - m_startPoint.X) * m_iCDGain;
                int iDiffY = (m_endPoint.Y - m_startPoint.Y) * m_iCDGain;

                // 새로운 마우스 위치 전역변수에 계산
                Point newPanelPos = new Point(panelButtonGroup.Location.X + iDiffX, panelButtonGroup.Location.Y + iDiffY);

                // 새로운 마우스 시작 좌표 저장
                m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                // 새로운 마우스 위치를 통해 버튼 그룹 이동
                panelButtonGroup.Location = newPanelPos;

                Invalidate();

                m_bMouseMoveFlag = true;
            }
        }

        private void button8_MouseUp(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true && m_bMouseMoveFlag == true)
            {
                m_iTotalButtonMoveCount = m_iTotalButtonMoveCount + 1;
                
                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "button8", "move", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }

            m_bMouseDownFlag = false;
            m_bMouseMoveFlag = false;

            // 초점을 항상 OK 버튼으로 고정            
            textBoxMessage.Focus();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (m_bMouseMoveFlag == false)
            {
                // Caret이 있는 위치 사이에 글자를 추가 처리
                textBoxMessage.Select(m_iCaretPosition, textBoxMessage.TextLength);
                string strTextAfter = textBoxMessage.SelectedText;

                textBoxMessage.Select(0, m_iCaretPosition);
                string strTextBefore = textBoxMessage.SelectedText;
                strTextBefore = strTextBefore + button8.Text;

                string strModifiedText = strTextBefore + strTextAfter;

                textBoxMessage.Text = strModifiedText;

                // 입력된 글자와 입력해야 하는 글자가 다르면 빨갛게 표시
                CheckCharacterBetweenInputAndTarget(); 

                m_iCaretPosition = m_iCaretPosition + 1;

                textBoxMessage.SelectionStart = m_iCaretPosition;

                // 버튼 클릭 횟수 기록
                m_iTotalButtonCountExceptBackSpace = m_iTotalButtonCountExceptBackSpace + 1;

                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "button8", "click", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }
        }               

        private void button9_MouseDown(object sender, MouseEventArgs e)
        {
            m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

            m_bMouseDownFlag = true;

            // 기록 저장 (이벤트, 시간, 시간차 등)
            m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "button9", "down", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
            m_eventTime = DateTime.Now;
        }

        private void button9_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true)
            {
                // 버튼을 포함하는 패널이 영역을 벗어나는지 확인
                int iRet = CheckButtonGroupPanelOutsideBoundary();
                if (iRet == 1 || iRet == 2 || iRet == 3 || iRet == 4)
                    return;

                m_endPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                int iDiffX = (m_endPoint.X - m_startPoint.X) * m_iCDGain;
                int iDiffY = (m_endPoint.Y - m_startPoint.Y) * m_iCDGain;

                // 새로운 마우스 위치 전역변수에 계산
                Point newPanelPos = new Point(panelButtonGroup.Location.X + iDiffX, panelButtonGroup.Location.Y + iDiffY);

                // 새로운 마우스 시작 좌표 저장
                m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                // 새로운 마우스 위치를 통해 버튼 그룹 이동
                panelButtonGroup.Location = newPanelPos;

                Invalidate();

                m_bMouseMoveFlag = true;
            }
        }

        private void button9_MouseUp(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true && m_bMouseMoveFlag == true)
            {
                m_iTotalButtonMoveCount = m_iTotalButtonMoveCount + 1;
                
                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "button9", "move", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }

            m_bMouseDownFlag = false;
            m_bMouseMoveFlag = false;

            // 초점을 항상 OK 버튼으로 고정            
            textBoxMessage.Focus();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (m_bMouseMoveFlag == false)
            {
                // Caret이 있는 위치 사이에 글자를 추가 처리
                textBoxMessage.Select(m_iCaretPosition, textBoxMessage.TextLength);
                string strTextAfter = textBoxMessage.SelectedText;

                textBoxMessage.Select(0, m_iCaretPosition);
                string strTextBefore = textBoxMessage.SelectedText;
                strTextBefore = strTextBefore + button9.Text;

                string strModifiedText = strTextBefore + strTextAfter;

                textBoxMessage.Text = strModifiedText;

                // 입력된 글자와 입력해야 하는 글자가 다르면 빨갛게 표시
                CheckCharacterBetweenInputAndTarget(); 

                m_iCaretPosition = m_iCaretPosition + 1;

                textBoxMessage.SelectionStart = m_iCaretPosition;

                // 버튼 클릭 횟수 기록
                m_iTotalButtonCountExceptBackSpace = m_iTotalButtonCountExceptBackSpace + 1;

                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "button9", "click", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }
        }                

        private void button0_MouseDown(object sender, MouseEventArgs e)
        {
            m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

            m_bMouseDownFlag = true;

            // 기록 저장 (이벤트, 시간, 시간차 등)
            m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "button0", "down", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
            m_eventTime = DateTime.Now;
        }

        private void button0_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true)
            {
                // 버튼을 포함하는 패널이 영역을 벗어나는지 확인
                int iRet = CheckButtonGroupPanelOutsideBoundary();
                if (iRet == 1 || iRet == 2 || iRet == 3 || iRet == 4)
                    return;

                m_endPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                int iDiffX = (m_endPoint.X - m_startPoint.X) * m_iCDGain;
                int iDiffY = (m_endPoint.Y - m_startPoint.Y) * m_iCDGain;

                // 새로운 마우스 위치 전역변수에 계산
                Point newPanelPos = new Point(panelButtonGroup.Location.X + iDiffX, panelButtonGroup.Location.Y + iDiffY);

                // 새로운 마우스 시작 좌표 저장
                m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                // 새로운 마우스 위치를 통해 버튼 그룹 이동
                panelButtonGroup.Location = newPanelPos;

                Invalidate();

                m_bMouseMoveFlag = true;
            }
        }

        private void button0_MouseUp(object sender, MouseEventArgs e)
        {            
            if (m_bMouseDownFlag == true && m_bMouseMoveFlag == true)
            {
                m_iTotalButtonMoveCount = m_iTotalButtonMoveCount + 1;
                
                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "button0", "move", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }

            m_bMouseDownFlag = false;
            m_bMouseMoveFlag = false;

            // 초점을 항상 OK 버튼으로 고정            
            textBoxMessage.Focus();
        }

        private void button0_Click(object sender, EventArgs e)
        {
            if (m_bMouseMoveFlag == false)
            {
                // Caret이 있는 위치 사이에 글자를 추가 처리
                textBoxMessage.Select(m_iCaretPosition, textBoxMessage.TextLength);
                string strTextAfter = textBoxMessage.SelectedText;

                textBoxMessage.Select(0, m_iCaretPosition);
                string strTextBefore = textBoxMessage.SelectedText;
                strTextBefore = strTextBefore + button0.Text;

                string strModifiedText = strTextBefore + strTextAfter;

                textBoxMessage.Text = strModifiedText;

                // 입력된 글자와 입력해야 하는 글자가 다르면 빨갛게 표시
                CheckCharacterBetweenInputAndTarget(); 

                m_iCaretPosition = m_iCaretPosition + 1;

                textBoxMessage.SelectionStart = m_iCaretPosition;

                // 버튼 클릭 횟수 기록
                m_iTotalButtonCountExceptBackSpace = m_iTotalButtonCountExceptBackSpace + 1;

                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "button0", "click", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }
        }                

        private void buttonQ_MouseDown(object sender, MouseEventArgs e)
        {
            m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

            m_bMouseDownFlag = true;

            // 기록 저장 (이벤트, 시간, 시간차 등)
            m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonQ", "down", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
            m_eventTime = DateTime.Now;
        }

        private void buttonQ_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true)
            {
                // 버튼을 포함하는 패널이 영역을 벗어나는지 확인
                int iRet = CheckButtonGroupPanelOutsideBoundary();
                if (iRet == 1 || iRet == 2 || iRet == 3 || iRet == 4)
                    return;

                m_endPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                int iDiffX = (m_endPoint.X - m_startPoint.X) * m_iCDGain;
                int iDiffY = (m_endPoint.Y - m_startPoint.Y) * m_iCDGain;

                // 새로운 마우스 위치 전역변수에 계산
                Point newPanelPos = new Point(panelButtonGroup.Location.X + iDiffX, panelButtonGroup.Location.Y + iDiffY);

                // 새로운 마우스 시작 좌표 저장
                m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                // 새로운 마우스 위치를 통해 버튼 그룹 이동
                panelButtonGroup.Location = newPanelPos;

                Invalidate();

                m_bMouseMoveFlag = true;
            }
        }

        private void buttonQ_MouseUp(object sender, MouseEventArgs e)
        {            
            if (m_bMouseDownFlag == true && m_bMouseMoveFlag == true)
            {
                m_iTotalButtonMoveCount = m_iTotalButtonMoveCount + 1;
                
                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonQ", "move", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }

            m_bMouseDownFlag = false;
            m_bMouseMoveFlag = false;

            // 초점을 항상 OK 버튼으로 고정            
            textBoxMessage.Focus();
        }

        private void buttonQ_Click(object sender, EventArgs e)
        {
            if (m_bMouseMoveFlag == false)
            {
                // Caret이 있는 위치 사이에 글자를 추가 처리
                textBoxMessage.Select(m_iCaretPosition, textBoxMessage.TextLength);
                string strTextAfter = textBoxMessage.SelectedText;

                textBoxMessage.Select(0, m_iCaretPosition);
                string strTextBefore = textBoxMessage.SelectedText;
                strTextBefore = strTextBefore + buttonQ.Text;

                string strModifiedText = strTextBefore + strTextAfter;

                textBoxMessage.Text = strModifiedText;

                // 입력된 글자와 입력해야 하는 글자가 다르면 빨갛게 표시
                CheckCharacterBetweenInputAndTarget(); 

                m_iCaretPosition = m_iCaretPosition + 1;

                textBoxMessage.SelectionStart = m_iCaretPosition;

                // 버튼 클릭 횟수 기록
                m_iTotalButtonCountExceptBackSpace = m_iTotalButtonCountExceptBackSpace + 1;

                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonQ", "click", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }
        }               

        private void buttonW_MouseDown(object sender, MouseEventArgs e)
        {
            m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

            m_bMouseDownFlag = true;

            // 기록 저장 (이벤트, 시간, 시간차 등)
            m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonW", "down", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
            m_eventTime = DateTime.Now;
        }

        private void buttonW_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true)
            {
                // 버튼을 포함하는 패널이 영역을 벗어나는지 확인
                int iRet = CheckButtonGroupPanelOutsideBoundary();
                if (iRet == 1 || iRet == 2 || iRet == 3 || iRet == 4)
                    return;

                m_endPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                int iDiffX = (m_endPoint.X - m_startPoint.X) * m_iCDGain;
                int iDiffY = (m_endPoint.Y - m_startPoint.Y) * m_iCDGain;

                // 새로운 마우스 위치 전역변수에 계산
                Point newPanelPos = new Point(panelButtonGroup.Location.X + iDiffX, panelButtonGroup.Location.Y + iDiffY);

                // 새로운 마우스 시작 좌표 저장
                m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                // 새로운 마우스 위치를 통해 버튼 그룹 이동
                panelButtonGroup.Location = newPanelPos;

                Invalidate();

                m_bMouseMoveFlag = true;
            }
        }

        private void buttonW_MouseUp(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true && m_bMouseMoveFlag == true)
            {
                m_iTotalButtonMoveCount = m_iTotalButtonMoveCount + 1;
                
                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonW", "move", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }

            m_bMouseDownFlag = false;
            m_bMouseMoveFlag = false;

            // 초점을 항상 OK 버튼으로 고정            
            textBoxMessage.Focus();
        }

        private void buttonW_Click(object sender, EventArgs e)
        {
            if (m_bMouseMoveFlag == false)
            {
                // Caret이 있는 위치 사이에 글자를 추가 처리
                textBoxMessage.Select(m_iCaretPosition, textBoxMessage.TextLength);
                string strTextAfter = textBoxMessage.SelectedText;

                textBoxMessage.Select(0, m_iCaretPosition);
                string strTextBefore = textBoxMessage.SelectedText;
                strTextBefore = strTextBefore + buttonW.Text;

                string strModifiedText = strTextBefore + strTextAfter;

                textBoxMessage.Text = strModifiedText;

                // 입력된 글자와 입력해야 하는 글자가 다르면 빨갛게 표시
                CheckCharacterBetweenInputAndTarget(); 

                m_iCaretPosition = m_iCaretPosition + 1;

                textBoxMessage.SelectionStart = m_iCaretPosition;

                // 버튼 클릭 횟수 기록
                m_iTotalButtonCountExceptBackSpace = m_iTotalButtonCountExceptBackSpace + 1;

                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonW", "click", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }
        }               

        private void buttonE_MouseDown(object sender, MouseEventArgs e)
        {
            m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

            m_bMouseDownFlag = true;

            // 기록 저장 (이벤트, 시간, 시간차 등)
            m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonE", "down", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
            m_eventTime = DateTime.Now;
        }

        private void buttonE_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true)
            {
                // 버튼을 포함하는 패널이 영역을 벗어나는지 확인
                int iRet = CheckButtonGroupPanelOutsideBoundary();
                if (iRet == 1 || iRet == 2 || iRet == 3 || iRet == 4)
                    return;

                m_endPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                int iDiffX = (m_endPoint.X - m_startPoint.X) * m_iCDGain;
                int iDiffY = (m_endPoint.Y - m_startPoint.Y) * m_iCDGain;

                // 새로운 마우스 위치 전역변수에 계산
                Point newPanelPos = new Point(panelButtonGroup.Location.X + iDiffX, panelButtonGroup.Location.Y + iDiffY);

                // 새로운 마우스 시작 좌표 저장
                m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                // 새로운 마우스 위치를 통해 버튼 그룹 이동
                panelButtonGroup.Location = newPanelPos;

                Invalidate();

                m_bMouseMoveFlag = true;
            }
        }

        private void buttonE_MouseUp(object sender, MouseEventArgs e)
        {            
            if (m_bMouseDownFlag == true && m_bMouseMoveFlag == true)
            {
                m_iTotalButtonMoveCount = m_iTotalButtonMoveCount + 1;
                
                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonE", "move", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }

            m_bMouseDownFlag = false;
            m_bMouseMoveFlag = false;

            // 초점을 항상 OK 버튼으로 고정            
            textBoxMessage.Focus();
        }

        private void buttonE_Click(object sender, EventArgs e)
        {
            if (m_bMouseMoveFlag == false)
            {
                // Caret이 있는 위치 사이에 글자를 추가 처리
                textBoxMessage.Select(m_iCaretPosition, textBoxMessage.TextLength);
                string strTextAfter = textBoxMessage.SelectedText;

                textBoxMessage.Select(0, m_iCaretPosition);
                string strTextBefore = textBoxMessage.SelectedText;
                strTextBefore = strTextBefore + buttonE.Text;

                string strModifiedText = strTextBefore + strTextAfter;

                textBoxMessage.Text = strModifiedText;

                // 입력된 글자와 입력해야 하는 글자가 다르면 빨갛게 표시
                CheckCharacterBetweenInputAndTarget(); 

                m_iCaretPosition = m_iCaretPosition + 1;

                textBoxMessage.SelectionStart = m_iCaretPosition;

                // 버튼 클릭 횟수 기록
                m_iTotalButtonCountExceptBackSpace = m_iTotalButtonCountExceptBackSpace + 1;

                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonE", "click", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }
        }

        private void buttonR_MouseDown(object sender, MouseEventArgs e)
        {
            m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

            m_bMouseDownFlag = true;

            // 기록 저장 (이벤트, 시간, 시간차 등)
            m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonR", "down", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
            m_eventTime = DateTime.Now;
        }

        private void buttonR_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true)
            {
                // 버튼을 포함하는 패널이 영역을 벗어나는지 확인
                int iRet = CheckButtonGroupPanelOutsideBoundary();
                if (iRet == 1 || iRet == 2 || iRet == 3 || iRet == 4)
                    return;

                m_endPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                int iDiffX = (m_endPoint.X - m_startPoint.X) * m_iCDGain;
                int iDiffY = (m_endPoint.Y - m_startPoint.Y) * m_iCDGain;

                // 새로운 마우스 위치 전역변수에 계산
                Point newPanelPos = new Point(panelButtonGroup.Location.X + iDiffX, panelButtonGroup.Location.Y + iDiffY);

                // 새로운 마우스 시작 좌표 저장
                m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                // 새로운 마우스 위치를 통해 버튼 그룹 이동
                panelButtonGroup.Location = newPanelPos;

                Invalidate();

                m_bMouseMoveFlag = true;
            }
        }

        private void buttonR_MouseUp(object sender, MouseEventArgs e)
        {            
            if (m_bMouseDownFlag == true && m_bMouseMoveFlag == true)
            {
                m_iTotalButtonMoveCount = m_iTotalButtonMoveCount + 1;
                
                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonR", "move", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }

            m_bMouseDownFlag = false;
            m_bMouseMoveFlag = false;
            
            // 초점을 항상 OK 버튼으로 고정            
            textBoxMessage.Focus();
        }

        private void buttonR_Click(object sender, EventArgs e)
        {
            if (m_bMouseMoveFlag == false)
            {
                // Caret이 있는 위치 사이에 글자를 추가 처리
                textBoxMessage.Select(m_iCaretPosition, textBoxMessage.TextLength);
                string strTextAfter = textBoxMessage.SelectedText;

                textBoxMessage.Select(0, m_iCaretPosition);
                string strTextBefore = textBoxMessage.SelectedText;
                strTextBefore = strTextBefore + buttonR.Text;

                string strModifiedText = strTextBefore + strTextAfter;

                textBoxMessage.Text = strModifiedText;

                // 입력된 글자와 입력해야 하는 글자가 다르면 빨갛게 표시
                CheckCharacterBetweenInputAndTarget(); 

                m_iCaretPosition = m_iCaretPosition + 1;

                textBoxMessage.SelectionStart = m_iCaretPosition;

                // 버튼 클릭 횟수 기록
                m_iTotalButtonCountExceptBackSpace = m_iTotalButtonCountExceptBackSpace + 1;

                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonR", "click", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }
        }                                      

        private void buttonT_MouseDown(object sender, MouseEventArgs e)
        {
            m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

            m_bMouseDownFlag = true;

            // 기록 저장 (이벤트, 시간, 시간차 등)
            m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonT", "down", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
            m_eventTime = DateTime.Now;
        }

        private void buttonT_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true)
            {
                // 버튼을 포함하는 패널이 영역을 벗어나는지 확인
                int iRet = CheckButtonGroupPanelOutsideBoundary();
                if (iRet == 1 || iRet == 2 || iRet == 3 || iRet == 4)
                    return;

                m_endPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                int iDiffX = (m_endPoint.X - m_startPoint.X) * m_iCDGain;
                int iDiffY = (m_endPoint.Y - m_startPoint.Y) * m_iCDGain;

                // 새로운 마우스 위치 전역변수에 계산
                Point newPanelPos = new Point(panelButtonGroup.Location.X + iDiffX, panelButtonGroup.Location.Y + iDiffY);

                // 새로운 마우스 시작 좌표 저장
                m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                // 새로운 마우스 위치를 통해 버튼 그룹 이동
                panelButtonGroup.Location = newPanelPos;

                Invalidate();

                m_bMouseMoveFlag = true;
            }
        }

        private void buttonT_MouseUp(object sender, MouseEventArgs e)
        {            
            if (m_bMouseDownFlag == true && m_bMouseMoveFlag == true)
            {
                m_iTotalButtonMoveCount = m_iTotalButtonMoveCount + 1;
                
                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonT", "move", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }

            m_bMouseDownFlag = false;
            m_bMouseMoveFlag = false;

            // 초점을 항상 OK 버튼으로 고정            
            textBoxMessage.Focus();
        }

        private void buttonT_Click(object sender, EventArgs e)
        {
            if (m_bMouseMoveFlag == false)
            {
                // Caret이 있는 위치 사이에 글자를 추가 처리
                textBoxMessage.Select(m_iCaretPosition, textBoxMessage.TextLength);
                string strTextAfter = textBoxMessage.SelectedText;

                textBoxMessage.Select(0, m_iCaretPosition);
                string strTextBefore = textBoxMessage.SelectedText;
                strTextBefore = strTextBefore + buttonT.Text;

                string strModifiedText = strTextBefore + strTextAfter;

                textBoxMessage.Text = strModifiedText;

                // 입력된 글자와 입력해야 하는 글자가 다르면 빨갛게 표시
                CheckCharacterBetweenInputAndTarget(); 

                m_iCaretPosition = m_iCaretPosition + 1;

                textBoxMessage.SelectionStart = m_iCaretPosition;

                // 버튼 클릭 횟수 기록
                m_iTotalButtonCountExceptBackSpace = m_iTotalButtonCountExceptBackSpace + 1;

                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonT", "click", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }
        }        

        private void buttonY_MouseDown(object sender, MouseEventArgs e)
        {
            m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

            m_bMouseDownFlag = true;

            // 기록 저장 (이벤트, 시간, 시간차 등)
            m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonY", "down", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
            m_eventTime = DateTime.Now;
        }

        private void buttonY_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true)
            {
                // 버튼을 포함하는 패널이 영역을 벗어나는지 확인
                int iRet = CheckButtonGroupPanelOutsideBoundary();
                if (iRet == 1 || iRet == 2 || iRet == 3 || iRet == 4)
                    return;

                m_endPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                int iDiffX = (m_endPoint.X - m_startPoint.X) * m_iCDGain;
                int iDiffY = (m_endPoint.Y - m_startPoint.Y) * m_iCDGain;

                // 새로운 마우스 위치 전역변수에 계산
                Point newPanelPos = new Point(panelButtonGroup.Location.X + iDiffX, panelButtonGroup.Location.Y + iDiffY);

                // 새로운 마우스 시작 좌표 저장
                m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                // 새로운 마우스 위치를 통해 버튼 그룹 이동
                panelButtonGroup.Location = newPanelPos;

                Invalidate();

                m_bMouseMoveFlag = true;
            }
        }

        private void buttonY_MouseUp(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true && m_bMouseMoveFlag == true)
            {
                m_iTotalButtonMoveCount = m_iTotalButtonMoveCount + 1;
                
                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonY", "move", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }

            m_bMouseDownFlag = false;
            m_bMouseMoveFlag = false;

            // 초점을 항상 OK 버튼으로 고정            
            textBoxMessage.Focus();
        }

        private void buttonY_Click(object sender, EventArgs e)
        {
            if (m_bMouseMoveFlag == false)
            {
                // Caret이 있는 위치 사이에 글자를 추가 처리
                textBoxMessage.Select(m_iCaretPosition, textBoxMessage.TextLength);
                string strTextAfter = textBoxMessage.SelectedText;

                textBoxMessage.Select(0, m_iCaretPosition);
                string strTextBefore = textBoxMessage.SelectedText;
                strTextBefore = strTextBefore + buttonY.Text;

                string strModifiedText = strTextBefore + strTextAfter;

                textBoxMessage.Text = strModifiedText;

                // 입력된 글자와 입력해야 하는 글자가 다르면 빨갛게 표시
                CheckCharacterBetweenInputAndTarget(); 

                m_iCaretPosition = m_iCaretPosition + 1;

                textBoxMessage.SelectionStart = m_iCaretPosition;

                // 버튼 클릭 횟수 기록
                m_iTotalButtonCountExceptBackSpace = m_iTotalButtonCountExceptBackSpace + 1;

                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonY", "click", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }            
        }                

        private void buttonU_MouseDown(object sender, MouseEventArgs e)
        {
            m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

            m_bMouseDownFlag = true;

            // 기록 저장 (이벤트, 시간, 시간차 등)
            m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonU", "down", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
            m_eventTime = DateTime.Now;
        }

        private void buttonU_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true)
            {
                // 버튼을 포함하는 패널이 영역을 벗어나는지 확인
                int iRet = CheckButtonGroupPanelOutsideBoundary();
                if (iRet == 1 || iRet == 2 || iRet == 3 || iRet == 4)
                    return;

                m_endPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                int iDiffX = (m_endPoint.X - m_startPoint.X) * m_iCDGain;
                int iDiffY = (m_endPoint.Y - m_startPoint.Y) * m_iCDGain;

                // 새로운 마우스 위치 전역변수에 계산
                Point newPanelPos = new Point(panelButtonGroup.Location.X + iDiffX, panelButtonGroup.Location.Y + iDiffY);

                // 새로운 마우스 시작 좌표 저장
                m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                // 새로운 마우스 위치를 통해 버튼 그룹 이동
                panelButtonGroup.Location = newPanelPos;

                Invalidate();

                m_bMouseMoveFlag = true;
            }
        }

        private void buttonU_MouseUp(object sender, MouseEventArgs e)
        {            
            if (m_bMouseDownFlag == true && m_bMouseMoveFlag == true)
            {
                m_iTotalButtonMoveCount = m_iTotalButtonMoveCount + 1;
                
                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonU", "move", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }

            m_bMouseDownFlag = false;
            m_bMouseMoveFlag = false;

            // 초점을 항상 OK 버튼으로 고정            
            textBoxMessage.Focus();
        }

        private void buttonU_Click(object sender, EventArgs e)
        {
            if (m_bMouseMoveFlag == false)
            {
                // Caret이 있는 위치 사이에 글자를 추가 처리
                textBoxMessage.Select(m_iCaretPosition, textBoxMessage.TextLength);
                string strTextAfter = textBoxMessage.SelectedText;

                textBoxMessage.Select(0, m_iCaretPosition);
                string strTextBefore = textBoxMessage.SelectedText;
                strTextBefore = strTextBefore + buttonU.Text;

                string strModifiedText = strTextBefore + strTextAfter;

                textBoxMessage.Text = strModifiedText;

                // 입력된 글자와 입력해야 하는 글자가 다르면 빨갛게 표시
                CheckCharacterBetweenInputAndTarget(); 

                m_iCaretPosition = m_iCaretPosition + 1;

                textBoxMessage.SelectionStart = m_iCaretPosition;

                // 버튼 클릭 횟수 기록
                m_iTotalButtonCountExceptBackSpace = m_iTotalButtonCountExceptBackSpace + 1;

                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonU", "click", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }
        }        

        private void buttonI_MouseDown(object sender, MouseEventArgs e)
        {
            m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

            m_bMouseDownFlag = true;

            // 기록 저장 (이벤트, 시간, 시간차 등)
            m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonI", "down", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
            m_eventTime = DateTime.Now;
        }

        private void buttonI_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true)
            {
                // 버튼을 포함하는 패널이 영역을 벗어나는지 확인
                int iRet = CheckButtonGroupPanelOutsideBoundary();
                if (iRet == 1 || iRet == 2 || iRet == 3 || iRet == 4)
                    return;

                m_endPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                int iDiffX = (m_endPoint.X - m_startPoint.X) * m_iCDGain;
                int iDiffY = (m_endPoint.Y - m_startPoint.Y) * m_iCDGain;

                // 새로운 마우스 위치 전역변수에 계산
                Point newPanelPos = new Point(panelButtonGroup.Location.X + iDiffX, panelButtonGroup.Location.Y + iDiffY);

                // 새로운 마우스 시작 좌표 저장
                m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                // 새로운 마우스 위치를 통해 버튼 그룹 이동
                panelButtonGroup.Location = newPanelPos;

                Invalidate();

                m_bMouseMoveFlag = true;
            }
        }

        private void buttonI_MouseUp(object sender, MouseEventArgs e)
        {            
            if (m_bMouseDownFlag == true && m_bMouseMoveFlag == true)
            {
                m_iTotalButtonMoveCount = m_iTotalButtonMoveCount + 1;
                
                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonI", "move", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }

            m_bMouseDownFlag = false;
            m_bMouseMoveFlag = false;

            // 초점을 항상 OK 버튼으로 고정            
            textBoxMessage.Focus();
        }

        private void buttonI_Click(object sender, EventArgs e)
        {
            if (m_bMouseMoveFlag == false)
            {
                // Caret이 있는 위치 사이에 글자를 추가 처리
                textBoxMessage.Select(m_iCaretPosition, textBoxMessage.TextLength);
                string strTextAfter = textBoxMessage.SelectedText;

                textBoxMessage.Select(0, m_iCaretPosition);
                string strTextBefore = textBoxMessage.SelectedText;
                strTextBefore = strTextBefore + buttonI.Text;

                string strModifiedText = strTextBefore + strTextAfter;

                textBoxMessage.Text = strModifiedText;

                // 입력된 글자와 입력해야 하는 글자가 다르면 빨갛게 표시
                CheckCharacterBetweenInputAndTarget(); 

                m_iCaretPosition = m_iCaretPosition + 1;

                textBoxMessage.SelectionStart = m_iCaretPosition;

                // 버튼 클릭 횟수 기록
                m_iTotalButtonCountExceptBackSpace = m_iTotalButtonCountExceptBackSpace + 1;

                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonI", "click", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }
        }        

        private void buttonO_MouseDown(object sender, MouseEventArgs e)
        {
            m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

            m_bMouseDownFlag = true;

            // 기록 저장 (이벤트, 시간, 시간차 등)
            m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonO", "down", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
            m_eventTime = DateTime.Now;
        }

        private void buttonO_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true)
            {
                // 버튼을 포함하는 패널이 영역을 벗어나는지 확인
                int iRet = CheckButtonGroupPanelOutsideBoundary();
                if (iRet == 1 || iRet == 2 || iRet == 3 || iRet == 4)
                    return;

                m_endPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                int iDiffX = (m_endPoint.X - m_startPoint.X) * m_iCDGain;
                int iDiffY = (m_endPoint.Y - m_startPoint.Y) * m_iCDGain;

                // 새로운 마우스 위치 전역변수에 계산
                Point newPanelPos = new Point(panelButtonGroup.Location.X + iDiffX, panelButtonGroup.Location.Y + iDiffY);

                // 새로운 마우스 시작 좌표 저장
                m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                // 새로운 마우스 위치를 통해 버튼 그룹 이동
                panelButtonGroup.Location = newPanelPos;

                Invalidate();

                m_bMouseMoveFlag = true;
            }
        }

        private void buttonO_MouseUp(object sender, MouseEventArgs e)
        {            
            if (m_bMouseDownFlag == true && m_bMouseMoveFlag == true)
            {
                m_iTotalButtonMoveCount = m_iTotalButtonMoveCount + 1;
                
                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonO", "move", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }

            m_bMouseDownFlag = false;
            m_bMouseMoveFlag = false;

            // 초점을 항상 OK 버튼으로 고정            
            textBoxMessage.Focus();
        }

        private void buttonO_Click(object sender, EventArgs e)
        {
            if (m_bMouseMoveFlag == false)
            {
                // Caret이 있는 위치 사이에 글자를 추가 처리
                textBoxMessage.Select(m_iCaretPosition, textBoxMessage.TextLength);
                string strTextAfter = textBoxMessage.SelectedText;

                textBoxMessage.Select(0, m_iCaretPosition);
                string strTextBefore = textBoxMessage.SelectedText;
                strTextBefore = strTextBefore + buttonO.Text;

                string strModifiedText = strTextBefore + strTextAfter;

                textBoxMessage.Text = strModifiedText;

                // 입력된 글자와 입력해야 하는 글자가 다르면 빨갛게 표시
                CheckCharacterBetweenInputAndTarget(); 

                m_iCaretPosition = m_iCaretPosition + 1;

                textBoxMessage.SelectionStart = m_iCaretPosition;

                // 버튼 클릭 횟수 기록
                m_iTotalButtonCountExceptBackSpace = m_iTotalButtonCountExceptBackSpace + 1;

                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonO", "click", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }
        }        

        private void buttonP_MouseDown(object sender, MouseEventArgs e)
        {
            m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

            m_bMouseDownFlag = true;

            // 기록 저장 (이벤트, 시간, 시간차 등)
            m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonP", "down", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
            m_eventTime = DateTime.Now;
        }

        private void buttonP_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true)
            {
                // 버튼을 포함하는 패널이 영역을 벗어나는지 확인
                int iRet = CheckButtonGroupPanelOutsideBoundary();
                if (iRet == 1 || iRet == 2 || iRet == 3 || iRet == 4)
                    return;

                m_endPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                int iDiffX = (m_endPoint.X - m_startPoint.X) * m_iCDGain;
                int iDiffY = (m_endPoint.Y - m_startPoint.Y) * m_iCDGain;

                // 새로운 마우스 위치 전역변수에 계산
                Point newPanelPos = new Point(panelButtonGroup.Location.X + iDiffX, panelButtonGroup.Location.Y + iDiffY);

                // 새로운 마우스 시작 좌표 저장
                m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                // 새로운 마우스 위치를 통해 버튼 그룹 이동
                panelButtonGroup.Location = newPanelPos;

                Invalidate();

                m_bMouseMoveFlag = true;
            }
        }

        private void buttonP_MouseUp(object sender, MouseEventArgs e)
        {            
            if (m_bMouseDownFlag == true && m_bMouseMoveFlag == true)
            {
                m_iTotalButtonMoveCount = m_iTotalButtonMoveCount + 1;
                
                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonP", "move", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }

            m_bMouseDownFlag = false;
            m_bMouseMoveFlag = false;

            // 초점을 항상 OK 버튼으로 고정            
            textBoxMessage.Focus();
        }

        private void buttonP_Click(object sender, EventArgs e)
        {
            if (m_bMouseMoveFlag == false)
            {
                // Caret이 있는 위치 사이에 글자를 추가 처리
                textBoxMessage.Select(m_iCaretPosition, textBoxMessage.TextLength);
                string strTextAfter = textBoxMessage.SelectedText;

                textBoxMessage.Select(0, m_iCaretPosition);
                string strTextBefore = textBoxMessage.SelectedText;
                strTextBefore = strTextBefore + buttonP.Text;

                string strModifiedText = strTextBefore + strTextAfter;

                textBoxMessage.Text = strModifiedText;

                // 입력된 글자와 입력해야 하는 글자가 다르면 빨갛게 표시
                CheckCharacterBetweenInputAndTarget(); 

                m_iCaretPosition = m_iCaretPosition + 1;

                textBoxMessage.SelectionStart = m_iCaretPosition;

                // 버튼 클릭 횟수 기록
                m_iTotalButtonCountExceptBackSpace = m_iTotalButtonCountExceptBackSpace + 1;

                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonP", "click", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }
        }        

        private void buttonA_MouseDown(object sender, MouseEventArgs e)
        {
            m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

            m_bMouseDownFlag = true;

            // 기록 저장 (이벤트, 시간, 시간차 등)
            m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonA", "down", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
            m_eventTime = DateTime.Now;
        }

        private void buttonA_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true)
            {
                // 버튼을 포함하는 패널이 영역을 벗어나는지 확인
                int iRet = CheckButtonGroupPanelOutsideBoundary();
                if (iRet == 1 || iRet == 2 || iRet == 3 || iRet == 4)
                    return;

                m_endPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                int iDiffX = (m_endPoint.X - m_startPoint.X) * m_iCDGain;
                int iDiffY = (m_endPoint.Y - m_startPoint.Y) * m_iCDGain;

                // 새로운 마우스 위치 전역변수에 계산
                Point newPanelPos = new Point(panelButtonGroup.Location.X + iDiffX, panelButtonGroup.Location.Y + iDiffY);

                // 새로운 마우스 시작 좌표 저장
                m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                // 새로운 마우스 위치를 통해 버튼 그룹 이동
                panelButtonGroup.Location = newPanelPos;

                Invalidate();

                m_bMouseMoveFlag = true;
            }
        }

        private void buttonA_MouseUp(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true && m_bMouseMoveFlag == true)
            {
                m_iTotalButtonMoveCount = m_iTotalButtonMoveCount + 1;
                
                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonA", "move", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }

            m_bMouseDownFlag = false;
            m_bMouseMoveFlag = false;

            // 초점을 항상 OK 버튼으로 고정            
            textBoxMessage.Focus();
        }

        private void buttonA_Click(object sender, EventArgs e)
        {
            if (m_bMouseMoveFlag == false)
            {
                // Caret이 있는 위치 사이에 글자를 추가 처리
                textBoxMessage.Select(m_iCaretPosition, textBoxMessage.TextLength);
                string strTextAfter = textBoxMessage.SelectedText;

                textBoxMessage.Select(0, m_iCaretPosition);
                string strTextBefore = textBoxMessage.SelectedText;
                strTextBefore = strTextBefore + buttonA.Text;

                string strModifiedText = strTextBefore + strTextAfter;

                textBoxMessage.Text = strModifiedText;

                // 입력된 글자와 입력해야 하는 글자가 다르면 빨갛게 표시
                CheckCharacterBetweenInputAndTarget(); 

                m_iCaretPosition = m_iCaretPosition + 1;

                textBoxMessage.SelectionStart = m_iCaretPosition;

                // 버튼 클릭 횟수 기록
                m_iTotalButtonCountExceptBackSpace = m_iTotalButtonCountExceptBackSpace + 1;

                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonA", "click", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }            
        }        

        private void buttonS_MouseDown(object sender, MouseEventArgs e)
        {
            m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

            m_bMouseDownFlag = true;

            // 기록 저장 (이벤트, 시간, 시간차 등)
            m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonS", "down", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
            m_eventTime = DateTime.Now;
        }

        private void buttonS_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true)
            {
                // 버튼을 포함하는 패널이 영역을 벗어나는지 확인
                int iRet = CheckButtonGroupPanelOutsideBoundary();
                if (iRet == 1 || iRet == 2 || iRet == 3 || iRet == 4)
                    return;

                m_endPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                int iDiffX = (m_endPoint.X - m_startPoint.X) * m_iCDGain;
                int iDiffY = (m_endPoint.Y - m_startPoint.Y) * m_iCDGain;

                // 새로운 마우스 위치 전역변수에 계산
                Point newPanelPos = new Point(panelButtonGroup.Location.X + iDiffX, panelButtonGroup.Location.Y + iDiffY);

                // 새로운 마우스 시작 좌표 저장
                m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                // 새로운 마우스 위치를 통해 버튼 그룹 이동
                panelButtonGroup.Location = newPanelPos;

                Invalidate();

                m_bMouseMoveFlag = true;
            }
        }
        
        private void buttonS_MouseUp(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true && m_bMouseMoveFlag == true)
            {
                m_iTotalButtonMoveCount = m_iTotalButtonMoveCount + 1;
                
                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonS", "move", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }

            m_bMouseDownFlag = false;
            m_bMouseMoveFlag = false;

            // 초점을 항상 OK 버튼으로 고정            
            textBoxMessage.Focus();
        }

        private void buttonS_Click(object sender, EventArgs e)
        {
            if (m_bMouseMoveFlag == false)
            {
                // Caret이 있는 위치 사이에 글자를 추가 처리
                textBoxMessage.Select(m_iCaretPosition, textBoxMessage.TextLength);
                string strTextAfter = textBoxMessage.SelectedText;

                textBoxMessage.Select(0, m_iCaretPosition);
                string strTextBefore = textBoxMessage.SelectedText;
                strTextBefore = strTextBefore + buttonS.Text;

                string strModifiedText = strTextBefore + strTextAfter;

                textBoxMessage.Text = strModifiedText;

                // 입력된 글자와 입력해야 하는 글자가 다르면 빨갛게 표시
                CheckCharacterBetweenInputAndTarget(); 

                m_iCaretPosition = m_iCaretPosition + 1;

                textBoxMessage.SelectionStart = m_iCaretPosition;

                // 버튼 클릭 횟수 기록
                m_iTotalButtonCountExceptBackSpace = m_iTotalButtonCountExceptBackSpace + 1;

                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonS", "click", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }
        }        

        private void buttonD_MouseDown(object sender, MouseEventArgs e)
        {
            m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

            m_bMouseDownFlag = true;

            // 기록 저장 (이벤트, 시간, 시간차 등)
            m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonD", "down", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
            m_eventTime = DateTime.Now;
        }

        private void buttonD_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true)
            {
                // 버튼을 포함하는 패널이 영역을 벗어나는지 확인
                int iRet = CheckButtonGroupPanelOutsideBoundary();
                if (iRet == 1 || iRet == 2 || iRet == 3 || iRet == 4)
                    return;

                m_endPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                int iDiffX = (m_endPoint.X - m_startPoint.X) * m_iCDGain;
                int iDiffY = (m_endPoint.Y - m_startPoint.Y) * m_iCDGain;

                // 새로운 마우스 위치 전역변수에 계산
                Point newPanelPos = new Point(panelButtonGroup.Location.X + iDiffX, panelButtonGroup.Location.Y + iDiffY);

                // 새로운 마우스 시작 좌표 저장
                m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                // 새로운 마우스 위치를 통해 버튼 그룹 이동
                panelButtonGroup.Location = newPanelPos;

                Invalidate();

                m_bMouseMoveFlag = true;
            }
        }

        private void buttonD_MouseUp(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true && m_bMouseMoveFlag == true)
            {
                m_iTotalButtonMoveCount = m_iTotalButtonMoveCount + 1;
                
                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonD", "move", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }

            m_bMouseDownFlag = false;
            m_bMouseMoveFlag = false;

            // 초점을 항상 OK 버튼으로 고정            
            textBoxMessage.Focus();
        }

        private void buttonD_Click(object sender, EventArgs e)
        {
            if (m_bMouseMoveFlag == false)
            {
                // Caret이 있는 위치 사이에 글자를 추가 처리
                textBoxMessage.Select(m_iCaretPosition, textBoxMessage.TextLength);
                string strTextAfter = textBoxMessage.SelectedText;

                textBoxMessage.Select(0, m_iCaretPosition);
                string strTextBefore = textBoxMessage.SelectedText;
                strTextBefore = strTextBefore + buttonD.Text;

                string strModifiedText = strTextBefore + strTextAfter;

                textBoxMessage.Text = strModifiedText;

                // 입력된 글자와 입력해야 하는 글자가 다르면 빨갛게 표시
                CheckCharacterBetweenInputAndTarget(); 

                m_iCaretPosition = m_iCaretPosition + 1;

                textBoxMessage.SelectionStart = m_iCaretPosition;

                // 버튼 클릭 횟수 기록
                m_iTotalButtonCountExceptBackSpace = m_iTotalButtonCountExceptBackSpace + 1;

                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonD", "click", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }
        }

        private void buttonF_MouseDown(object sender, MouseEventArgs e)
        {
            m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

            m_bMouseDownFlag = true;

            // 기록 저장 (이벤트, 시간, 시간차 등)
            m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonF", "down", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
            m_eventTime = DateTime.Now;
        }

        private void buttonF_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true)
            {
                // 버튼을 포함하는 패널이 영역을 벗어나는지 확인
                int iRet = CheckButtonGroupPanelOutsideBoundary();
                if (iRet == 1 || iRet == 2 || iRet == 3 || iRet == 4)
                    return;

                m_endPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                int iDiffX = (m_endPoint.X - m_startPoint.X) * m_iCDGain;
                int iDiffY = (m_endPoint.Y - m_startPoint.Y) * m_iCDGain;

                // 새로운 마우스 위치 전역변수에 계산
                Point newPanelPos = new Point(panelButtonGroup.Location.X + iDiffX, panelButtonGroup.Location.Y + iDiffY);

                // 새로운 마우스 시작 좌표 저장
                m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                // 새로운 마우스 위치를 통해 버튼 그룹 이동
                panelButtonGroup.Location = newPanelPos;

                Invalidate();

                m_bMouseMoveFlag = true;
            }
        }

        private void buttonF_MouseUp(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true && m_bMouseMoveFlag == true)
            {
                m_iTotalButtonMoveCount = m_iTotalButtonMoveCount + 1;
                
                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonF", "move", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }

            m_bMouseDownFlag = false;
            m_bMouseMoveFlag = false;

            // 초점을 항상 OK 버튼으로 고정            
            textBoxMessage.Focus();
        }

        private void buttonF_Click(object sender, EventArgs e)
        {
            if (m_bMouseMoveFlag == false)
            {
                // Caret이 있는 위치 사이에 글자를 추가 처리
                textBoxMessage.Select(m_iCaretPosition, textBoxMessage.TextLength);
                string strTextAfter = textBoxMessage.SelectedText;

                textBoxMessage.Select(0, m_iCaretPosition);
                string strTextBefore = textBoxMessage.SelectedText;
                strTextBefore = strTextBefore + buttonF.Text;

                string strModifiedText = strTextBefore + strTextAfter;

                textBoxMessage.Text = strModifiedText;

                // 입력된 글자와 입력해야 하는 글자가 다르면 빨갛게 표시
                CheckCharacterBetweenInputAndTarget(); 

                m_iCaretPosition = m_iCaretPosition + 1;

                textBoxMessage.SelectionStart = m_iCaretPosition;

                // 버튼 클릭 횟수 기록
                m_iTotalButtonCountExceptBackSpace = m_iTotalButtonCountExceptBackSpace + 1;

                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonF", "click", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }
        }       

        private void buttonG_MouseDown(object sender, MouseEventArgs e)
        {
            m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

            m_bMouseDownFlag = true;

            // 기록 저장 (이벤트, 시간, 시간차 등)
            m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonG", "down", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
            m_eventTime = DateTime.Now;
        }

        private void buttonG_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true)
            {
                // 버튼을 포함하는 패널이 영역을 벗어나는지 확인
                int iRet = CheckButtonGroupPanelOutsideBoundary();
                if (iRet == 1 || iRet == 2 || iRet == 3 || iRet == 4)
                    return;

                m_endPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                int iDiffX = (m_endPoint.X - m_startPoint.X) * m_iCDGain;
                int iDiffY = (m_endPoint.Y - m_startPoint.Y) * m_iCDGain;

                // 새로운 마우스 위치 전역변수에 계산
                Point newPanelPos = new Point(panelButtonGroup.Location.X + iDiffX, panelButtonGroup.Location.Y + iDiffY);

                // 새로운 마우스 시작 좌표 저장
                m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                // 새로운 마우스 위치를 통해 버튼 그룹 이동
                panelButtonGroup.Location = newPanelPos;

                Invalidate();

                m_bMouseMoveFlag = true;
            }
        }

        private void buttonG_MouseUp(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true && m_bMouseMoveFlag == true)
            {
                m_iTotalButtonMoveCount = m_iTotalButtonMoveCount + 1;
                
                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonG", "move", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }

            m_bMouseDownFlag = false;
            m_bMouseMoveFlag = false;

            // 초점을 항상 OK 버튼으로 고정            
            textBoxMessage.Focus();
        }

        private void buttonG_Click(object sender, EventArgs e)
        {
            if (m_bMouseMoveFlag == false)
            {
                // Caret이 있는 위치 사이에 글자를 추가 처리
                textBoxMessage.Select(m_iCaretPosition, textBoxMessage.TextLength);
                string strTextAfter = textBoxMessage.SelectedText;

                textBoxMessage.Select(0, m_iCaretPosition);
                string strTextBefore = textBoxMessage.SelectedText;
                strTextBefore = strTextBefore + buttonG.Text;

                string strModifiedText = strTextBefore + strTextAfter;

                textBoxMessage.Text = strModifiedText;

                // 입력된 글자와 입력해야 하는 글자가 다르면 빨갛게 표시
                CheckCharacterBetweenInputAndTarget(); 

                m_iCaretPosition = m_iCaretPosition + 1;

                textBoxMessage.SelectionStart = m_iCaretPosition;

                // 버튼 클릭 횟수 기록
                m_iTotalButtonCountExceptBackSpace = m_iTotalButtonCountExceptBackSpace + 1;

                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonG", "click", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }
        }        

        private void buttonH_MouseDown(object sender, MouseEventArgs e)
        {
            m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

            m_bMouseDownFlag = true;

            // 기록 저장 (이벤트, 시간, 시간차 등)
            m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonH", "down", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
            m_eventTime = DateTime.Now;
        }

        private void buttonH_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true)
            {
                // 버튼을 포함하는 패널이 영역을 벗어나는지 확인
                int iRet = CheckButtonGroupPanelOutsideBoundary();
                if (iRet == 1 || iRet == 2 || iRet == 3 || iRet == 4)
                    return;

                m_endPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                int iDiffX = (m_endPoint.X - m_startPoint.X) * m_iCDGain;
                int iDiffY = (m_endPoint.Y - m_startPoint.Y) * m_iCDGain;

                // 새로운 마우스 위치 전역변수에 계산
                Point newPanelPos = new Point(panelButtonGroup.Location.X + iDiffX, panelButtonGroup.Location.Y + iDiffY);

                // 새로운 마우스 시작 좌표 저장
                m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                // 새로운 마우스 위치를 통해 버튼 그룹 이동
                panelButtonGroup.Location = newPanelPos;

                Invalidate();

                m_bMouseMoveFlag = true;
            }
        }

        private void buttonH_MouseUp(object sender, MouseEventArgs e)
        {            
            if (m_bMouseDownFlag == true && m_bMouseMoveFlag == true)
            {
                m_iTotalButtonMoveCount = m_iTotalButtonMoveCount + 1;
                
                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonH", "move", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }

            m_bMouseDownFlag = false;
            m_bMouseMoveFlag = false;

            // 초점을 항상 OK 버튼으로 고정            
            textBoxMessage.Focus();
        }

        private void buttonH_Click(object sender, EventArgs e)
        {
            if (m_bMouseMoveFlag == false)
            {
                // Caret이 있는 위치 사이에 글자를 추가 처리
                textBoxMessage.Select(m_iCaretPosition, textBoxMessage.TextLength);
                string strTextAfter = textBoxMessage.SelectedText;

                textBoxMessage.Select(0, m_iCaretPosition);
                string strTextBefore = textBoxMessage.SelectedText;
                strTextBefore = strTextBefore + buttonH.Text;

                string strModifiedText = strTextBefore + strTextAfter;

                textBoxMessage.Text = strModifiedText;

                // 입력된 글자와 입력해야 하는 글자가 다르면 빨갛게 표시
                CheckCharacterBetweenInputAndTarget(); 

                m_iCaretPosition = m_iCaretPosition + 1;

                textBoxMessage.SelectionStart = m_iCaretPosition;

                // 버튼 클릭 횟수 기록
                m_iTotalButtonCountExceptBackSpace = m_iTotalButtonCountExceptBackSpace + 1;

                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonH", "click", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }
        }        

        private void buttonJ_MouseDown(object sender, MouseEventArgs e)
        {
            m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

            m_bMouseDownFlag = true;

            // 기록 저장 (이벤트, 시간, 시간차 등)
            m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonJ", "down", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
            m_eventTime = DateTime.Now;
        }

        private void buttonJ_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true)
            {
                // 버튼을 포함하는 패널이 영역을 벗어나는지 확인
                int iRet = CheckButtonGroupPanelOutsideBoundary();
                if (iRet == 1 || iRet == 2 || iRet == 3 || iRet == 4)
                    return;

                m_endPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                int iDiffX = (m_endPoint.X - m_startPoint.X) * m_iCDGain;
                int iDiffY = (m_endPoint.Y - m_startPoint.Y) * m_iCDGain;

                // 새로운 마우스 위치 전역변수에 계산
                Point newPanelPos = new Point(panelButtonGroup.Location.X + iDiffX, panelButtonGroup.Location.Y + iDiffY);

                // 새로운 마우스 시작 좌표 저장
                m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                // 새로운 마우스 위치를 통해 버튼 그룹 이동
                panelButtonGroup.Location = newPanelPos;

                Invalidate();

                m_bMouseMoveFlag = true;
            }
        }

        private void buttonJ_MouseUp(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true && m_bMouseMoveFlag == true)
            {
                m_iTotalButtonMoveCount = m_iTotalButtonMoveCount + 1;
                
                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonJ", "move", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }

            m_bMouseDownFlag = false;
            m_bMouseMoveFlag = false;

            // 초점을 항상 OK 버튼으로 고정            
            textBoxMessage.Focus();
        }

        private void buttonJ_Click(object sender, EventArgs e)
        {
            if (m_bMouseMoveFlag == false)
            {
                // Caret이 있는 위치 사이에 글자를 추가 처리
                textBoxMessage.Select(m_iCaretPosition, textBoxMessage.TextLength);
                string strTextAfter = textBoxMessage.SelectedText;

                textBoxMessage.Select(0, m_iCaretPosition);
                string strTextBefore = textBoxMessage.SelectedText;
                strTextBefore = strTextBefore + buttonJ.Text;

                string strModifiedText = strTextBefore + strTextAfter;

                textBoxMessage.Text = strModifiedText;

                // 입력된 글자와 입력해야 하는 글자가 다르면 빨갛게 표시
                CheckCharacterBetweenInputAndTarget(); 

                m_iCaretPosition = m_iCaretPosition + 1;

                textBoxMessage.SelectionStart = m_iCaretPosition;

                // 버튼 클릭 횟수 기록
                m_iTotalButtonCountExceptBackSpace = m_iTotalButtonCountExceptBackSpace + 1;

                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonJ", "click", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }            
        }        

        private void buttonK_MouseDown(object sender, MouseEventArgs e)
        {
            m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

            m_bMouseDownFlag = true;

            // 기록 저장 (이벤트, 시간, 시간차 등)
            m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonK", "down", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
            m_eventTime = DateTime.Now;
        }

        private void buttonK_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true)
            {
                // 버튼을 포함하는 패널이 영역을 벗어나는지 확인
                int iRet = CheckButtonGroupPanelOutsideBoundary();
                if (iRet == 1 || iRet == 2 || iRet == 3 || iRet == 4)
                    return;

                m_endPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                int iDiffX = (m_endPoint.X - m_startPoint.X) * m_iCDGain;
                int iDiffY = (m_endPoint.Y - m_startPoint.Y) * m_iCDGain;

                // 새로운 마우스 위치 전역변수에 계산
                Point newPanelPos = new Point(panelButtonGroup.Location.X + iDiffX, panelButtonGroup.Location.Y + iDiffY);

                // 새로운 마우스 시작 좌표 저장
                m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                // 새로운 마우스 위치를 통해 버튼 그룹 이동
                panelButtonGroup.Location = newPanelPos;

                Invalidate();

                m_bMouseMoveFlag = true;
            }
        }

        private void buttonK_MouseUp(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true && m_bMouseMoveFlag == true)
            {
                m_iTotalButtonMoveCount = m_iTotalButtonMoveCount + 1;
                
                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonK", "move", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }

            m_bMouseDownFlag = false;
            m_bMouseMoveFlag = false;

            // 초점을 항상 OK 버튼으로 고정            
            textBoxMessage.Focus();
        }

        private void buttonK_Click(object sender, EventArgs e)
        {
            if (m_bMouseMoveFlag == false)
            {
                // Caret이 있는 위치 사이에 글자를 추가 처리
                textBoxMessage.Select(m_iCaretPosition, textBoxMessage.TextLength);
                string strTextAfter = textBoxMessage.SelectedText;

                textBoxMessage.Select(0, m_iCaretPosition);
                string strTextBefore = textBoxMessage.SelectedText;
                strTextBefore = strTextBefore + buttonK.Text;

                string strModifiedText = strTextBefore + strTextAfter;

                textBoxMessage.Text = strModifiedText;

                // 입력된 글자와 입력해야 하는 글자가 다르면 빨갛게 표시
                CheckCharacterBetweenInputAndTarget(); 

                m_iCaretPosition = m_iCaretPosition + 1;

                textBoxMessage.SelectionStart = m_iCaretPosition;

                // 버튼 클릭 횟수 기록
                m_iTotalButtonCountExceptBackSpace = m_iTotalButtonCountExceptBackSpace + 1;

                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonK", "click", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }
        }        

        private void buttonL_MouseDown(object sender, MouseEventArgs e)
        {
            m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

            m_bMouseDownFlag = true;

            // 기록 저장 (이벤트, 시간, 시간차 등)
            m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonL", "down", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
            m_eventTime = DateTime.Now;
        }

        private void buttonL_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true)
            {
                // 버튼을 포함하는 패널이 영역을 벗어나는지 확인
                int iRet = CheckButtonGroupPanelOutsideBoundary();
                if (iRet == 1 || iRet == 2 || iRet == 3 || iRet == 4)
                    return;

                m_endPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                int iDiffX = (m_endPoint.X - m_startPoint.X) * m_iCDGain;
                int iDiffY = (m_endPoint.Y - m_startPoint.Y) * m_iCDGain;

                // 새로운 마우스 위치 전역변수에 계산
                Point newPanelPos = new Point(panelButtonGroup.Location.X + iDiffX, panelButtonGroup.Location.Y + iDiffY);

                // 새로운 마우스 시작 좌표 저장
                m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                // 새로운 마우스 위치를 통해 버튼 그룹 이동
                panelButtonGroup.Location = newPanelPos;

                Invalidate();

                m_bMouseMoveFlag = true;
            }
        }

        private void buttonL_MouseUp(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true && m_bMouseMoveFlag == true)
            {
                m_iTotalButtonMoveCount = m_iTotalButtonMoveCount + 1;
                
                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonL", "move", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }

            m_bMouseDownFlag = false;
            m_bMouseMoveFlag = false;

            // 초점을 항상 OK 버튼으로 고정            
            textBoxMessage.Focus();
        }

        private void buttonL_Click(object sender, EventArgs e)
        {
            if (m_bMouseMoveFlag == false)
            {
                // Caret이 있는 위치 사이에 글자를 추가 처리
                textBoxMessage.Select(m_iCaretPosition, textBoxMessage.TextLength);
                string strTextAfter = textBoxMessage.SelectedText;

                textBoxMessage.Select(0, m_iCaretPosition);
                string strTextBefore = textBoxMessage.SelectedText;
                strTextBefore = strTextBefore + buttonL.Text;

                string strModifiedText = strTextBefore + strTextAfter;

                textBoxMessage.Text = strModifiedText;

                // 입력된 글자와 입력해야 하는 글자가 다르면 빨갛게 표시
                CheckCharacterBetweenInputAndTarget(); 

                m_iCaretPosition = m_iCaretPosition + 1;

                textBoxMessage.SelectionStart = m_iCaretPosition;

                // 버튼 클릭 횟수 기록
                m_iTotalButtonCountExceptBackSpace = m_iTotalButtonCountExceptBackSpace + 1;

                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonL", "click", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }
        }        

        private void buttonZ_MouseDown(object sender, MouseEventArgs e)
        {
            m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

            m_bMouseDownFlag = true;

            // 기록 저장 (이벤트, 시간, 시간차 등)
            m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonZ", "down", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
            m_eventTime = DateTime.Now;
        }

        private void buttonZ_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true)
            {
                // 버튼을 포함하는 패널이 영역을 벗어나는지 확인
                int iRet = CheckButtonGroupPanelOutsideBoundary();
                if (iRet == 1 || iRet == 2 || iRet == 3 || iRet == 4)
                    return;

                m_endPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                int iDiffX = (m_endPoint.X - m_startPoint.X) * m_iCDGain;
                int iDiffY = (m_endPoint.Y - m_startPoint.Y) * m_iCDGain;

                // 새로운 마우스 위치 전역변수에 계산
                Point newPanelPos = new Point(panelButtonGroup.Location.X + iDiffX, panelButtonGroup.Location.Y + iDiffY);

                // 새로운 마우스 시작 좌표 저장
                m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                // 새로운 마우스 위치를 통해 버튼 그룹 이동
                panelButtonGroup.Location = newPanelPos;

                Invalidate();

                m_bMouseMoveFlag = true;
            }
        }

        private void buttonZ_MouseUp(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true && m_bMouseMoveFlag == true)
            {
                m_iTotalButtonMoveCount = m_iTotalButtonMoveCount + 1;

                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonZ", "move", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }

            m_bMouseDownFlag = false;
            m_bMouseMoveFlag = false;

            // 초점을 항상 OK 버튼으로 고정            
            textBoxMessage.Focus();
        }

        private void buttonZ_Click(object sender, EventArgs e)
        {
            if (m_bMouseMoveFlag == false)
            {
                // Caret이 있는 위치 사이에 글자를 추가 처리
                textBoxMessage.Select(m_iCaretPosition, textBoxMessage.TextLength);
                string strTextAfter = textBoxMessage.SelectedText;

                textBoxMessage.Select(0, m_iCaretPosition);
                string strTextBefore = textBoxMessage.SelectedText;
                strTextBefore = strTextBefore + buttonZ.Text;

                string strModifiedText = strTextBefore + strTextAfter;

                textBoxMessage.Text = strModifiedText;

                // 입력된 글자와 입력해야 하는 글자가 다르면 빨갛게 표시
                CheckCharacterBetweenInputAndTarget(); 

                m_iCaretPosition = m_iCaretPosition + 1;

                textBoxMessage.SelectionStart = m_iCaretPosition;

                // 버튼 클릭 횟수 기록
                m_iTotalButtonCountExceptBackSpace = m_iTotalButtonCountExceptBackSpace + 1;

                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonZ", "click", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }
        }        

        private void buttonX_MouseDown(object sender, MouseEventArgs e)
        {
            m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

            m_bMouseDownFlag = true;

            // 기록 저장 (이벤트, 시간, 시간차 등)
            m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonX", "down", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
            m_eventTime = DateTime.Now;
        }

        private void buttonX_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true)
            {
                // 버튼을 포함하는 패널이 영역을 벗어나는지 확인
                int iRet = CheckButtonGroupPanelOutsideBoundary();
                if (iRet == 1 || iRet == 2 || iRet == 3 || iRet == 4)
                    return;

                m_endPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                int iDiffX = (m_endPoint.X - m_startPoint.X) * m_iCDGain;
                int iDiffY = (m_endPoint.Y - m_startPoint.Y) * m_iCDGain;

                // 새로운 마우스 위치 전역변수에 계산
                Point newPanelPos = new Point(panelButtonGroup.Location.X + iDiffX, panelButtonGroup.Location.Y + iDiffY);

                // 새로운 마우스 시작 좌표 저장
                m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                // 새로운 마우스 위치를 통해 버튼 그룹 이동
                panelButtonGroup.Location = newPanelPos;

                Invalidate();

                m_bMouseMoveFlag = true;
            }
        }

        private void buttonX_MouseUp(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true && m_bMouseMoveFlag == true)
            {
                m_iTotalButtonMoveCount = m_iTotalButtonMoveCount + 1;

                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonX", "move", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }

            m_bMouseDownFlag = false;
            m_bMouseMoveFlag = false;

            // 초점을 항상 OK 버튼으로 고정            
            textBoxMessage.Focus();
        }

        private void buttonX_Click(object sender, EventArgs e)
        {
            if (m_bMouseMoveFlag == false)
            {
                // Caret이 있는 위치 사이에 글자를 추가 처리
                textBoxMessage.Select(m_iCaretPosition, textBoxMessage.TextLength);
                string strTextAfter = textBoxMessage.SelectedText;

                textBoxMessage.Select(0, m_iCaretPosition);
                string strTextBefore = textBoxMessage.SelectedText;
                strTextBefore = strTextBefore + buttonX.Text;

                string strModifiedText = strTextBefore + strTextAfter;

                textBoxMessage.Text = strModifiedText;

                // 입력된 글자와 입력해야 하는 글자가 다르면 빨갛게 표시
                CheckCharacterBetweenInputAndTarget(); 

                m_iCaretPosition = m_iCaretPosition + 1;

                textBoxMessage.SelectionStart = m_iCaretPosition;

                // 버튼 클릭 횟수 기록
                m_iTotalButtonCountExceptBackSpace = m_iTotalButtonCountExceptBackSpace + 1;

                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonX", "click", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }
        }

        private void buttonC_MouseDown(object sender, MouseEventArgs e)
        {
            m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

            m_bMouseDownFlag = true;

            // 기록 저장 (이벤트, 시간, 시간차 등)
            m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonC", "down", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
            m_eventTime = DateTime.Now;
        }

        private void buttonC_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true)
            {
                // 버튼을 포함하는 패널이 영역을 벗어나는지 확인
                int iRet = CheckButtonGroupPanelOutsideBoundary();
                if (iRet == 1 || iRet == 2 || iRet == 3 || iRet == 4)
                    return;

                m_endPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                int iDiffX = (m_endPoint.X - m_startPoint.X) * m_iCDGain;
                int iDiffY = (m_endPoint.Y - m_startPoint.Y) * m_iCDGain;

                // 새로운 마우스 위치 전역변수에 계산
                Point newPanelPos = new Point(panelButtonGroup.Location.X + iDiffX, panelButtonGroup.Location.Y + iDiffY);

                // 새로운 마우스 시작 좌표 저장
                m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                // 새로운 마우스 위치를 통해 버튼 그룹 이동
                panelButtonGroup.Location = newPanelPos;

                Invalidate();

                m_bMouseMoveFlag = true;
            }
        }

        private void buttonC_MouseUp(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true && m_bMouseMoveFlag == true)
            {
                m_iTotalButtonMoveCount = m_iTotalButtonMoveCount + 1;

                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonC", "move", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }

            m_bMouseDownFlag = false;
            m_bMouseMoveFlag = false;

            // 초점을 항상 OK 버튼으로 고정            
            textBoxMessage.Focus();
        }

        private void buttonC_Click(object sender, EventArgs e)
        {
            if (m_bMouseMoveFlag == false)
            {
                // Caret이 있는 위치 사이에 글자를 추가 처리
                textBoxMessage.Select(m_iCaretPosition, textBoxMessage.TextLength);
                string strTextAfter = textBoxMessage.SelectedText;

                textBoxMessage.Select(0, m_iCaretPosition);
                string strTextBefore = textBoxMessage.SelectedText;
                strTextBefore = strTextBefore + buttonC.Text;

                string strModifiedText = strTextBefore + strTextAfter;

                textBoxMessage.Text = strModifiedText;

                // 입력된 글자와 입력해야 하는 글자가 다르면 빨갛게 표시
                CheckCharacterBetweenInputAndTarget(); 

                m_iCaretPosition = m_iCaretPosition + 1;

                textBoxMessage.SelectionStart = m_iCaretPosition;

                // 버튼 클릭 횟수 기록
                m_iTotalButtonCountExceptBackSpace = m_iTotalButtonCountExceptBackSpace + 1;

                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonC", "click", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }
        }        
                
        private void buttonV_MouseDown(object sender, MouseEventArgs e)
        {
            m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

            m_bMouseDownFlag = true;

            // 기록 저장 (이벤트, 시간, 시간차 등)
            m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonV", "down", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
            m_eventTime = DateTime.Now;
        }

        private void buttonV_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true)
            {
                // 버튼을 포함하는 패널이 영역을 벗어나는지 확인
                int iRet = CheckButtonGroupPanelOutsideBoundary();
                if (iRet == 1 || iRet == 2 || iRet == 3 || iRet == 4)
                    return;

                m_endPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                int iDiffX = (m_endPoint.X - m_startPoint.X) * m_iCDGain;
                int iDiffY = (m_endPoint.Y - m_startPoint.Y) * m_iCDGain;

                // 새로운 마우스 위치 전역변수에 계산
                Point newPanelPos = new Point(panelButtonGroup.Location.X + iDiffX, panelButtonGroup.Location.Y + iDiffY);

                // 새로운 마우스 시작 좌표 저장
                m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                // 새로운 마우스 위치를 통해 버튼 그룹 이동
                panelButtonGroup.Location = newPanelPos;

                Invalidate();

                m_bMouseMoveFlag = true;
            }
        }

        private void buttonV_MouseUp(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true && m_bMouseMoveFlag == true)
            {
                m_iTotalButtonMoveCount = m_iTotalButtonMoveCount + 1;

                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonV", "move", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }

            m_bMouseDownFlag = false;
            m_bMouseMoveFlag = false;

            // 초점을 항상 OK 버튼으로 고정            
            textBoxMessage.Focus();
        }

        private void buttonV_Click(object sender, EventArgs e)
        {
            if (m_bMouseMoveFlag == false)
            {
                // Caret이 있는 위치 사이에 글자를 추가 처리
                textBoxMessage.Select(m_iCaretPosition, textBoxMessage.TextLength);
                string strTextAfter = textBoxMessage.SelectedText;

                textBoxMessage.Select(0, m_iCaretPosition);
                string strTextBefore = textBoxMessage.SelectedText;
                strTextBefore = strTextBefore + buttonV.Text;

                string strModifiedText = strTextBefore + strTextAfter;

                textBoxMessage.Text = strModifiedText;

                // 입력된 글자와 입력해야 하는 글자가 다르면 빨갛게 표시
                CheckCharacterBetweenInputAndTarget(); 

                m_iCaretPosition = m_iCaretPosition + 1;

                textBoxMessage.SelectionStart = m_iCaretPosition;

                // 버튼 클릭 횟수 기록
                m_iTotalButtonCountExceptBackSpace = m_iTotalButtonCountExceptBackSpace + 1;

                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonV", "click", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }
        }        

        private void buttonB_MouseDown(object sender, MouseEventArgs e)
        {
            m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

            m_bMouseDownFlag = true;

            // 기록 저장 (이벤트, 시간, 시간차 등)
            m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonB", "down", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
            m_eventTime = DateTime.Now;
        }

        private void buttonB_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true)
            {
                // 버튼을 포함하는 패널이 영역을 벗어나는지 확인
                int iRet = CheckButtonGroupPanelOutsideBoundary();
                if (iRet == 1 || iRet == 2 || iRet == 3 || iRet == 4)
                    return;

                m_endPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                int iDiffX = (m_endPoint.X - m_startPoint.X) * m_iCDGain;
                int iDiffY = (m_endPoint.Y - m_startPoint.Y) * m_iCDGain;

                // 새로운 마우스 위치 전역변수에 계산
                Point newPanelPos = new Point(panelButtonGroup.Location.X + iDiffX, panelButtonGroup.Location.Y + iDiffY);

                // 새로운 마우스 시작 좌표 저장
                m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                // 새로운 마우스 위치를 통해 버튼 그룹 이동
                panelButtonGroup.Location = newPanelPos;

                Invalidate();

                m_bMouseMoveFlag = true;
            }
        }

        private void buttonB_MouseUp(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true && m_bMouseMoveFlag == true)
            {
                m_iTotalButtonMoveCount = m_iTotalButtonMoveCount + 1;

                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonB", "move", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }

            m_bMouseDownFlag = false;
            m_bMouseMoveFlag = false;

            // 초점을 항상 OK 버튼으로 고정            
            textBoxMessage.Focus();
        }

        private void buttonB_Click(object sender, EventArgs e)
        {
            if (m_bMouseMoveFlag == false)
            {
                // Caret이 있는 위치 사이에 글자를 추가 처리
                textBoxMessage.Select(m_iCaretPosition, textBoxMessage.TextLength);
                string strTextAfter = textBoxMessage.SelectedText;

                textBoxMessage.Select(0, m_iCaretPosition);
                string strTextBefore = textBoxMessage.SelectedText;
                strTextBefore = strTextBefore + buttonB.Text;

                string strModifiedText = strTextBefore + strTextAfter;

                textBoxMessage.Text = strModifiedText;

                // 입력된 글자와 입력해야 하는 글자가 다르면 빨갛게 표시
                CheckCharacterBetweenInputAndTarget(); 

                m_iCaretPosition = m_iCaretPosition + 1;

                textBoxMessage.SelectionStart = m_iCaretPosition;

                // 버튼 클릭 횟수 기록
                m_iTotalButtonCountExceptBackSpace = m_iTotalButtonCountExceptBackSpace + 1;

                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonB", "click", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }
        }        

        private void buttonN_MouseDown(object sender, MouseEventArgs e)
        {
            m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

            m_bMouseDownFlag = true;

            // 기록 저장 (이벤트, 시간, 시간차 등)
            m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonN", "down", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
            m_eventTime = DateTime.Now;
        }

        private void buttonN_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true)
            {
                // 버튼을 포함하는 패널이 영역을 벗어나는지 확인
                int iRet = CheckButtonGroupPanelOutsideBoundary();
                if (iRet == 1 || iRet == 2 || iRet == 3 || iRet == 4)
                    return;

                m_endPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                int iDiffX = (m_endPoint.X - m_startPoint.X) * m_iCDGain;
                int iDiffY = (m_endPoint.Y - m_startPoint.Y) * m_iCDGain;

                // 새로운 마우스 위치 전역변수에 계산
                Point newPanelPos = new Point(panelButtonGroup.Location.X + iDiffX, panelButtonGroup.Location.Y + iDiffY);

                // 새로운 마우스 시작 좌표 저장
                m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                // 새로운 마우스 위치를 통해 버튼 그룹 이동
                panelButtonGroup.Location = newPanelPos;

                Invalidate();

                m_bMouseMoveFlag = true;
            }
        }

        private void buttonN_MouseUp(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true && m_bMouseMoveFlag == true)
            {
                m_iTotalButtonMoveCount = m_iTotalButtonMoveCount + 1;

                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonN", "move", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }

            m_bMouseDownFlag = false;
            m_bMouseMoveFlag = false;

            // 초점을 항상 OK 버튼으로 고정            
            textBoxMessage.Focus();
        }

        private void buttonN_Click(object sender, EventArgs e)
        {
            if (m_bMouseMoveFlag == false)
            {
                // Caret이 있는 위치 사이에 글자를 추가 처리
                textBoxMessage.Select(m_iCaretPosition, textBoxMessage.TextLength);
                string strTextAfter = textBoxMessage.SelectedText;

                textBoxMessage.Select(0, m_iCaretPosition);
                string strTextBefore = textBoxMessage.SelectedText;
                strTextBefore = strTextBefore + buttonN.Text;

                string strModifiedText = strTextBefore + strTextAfter;

                textBoxMessage.Text = strModifiedText;

                // 입력된 글자와 입력해야 하는 글자가 다르면 빨갛게 표시
                CheckCharacterBetweenInputAndTarget(); 

                m_iCaretPosition = m_iCaretPosition + 1;

                textBoxMessage.SelectionStart = m_iCaretPosition;

                // 버튼 클릭 횟수 기록
                m_iTotalButtonCountExceptBackSpace = m_iTotalButtonCountExceptBackSpace + 1;

                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonN", "click", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }
        }        

        private void buttonM_MouseDown(object sender, MouseEventArgs e)
        {
            m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

            m_bMouseDownFlag = true;

            // 기록 저장 (이벤트, 시간, 시간차 등)
            m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonM", "down", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
            m_eventTime = DateTime.Now;
        }

        private void buttonM_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true)
            {
                // 버튼을 포함하는 패널이 영역을 벗어나는지 확인
                int iRet = CheckButtonGroupPanelOutsideBoundary();
                if (iRet == 1 || iRet == 2 || iRet == 3 || iRet == 4)
                    return;

                m_endPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                int iDiffX = (m_endPoint.X - m_startPoint.X) * m_iCDGain;
                int iDiffY = (m_endPoint.Y - m_startPoint.Y) * m_iCDGain;

                // 새로운 마우스 위치 전역변수에 계산
                Point newPanelPos = new Point(panelButtonGroup.Location.X + iDiffX, panelButtonGroup.Location.Y + iDiffY);

                // 새로운 마우스 시작 좌표 저장
                m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                // 새로운 마우스 위치를 통해 버튼 그룹 이동
                panelButtonGroup.Location = newPanelPos;

                Invalidate();

                m_bMouseMoveFlag = true;
            }
        }

        private void buttonM_MouseUp(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true && m_bMouseMoveFlag == true)
            {
                m_iTotalButtonMoveCount = m_iTotalButtonMoveCount + 1;

                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonM", "move", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }

            m_bMouseDownFlag = false;
            m_bMouseMoveFlag = false;

            // 초점을 항상 OK 버튼으로 고정            
            textBoxMessage.Focus();
        }

        private void buttonM_Click(object sender, EventArgs e)
        {
            if (m_bMouseMoveFlag == false)
            {
                // Caret이 있는 위치 사이에 글자를 추가 처리
                textBoxMessage.Select(m_iCaretPosition, textBoxMessage.TextLength);
                string strTextAfter = textBoxMessage.SelectedText;

                textBoxMessage.Select(0, m_iCaretPosition);
                string strTextBefore = textBoxMessage.SelectedText;
                strTextBefore = strTextBefore + buttonM.Text;

                string strModifiedText = strTextBefore + strTextAfter;

                textBoxMessage.Text = strModifiedText;

                // 입력된 글자와 입력해야 하는 글자가 다르면 빨갛게 표시
                CheckCharacterBetweenInputAndTarget(); 

                m_iCaretPosition = m_iCaretPosition + 1;

                textBoxMessage.SelectionStart = m_iCaretPosition;

                // 버튼 클릭 횟수 기록
                m_iTotalButtonCountExceptBackSpace = m_iTotalButtonCountExceptBackSpace + 1;

                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonM", "click", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }
        }

        private void buttonSpace_MouseDown(object sender, MouseEventArgs e)
        {
            m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

            m_bMouseDownFlag = true;

            // 기록 저장 (이벤트, 시간, 시간차 등)
            m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonSpace", "down", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
            m_eventTime = DateTime.Now;
        }

        private void buttonSpace_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true)
            {
                // 버튼을 포함하는 패널이 영역을 벗어나는지 확인
                int iRet = CheckButtonGroupPanelOutsideBoundary();
                if (iRet == 1 || iRet == 2 || iRet == 3 || iRet == 4)
                    return;

                m_endPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                int iDiffX = (m_endPoint.X - m_startPoint.X) * m_iCDGain;
                int iDiffY = (m_endPoint.Y - m_startPoint.Y) * m_iCDGain;

                // 새로운 마우스 위치 전역변수에 계산
                Point newPanelPos = new Point(panelButtonGroup.Location.X + iDiffX, panelButtonGroup.Location.Y + iDiffY);

                // 새로운 마우스 시작 좌표 저장
                m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                // 새로운 마우스 위치를 통해 버튼 그룹 이동
                panelButtonGroup.Location = newPanelPos;

                Invalidate();

                m_bMouseMoveFlag = true;
            }
        }

        private void buttonSpace_MouseUp(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true && m_bMouseMoveFlag == true)
            {
                m_iTotalButtonMoveCount = m_iTotalButtonMoveCount + 1;

                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonSpace", "move", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }

            m_bMouseDownFlag = false;
            m_bMouseMoveFlag = false;

            // 초점을 항상 OK 버튼으로 고정            
            textBoxMessage.Focus();
        }

        private void buttonSpace_Click(object sender, EventArgs e)
        {
            if (m_bMouseMoveFlag == false)
            {
                // Caret이 있는 위치 사이에 글자를 추가 처리
                textBoxMessage.Select(m_iCaretPosition, textBoxMessage.TextLength);
                string strTextAfter = textBoxMessage.SelectedText;

                textBoxMessage.Select(0, m_iCaretPosition);
                string strTextBefore = textBoxMessage.SelectedText;
                strTextBefore = strTextBefore + " ";

                string strModifiedText = strTextBefore + strTextAfter;

                textBoxMessage.Text = strModifiedText;

                // 입력된 글자와 입력해야 하는 글자가 다르면 빨갛게 표시
                CheckCharacterBetweenInputAndTarget(); 

                m_iCaretPosition = m_iCaretPosition + 1;

                textBoxMessage.SelectionStart = m_iCaretPosition;

                // 버튼 클릭 횟수 기록
                m_iTotalButtonCountExceptBackSpace = m_iTotalButtonCountExceptBackSpace + 1;

                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonSpace", "click", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }
        }

        private void buttonBackSpace_MouseDown(object sender, MouseEventArgs e)
        {
            m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

            m_bMouseDownFlag = true;

            // 기록 저장 (이벤트, 시간, 시간차 등)
            m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonBackSpace", "down", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
            m_eventTime = DateTime.Now;
        }

        private void buttonBackSpace_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true)
            {
                // 버튼을 포함하는 패널이 영역을 벗어나는지 확인
                int iRet = CheckButtonGroupPanelOutsideBoundary();
                if (iRet == 1 || iRet == 2 || iRet == 3 || iRet == 4)
                    return;

                m_endPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                int iDiffX = (m_endPoint.X - m_startPoint.X) * m_iCDGain;
                int iDiffY = (m_endPoint.Y - m_startPoint.Y) * m_iCDGain;

                // 새로운 마우스 위치 전역변수에 계산
                Point newPanelPos = new Point(panelButtonGroup.Location.X + iDiffX, panelButtonGroup.Location.Y + iDiffY);

                // 새로운 마우스 시작 좌표 저장
                m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                // 새로운 마우스 위치를 통해 버튼 그룹 이동
                panelButtonGroup.Location = newPanelPos;

                Invalidate();

                m_bMouseMoveFlag = true;
            }
        }

        private void buttonBackSpace_MouseUp(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true && m_bMouseMoveFlag == true)
            {
                m_iTotalButtonMoveCount = m_iTotalButtonMoveCount + 1;

                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonBackSpace", "move", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }

            m_bMouseDownFlag = false;
            m_bMouseMoveFlag = false;

            // 초점을 항상 OK 버튼으로 고정            
            textBoxMessage.Focus();
        }

        private void buttonBackSpace_Click(object sender, EventArgs e)
        {
            if (m_bMouseMoveFlag == false)
            {
                // Caret이 있는 위치 사이에 글자를 추가 처리             
                textBoxMessage.Select(m_iCaretPosition, textBoxMessage.TextLength);
                string strTextAfter = textBoxMessage.SelectedText;

                textBoxMessage.Select(0, m_iCaretPosition-1);
                string strTextBefore = textBoxMessage.SelectedText;

                string strModifiedText = strTextBefore + strTextAfter;

                textBoxMessage.Text = strModifiedText;
                
                // 입력된 글자와 입력해야 하는 글자가 다르면 빨갛게 표시
                CheckCharacterBetweenInputAndTarget(); 

                m_iCaretPosition = m_iCaretPosition - 1;
                if (m_iCaretPosition < 0)
                    m_iCaretPosition = 0;

                textBoxMessage.SelectionStart = m_iCaretPosition;

                // 버튼 클릭 횟수 기록
                m_iTotalButtonBackSpaceCount = m_iTotalButtonBackSpaceCount + 1;

                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "buttonBackSpace", "click", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }
        }

        private void panelButtonGroup_MouseDown(object sender, MouseEventArgs e)
        {
            m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

            m_bMouseDownFlag = true;

            // 기록 저장 (이벤트, 시간, 시간차 등)
            m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "Panel", "down", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
            m_eventTime = DateTime.Now;
        }

        private void panelButtonGroup_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true)
            {
                // 버튼을 포함하는 패널이 영역을 벗어나는지 확인
                int iRet = CheckButtonGroupPanelOutsideBoundary();
                if (iRet == 1 || iRet == 2 || iRet == 3 || iRet == 4)
                    return;

                m_endPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                int iDiffX = (m_endPoint.X - m_startPoint.X) * m_iCDGain;
                int iDiffY = (m_endPoint.Y - m_startPoint.Y) * m_iCDGain;

                // 새로운 마우스 위치 전역변수에 계산
                Point newPanelPos = new Point(panelButtonGroup.Location.X + iDiffX, panelButtonGroup.Location.Y + iDiffY);

                // 새로운 마우스 시작 좌표 저장
                m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                // 새로운 마우스 위치를 통해 버튼 그룹 이동
                panelButtonGroup.Location = newPanelPos;

                Invalidate();

                m_bMouseMoveFlag = true;
            }
        }

        private void panelButtonGroup_MouseUp(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true && m_bMouseMoveFlag == true)
            {
                m_iTotalButtonMoveCount = m_iTotalButtonMoveCount + 1;

                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "Panel", "move", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }

            m_bMouseDownFlag = false;
            m_bMouseMoveFlag = false;

            // 초점을 항상 OK 버튼으로 고정
            textBoxMessage.Focus();
        }

        private void pictureBoxWatchCenter_MouseDown(object sender, MouseEventArgs e)
        {
            m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

            m_bMouseDownFlag = true;

            // 기록 저장 (이벤트, 시간, 시간차 등)
            m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "pictureBox", "down", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
            m_eventTime = DateTime.Now;
        }

        private void pictureBoxWatchCenter_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true)
            {
                // 버튼을 포함하는 패널이 영역을 벗어나는지 확인
                int iRet = CheckButtonGroupPanelOutsideBoundary();
                if (iRet == 1 || iRet == 2 || iRet == 3 || iRet == 4)
                    return;

                m_endPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                int iDiffX = (m_endPoint.X - m_startPoint.X) * m_iCDGain;
                int iDiffY = (m_endPoint.Y - m_startPoint.Y) * m_iCDGain;

                // 새로운 마우스 위치 전역변수에 계산
                Point newPanelPos = new Point(panelButtonGroup.Location.X + iDiffX, panelButtonGroup.Location.Y + iDiffY);

                // 새로운 마우스 시작 좌표 저장
                m_startPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                // 새로운 마우스 위치를 통해 버튼 그룹 이동
                panelButtonGroup.Location = newPanelPos;

                Invalidate();

                m_bMouseMoveFlag = true;
            }
        }

        private void pictureBoxWatchCenter_MouseUp(object sender, MouseEventArgs e)
        {
            if (m_bMouseDownFlag == true && m_bMouseMoveFlag == true)
            {
                m_iTotalButtonMoveCount = m_iTotalButtonMoveCount + 1;

                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "pictureBox", "move", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }

            m_bMouseDownFlag = false;
            m_bMouseMoveFlag = false;

            // 초점을 항상 OK 버튼으로 고정
            textBoxMessage.Focus();
        }


        // 닫기 버튼
        private void buttonClose_Click(object sender, EventArgs e)
        {
           
            // 자원 해제
            m_streamWriter.Close();
            m_fileStream.Close();
            
            // 프로그램 종료
            this.Close();
        }


        // 두 문자열을 비교하여 정확도율을 리턴
        private float CompareText(string strProposed, string strInput)
        {
            float dResults = 0.0f;

            if (strProposed.Equals(strInput) == true)
            {
                // 만일 같으면 100을 리턴
                dResults = 100.0f;
                return dResults;
            }
            else
            {
                // 만일 같지 않으면 비교 알고리즘에 따른 정확도를 계산하여 실수를 리턴
                return dResults;
            }
        }


        // 확인 버튼
        private void buttonDoExperiment_Click(object sender, EventArgs e)
        {
            // 기록할 변수 초기화
            m_dTotalCharacterPerMinute = 0;                 // 1분당 걸린 시간
            m_iTotalButtonCountExceptBackSpace = 0;         // BackSpace 제외 버튼 눌린 횟수
            m_iTotalButtonBackSpaceCount = 0;               // BackSpace 버튼 눌린 횟수
            m_iTotalGestureSpaceCount = 0;                  // Space 제스처 눌린 횟수
            m_iTotalGestureBackSpaceCount = 0;              // BackSpace 제스처 눌린 횟수
            m_iTotalButtonMoveCount = 0;                    // 총 키보드 이동을 위한 횟수
            
            // 실험 횟수 저장
            m_streamWriter.WriteLine(labelTrialNo.Text);
           
            if (m_iTrialNo == 1)
            {
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "확인", "", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), "00:00:00.0000000");
                m_eventTime = DateTime.Now;
            }
            else if (m_iTrialNo > 1)
            {
                // 기록 저장 (이벤트, 시간, 시간차 등)
                m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "확인", "", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);
                m_eventTime = DateTime.Now;
            }           

            // 확인 버튼 사용 불능으로 변경
            buttonDoExperiment.Enabled = false;
            buttonDoExperiment.BackColor = Color.Salmon;
            buttonDoExperiment.Text = "PROGRESSING";

            buttonSend.Enabled = true;
            buttonSend.BackColor = Color.Gainsboro;

            // 윈도우를 보이게 함
            pictureBoxWatchTop.Visible = true;
            pictureBoxWatchBottom.Visible = true;
            pictureBoxWatchLeft.Visible = true;
            pictureBoxWatchRight.Visible = true;
            pictureBoxWatchCenter.Visible = true;
            textBoxMessage.Visible = true;
            panelButtonGroup.Visible = true;

            // Trial 시간을 계산하기 위해 시작 시간 계산
            m_dtStartTrialTime = DateTime.Now;
        }


        // 대상 텍스트 집합에서 랜덤으로 문장을 추출하여 반환
        private string ReadRandomTargetTextSet()
        {
            string strRet = null;

            Random rand = new Random(DateTime.Now.Millisecond);
            iRandomTargetTextNo = rand.Next(0, ControlForm.m_alTargetTextSet.Count + 1);

            strRet = (string)ControlForm.m_alTargetTextSet[iRandomTargetTextNo];

            return strRet;
        }

        // 입력된 문자의 Word 수
        private int m_iTotalTrialWordCount = 0;

        // 문자 전송 버튼
        private void buttonSend_Click(object sender, EventArgs e)
        {
            // 이벤트 출력
            m_streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "SEND", "", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("hh:mm:ss.fffffff"), DateTime.Now - m_eventTime);

            // 최종 입력 된 글자의 끝단 빈칸 제거
            string strTrimmedInputText = textBoxMessage.Text.Trim();

            // Trial 별 결과 Summarys
            m_streamWriter.WriteLine("*********************Summary*************************");

            // 입력해야 하는 글자
            m_streamWriter.WriteLine("입력해야하는 글자\t{0}", labelTargetText.Text);

            // 입력 된 글자
            m_streamWriter.WriteLine("입력 된 글자\t{0}", strTrimmedInputText);

            // 입력 된 글자 수
            m_streamWriter.WriteLine("입력 된 글자 수\t{0}", strTrimmedInputText.Length.ToString());

            // Trial이 시작된 시간과 끝난 시간과의 차이 계산
            m_dtEndTrialTime = DateTime.Now;
            m_tsTotalTrialTime = m_dtEndTrialTime - m_dtStartTrialTime;
            m_streamWriter.WriteLine("Trial 별 걸린 총 시간\t{0}", m_tsTotalTrialTime.ToString());

            // 1분 당 Character 입력 수 (Character Per Minute)
            m_dTotalCharacterPerMinute = 60 * strTrimmedInputText.Length / m_tsTotalTrialTime.TotalSeconds;
            m_streamWriter.WriteLine("Trial 별 Character Per Minute\t{0}", m_dTotalCharacterPerMinute);

            // 입력된 문자의 Word 수
            string[] strSeparatedArray = strTrimmedInputText.Split(' ');
            m_iTotalTrialWordCount = strSeparatedArray.Length;
            m_streamWriter.WriteLine("Trial 별 총 Word 수\t{0}", m_iTotalTrialWordCount);

            // Word Per Minute
            double m_dTotalWordPerMinute = 60 * m_iTotalTrialWordCount / m_tsTotalTrialTime.TotalSeconds;
            m_streamWriter.WriteLine("Trial 별 Word Per Per Minute\t{0}", m_dTotalWordPerMinute);

            m_streamWriter.WriteLine("총 버튼 횟수 (Backspace 제외)\t{0}", m_iTotalButtonCountExceptBackSpace);
            
            m_streamWriter.WriteLine("총 Backspace 버튼 횟수\t{0}", m_iTotalButtonBackSpaceCount);
            
            m_streamWriter.WriteLine("총 Space Gesture 횟수\t{0}", m_iTotalGestureSpaceCount);
            
            m_streamWriter.WriteLine("총 Backspace Gesutre 횟수\t{0}", m_iTotalGestureBackSpaceCount);
            
            m_streamWriter.WriteLine("총 키보드 이동을 위한 횟수\t{0}", m_iTotalButtonMoveCount);

            m_streamWriter.WriteLine("*****************************************************");
            
            // 빈 줄 출력
            m_streamWriter.WriteLine();

            // 텍스트 박스 초기화
            textBoxMessage.Text = "";

            // 텍스트 박스 Caret 위치 초기화
            m_iCaretPosition = textBoxMessage.TextLength;

            // 버튼 패널 위치 초기화
            InitializeButtonGroupPanelLocation();
            
            if (m_iTrialNo >= m_iTrialNoMax)
            {
                // 만일 Trial 횟수가 끝났다면, Close 버튼 활성화하고, 종료 준비

                buttonDoExperiment.Enabled = false;
                buttonDoExperiment.BackColor = Color.Gainsboro;
                buttonDoExperiment.Text = "확인";

                buttonSend.Enabled = false;
                buttonSend.BackColor = Color.Gainsboro;
            }
            else if (m_iTrialNo < m_iTrialNoMax)
            {
                // Trial 횟수보다 적으면, ...

                // 실험 횟수 업데이트
                m_iTrialNo = m_iTrialNo + 1;
                labelTrialNo.Text = "trial " + m_iTrialNo.ToString();

                // 실험에 다른 대상 문자 집합 업데이트
                // 랜덥화 하여 추출
                labelTargetText.Text = ReadRandomTargetTextSet();

                //Console.WriteLine("지울 번호는" + iRandomTargetTextNo);
                ControlForm.m_alTargetTextSet.RemoveAt(iRandomTargetTextNo);
                //Console.WriteLine("지워진 이후" + ControlForm.m_alTargetTextSet.Count);

                // 확인 버튼 사용 가능으로 변경
                buttonDoExperiment.Enabled = true;
                buttonDoExperiment.BackColor = Color.Gainsboro;
                buttonDoExperiment.Text = "확인";

                buttonSend.Enabled = false;
                buttonSend.BackColor = Color.Salmon;
            }

            // 윈도우를 보이지 않도록 함
            pictureBoxWatchTop.Visible = false;
            pictureBoxWatchBottom.Visible = false;
            pictureBoxWatchLeft.Visible = false;
            pictureBoxWatchRight.Visible = false;
            pictureBoxWatchCenter.Visible = false;
            textBoxMessage.Visible = false;
            panelButtonGroup.Visible = false;
        }


    }
}
