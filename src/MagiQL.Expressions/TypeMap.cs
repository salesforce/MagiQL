using System.Collections.Generic;
using System.Linq;
using MagiQL.Expressions.Model;

namespace MagiQL.Expressions
{
	public class TypeMap
	{
		private Dictionary<string, TypeMapItem> Synonyms { get; set; }
		private Dictionary<string, TypeMapItem> Items { get; set; }

		public TypeMap()
		{
			Synonyms = new Dictionary<string, TypeMapItem>();
			Items = new Dictionary<string,TypeMapItem>();
		}

		public void Add(string name, DataType type)
		{
			Add(name, name, type);
		}

		public void Add(string name, DataType type, string[] synonyms)
		{
			TypeMapItem item;

			foreach (var syn in synonyms)
			{
				if (Synonyms.ContainsKey(syn.ToLower()))
				{
					throw new ExpressionException("Synonym '" + syn + "' already added to type map");
				}
			}

			if (!Items.ContainsKey(name.ToLower()))
			{
				item = Items[name.ToLower()] =
					new TypeMapItem
					{
						Name = name,
						DataType = type,
						Synonyms = synonyms.ToList()
					};
			}
			else
			{
				item = Items[name.ToLower()];
				item.Synonyms.AddRange(synonyms);
			}

			foreach (var syn in synonyms)
			{
				if (!Synonyms.ContainsKey(syn.ToLower()))
				{
					Synonyms.Add(syn.ToLower(), item);
				}
			}

			if (!Synonyms.ContainsKey(name.ToLower()))
			{
				Synonyms.Add(name.ToLower(), item);
			}
		}

		public void Add(string name, string synonym, DataType type)
		{
			Add(name, type, new[] { synonym });
		}

		public DataType? Find(string synonym)
		{
			synonym = synonym.ToLower();

			if (Synonyms.ContainsKey(synonym))
			{
				return Synonyms[synonym].DataType;
			}

			return null;
		}

		private class TypeMapItem
		{
			public string Name { get; set; }
			public DataType DataType { get; set; }
			public List<string> Synonyms { get; set; }
		}
	}
}
