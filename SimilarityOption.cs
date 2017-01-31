using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace hw3
{
    public enum SimilarityFunctions
    {
        Jaccard,
        Cosine,
        LSquare
    }

    public enum SimilarityModes
    {
        AllWayAverage,
        MostNearestNeighbor
    }

    public class SimilarityOption
    {
        [Option("dataFile", DefaultValue = "data50.csv", HelpText = "Data file")]
        public string DataFile { get; set; }

        [Option("labelFile", DefaultValue = "label.csv", HelpText = "Label file")]
        public string LabelFile { get; set; }

        [Option("groupsFile", DefaultValue = "groups.csv", HelpText = "Groups file")]
        public string GroupsFile { get; set; }

        [Option("similarityFunction", DefaultValue = SimilarityFunctions.Jaccard, HelpText = "Similarity function choice")]
        public SimilarityFunctions SimilarityFunction { get; set; }

        [Option("similarityMode", DefaultValue = SimilarityModes.AllWayAverage, HelpText = "Similarity mode choice")]
        public SimilarityModes SimilarityMode { get; set; }
    }
}
