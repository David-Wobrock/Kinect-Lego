using Lego.Ev3.Core;
using Lego.Ev3.Desktop;
using RobotCore;
using System;
using System.Windows;
using System.Windows.Controls;

namespace WPFKinect
{
    /// <summary>
    /// Logique d'interaction pour ConnexionWindow.xaml
    /// </summary>
    public partial class ConnexionWindow : Window
    {
        private const string BT = "Bluetooth";
        private const string USB = "USB";

        private TextBlock TextBlockStatus = null;

        public ConnexionWindow(TextBlock textBlockStatus)
        {
            InitializeComponent();
            mComboBoxTypeConnexion.Items.Add(USB);
            mComboBoxTypeConnexion.Items.Add(BT);

            TextBlockStatus = textBlockStatus;
        }

        private async void BoutonConnexion(object sender, RoutedEventArgs e)
        {
            String contenuCombo = mComboBoxTypeConnexion.SelectedItem.ToString();
            ICommunication communication;
            String mode = null;
                
            // Trouver le mode de connexion
            switch (contenuCombo)
            {
                case BT:
                    communication = new BluetoothCommunication(mTextBoxComport.Text);
                    mode = BT;
                    break;
                case USB:
                    communication = new UsbCommunication();
                    mode = USB;
                    break;
                default:
                    communication = null;
                    break;
            }

            // Connexion
            if (communication != null)
            {
                GestionRobot.getInstance().brick = new Brick(communication, true);
                try
                {
                    await GestionRobot.getInstance().brick.ConnectAsync();
                    TextBlockStatus.Text = String.Format("Robot Lego : Connecté en : {0}", mode);
                    Close();
                }
                catch (Exception)
                {
                    MessageBox.Show("Could not connect", "Error", MessageBoxButton.OK);
                }
            }
            else
                MessageBox.Show("Invalid connection type for this device", "Error", MessageBoxButton.OK);
        }
    }
}
