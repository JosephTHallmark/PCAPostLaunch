using System;
using System.Collections.Generic;
using System.Drawing;

namespace PCAapp {
    /// <summary>
    /// Class used to simplify outputing the covar matrix
    /// </summary>
    class adder {
        double iter;
        Point location;

        public double Value {
            get { return iter; }
        }

        public adder(double itr, int y, int x) {
            iter = itr;
            Point pnt = new Point(y, x);
            location = pnt;
        }

        public adder(double itr, Point pnt) {
            iter = itr;
            location = pnt;
        }

        public override string ToString() {
            string output = iter.ToString("F4").PadLeft(7) + " [" + (location.X + 1).ToString().PadRight(2) + "," + (location.Y + 1).ToString().PadRight(2) + "]";
            return output.PadRight(15);
        }
    }

    /// <summary>
    /// Class to store both eigenValues and eigenVectors
    /// </summary
    public class eigens {
        public double[,] matrix;
        public double[] eigenValues;
        /// <summary>
        /// [dimension, vector]
        /// each vector is comprised of X dimensions
        /// </summary>
        //  Vector1 Vector2 Vector3
        //  dim 1   dim 1   dim 1
        //  dim 2   dim 2   dim 2
        //  dim 3   dim 3   dim 3
        //  dim 4   dim 4   dim 4
        //  dim 5   dim 5   dim 5
        //  ...     ...     ... 
        //  dim 12  dim 12  dim 12
        public double[,] eigenVectors;
        /// <summary>
        /// The 3 eigenVectors with the highest eigenValues will become the new dimensions
        /// </summary>
        public double[,] top3;
    }

    static class DataStructure {
        // Privates
        /// <summary>
        /// Number of elements
        /// </summary>
        static int size;
        /// <summary>
        /// Number of dimensions
        /// </summary>
        static int width; 
        //Data
        static List<List<double>> data; //this is created via window.cs
        static List<List<double>> dataTransposed;
        static List<double> means = new List<double>();
        static List<string> labels;

        //Zeroed data
        /// Floating point operations have inaccuracies 
        /// when subtracting the mean from the data set 
        /// to produce a data set for PCA 
        /// its always off by a tiny bit
        static List<List<double>> zeroData = new List<List<double>>();
        static List<List<double>> zeroDataTransposed = new List<List<double>>();
        static List<List<double>> finalData = new List<List<double>>(); // this will be the new data that is under 3 dimensions and is plot-able
        static List<List<double>> finalDataRealigned;
        static List<double> zeroMeans = new List<double>();
        static List<double> zeroVariance = new List<double>();
        static List<List<adder>> zeroCoVar = new List<List<adder>>();

        // Privates for eignevector stuff
        //static alglib.sparsematrix matrix;
        //static eigens normalEigens = new eigens(); //see *//
        static eigens zeroEigens = new eigens();

        /// <summary>
        /// Top 3 eigen vectors based on their value. vector = col
        /// </summary>
        static List<List<double>> featureVector = new List<List<double>>(); 

        // Publics
        static public List<List<double>> Data{
            get { return data; }
            set { data = value;
                size = data.Count; //sets size (number of elements)
                width = data[0].Count; //sets the number of dimensions(length of each element)
            }
        }

        static public List<String> Labels {
            get { return labels; }
            set { labels = value; }
        }

        static public int Width {
            get { return width; }
        }

        static public int Size {
           get { return size; }
        }

        static public List<List<double>> FinalDataRealigned {
            get { return finalDataRealigned; }
            set { finalDataRealigned = value; }
        }

        static public List<List<double>> FinalData {
            get { return finalData; }
        }

        static public List<List<double>> FeatureVectors {
            get { return featureVector; }
            set { featureVector = value; }
        }

        // Methods
        ///<summary>
        ///top level call of all process functions
        ///</summary>
        static public void Process() {
            dataTransposed = transpose(data);
            means = findMeans(data);
            zeroMatrix();
            zeroMeans = findMeans(zeroData);

            //variance = findVariance(data, means);//see *//
            zeroVariance = findVariance(zeroData, zeroMeans);

            //CoVar = findCovariance(data, means);//see *//
            zeroCoVar = findCovariance(zeroData, zeroMeans);

            //findEigenVectors(CoVar, normalEigens);// this is where zeroing out the data set shows differences 
            findEigenVectors(zeroCoVar, zeroEigens);// see *//

            featureVector = formFeatureVector(zeroEigens);

            zeroDataTransposed = transpose(zeroData);
            finalData = createFinalData(dataTransposed);
            finalDataRealigned = transpose(finalData);
        }

        static private List<double> findMeans(List<List<double>> input) {
            //Means
            List<double> output = new List<double>();
            //int width = data[0].Count;
            //int height = data.Count;
            // Foreach Col, process Rows
            for (int x = 0; x < width; x++) { // Cols
                double sum = 0;
                //for (int y = 0; y < height; y++) { // Rows
                for (int y = 0; y < size; y++) {
                    sum += input[y][x];
                }
                double mean = new double();
                //mean = sum / height;
                mean = sum / size;
                output.Add(mean);
            }
            return output;
        }

        static private void zeroMatrix() {
            for (int i = 0; i < size; i++) {
                List<double> line = new List<double>();
                for (int j = 0; j < width; j++) {
                    line.Add(Math.Round(data[i][j] - means[j],6));
                    //line.Add(data[i][j] - means[j]);
                }
                zeroData.Add(line);
            }
        }        

        static private List<double> findVariance(List<List<double>> input, List<double> meanList) {
            List<double> output = new List<double>();
            // Foreach Col, process Rows
            for (int x = 0; x < width; x++) { // Cols
                double sum = 0;
                double square = 0;
                double subMean = 0;
                double vary = new double();
                for (int y = 0; y < size; y++) { // Rows
                    subMean = input[y][x] - meanList[x];
                    square = subMean;
                    square *= square;
                    sum += square;
                }
                vary = sum / (size - 1);
                output.Add(vary);
            }
            return output;
        }

        static private List<List<adder>> findCovariance(List<List<double>> input, List<double> meanList) {
            List<List<adder>> output = new List<List<adder>>();
            for (int y = 0; y < width; y++) { // for each row (in the 12x12 covariance matrix              
                List<adder> line = new List<adder>();
                for (int x = 0; x < width; x++) { // for each dimension
                    double sum = new double();
                    double iterX;
                    double iterY;
                    double iter = new double();
                    for (int i = 0; i < size; i++) { //sum of (Xi-Xmean)*(Yi-Ymean)/(size-1)
                        iterX = input[i][x]; // - meanList[x];  //These two lines are likely unneeded since 
                        iterY = input[i][y]; // - meanList[y];  //PCA already requires a dataset with mean of zero
                        iter = iterX * iterY;
                        sum += iter;
                    }
                    sum = sum / (size - 1);
                    adder temp = new adder(sum, y, x);
                    line.Add(temp);
                }
                output.Add(line);
            }
            return output;
        }

        static private void findEigenVectors(List<List<adder>> inputMatrix, eigens output) {
            //create a matrix array from the list<list<double>>
            double[] valuesOut;
            double[,] vectorsOut;
            output.matrix = new double[width, width];
            listToDouble(inputMatrix, output);

            alglib.smatrixevd(output.matrix, width, 1, false, out valuesOut, out vectorsOut);

            output.eigenValues = valuesOut;
            output.eigenVectors = vectorsOut;
        }

        private static void listToDouble(List<List<adder>> inputMatrix, eigens output) {
            for (int i = 0; i < inputMatrix.Count; i++) {
                for (int j = 0; j < inputMatrix.Count; j++) {
                    output.matrix[i, j] = inputMatrix[i][j].Value;
                }
            }
        }

        private static double[,] listToDouble(List<List<double>> inputMatrix) {
            double[,] output = new double[inputMatrix.Count, inputMatrix[0].Count];
            for (int i = 0; i < inputMatrix.Count; i++) {
                for (int j = 0; j < inputMatrix[0].Count; j++) {
                    output[i, j] = inputMatrix[i][j];
                }
            }
            return output;
        }

        private static List<List<double>> formFeatureVector(eigens input) {
            List<List<double>> output = new List<List<double>>();
            //!! Concern, do eigen values of a greater absolute value while negative supersede positive values? 

            // Alglib eigenVectors and values are sorted in ascending order, so last 3 entries will be the 3 highest vectors
            //eigenVector   [dimension, vector]
            //              [row,col]
            //  Vector1 Vector2 Vector3 ... Vector 12
            //  dim 1   dim 1   dim 1
            //  dim 2   dim 2   dim 2
            //  dim 3   dim 3   dim 3
            //  dim 4   dim 4   dim 4
            //  dim 5   dim 5   dim 5
            //  ...     ...     ... 
            //  dim 12  dim 12  dim 12
            
            //this makes the feature vector matrix with 3 rows of one eigenvector per
            //it is used like this when translating the data to new dimensions in pca
            for (int i = width - 1; i > width - 4; i--) {
                List<double> line = new List<double>();
                for(int j = 0; j < width; j++) { 
                    line.Add(input.eigenVectors[j, i]);
                }
                output.Add(line);
            }
            return output;
        }

        /// <summary>
        /// Takes an input of List(List(double)) as input and flips rows and cols
        /// </summary>
        /// <param name="input">Input list</param>
        /// <returns>Input list transposed</returns>
        public static List<List<double>>transpose(List<List<double>> input) {
            List<List<double>> output = new List<List<double>>();

            int width = input[0].Count;
            int size = input.Count;

            for(int i = 0; i < width; i++) {//cols
                List<double> line = new List<double>();
                for (int j = 0; j < size; j++) {//rows
                    line.Add(input[j][i]);
                }
                output.Add(line);
            }

            return output;
        }

        public static List<List<double>> createFinalData(List<List<double>> transposedInput) {
            //this can probably be reused for user input, instead of inputing an X element reference, have 1 element of transposed data
            List<List<double>> output = new List<List<double>>();
            double[,] feature = listToDouble(featureVector);
            int m = featureVector.Count;
            double[,] input = listToDouble(transposedInput);
            int n = transposedInput[0].Count;
            double[,] final = new double[m, n];
            int k = featureVector[0].Count;

            //op1(A) (which is MxK) 3*12 feature
            //op2(B) (which is KxN) 12*5 input
            //C (which is MxN) final = 3 * 5 

            alglib.rmatrixgemm(m, n, k, 1, feature, 0, 0, 0, input, 0, 0, 0, 0, ref final, 0, 0);
            for (int i = 0; i < m; i++) {
                List<Double> line = new List<double>();
                for (int j = 0; j < n; j++) {
                    line.Add(final[i, j]);
                }
                output.Add(line);
            }

            return output;
        }

        public static void clear() {    
            //resets all the data
            //simple fix for errors created by opening files. 
            //could be coded better that data isnt left over from 
            //previous files
            size = new int();
            width = new int();
            means = new List<double>();
            zeroData = new List<List<double>>();
            zeroDataTransposed = new List<List<double>>();
            finalData = new List<List<double>>(); // this will be the new data that is under 3 dimensions and is plot-able
            finalDataRealigned.Clear();
            zeroMeans = new List<double>();
            zeroVariance = new List<double>();
            zeroCoVar = new List<List<adder>>();
            zeroEigens = new eigens();
            featureVector = new List<List<double>>();
        }

        #region ToStrings
        /// ToStrings
        static public string toString() {
            string output = "";
            //Data
            for (int i = 0; i < data.Count; i++) { //step over sets(rows)
                output += "Set: ".PadRight(5) + "[" + data[i][0].ToString().PadRight(9) + ",";
                for (int j = 1; j < data[i].Count - 1; j++) { //step over items in sets(cols)
                    output += data[i][j].ToString().PadRight(9) + ",";
                }
                output += data[i][data[i].Count - 1].ToString().PadRight(9) + "]\r\n";
            }
            //Mean
            output += "Mean:".PadRight(6);
            foreach (double item in means) {
                output += item.ToString("F7").PadRight(10);
            }
            output += "\r\n";

            ////Variance
            //output += "Vari:".PadRight(6);
            //foreach (double item in variance) {
            //    output += item.ToString("F7").PadRight(10);
            //}
            //output += "\r\n"; 
            //see *//

            //Data
            output += "This is the dataset after it has been centered on its mean as required by PCA \r\n";
            for (int i = 0; i < data.Count; i++) { //step over sets(rows)
                output += "Set: ".PadRight(5) + "[" + zeroData[i][0].ToString().PadRight(9) + ",";
                for (int j = 1; j < zeroData[i].Count - 1; j++) { //step over items in sets(cols)
                    output += zeroData[i][j].ToString().PadRight(9) + ",";
                }
                output += zeroData[i][data[i].Count - 1].ToString().PadRight(9) + "]\r\n";
            }
            //Mean
            output += "Mean:".PadRight(6);
            foreach (double item in zeroMeans) {
                output += item.ToString("F7").PadRight(10);
            }
            output += "\r\n";

            //Variance
            output += "Vari:".PadRight(6);
            foreach (double item in zeroVariance) {
                output += item.ToString("F7").PadRight(10);
            }
            output += "\r\n\r\n";

            output += "This is the data set transposed for use in PCA \r\n\r\n";
            for (int i = 0; i < zeroDataTransposed.Count; i++) { //step over sets(rows)
                output += "Set: ".PadRight(5) + "[" + zeroDataTransposed[i][0].ToString().PadRight(9) + ",";
                for (int j = 1; j < zeroDataTransposed[i].Count - 1; j++) { //step over items in sets(cols)
                    output += zeroDataTransposed[i][j].ToString().PadRight(9) + ",";
                }
                output += zeroDataTransposed[i][zeroDataTransposed[i].Count - 1].ToString().PadRight(9) + "]\r\n";
            }

            return output;
        }

        static public string CoVarToString() {
            string output = "";

            //for (int i = 0; i < CoVar.Count; i++) { //step over rows
            //    for(int j = 0; j < CoVar[0].Count; j++) { // cols
            //        output += CoVar[i][j].ToString();
            //    }
            //    output += "\r\n";
            //} //see *//

            ///
            ///This shows that even if you zero out the data as the PCA paper described you get the same covariance matrix
            ///Why can PCA only work on sets that have a mean of zero, Wonder if it is in the transposing to a new dimension?
            ///
            output += "Covariance: \r\n";

            for (int i = 0; i < zeroCoVar.Count; i++) { //step over rows
                for (int j = 0; j < zeroCoVar[0].Count; j++) { // cols
                    output += zeroCoVar[i][j].ToString();
                }
                output += "\r\n";
            }

            return output;
        }

        static public string eigenToString() {
            string output = "";

            //for (int i = 0; i < width; i++) {
            //    output += "Eigen Value: " + normalEigens.eigenValues[i].ToString("F5").PadRight(8) + " [";
            //    for (int j = 0; j < width - 1; j++) {
            //        output += normalEigens.eigenVectors[i, j].ToString("F5").PadRight(8) + ",";
            //    }
            //    output += normalEigens.eigenVectors[i, width - 1].ToString("F5").PadRight(8) + "]\r\n";
            //} //see *//

            output += "Zeroed Data\r\n";
            
            // Value [Vector]=> for every col print every row 
            for (int i = 0; i < width; i++) {//col
                output += "Eigen Value: " + zeroEigens.eigenValues[i].ToString("F5").PadRight(8) + " [";
                for(int j = 0; j < width - 1; j++) {//row
                    output += zeroEigens.eigenVectors[j,i].ToString("F5").PadRight(8) + ",";
                }
                output += zeroEigens.eigenVectors[width - 1, i].ToString("F5").PadRight(8) + "]\r\n";
            }

            output += "\r\nFeatureVector(Highest to Lowest)\r\n";

            for (int i = 0; i < 3; i++) {//cols
                output += '[';
                for (int j = 0; j < width - 1; j++) {//rows
                    output += featureVector[i][j].ToString("F5").PadRight(8) + ",";
                }
                output += featureVector[i][width - 1].ToString("F5").PadRight(8) + "]\r\n";
            }

            output += "\r\n\r\nThis is the data set translated to 3 dimensions\r\n";

            for(int i = 0; i < finalData.Count; i++) {
                output += '[';
                for(int j = 0; j < finalData[0].Count - 1; j++) {
                    output += finalData[i][j].ToString("F4").PadLeft(7) + ",";
                }
                output += finalData[i][finalData[0].Count - 1].ToString("F4").PadLeft(7) + "]\r\n";
            }

            output += "\r\n\r\nThis is the realigned data set translated to 3 dimensions\r\n";

            for (int i = 0; i < finalDataRealigned.Count; i++) {
                output += '[';
                for (int j = 0; j < finalDataRealigned[0].Count - 1; j++) {
                    output += finalDataRealigned[i][j].ToString("F4").PadLeft(7) + ",";
                }
                output += finalDataRealigned[i][finalDataRealigned[0].Count - 1].ToString("F4").PadLeft(7) + "]\r\n";
            }

            return output;
        }
        #endregion
    }
}

