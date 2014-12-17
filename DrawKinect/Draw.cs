using System.Collections.Generic;
using System.Linq;

namespace DrawKinect
{
    public class Draw
    {
        /// <summary>
        /// La liste des segments dessinée par les gestes devant la Kinect
        /// </summary>
        private List<Segment> ListeSegments = new List<Segment>();

        /// <summary>
        /// L'instance unique de la classe Draw
        /// </summary>
        private static Draw instance = null;

        /// <summary>
        /// Le constructeur privé de Draw
        /// </summary>
        private Draw()
        {}

        /// <summary>
        /// Retourne l'instance unique de Draw
        /// </summary>
        /// <returns>L'instance unique de Draw</returns>
        public static Draw getInstance()
        {
            if (instance == null)
                instance = new Draw();
            return instance;
        }

        /// <summary>
        /// Permet d'ajouter un segment à la liste des segments
        /// </summary>
        /// <param name="s">Le segment à ajouter</param>
        public void Add(Segment s)
        {
            ListeSegments.Add(s);
        }

        /// <summary>
        /// Renvoie la liste de segments et vide la liste
        /// </summary>
        /// <returns>La liste de segments</returns>
        public List<Segment> GetListAndClear(IRécupérationSegments modeRécupération)
        {
            List<Segment> MaSegmentListe = GetList(modeRécupération);

            ListeSegments.Clear();

            return MaSegmentListe;
        }

        /// <summary>
        /// Récupère la liste à dessiner par le robot
        /// </summary>
        /// <returns></returns>
        public List<Segment> GetList(IRécupérationSegments modeRécupération)
        {
            return modeRécupération.GetList(ListeSegments);
        }

        /// <summary>
        /// Vide la liste de segment
        /// </summary>
        public void Clear()
        {
            ListeSegments.Clear();
        }
    }
}
