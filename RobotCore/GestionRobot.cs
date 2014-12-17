using System;
using System.Collections.Generic;
using System.Windows;
using System.Threading.Tasks;
using Lego.Ev3.Core;
using DrawKinect;

namespace RobotCore
{
    public class GestionRobot
    {
        /// <summary>
        /// L'instance unique de GestionRobot
        /// </summary>
        private static GestionRobot instance = null;

        /// <summary>
        /// Le constructeur privé de GestionRobot
        /// </summary>
        private GestionRobot()
        {}

        /// <summary>
        /// Permet l'utilisation de l'instance unique de la classe GestionRobot
        /// </summary>
        /// <returns>L'instance unique de GestionRobot</returns>
        public static GestionRobot getInstance()
        {
            if (instance == null)
                instance = new GestionRobot();
            return instance;
        }

        /// <summary>
        /// La brick Lego
        /// </summary>
        public Brick brick = null;

        private OutputPort MoteurStylo = OutputPort.A;
        private OutputPort MoteurGauche = OutputPort.B;
        private OutputPort MoteurDroit = OutputPort.D;

        /// <summary>
        /// La position x du robot sur la feuille
        /// </summary>
        private double x;

        /// <summary>
        /// La position y du robot sur la feuille
        /// </summary>
        private double y;

        /// <summary>
        /// L'orientation du robot (de 0 à 360)
        /// </summary>
        private double orientationActuelle;

        /// <summary>
        /// Baisse le crayon pour pouvoir dessiner
        /// </summary>
        private async Task PenDown()
        {
            int temps = 600;
            await GestionRobot.getInstance().brick.DirectCommand.TurnMotorAtPowerForTimeAsync(MoteurStylo, 20, Convert.ToUInt32(temps), false);
            System.Threading.Thread.Sleep(temps);
        }

        /// <summary>
        /// Lève le crayon, arrêt du dessin
        /// </summary>
        private async Task PenUp()
        {
            System.Threading.Thread.Sleep(700);

            int temps = 400;
            await GestionRobot.getInstance().brick.DirectCommand.TurnMotorAtPowerForTimeAsync(MoteurStylo, -15, Convert.ToUInt32(temps), false);
            System.Threading.Thread.Sleep(temps);
        }

        /// <summary>
        /// Ordonne au robot de dessiner les segments actuellement dans la liste
        /// </summary>
        /// <param name="largeur">Largeur de la feuille</param>
        /// <param name="longueur">Longueur de la feuille</param>
        public async void Draw(double largeur, double longueur, IRécupérationSegments modeRécupération)
        {
            // Si c'est le premier dessin, on initialise la position du robot au milieu de la feuille
            x = largeur / 2;
            y = longueur / 2;
            orientationActuelle = 0;

            // Récupération de la liste des segments actuelle & dessiner les segments
            List<Segment> ListeSegments = DrawKinect.Draw.getInstance().GetListAndClear(modeRécupération);
            // Dessine chaque segment
            foreach (Segment s in ListeSegments)
                await DrawSegment(s, largeur, longueur);
        }

        /// <summary>
        /// Dessine un segment
        /// </summary>
        /// <param name="segment">Le segment à dessiner</param>
        /// <param name="largeur">La largeur de la feuille sur laquelle on dessine</param>
        /// <param name="longueur">La longueur de la feuille sur laquelle on dessine</param>
        /// <returns></returns>
        private async Task DrawSegment(Segment segment, double largeur, double longueur)
        {
            // Calcule les coordonnées réelles du segment, selon la taille de la feuille
            Point p1 = CalculCoordonnéesRéelles(segment.x1, segment.y1, largeur, longueur);
            Point p2 = CalculCoordonnéesRéelles(segment.x2, segment.y2, largeur, longueur);

            // Oriente vers la première position
            double angleATourner = CalculAngleRobotPoint(p1);
            int angle = Convert.ToInt32(angleATourner);
            if (angle != 0)
            {
                if (angle > 180)
                    await TurnRobotLeft(360 - angle);
                else
                    await TurnRobotRight(angle);
                orientationActuelle = (orientationActuelle + angleATourner) % 360;
            }

            // Va à la première position
            double distance = CalculDistanceRobotPoint(p1);
            if (Convert.ToInt32(distance) != 0)
            {
                await MoveRobotForDistance(Convert.ToInt32(distance));
                x = p1.X;
                y = p1.Y;
            }

            // Positionne le robot dans la bonne direction
            angleATourner = CalculAngleRobotPoint(p2);
            angle = Convert.ToInt32(angleATourner);
            if (angle != 0)
            {
                if (angle > 180)
                    await TurnRobotLeft(360 - angle);
                else
                    await TurnRobotRight(angle);
                orientationActuelle = (orientationActuelle + angleATourner) % 360;
            }

            // Baisser le crayon, pour dessiner le segment
            await PenDown();

            // Aller à la 2ème position
            distance = CalculDistanceRobotPoint(p2);
            if (Convert.ToInt32(distance) != 0)
            {
                await MoveRobotForDistance(Convert.ToInt32(distance));
                x = p2.X;
                y = p2.Y;
            }

            // Lever le crayon
            await PenUp();
        }

        /// <summary>
        /// Calcul les coordonnées réelles à partir de la taille de le feuille et les coordonnées du point du segment
        /// </summary>
        /// <param name="x">x du point du segment (color point, de 640x480)</param>
        /// <param name="y">y du point du segment (color point, de 640x480)</param>
        /// <returns>Le point réel, selon les dimensions de la feuille</returns>
        private Point CalculCoordonnéesRéelles(double x, double y, double largeur, double longueur)
        {
            double nouveauX, nouveauY;
            
            nouveauX = (x * longueur) / 640;
            nouveauY = (y * largeur) / 480;

            return new Point(nouveauX, nouveauY);
        }

        /// <summary>
        /// Calcul l'angle que le robot doit effectuer pour regarder en direction du point
        /// </summary>
        /// <param name="p">Le point à regarder</param>
        /// <returns>L'angle à tourner</returns>
        private double CalculAngleRobotPoint(Point p)
        {
            if (x == p.X && y == p.Y)
                return 0;

            int coeff = 0;
            double alpha1;

            // +x & -y, ajout d'un coeff de 0°
            alpha1 = (180 * Math.Atan(Math.Abs(p.X - x) / Math.Abs(p.Y - y))) / Math.PI;
            // +x & +y, ajout d'un coeff de 90°
            if (p.X >= x && p.Y >= y)
            {
                coeff = 90;
                alpha1 = (180 * Math.Atan(Math.Abs(p.Y - y) / Math.Abs(p.X - x))) / Math.PI;
            }
            // - x & +y, ajout d'un coeff de 180°
            else if (p.X <= x && p.Y > y)
            {
                coeff = 180;
                alpha1 = (180 * Math.Atan(Math.Abs(p.X - x) / Math.Abs(p.Y - y))) / Math.PI;
            }
            // - x & -y, ajout d'un coeff de 270°
            else if (p.X <= x && p.Y <= y)
            {
                coeff = 270;
                alpha1 = (180 * Math.Atan(Math.Abs(p.Y - y) / Math.Abs(p.X - x))) / Math.PI;
            }

            double angleATourner = (coeff + alpha1) - orientationActuelle;

            if (angleATourner < 0)
                return angleATourner + 360;

            return angleATourner;
        }

        /// <summary>
        /// Calcul la distance entre le robot et le point (simple Pythagore)
        /// </summary>
        /// <param name="p">Le point vers lequel il faut avancer</param>
        /// <returns>La distance entre le robot et le point</returns>
        private double CalculDistanceRobotPoint(Point p)
        {
            double a = Math.Abs(p.Y - y);
            double b = Math.Abs(p.X - x);
            
            return Math.Sqrt(a * a + b * b);
        }

        /// <summary>
        /// Tourne le robot à droite
        /// </summary>
        /// <param name="angle">L'angle que le robot doit faire, en degrées</param>
        /// <returns></returns>
        private async Task TurnRobotRight(int angle)
        {
            int temps = (1450 * angle) / 90;
            uint tps = Convert.ToUInt32(temps);
            await GestionRobot.getInstance().brick.DirectCommand.TurnMotorAtPowerForTimeAsync(MoteurDroit, 50, tps, false);
            await GestionRobot.getInstance().brick.DirectCommand.TurnMotorAtPowerForTimeAsync(MoteurGauche, -50, tps, false);

            System.Threading.Thread.Sleep(temps);
        }

        /// <summary>
        /// Tourne le robot à gauche
        /// </summary>
        /// <param name="angle">L'angle que le robot doit effectuer</param>
        /// <returns></returns>
        private async Task TurnRobotLeft(int angle)
        {
            int temps = (1450 * angle) / 90;
            uint tps = Convert.ToUInt32(temps);
            await GestionRobot.getInstance().brick.DirectCommand.TurnMotorAtPowerForTimeAsync(MoteurGauche, 50, tps, false);
            await GestionRobot.getInstance().brick.DirectCommand.TurnMotorAtPowerForTimeAsync(MoteurDroit, -50, tps, false);

            System.Threading.Thread.Sleep(temps);
        }

        /// <summary>
        /// Fait avancer le robot sur une certaine distance
        /// </summary>
        /// <param name="distance">La distance que le robot doit faire</param>
        /// <returns></returns>
        private async Task MoveRobotForDistance(int distance)
        {
            int temps = (1000 * distance) / 11; // Produit en croix, sachant que puissance 50 et 1000ms = 11cm
            await MoveRobot(temps, 50);
        }

        /// <summary>
        /// Déplace le robot, en utilisant ses 2 moteurs
        /// </summary>
        /// <param name="temps">Le temps durant lequel il doit avancer</param>
        /// <param name="puissance">La puissance à laquelle il doit avancer</param>
        private async Task MoveRobot(int temps, int puissance)
        {
            uint tps = Convert.ToUInt32(temps);
            await GestionRobot.getInstance().brick.DirectCommand.TurnMotorAtPowerForTimeAsync(MoteurGauche, puissance + 1, tps, false);
            await GestionRobot.getInstance().brick.DirectCommand.TurnMotorAtPowerForTimeAsync(MoteurDroit, puissance, tps, false);

            System.Threading.Thread.Sleep(temps);
        }
    }
}