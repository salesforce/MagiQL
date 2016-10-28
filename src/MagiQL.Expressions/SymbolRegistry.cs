using System;
using System.Collections.Generic;
using MagiQL.Expressions.Model;

namespace MagiQL.Expressions
{
	public class SymbolRegistry : SymbolRegistry<object>
	{

	}

	public class SymbolRegistry<T>
	{
		public TypeMap TypeMap { get; private set; }
		public Dictionary<string, Func<T, double>> Functions { get; set; }

		public SymbolRegistry()
		{
			TypeMap = new TypeMap();
			Functions = new Dictionary<string,Func<T,double>>();
		}

		public void Add(string name, DataType type, Func<T, double> fn)
		{
			Add(name, name, type, fn);
		}

		public void Add(string name, string synonym, DataType type, Func<T, double> fn)
		{
			TypeMap.Add(name, synonym, type);
			Functions[synonym.ToLower()] = fn;
		}

		public void Add(string name, DataType type, Func<T, double> fn, params string[] synonynms)
		{
			TypeMap.Add(name, type, synonynms);

			foreach (var syn in synonynms)
			{
				Functions[syn.ToLower()] = fn;
			}

			if (!Functions.ContainsKey(name.ToLower()))
			{
				Functions[name.ToLower()] = fn;	
			}
		}

		public DataType? FindType(string synonym)
		{
			if (synonym == "true" || synonym == "false")
			{
				return DataType.Boolean;
			}

			return TypeMap.Find(synonym);
		}

		public double Evaluate(T data, string synonym)
		{
			if (synonym == "true" || synonym == "false")
			{
				return 0; // ANDY
			}
		    if (synonym == "false")
		    {
		        return 0; // ANDY
		    }

		    return Functions[synonym.ToLower()](data);
		}
	}
}
