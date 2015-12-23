using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BFGenerator
{
	public class Program
	{
		static Random rnd = new Random(1);
		static Func<BinaryFunction>[] funcs = BinaryFunction.All.ToArray();
		const int negationProbability = 20;

		static BooleanFunction CreateNegation(BooleanFunction f)
		{
			if (rnd.Next(100) < negationProbability)
				return new Negation { InnerFunction = f };
			return f;
		}

		static BooleanFunction CreateUnary(bool[] usedVariables)
		{
			int variable=-1;
			var unused = Enumerable.Range(0, usedVariables.Length).Where(z => !usedVariables[z]).ToList();
			if (unused.Count != 0)
				variable = unused[rnd.Next(unused.Count)];
			else
				variable = rnd.Next(usedVariables.Length);
			var f = new Variable { Number = variable };
			usedVariables[variable] = true;
			return CreateNegation(f);
		}

		static BooleanFunction CreateBinary(bool[] usedVariables, int functionsCount)
		{
			if (functionsCount == 0) return CreateUnary(usedVariables);
			var leftCount = rnd.Next(0, functionsCount - 1);
			var rightCount = functionsCount - 1 - leftCount;
			var function = funcs[rnd.Next(funcs.Length)]();
			function.Left = CreateBinary(usedVariables, leftCount);
			function.Right = CreateBinary(usedVariables, rightCount);
			return CreateNegation(function);
		}

	
		static string Cell(bool value)
		{
			return (value ? "1" : "0") + "&";
		}

		static string MakeText(int number, BooleanFunction f, int varCount, bool withAnswer)
		{
			StringBuilder builder = new StringBuilder();

			builder.Append("Case \\#" + number);
			
			var operations = f.Subtree.OfType<Operation>().Reverse().ToArray();
			for (int i = 0; i < operations.Length; i++)
				operations[i].Letter = ((char)('A' + i)).ToString();

			string arrayTail = "";
			for (int i = 0; i < varCount; i++)
				arrayTail += "c";
			arrayTail += "||";
			for (int i = 0; i < operations.Length; i++)
			{
				if (i != 0 && i % 3 == 0)
					arrayTail += "|"; 
				arrayTail += "c";
			}
			arrayTail += "c";
			

			builder.Append("$$\n"+f.Tex() + "$$\n\n");

			builder.Append("\\begin{tabular}{" + arrayTail + "}\n");
			for (int i = 0; i < varCount; i++)
				builder.Append(Variable.Names[i] + "&");
			foreach (var e in operations)
				builder.Append("\\rotatebox{90}{$" + e.Letter + "=" + e.LetterTex() + "$}&");
			builder.Append("\\\\\n\\hline\n");

			var values = new bool[varCount];
			do
			{
				for (int i = 0; i < varCount; i++)
					builder.Append(Cell(values[i]));
				foreach (var e in f.Subtree.OfType<Operation>().Reverse())
					builder.Append(withAnswer?Cell(e.Evaluate(values)):"&");
				builder.Append("\\\\\n\\hline\n");
			} while (Next(values));
			builder.Append("\\end{tabular}\n\n");
			builder.Append("\\pagebreak\n\n");
			return builder.ToString();
		}

		static string Exercise(int number)
		{
			int varCount = 5;
			int funcCount = 9;
			
			BooleanFunction f = null;
			while (true)
			{
				
				f = CreateBinary(new bool[varCount], funcCount);
				var factVarCount = f.Subtree.OfType<Variable>().Select(z => z.Number).Distinct().Count();
				if (factVarCount == varCount) break;
			}
			return MakeText(number,f, varCount,false)+MakeText(number,f, varCount,true);
		}

		public static bool Next(bool[] values)
		{
			for (int i = values.Length - 1; i >= 0; i--)
				if (!values[i])
				{
					values[i] = true;
					return true;
				}
				else
					values[i] = false;
			return false;
		}

		public static void Main()
		{
			var s = Exercise(1);
			Console.WriteLine(s);
			File.WriteAllText("..\\..\\..\\BFGeneratorLatex\\output.tex",s);

		}

	}
}
