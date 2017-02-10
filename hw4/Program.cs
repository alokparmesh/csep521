using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hw4
{
    public class Program
    {
        private static int n = 100; // dimensions of data
        private static int m = 1000; // // number of data points

        public static void Main(string[] args)
        {

            var X = Matrix<double>.Build.Random(m, n, new Normal(0.0, 1.0));
            var a_true = Vector<double>.Build.Random(n, new Normal(0.0, 1.0));
            var y = X.Multiply(a_true) + Vector<double>.Build.Random(m, new Normal(0.0, 0.1));

            Vector<double> closedSol = GetClosedFormSolution(X, y);
            PrintObjectiveFunctionValue(X, y, closedSol);

            //Vector<double> gradientDescentSol = GetGradientDescentSolution(X, y, 20, 0.001);
            //PrintObjectiveFunctionValue(X, y, gradientDescentSol);


            Vector<double> stochasticgradientDescentSol = GetStochasticGradientDescentSolution(X, y, 1000, 0.02);
            PrintObjectiveFunctionValue(X, y, stochasticgradientDescentSol);
        }

        private static void PrintObjectiveFunctionValue(Matrix<double> X, Vector<double> y, Vector<double> closedSol)
        {
            double objectiveFunctionValueClosedSol = GetObjectiveFunctionValue(X, y, closedSol);
            Console.WriteLine(objectiveFunctionValueClosedSol);
        }

        private static double GetObjectiveFunctionValue(Matrix<double> X, Vector<double> y, Vector<double> a)
        {
            var error = y - X.Multiply(a);
            return Math.Pow(error.L2Norm(), 2) * 0.5;
        }

        private static Vector<double> GetClosedFormSolution(Matrix<double> X, Vector<double> y)
        {
            return (X.Transpose() * X).Inverse() * (X.Transpose() * y);
        }

        private static Vector<double> GetGradientDescentSolution(Matrix<double> X, Vector<double> y, int iterations, double stepsize)
        {
            var a = Vector<double>.Build.Dense(n, 0.0);

            for (int i = 0; i < iterations; i++)
            {
                var error = y - X.Multiply(a);
                a = a + stepsize * X.Transpose().Multiply(error);
                PrintObjectiveFunctionValue(X, y, a);
            }
            return a;
        }

        private static Vector<double> GetStochasticGradientDescentSolution(Matrix<double> X, Vector<double> y, int iterations, double stepsize)
        {
            Random r = new Random();
            var a = Vector<double>.Build.Dense(n, 0.0);

            for (int i = 0; i < iterations; i++)
            {
                int pick = r.Next(0, m);
                var subMatrix = X.SubMatrix(pick, 1, 0, n);
                var subVector = y.SubVector(pick,1);
                var error = subVector - subMatrix.Multiply(a);

                a = a + stepsize * subMatrix.Transpose().Multiply(error);
                PrintObjectiveFunctionValue(X, y, a);
            }
            return a;
        }
    }
}
