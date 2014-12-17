namespace DrawKinect
{
    public class Segment
    {
        public double x1
        {
            get;
            private set;
        }

        public double y1
        {
            get;
            private set;
        }

        public double x2
        {
            get;
            private set;
        }

        public double y2
        {
            get;
            private set;
        }

        public Segment(double X1, double Y1, double X2, double Y2)
        {
            x1 = X1;
            y1 = Y1;
            x2 = X2;
            y2 = Y2;
        }
    }
}
