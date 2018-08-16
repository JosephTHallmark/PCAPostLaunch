using System.Collections.Generic;
using System;

namespace PCA_with_number_files {
    static class DataReader {
        // Privates
        static string filePath;
        static List<List<double>> data;
        static List<double> line;
        // Constructor
        
        // Publics
        static public string FilePath {
            get { return filePath; }
            set { filePath = value;
                parse(); }
        }
        static public List<List<double>> Data{
            get { return data; }
        }
        // Methods
        static void parse() {
            try {
                data = new List<List<double>>();
                line = new List<double>();
                string[] lines = System.IO.File.ReadAllLines(filePath);
                for (int i = 0; i < lines.Length; i++) {
                    line = new List<double>(); //do this to prevent changing records already placed in a list
                    foreach (string token in lines[i].Split(',')) {
                        line.Add(Math.Round(double.Parse(token),4));
                    }
                    data.Add(line);
                }
            } catch (Exception e) {
                throw new Exception("Your data file is probably not double, double, double\n", e);
                Environment.Exit(1);
            }
            //try catch and still produce and error state if text file is blank             
        }
    }
}

