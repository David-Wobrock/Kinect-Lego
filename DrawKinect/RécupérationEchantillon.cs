using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrawKinect
{
    public class RécupérationEchantillon : IRécupérationSegments
    {
        public List<Segment> GetList(List<Segment> ListeSegments)
        {
            List<Segment> MaSegmentListe = new List<Segment>();
            List<Segment> CopieDeLaListe = new List<Segment>(ListeSegments);
            int cpt = 0;
            const int espaceAutourDuPointPrécédent = 15, ajouterToutLes = 4;
            bool vientDetreAjoute = false;

            Segment segmentAAjouter = CopieDeLaListe.First();
            CopieDeLaListe.RemoveAt(0);
            foreach (Segment s in CopieDeLaListe)
            {
                if (vientDetreAjoute)
                {
                    segmentAAjouter = s;
                    vientDetreAjoute = false;
                }

                // Si le segment suivant à côté, on allonge le segment que l'on va ajouter
                if (segmentAAjouter.x2 - espaceAutourDuPointPrécédent < s.x1 && segmentAAjouter.x2 + espaceAutourDuPointPrécédent > s.x1
                    &&
                    segmentAAjouter.y2 - espaceAutourDuPointPrécédent < s.y1 && segmentAAjouter.y2 + espaceAutourDuPointPrécédent > s.y1)
                {
                    segmentAAjouter = new Segment(segmentAAjouter.x1, segmentAAjouter.y1, s.x2, s.y2);
                    cpt = (cpt + 1) % ajouterToutLes;

                    if (cpt == (ajouterToutLes - 1))
                    {
                        MaSegmentListe.Add(segmentAAjouter);
                        vientDetreAjoute = true;
                    }
                }
                // Sinon, le prochain segment est loin, donc on ajoute l'actuel et recommence (et cpt à 0);
                else
                {
                    MaSegmentListe.Add(segmentAAjouter);
                    vientDetreAjoute = true;
                    cpt = 0;
                }
            }

            return MaSegmentListe;
        }
    }
}
