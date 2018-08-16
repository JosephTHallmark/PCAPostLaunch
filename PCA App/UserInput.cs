using System;
using System.Collections.Generic;

namespace PCAapp {

    static class UserInput {
        // Privates
        // Should only be one element of X dimensions
        // this is a list of list for the transpose
        static List<List<double>> data;
        static List<List<double>> transData; // data that has been transposed

        static List<List<double>> finalData; // transData that has been translated through the feature vector of data structure
                                             // Constructor
        static List<List<double>> finalDataRealigned;
        static string output = "Please open a user input file";  //Used for output if user input was improper size 

        static List<double> euclideanDistances; //this holds the euclidean distance of the user input element from all reference elements

        public static int closestIndex;
        public static double closestDist;

        // Publics
        static public List<List<double>> Data {
            get { return data; }
            set { data = value; }
        }

        static public List<List<double>> FinalDataRealinged {
            get { return finalDataRealigned; }
        }

        // Methods
        static public void Process() {
            euclideanDistances = new List<double>();
            transData = DataStructure.transpose(data);
            finalData = DataStructure.createFinalData(transData);
            finalDataRealigned = DataStructure.transpose(finalData);
            eDistances();
            match();
            //parse();
        }

        static public void maxDisProcess() {
            transData = DataStructure.transpose(data);
            finalData = DataStructure.createFinalData(transData);
            finalDataRealigned = DataStructure.transpose(finalData);
        }

        static private void eDistances() {
            //This is the line that will need to be updated when the data reader for student mode is created 
            List<List<double>> reference = DataStructure.FinalDataRealigned;
            for (int i = 0; i < reference.Count; i++) {// for each element (row) in reference data
                double xdiff = finalDataRealigned[0][0] - reference[i][0];
                double ydiff = finalDataRealigned[0][1] - reference[i][1];
                double zdiff = finalDataRealigned[0][2] - reference[i][2];

                xdiff = Math.Pow(xdiff, 2);
                ydiff = Math.Pow(ydiff, 2);
                zdiff = Math.Pow(zdiff, 2);

                double total = xdiff + ydiff + zdiff;
                total = Math.Sqrt(total);

                euclideanDistances.Add(total);
            }
        }

        static private void match() {
            int minIndex = -1;
            double minDist = double.PositiveInfinity;
            for(int i = 0; i < euclideanDistances.Count; i++) {
                if (euclideanDistances[i] < minDist) {
                    minIndex = i;
                    minDist = euclideanDistances[i];
                }
            }
            closestIndex = minIndex;
            closestDist = minDist;
        }

        //static private void parse() {
        //    output = "";
        //    //Data
        //    for (int i = 0; i < data.Count; i++) { //step over sets(rows)
        //        output += "Set: ".PadRight(5) + "[" + data[i][0].ToString().PadRight(9) + ",";
        //        for (int j = 1; j < data[i].Count - 1; j++) { //step over items in sets(cols)
        //            output += data[i][j].ToString().PadRight(9) + ",";
        //        }
        //        output += data[i][data[i].Count - 1].ToString().PadRight(9) + "]\r\n";
        //    }

        //    output += "\r\nThis is the transposed data\r\n";

        //    for (int i = 0; i < transData.Count; i++) { //step over sets(rows)
        //        output += "Set: ".PadRight(5) + "[" + transData[i][0].ToString("F4").PadRight(9) + "]\r\n";
        //    }

        //    output += "\r\nThis is the final data\r\n";

        //    for (int i = 0; i < finalData.Count; i++) { //step over sets(rows)
        //        output += "Set: ".PadRight(5) + "[" + finalData[i][0].ToString("F4").PadRight(9) + "]\r\n";                
        //    }

        //    output += "\r\nThis is the realigned final data\r\n";

        //    for (int i = 0; i < finalDataRealigned.Count; i++) { //step over sets(rows)
        //        output += "Set: ".PadRight(5) + "[" + finalDataRealigned[i][0].ToString("F4").PadRight(9) + ",";
        //        for (int j = 1; j < finalDataRealigned[i].Count - 1; j++) { //step over items in sets(cols)
        //            output += finalDataRealigned[i][j].ToString("F4").PadRight(9) + ",";
        //        }
        //        output += finalDataRealigned[i][finalDataRealigned[i].Count - 1].ToString("F4").PadRight(9) + "]\r\n";
        //    }

        //    output += "\r\n\r\nThese are euclidean distances from each of the reference elements\r\n";

        //    for (int i = 0; i < euclideanDistances.Count; i++) {
        //        output += "[" + euclideanDistances[i].ToString("F4") + "]\r\n";
        //    }
            
        //    output += "\r\n\r\nYour result\r\nYou were closest to [";
        //    for(int i = 0; i < DataStructure.Data[closestIndex].Count - 1; i++) {
        //        output += DataStructure.Data[closestIndex][i].ToString() + ',';
        //    }
        //    output += DataStructure.Data[closestIndex][DataStructure.Data[closestIndex].Count - 1] + "]\r\n";
        //    output += "With a distance of " + closestDist;

        //}

        static public string toString() {
            return output;
        }
    }
}

