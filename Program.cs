using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CsvHelper;
using System.Diagnostics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Distributions;

namespace hw3
{
    public class Program
    {
        private static int k = 0;
        static void Main(string[] args)
        {
            ExecutionOption options = new ExecutionOption();

            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                List<string> groups = ReadGroups(options);
                List<Article> articles = ReadLabels(options, groups);
                ReadData(options, articles);

                /*
                foreach (Article article in articles.Take(5))
                {
                    Console.WriteLine(article);
                }
                */

                if (options.ExecutionMode == ExecutionMode.AllWayAverage)
                {
                    double[,] sum = new double[groups.Count, groups.Count];
                    int[,] count = new int[groups.Count, groups.Count];

                    for (int i = 0; i < articles.Count; i++)
                    {
                        Article ai = articles[i];
                        for (int j = i + 1; j < articles.Count; j++)
                        {

                            Article aj = articles[j];

                            sum[ai.GroupId, aj.GroupId] += GetDistance(ai.WordCounts, aj.WordCounts, options.SimilarityFunction);
                            count[ai.GroupId, aj.GroupId] += 1;
                        }
                    }

                    using (var sr = new StreamWriter("output.csv"))
                    {
                        var writer = new CsvWriter(sr);
                        for (int i = 0; i < groups.Count; i++)
                        {
                            for (int j = 0; j < groups.Count; j++)
                            {
                                double average = j > i ? sum[i, j] / count[i, j] : sum[j, i] / count[j, i];
                                int fieldCount = j > i ? count[i, j] : count[j, i];
                                if (fieldCount > 0)
                                {
                                    writer.WriteField(average);
                                }
                                else
                                {
                                    writer.WriteField(0.0);
                                }
                            }

                            writer.NextRecord();
                        }
                    }
                }
                else if (options.ExecutionMode == ExecutionMode.MostNearestNeighbor)
                {
                    int[,] count = new int[groups.Count, groups.Count];
                    for (int i = 0; i < articles.Count; i++)
                    {
                        double maxSum = 0.0;
                        int maxGroupId = -1;

                        Article ai = articles[i];
                        for (int j = 0; j < articles.Count; j++)
                        {
                            Article aj = articles[j];

                            if (ai.GroupId == aj.GroupId)
                            {
                                continue;
                            }

                            double distance = GetDistance(ai.WordCounts, aj.WordCounts, options.SimilarityFunction);

                            if (distance > maxSum)
                            {
                                maxSum = distance;
                                maxGroupId = aj.GroupId;
                            }
                        }

                        if (maxGroupId >= 0)
                        {
                            count[ai.GroupId, maxGroupId] += 1;
                        }
                    }

                    using (var sr = new StreamWriter("output.csv"))
                    {
                        var writer = new CsvWriter(sr);
                        for (int i = 0; i < groups.Count; i++)
                        {
                            for (int j = 0; j < groups.Count; j++)
                            {
                                writer.WriteField(count[i, j]);
                            }

                            writer.NextRecord();
                        }
                    }
                }
                else if (options.ExecutionMode == ExecutionMode.MostNearestNeighborWithRandomProjection)
                {
                    var M = Matrix<double>.Build;
                    var matrix = M.Random(options.RandomProjectionDimCount, k + 1, new Normal(0.0, 1.0));

                    DimensionReduceArticles(articles, matrix);

                    int[,] count = new int[groups.Count, groups.Count];
                    for (int i = 0; i < articles.Count; i++)
                    {
                        double maxSum = 0.0;
                        int maxGroupId = -1;

                        Article ai = articles[i];
                        for (int j = 0; j < articles.Count; j++)
                        {
                            Article aj = articles[j];

                            if (ai.GroupId == aj.GroupId)
                            {
                                continue;
                            }

                            double distance = GetDistance(ai.WordCountVector, aj.WordCountVector, options.SimilarityFunction);

                            if (distance > maxSum)
                            {
                                maxSum = distance;
                                maxGroupId = aj.GroupId;
                            }
                        }

                        if (maxGroupId >= 0)
                        {
                            count[ai.GroupId, maxGroupId] += 1;
                        }
                    }

                    using (var sr = new StreamWriter("output.csv"))
                    {
                        var writer = new CsvWriter(sr);
                        for (int i = 0; i < groups.Count; i++)
                        {
                            for (int j = 0; j < groups.Count; j++)
                            {
                                writer.WriteField(count[i, j]);
                            }

                            writer.NextRecord();
                        }
                    }
                }
                else if (options.ExecutionMode == ExecutionMode.RandomProjectionCosineScatter)
                {
                    throw new NotSupportedException("Unknown execution mode");
                }
                else
                {
                    throw new NotSupportedException("Unknown execution mode");
                }

                Console.WriteLine("----------------------------------------------------------------------");
            }
        }

        private static void DimensionReduceArticles(List<Article> articles, Matrix<double> matrix)
        {
            foreach (Article article in articles)
            {
                Vector <double> v = Vector<double>.Build.Sparse(k+1, i => article.WordCounts.ContainsKey(i) ? article.WordCounts[i] : 0.0);
                article.WordCountVector = matrix.Multiply(v).ToArray();
            }
        }

        private static double GetDistance(Dictionary<int, int> wordCounts1, Dictionary<int, int> wordCounts2, SimilarityFunction similarityFunction)
        {
            switch (similarityFunction)
            {
                case SimilarityFunction.Jaccard:
                    return GetJacquardDistance(wordCounts1, wordCounts2);
                case SimilarityFunction.Cosine:
                    return GetCosineDistance(wordCounts1, wordCounts2);
                case SimilarityFunction.LSquare:
                    return GetLSquareDistance(wordCounts1, wordCounts2);
                default:
                    throw new NotSupportedException("Unknown distance measure"); ;
            }
        }

        private static double GetJacquardDistance(Dictionary<int, int> wordCounts1, Dictionary<int, int> wordCounts2)
        {
            double numerator = 0.0;
            double denominator = 0.0;

            foreach (var item in wordCounts1)
            {
                if (wordCounts2.ContainsKey(item.Key))
                {
                    numerator += Math.Min(item.Value, wordCounts2[item.Key]);
                    denominator += Math.Max(item.Value, wordCounts2[item.Key]);
                }
                else
                {
                    denominator += item.Value;
                }
            }

            foreach (var item in wordCounts2)
            {
                if (wordCounts1.ContainsKey(item.Key))
                {
                    continue;
                }
                else
                {
                    denominator += item.Value;
                }
            }

            if (denominator > 0)
            {
                return numerator / denominator;
            }
            else
            {
                return 1.0;
            }
        }

        private static double GetCosineDistance(Dictionary<int, int> wordCounts1, Dictionary<int, int> wordCounts2)
        {
            double xy = 0.0;
            double xdistance = 0.0;
            double ydistance = 0.0;

            foreach (var item in wordCounts1)
            {
                xdistance += item.Value * item.Value;

                if (wordCounts2.ContainsKey(item.Key))
                {
                    xy += item.Value * wordCounts2[item.Key];
                }
            }

            foreach (var item in wordCounts2)
            {
                ydistance += item.Value * item.Value;
            }

            if (xdistance > 0 && ydistance > 0.0)
            {
                return xy / (Math.Sqrt(xdistance) * Math.Sqrt(ydistance));
            }
            else
            {
                return 1.0;
            }
        }

        private static double GetLSquareDistance(Dictionary<int, int> wordCounts1, Dictionary<int, int> wordCounts2)
        {
            double sumOfSquares = 0.0;

            foreach (var item in wordCounts1)
            {
                if (wordCounts2.ContainsKey(item.Key))
                {
                    sumOfSquares += Math.Pow(Math.Abs(item.Value - wordCounts2[item.Key]), 2);
                }
                else
                {
                    sumOfSquares += item.Value * item.Value;
                }
            }

            foreach (var item in wordCounts2)
            {
                if (wordCounts1.ContainsKey(item.Key))
                {
                    continue;
                }
                else
                {
                    sumOfSquares += item.Value * item.Value;
                }
            }

            return Math.Sqrt(sumOfSquares) * -1.0;
        }

        private static double GetDistance(double[] wordCountVector1, double[] wordCountVector2, SimilarityFunction similarityFunction)
        {
            switch (similarityFunction)
            {
                case SimilarityFunction.Jaccard:
                   // return GetJacquardDistance(wordCountVector1, wordCountVector2);
                case SimilarityFunction.Cosine:
                    return GetCosineDistance(wordCountVector1, wordCountVector2);
                case SimilarityFunction.LSquare:
                   // return GetLSquareDistance(wordCountVector1, wordCountVector2);
                default:
                    throw new NotSupportedException("Unknown distance measure"); ;
            }
        }

        private static double GetCosineDistance(double[] wordCountVector1, double[] wordCountVector2)
        {
            double xy = 0.0;
            double xdistance = 0.0;
            double ydistance = 0.0;

            for (int i = 0; i < wordCountVector1.Length; i++)
            {
                xdistance += wordCountVector1[i] * wordCountVector1[i];
                ydistance += wordCountVector2[i] * wordCountVector2[i];
                xy += wordCountVector1[i] * wordCountVector2[i];
            }

            if (xdistance > 0 && ydistance > 0.0)
            {
                return xy / (Math.Sqrt(xdistance) * Math.Sqrt(ydistance));
            }
            else
            {
                return 1.0;
            }
        }

        private static List<string> ReadGroups(ExecutionOption options)
        {
            List<string> groups = new List<string>();

            if (!File.Exists(options.GroupsFile))
            {
                throw new ArgumentException(string.Format("File not found {0}", options.LabelFile));
            }
            else
            {
                using (var sr = new StreamReader(options.GroupsFile))
                {
                    var reader = new CsvReader(sr);
                    reader.Configuration.HasHeaderRecord = false;

                    while (reader.Read())
                    {
                        var group = reader.GetField<string>(0);
                        groups.Add(group);
                    }
                }
            }

            return groups;
        }

        private static List<Article> ReadLabels(ExecutionOption options, List<string> groups)
        {
            List<Article> articles = new List<Article>();

            if (!File.Exists(options.LabelFile))
            {
                throw new ArgumentException(string.Format("File not found {0}", options.LabelFile));
            }
            else
            {

                using (var sr = new StreamReader(options.LabelFile))
                {
                    var reader = new CsvReader(sr);
                    reader.Configuration.HasHeaderRecord = false;

                    while (reader.Read())
                    {
                        int label = reader.GetField<int>(0);
                        Article a = new Article
                        {
                            GroupId = label - 1
                        };

                        articles.Add(a);
                    }
                }
            }

            return articles;
        }

        private static void ReadData(ExecutionOption options, List<Article> articles)
        {
            if (!File.Exists(options.DataFile))
            {
                throw new ArgumentException(string.Format("File not found {0}", options.DataFile));
            }
            else
            {
                using (var sr = new StreamReader(options.DataFile))
                {
                    var reader = new CsvReader(sr);
                    reader.Configuration.HasHeaderRecord = false;

                    while (reader.Read())
                    {
                        var articleId = reader.GetField<int>(0);
                        var wordId = reader.GetField<int>(1);
                        var wordCount = reader.GetField<int>(2);
                        articles[articleId - 1].WordCounts[wordId] = wordCount;

                        if (k < wordId)
                        {
                            k = wordId;
                        }
                    }
                }
            }
        }

    }
}
