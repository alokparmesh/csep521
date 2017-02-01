using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace hw3
{
    public enum SimilarityFunction
    {
        Jaccard,
        Cosine,
        LSquare
    }

    public enum ExecutionMode
    {
        AllWayAverage,
        MostNearestNeighbor,
        MostNearestNeighborWithRandomProjection,
        RandomProjectionCosineScatter
    }

    public enum RandomProjectionMode
    {
        Gaussian,
        Binary
    }

    public class ExecutionOption
    {
        [Option("dataFile", DefaultValue = "data50.csv", HelpText = "Data file")]
        public string DataFile { get; set; }

        [Option("labelFile", DefaultValue = "label.csv", HelpText = "Label file")]
        public string LabelFile { get; set; }

        [Option("groupsFile", DefaultValue = "groups.csv", HelpText = "Groups file")]
        public string GroupsFile { get; set; }

        [Option("similarityFunction", DefaultValue = SimilarityFunction.Cosine, HelpText = "Similarity function choice")]
        public SimilarityFunction SimilarityFunction { get; set; }

        [Option("executionMode", DefaultValue = ExecutionMode.MostNearestNeighborWithRandomProjection, HelpText = "Similarity mode choice")]
        public ExecutionMode ExecutionMode { get; set; }

        [Option("randomProjectionDimCount", DefaultValue = 50, HelpText = "Random projection dimension count")]
        public int RandomProjectionDimCount { get; set; }

        [Option("randomProjectionMode", DefaultValue = RandomProjectionMode.Gaussian, HelpText = "Random projection mode")]
        public RandomProjectionMode RandomProjectionMode { get; set; }
    }
}
