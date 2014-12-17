using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Kinect;
using DrawKinect;
using RobotCore;

namespace WPFKinect
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private KinectSensor KinectSensor = null;
        private KinectStreams KinectStream = null;

        public MainWindow()
        {
            InitializeComponent();

            // Remplissage de la combo de tailles de feuille
            List<string> Formats = new List<string>();
            for(int i = 0; i < 6; ++i)
                Formats.Add(String.Format("A{0}", i));
            foreach(String s in Formats)
                mComboBoxTailleFeuille.Items.Add(s);

            mComboBoxTailleFeuille.SelectedItem = "A1";
        }

        /// <summary>
        /// Lancement de la Kinect au lancement de l'application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartKinect(object sender, RoutedEventArgs e)
        {
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    KinectSensor = potentialSensor;
                    break;
                }
            }

            try
            {
                KinectSensor.Start();
            }
            catch (Exception)
            {
                KinectSensor = null;
            }

            if (KinectSensor == null)
                mTextBlockStatusKinect.Text = "404 Kinect not found";
            else
                mTextBlockStatusKinect.Text = "Kinect prêt à l'utilisation";
        }

        /// <summary>
        /// Arrête la Kinect et efface l'image de la Kinect, lors de la fermeture de l'application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StopKinect(object sender, System.ComponentModel.CancelEventArgs e)
        {
            mCanvasImageKinect.Children.Clear();
            if (KinectSensor != null)
                KinectSensor.Stop();
        }

        /// <summary>
        /// Lancement du flux Kinect (Bouton Start)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartStream(object sender, RoutedEventArgs e)
        {
            if (KinectSensor == null)
                return;
            KinectStream = new KinectStreams();
            KinectStream.Init(KinectSensor, mCanvasImageKinect, mCanvasDessin);
            mTextBlockStatusKinect.Text = "Flux Kinect lancé";
        }

        /// <summary>
        /// Arrête le flux Kinect (Bouton Stop)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StopStream(object sender, RoutedEventArgs e)
        {
            if (KinectStream == null || KinectSensor == null)
                return;
            KinectStream.Close();
            mCanvasImageKinect.Children.Clear();
            mTextBlockStatusKinect.Text = "Kinect prêt à l'utilisation";
        }

        /// <summary>
        /// Ouvre la fenêtre de connexion du robot Lego (bouton Connect Lego)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConnectLego(object sender, RoutedEventArgs e)
        {
            ConnexionWindow win = new ConnexionWindow(mTextBlockStatusLego);
            win.ShowDialog();
        }

        /// <summary>
        /// Efface le dessin (bouton Clear) et vide la liste de segments
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearDrawing(object sender, RoutedEventArgs e)
        {
            mCanvasDessin.Children.Clear();
            DrawKinect.Draw.getInstance().Clear();
        }

        /// <summary>
        /// Terminer l'application (bouton End) et efface les dessins
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EndApplication(object sender, RoutedEventArgs e)
        {
            ClearDrawing(null, null);
            StopStream(null, null);
            Close();
        }

        /// <summary>
        /// Event : Modifie les valeurs des champs de longueurs et largeurs de la feuille quand on change la valeur de la combo
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SizeComboBoxModified(object sender, SelectionChangedEventArgs e)
        {
            switch (mComboBoxTailleFeuille.SelectedItem as String)
            {
                case "A0":
                    mTextBoxLargeur.Text = "84,1";
                    mTextBoxLongueur.Text = "118,9";
                    break;
                case "A1":
                    mTextBoxLargeur.Text = "59,4";
                    mTextBoxLongueur.Text = "84,1";
                    break;
                case "A2":
                    mTextBoxLargeur.Text = "42";
                    mTextBoxLongueur.Text = "59,4";
                    break;
                case "A3":
                    mTextBoxLargeur.Text = "29,7";
                    mTextBoxLongueur.Text = "42";
                    break;
                case "A4":
                    mTextBoxLargeur.Text = "21";
                    mTextBoxLongueur.Text = "29,7";
                    break;
                case "A5":
                    mTextBoxLargeur.Text = "14,8";
                    mTextBoxLongueur.Text = "21";
                    break;
                default:
                    break;
            }
        }

        private void DrawByRobot(IRécupérationSegments modeRécupération)
        {
            double largeur, longueur;

            try
            {
                largeur = Convert.ToDouble(mTextBoxLargeur.Text.Replace('.', ','));
                longueur = Convert.ToDouble(mTextBoxLongueur.Text.Replace('.', ','));
            }
            catch (Exception)
            {
                MessageBox.Show("Largeur et/ou longueur incorrect(s).", "Erreur format");
                return;
            }
            if (largeur <= 0 || longueur <= 0)
            {
                MessageBox.Show("La largeur et la longueur doivent être supérieur à 0", "Erreur valeur");
                return;
            }

            mCanvasDessin.Children.Clear();
            dessinerListeSegment(Draw.getInstance().GetList(modeRécupération));
            if (GestionRobot.getInstance().brick != null)
                GestionRobot.getInstance().Draw(largeur, longueur, modeRécupération);
        }

        private void DrawEchantillon(object sender, RoutedEventArgs e)
        {
            DrawByRobot(new RécupérationEchantillon());
        }

        private void DrawPointsDistants(object sender, RoutedEventArgs e)
        {
            DrawByRobot(new RécupérationPointsDistants());
        }

        private void dessinerListeSegment(List<Segment> liste)
        {
            foreach (Segment s in liste)
            {
                Line l = new Line();
                l.Stroke = Brushes.Blue;
                l.StrokeThickness = 5;

                l.X1 = s.x1;
                l.Y1 = s.y1;
                l.X2 = s.x2;
                l.Y2 = s.y2;

                mCanvasDessin.Children.Add(l);
            }
        }
    }
}
