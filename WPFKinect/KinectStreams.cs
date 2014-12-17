using DrawKinect;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WPFKinect
{
    public class KinectStreams
    {
        // Variables générales
        private KinectSensor Sensor;
        private Canvas canvas;
        private Canvas canvasDessin;
        private Canvas canvasPosition = new Canvas();
        private CoordinateMapper coordMap;
        private SkeletonPoint DernierPoint;
        
        // Variables pour flux de couleur
        private Image image;
        private byte[] colorPixels;
        private WriteableBitmap bitmap;

        // Variables pour le flux d'interaction
        private InteractionStream interactionStream = null;
        private InteractionHandEventType actualHandState = InteractionHandEventType.None;
        private InteractionHandType actualHandType = InteractionHandType.None;

        /// <summary>
        /// Lancement du flux
        /// </summary>
        /// <param name="KinectSensor">La Kinect</param>
        /// <param name="mCanvasImageKinect">Le canvas de la fenêtre où l'on doit afficher l'image</param>
        internal void Init(KinectSensor KinectSensor, Canvas mCanvasImageKinect, Canvas mCanvasDessin)
        {
            Sensor = KinectSensor;
            canvas = mCanvasImageKinect;
            canvasDessin = mCanvasDessin;
            canvas.Children.Clear();
            canvasDessin.Children.Clear();

            if (Sensor == null)
                return;
            
            coordMap = new CoordinateMapper(Sensor);

            // Flux de couleur
            Sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
            colorPixels = new byte[Sensor.ColorStream.FramePixelDataLength];
            bitmap = new WriteableBitmap(Sensor.ColorStream.FrameWidth, Sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);
            image = new Image();
            image.Source = bitmap;
            canvas.Children.Add(image);
            Sensor.ColorFrameReady += EventColorFrameReady;

            // Flux de profondeur
            //Sensor.DepthStream.Range = DepthRange.Near;
            Sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
            Sensor.DepthFrameReady += EventDepthFrameReady;
            
            // Flux du squelette
            //Sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
            Sensor.SkeletonStream.EnableTrackingInNearRange = true;
            Sensor.SkeletonStream.Enable();
            Sensor.SkeletonFrameReady += EventSkeletonFrameReady;

            // Flux d'interaction
            interactionStream = new InteractionStream(Sensor, new MyInteraction());
            interactionStream.InteractionFrameReady += EventInteractionFrameReady;
        }

        

        /// <summary>
        /// Fermeture des flux
        /// </summary>
        internal void Close()
        {
            // Fermeture et arrêt couleur
            Sensor.ColorFrameReady -= EventColorFrameReady;
            Sensor.ColorStream.Disable();

            // Fermeture et arrêt profondeur
            Sensor.DepthFrameReady -= EventDepthFrameReady;
            Sensor.DepthStream.Disable();

            // Fermeture et arrêt squelette
            Sensor.SkeletonFrameReady -= EventSkeletonFrameReady;
            Sensor.SkeletonStream.Disable();

            // Fermeture et arrêt interaction
            interactionStream.InteractionFrameReady -= EventInteractionFrameReady;
            interactionStream.Dispose();

            canvas.Children.Clear();
        }

        /// <summary>
        /// Evènement : A chaque nouvelle image de couleur
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EventColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            // Récupération de l'image de couleur
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame == null) // Si pas d'image
                    return;
                    
                // Copie dans bitmap, source de l'image dans le canvas
                colorFrame.CopyPixelDataTo(colorPixels);
                bitmap.WritePixels(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight), colorPixels, bitmap.PixelWidth * sizeof(int), 0);
            }
        }

        /// <summary>
        /// Evènements : A chaque nouvelle image de profondeur, on envoie les informations au flux d'interaction
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EventDepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame == null)
                    return;

                try
                {
                    interactionStream.ProcessDepth(depthFrame.GetRawPixelData(), depthFrame.Timestamp);
                }
                catch (InvalidOperationException) { }
            }
        }

        /// <summary>
        /// Evènement : A chaque nouvelle image de squelette
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EventSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            // Récupération des squelettes trouvés et envoie au interaction stream
            Skeleton[] skeletons = null;
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame == null)
                    return;

                try
                {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                    interactionStream.ProcessSkeleton(skeletons, Sensor.AccelerometerGetCurrentReading(), skeletonFrame.Timestamp);
                }
                catch (InvalidOperationException) { }
            }
            
            // Vider les anciens os dessinés, donc rajout de l'image sur la canvas
            canvas.Children.Clear();
            canvas.Children.Add(image);
            // Si il n'y a pas encore le canvas de position, on l'ajoute
            if (!canvasDessin.Children.Contains(canvasPosition))
                 canvasDessin.Children.Add(canvasPosition);
            
            if (skeletons.Length == 0) // Si pas de squelette
                return;

            var skels = skeletons.OrderBy(sk => sk.Position.Z);
            //var skels = skeletons.Where(sk => sk.TrackingState == SkeletonTrackingState.Tracked).OrderBy(sk => sk.Position.Z);
            bool skelDrawn = false;
            // Pour chaque squelette
            foreach(Skeleton skel in skels)
                if (skel.TrackingState == SkeletonTrackingState.Tracked && !skelDrawn) // Si on trouve le squelette et on en a dessiné un
                {
                    // On dessine la main actuelle
                    if (actualHandType == InteractionHandType.Left)
                        DrawHand(skel, JointType.HandLeft);
                    else if (actualHandType == InteractionHandType.Right)
                        DrawHand(skel, JointType.HandRight);
                    skelDrawn = true;
                }
        }

        /// <summary>
        /// Evènement : si on a les informations de profondeur et du squelette -> définit l'état actuel de la main
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EventInteractionFrameReady(object sender, InteractionFrameReadyEventArgs e)
        {
            UserInfo[] userInfos = new UserInfo[InteractionFrame.UserInfoArrayLength];
            using (InteractionFrame interactionFrame = e.OpenInteractionFrame())
            {
                if (interactionFrame == null)
                    return;

                interactionFrame.CopyInteractionDataTo(userInfos);
            }

            // On parcourt les informations trouvées
            bool handFound = false;
            foreach (UserInfo userInfo in userInfos)
            {
                // Si mal reconnu ou on déjà trouvé une main qui est affichée
                if (userInfo.SkeletonTrackingId == 0 || handFound) 
                    continue;

                var hands = userInfo.HandPointers;
                if (hands.Count == 0) // Si aucune main reconnue
                    continue;

                foreach (var hand in hands)
                {
                    // La main doit être trackée, active et la main 'primaire' (= première levée des deux)
                    if (!hand.IsActive || !hand.IsTracked)
                        continue;

                    handFound = true;
                    actualHandType = hand.HandType;

                    // Si la main devient grip et c'était pas son ancien état
                    if (hand.HandEventType == InteractionHandEventType.Grip && hand.HandEventType != actualHandState)
                        actualHandState = InteractionHandEventType.Grip;
                    // Si la main lâche le grip
                    else if (hand.HandEventType == InteractionHandEventType.GripRelease && hand.HandEventType != actualHandState)
                        actualHandState = InteractionHandEventType.GripRelease;
                }
            }
        }

        /// <summary>
        /// Dessine une main
        /// </summary>
        /// <param name="skel">Squelette auquel appartient la main</param>
        /// <param name="jointType">Type du joint à dessiner</param>
        private void DrawHand(Skeleton skel, JointType jointType)
        {
            // Le joint à dessiner
            Joint joint = skel.Joints[jointType];
            // Le color point associer
            ColorImagePoint colorPoint = coordMap.MapSkeletonPointToColorPoint(joint.Position, ColorImageFormat.RgbResolution640x480Fps30);

            // Si le joint n'est pas trouvé ou pas bien trouvé
            if (joint.TrackingState == JointTrackingState.NotTracked || joint.TrackingState == JointTrackingState.Inferred)
                return;

            // Si on a le droit de dessiner, on affiche un crayon
            if (actualHandState == InteractionHandEventType.GripRelease)
            {
                // Afficher crayon pour dessiner (main ouverte)
                Image i = new Image();
                i.Source = new BitmapImage(new Uri("img\\crayon.gif", UriKind.Relative));
                i.Height = 100;
                i.Width = 100;
                i.Margin = new Thickness(colorPoint.X, colorPoint.Y - i.Width, 0, 0);
                canvas.Children.Add(i);

                // Les points à dessiner, en color point
                ColorImagePoint dernierPointColor = coordMap.MapSkeletonPointToColorPoint(DernierPoint, ColorImageFormat.RgbResolution640x480Fps30);
                ColorImagePoint pointActuelColor = coordMap.MapSkeletonPointToColorPoint(joint.Position, ColorImageFormat.RgbResolution640x480Fps30);

                // Ajout dans la liste de segment (avec les coordonnées de color point)
                Draw.getInstance().Add(new Segment(dernierPointColor.X, dernierPointColor.Y, pointActuelColor.X, pointActuelColor.Y));

                // Dessine le segment sur le canvas de dessin
                Line l = new Line();
                l.Stroke = Brushes.Red;
                l.StrokeThickness = 5;

                l.X1 = dernierPointColor.X;
                l.Y1 = dernierPointColor.Y;
                l.X2 = pointActuelColor.X;
                l.Y2 = pointActuelColor.Y;

                canvasDessin.Children.Add(l);
            }

            // Si on n'a pas le droit de dessiner, on affiche un curseur et la main d'arrêt
            else if (actualHandState == InteractionHandEventType.Grip)
            {
                // Afficher main d'arrêt de dessin (main fermer)
                Image i = new Image();
                i.Source = new BitmapImage(new Uri("img\\mainArret.gif", UriKind.Relative));
                i.Height = 130;
                i.Width = 130;
                i.Margin = new Thickness(colorPoint.X - (i.Height / 2), colorPoint.Y - (i.Width / 2), 0, 0);
                canvas.Children.Add(i);

                // Afficher position de la main
                canvasPosition.Children.Clear();

                Line l1 = new Line();
                Line l2 = new Line();
                l1.Stroke = Brushes.Green;
                l2.Stroke = Brushes.Green;
                l1.StrokeThickness = 3;
                l2.StrokeThickness = 3;

                int TailleCroixPositionMain = 10;
                l1.X1 = colorPoint.X - TailleCroixPositionMain;
                l1.Y1 = colorPoint.Y + TailleCroixPositionMain;
                l1.X2 = colorPoint.X + TailleCroixPositionMain;
                l1.Y2 = colorPoint.Y - TailleCroixPositionMain;
                l2.X1 = colorPoint.X + TailleCroixPositionMain;
                l2.Y1 = colorPoint.Y + TailleCroixPositionMain;
                l2.X2 = colorPoint.X - TailleCroixPositionMain;
                l2.Y2 = colorPoint.Y - TailleCroixPositionMain;

                canvasPosition.Children.Add(l1);
                canvasPosition.Children.Add(l2);
            }
            DernierPoint = joint.Position;
        }
    }
}
