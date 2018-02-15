using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SeismosFilteringLibrary
{
    public class ZeroPhaseButterworthFilter
    {

        #region Properties
        private double cutOffFrequency;

        public double CutOffFrequency
        {
            get { return cutOffFrequency; }
            set { cutOffFrequency = value; }
        }

        private double sampleRate;

        public double SampleRate
        {
            get { return sampleRate; }
            set { sampleRate = value; }
        }

        const double pi = 3.14159265358979;
        
        #endregion
        #region Constructors
        public ZeroPhaseButterworthFilter() { }
        public ZeroPhaseButterworthFilter(double coFreq)
        {
            cutOffFrequency = coFreq;
        }
        #endregion
        #region Methods
        public double[] ApplyFilter(double[] dataIn)
        {
            double[] dataOut = new double[dataIn.Length];
            long lastSampleIndex = dataIn.Length - 1;
            double[] extendedTempArray = new double[lastSampleIndex + 2];
            double cornerFrequency = Math.Tan(cutOffFrequency * pi / sampleRate);
            double c1 = Math.Sqrt(2) * cornerFrequency;
            double c2 = Math.Pow(cornerFrequency, 2);
            double a = c1 / (1 + c1 + c2);
            double b = 2 * a;
            double c = a;
            double c3 = b / c2;
            double d = -2 * a + c3;
            double e = 1 - (2 * a) - c3;

            // Make sure we actually need to apply a filter.... ////////////
            if (CutOffFrequency == 0)
            {
                dataOut = dataIn;
                return dataOut;
            }
            ////////////////////////////////////////////////////////////////

            // Fill the extended array... //////////////////////////////////
            extendedTempArray[0] = dataIn[0];
            extendedTempArray[1] = dataIn[0];
            extendedTempArray[lastSampleIndex + 2] = dataIn[lastSampleIndex];
            extendedTempArray[lastSampleIndex + 3] = dataIn[lastSampleIndex];
            for ( long i = 0; i < lastSampleIndex; i++)
            {
                extendedTempArray[2 + i] = dataIn[i];
            }
            /////////////////////////////////////////////////////////////////

            //// Build Recursive Filter ///////////////////////////////////////
            double[] recursiveArray = new double[lastSampleIndex + 4];
            recursiveArray[0] = dataIn[0];
            recursiveArray[1] = dataIn[1];
            for (long i = 2; i < lastSampleIndex + 2; i++)
            {
                recursiveArray[i] = a * extendedTempArray[i] + b * extendedTempArray[i - 1] + 
                    c * extendedTempArray[i - 2] + d * extendedTempArray[i - 1] + 
                    e * extendedTempArray[i - 2];
            }
            recursiveArray[lastSampleIndex + 2] = recursiveArray[lastSampleIndex + 1];
            recursiveArray[lastSampleIndex + 3] = recursiveArray[lastSampleIndex + 1];
            ///////////////////////////////////////////////////////////////////

            /////// Apply filter //////////////////////////////////////////////
            double[] tempFilteredArray = new double[lastSampleIndex + 2];
            tempFilteredArray[lastSampleIndex] = recursiveArray[lastSampleIndex + 2];
            tempFilteredArray[lastSampleIndex + 1] = recursiveArray[lastSampleIndex + 3];
            for (long i = -lastSampleIndex + 1; i <= 0; i++)
            {
                tempFilteredArray[-i] = a * recursiveArray[-i + 2] + b * recursiveArray[-i + 3] + 
                    c * recursiveArray[-i + 4] + d * tempFilteredArray[-i + 1] + 
                    e * tempFilteredArray[-i + 2];
            }
            ////////////////////////////////////////////////////////////////////

            //////////// Populate Output Array /////////////////////////////////
            for(long i = 0; i < lastSampleIndex; i++)
            {
                dataOut[i] = tempFilteredArray[i];
            }

            return dataOut;
        }
        #endregion


    }
}
