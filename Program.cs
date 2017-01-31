using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CsvHelper;
using System.Diagnostics;

namespace hw3
{
    public class Program
    {
        static void Main(string[] args)
        {
            SimilarityOption options = new SimilarityOption();

            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                List<string> groups = ReadGroups(options);
                List<Article> articles = ReadLabels(options, groups);
                ReadData(options, articles);

                foreach (Article article in articles.Take(5))
                {
                    Console.WriteLine(article);
                }

                if (options.SimilarityMode == SimilarityModes.AllWayAverage)
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
                                double average = j > i ? sum[i, j] / count[i, j]: sum[j, i] / count[j, i];
                                int fieldCount = j > i ?count[i, j] : count[j, i];
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

                Console.WriteLine("----------------------------------------------------------------------");
            }
        }

        private static double GetDistance(Dictionary<int, int> wordCounts1, Dictionary<int, int> wordCounts2, SimilarityFunctions similarityFunction)
        {
            double numerator = 0.0;
            double denominator = 0.0;

            foreach (var item in wordCounts1)
            {
                if(wordCounts2.ContainsKey(item.Key))
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

        private static List<string> ReadGroups(SimilarityOption options)
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

        private static List<Article> ReadLabels(SimilarityOption options, List<string> groups)
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
                            GroupId = label -1
                        };

                        articles.Add(a);
                    }
                }
            }

            return articles;
        }

        private static void ReadData(SimilarityOption options, List<Article> articles)
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
                    }
                }
            }
        }

    }
}
