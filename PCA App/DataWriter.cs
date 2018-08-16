using System.Collections.Generic;
using System.Linq;

namespace PCAapp
{

    public class DataWriter {
        public string filepath;
        static int dimensionSize;
        static int numberOfPics;
        static List<string> labels;
        static List<List<double>> vectors;
        static List<List<double>> finalData;

        public string Filepath {
            set { filepath = value; }
        }

        public int Dimensionsize {
            set { dimensionSize = value; }

            get { return dimensionSize; }
        }

        public int NumberOfPics {
            get { return numberOfPics; }
            set { numberOfPics = value; }
        }

        public List<string> Labels {
            set { labels = value; }
        }

        public List<List<double>> Vectors {
            set { vectors = value; }

            get { return vectors; }
        }

        public List<List<double>> FinalData {
            set { finalData = value; }
            get { return finalData;  }
        }

        public void writeDimension() {
            System.IO.File.WriteAllText(filepath, dimensionSize + "\n" + numberOfPics + "\n");
        }

        public void writeLabels() {
            for (int i = 0; i < labels.Count(); i++)
            {
                System.IO.File.AppendAllText(filepath, labels[i] + "\n");
            }
        }

        public void writeVectors() {
            for (int i = 0; i < vectors.Count(); i++)
            {
                for (int j = 0; j < vectors[1].Count()-1; j++)
                {
                    System.IO.File.AppendAllText(filepath, vectors[i][j] + ",");
                }
                System.IO.File.AppendAllText(filepath, vectors[i][vectors[1].Count() - 1].ToString());//so there is no trailing space character
                System.IO.File.AppendAllText(filepath, "\n");
            }
        }

        public void writeFinalDataRealigned() {
            for (int i = 0; i < finalData.Count(); i++)
            {
                for (int j = 0; j < finalData[1].Count() - 1; j++)
                {
                    System.IO.File.AppendAllText(filepath, finalData[i][j] + ",");
                }
                System.IO.File.AppendAllText(filepath, finalData[i][finalData[1].Count() - 1].ToString());//so there isnt a trailing space character
                System.IO.File.AppendAllText(filepath, "\n");
            }
        }
    }
}