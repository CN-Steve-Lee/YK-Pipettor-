using System;
using System.Configuration;

using System.IO.Ports;
using System.Threading;
using System.Windows;

namespace Pipettor
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            log.InfoFormat("Pipettor Running Successfully! (Version {0})",ConfigurationManager.AppSettings.Get("Version"));
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

            MotorController.Instance.OpenSerialPort();

            //MotorController.Instance.OnEncoderValue += Instance_onEncoderValue;
            double arm1Position = double.Parse(ConfigurationManager.AppSettings["Arm1StartPosition"]);
            double arm2Position = double.Parse(ConfigurationManager.AppSettings["Arm2StartPosition"]);
            MotorController.Instance.Rotate2ABSAngle(1, arm1Position);
            Thread.Sleep(100);
            MotorController.Instance.Rotate2ABSAngle(2, arm2Position);
            log.InfoFormat("Arms Returned To Zero Successfully! Now Arm1 at: {0},Arm2 at: {1}\n",arm1Position,arm2Position );
        }

        //private void Instance_onEncoderValue(object sender, System.Collections.Generic.Dictionary<int, float> id_encoderValue)
        //{
        //    if (id_encoderValue.ContainsKey(1))
        //    {
        //        MessageBox.Show(id_encoderValue[1].ToString());//test
        //        MotorController.Instance.Rotate2ABSAngle(1, id_encoderValue[1]);
        //    }
        //    if (id_encoderValue.ContainsKey(2))
        //    {
        //        MessageBox.Show(id_encoderValue[2].ToString());//test
        //        MotorController.Instance.Rotate2ABSAngle(2, id_encoderValue[2]);
        //    }
        //}
        private void btnSetDegree_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int tubeID = int.Parse(txtTubeID.Text);
                myCanvas.Move2Tube(tubeID);
            }
            catch (Exception e2)
            {
                MessageBox.Show("Error", e2.Message);
                log.Info("Line 51:"+e2.Message);
            }
        }

        private void btnNextDegree_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                int tubeID = int.Parse(txtTubeID.Text);
                tubeID++;
                myCanvas.Move2Tube(tubeID);
                txtTubeID.Text = tubeID.ToString();
               
            }
            catch (Exception e1)
            {
                MessageBox.Show("Error",e1.Message);
                log.Error("Line 65:" + e1.Message);
            }
        }

        private void BackOrigin_Click(object sender, RoutedEventArgs e)
        {
            double arm1Position = double.Parse(ConfigurationManager.AppSettings["Arm1StartPosition"]);
            double arm2Position = double.Parse(ConfigurationManager.AppSettings["Arm2StartPosition"]);
            MotorController.Instance.Rotate2ABSAngle(1, arm1Position);
            Thread.Sleep(100);
            MotorController.Instance.Rotate2ABSAngle(2, arm2Position);
            log.InfoFormat("Arms Returned To Zero Successfully! Now Arm1 at: {0},Arm2 at: {1}\n", arm1Position, arm2Position);
            
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            log.Info("Exit");
            this.Close();
        }
    }
}
