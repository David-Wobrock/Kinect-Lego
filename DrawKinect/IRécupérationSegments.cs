using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrawKinect
{
    public interface IRécupérationSegments
    {
        /// <summary>
        /// Récupère la liste de segments à dessiner de la façon que l'on veux
        /// </summary>
        /// <returns></returns>
        List<Segment> GetList(List<Segment> ListeSegments);
    }
}
