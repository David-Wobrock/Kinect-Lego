using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrawKinect
{
    public class RécupérationPointsDistants : IRécupérationSegments
    {
        public List<Segment> GetList(List<Segment> ListeSegments)
        {
            List<Segment> MaSegmentListe = new List<Segment>();
            List<Segment> CopieDeLaListe = new List<Segment>(ListeSegments);
            const int distanceDuSegment = 15;

            Segment segmentActuel = CopieDeLaListe.First();
            CopieDeLaListe.RemoveAt(0);
            foreach (Segment s in CopieDeLaListe)
            {
                // Si le point est trop loin de la droite, on ajoute le segment en cours
                if (DistancePointDroite(segmentActuel.x1, segmentActuel.y1, s.x2, s.y2, s.x1, s.y1) > distanceDuSegment)
                {
                    MaSegmentListe.Add(segmentActuel);
                    segmentActuel = s;
                }
                // Sinon on agrandit le segment en cours
                else
                    segmentActuel = new Segment(segmentActuel.x1, segmentActuel.y1, s.x2, s.y2);
            }
            // On ajoute dans tous les cas le dernier segment
            MaSegmentListe.Add(segmentActuel);

            return MaSegmentListe;
        }

        /// <summary>
        /// Calcul la distance du point M à la droite A1A2
        /// </summary>
        /// <param name="A1x">x de A1</param>
        /// <param name="A1y">y de A1</param>
        /// <param name="A2x">x de A2</param>
        /// <param name="A2y">y de A2</param>
        /// <param name="Mx">x de M</param>
        /// <param name="My">y de M</param>
        /// <returns>La distance entre le point M et la droite A1A2</returns>
        private double DistancePointDroite(double A1x, double A1y, double A2x, double A2y, double Mx, double My)
        {
            // Calcul de l'équation de la droite A1A2, de la forme : ax + by + c = 0
            double a = A2y - A1y;
            double b = A2x - A1x;
            double c = -(b * A1y + a * A1x);

            // Calcul de la distance D(A1A2, M)
            double distanceNominateur = Math.Abs(a * Mx + b * My + c);
            double distanceDénominteur = Math.Sqrt((a * a) + (b * b));

            return distanceNominateur / distanceDénominteur;
        }
    }
}
