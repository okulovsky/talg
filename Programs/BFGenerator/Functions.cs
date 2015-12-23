using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BFGenerator
{
	public abstract class BooleanFunction
	{
		public abstract bool Evaluate(bool[] arguments);
		public abstract string Tex();
		public abstract string ToLetter();
		public abstract IEnumerable<BooleanFunction> Subtree { get;  }
	}

	public class Variable : BooleanFunction
	{
		public int Number;
		public const string Names = "xyzuvw";
		public override string ToString()
		{
			return Names[Number].ToString();
		}

		public override string Tex()
		{
			return ToString();
		}

		public override string ToLetter()
		{
			 return ToString();
		}


		public override bool Evaluate(bool[] arguments)
		{
			return arguments[Number];
		}
		public override IEnumerable<BooleanFunction> Subtree
		{
			get { yield return this; }
		}
	}

	public abstract class Operation : BooleanFunction 
	{
		public string Letter { get; set; }
		public override string ToLetter()
		{
			return Letter;
		}
		public abstract string LetterTex();
	}

	public class Negation : Operation
	{
		public BooleanFunction InnerFunction;

		public override string ToString()
		{
			return "(!" + InnerFunction.ToString()+")";
		}
		public override string Tex()
		{
			return "(\\neg " + InnerFunction.Tex() + ")";
		}
		public override string LetterTex()
		{
			return "\\neg " + InnerFunction.ToLetter();
		}

		public override bool Evaluate(bool[] arguments)
		{
			return !InnerFunction.Evaluate(arguments);
		}


		public override IEnumerable<BooleanFunction> Subtree
		{
			get
			{
				yield return this;
				foreach (var e in InnerFunction.Subtree)
					yield return e;
			}
		}
	}

	public class BinaryFunction : Operation
	{
		public string Sign;
		public string TexSign;
		public Func<bool, bool, bool> Func;
		public BooleanFunction Left;
		public BooleanFunction Right;

		public override string Tex()
		{
			return "(" + Left.Tex() + TexSign +" "+ Right.Tex() + ")";
		}

		public override string ToString()
		{
			return "(" + Left.ToString() + Sign + Right.ToString() + ")";
		}

		public override bool Evaluate(bool[] arguments)
		{
			return Func(Left.Evaluate(arguments), Right.Evaluate(arguments));
		}

		public BinaryFunction(string sign, string texsign, Func<bool,bool,bool> func)
		{
			this.TexSign = texsign;
			this.Sign = sign;
			this.Func = func;
		}

		public static IEnumerable<Func<BinaryFunction>> All
		{
			get
			{
				yield return () => new BinaryFunction("||","\\wedge", (a, b) => a && b);
				yield return () => new BinaryFunction("&&","\\vee", (a, b) => a || b);
				yield return () => new BinaryFunction("->","\\rightarrow", (a, b) => !a || b);
				yield return () => new BinaryFunction("+","\\oplus", (a, b) => (a&&!b)||(!a&&b));
			}
		}

		public override string LetterTex()
		{
			return Left.ToLetter() + TexSign + " " + Right.ToLetter();
		}

		public override IEnumerable<BooleanFunction> Subtree
		{
			get
			{
				yield return this;

				foreach (var e in Right.Subtree) yield return e;
				foreach (var e in Left.Subtree) yield return e;
			}
		}

	}
}
