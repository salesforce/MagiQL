using System;
using System.Linq;
using MagiQL.DataAdapters.Infrastructure.Sql.CalculatedColumnCompiler;
using NUnit.Framework;

namespace MagiQL.DataAdapters.Infrastructure.Sql.Tests.Unit.CalculatedColumnCompiler
{
    [TestFixture]
    public class SqlExpressionParserTests
    {
        // math
        [TestCase("ConversionValue", "ConversionValue")]
        [TestCase("L0 + L1", "(ISNULL(L0,0) + ISNULL(L1,0))")]
        [TestCase("(L0 + L1 + L2 + L3 + L4 + L5 + L6 + L7)", "(ISNULL((ISNULL((ISNULL((ISNULL((ISNULL((ISNULL((ISNULL(L0,0) + ISNULL(L1,0)),0) + ISNULL(L2,0)),0) + ISNULL(L3,0)),0) + ISNULL(L4,0)),0) + ISNULL(L5,0)),0) + ISNULL(L6,0)),0) + ISNULL(L7,0))")]
        [TestCase("Spend + 15%", "(ISNULL(Spend,0) + (ISNULL(Spend,0) * 0.15))")]
        [TestCase("Spend * 1.38045", "(Spend * 1.38045)")]
        [TestCase("Reach / Audience", "((1.0 * Reach) / NULLIF(Audience, 0))")]
        [TestCase("spend/clicks*impressions", "(((1.0 * spend) / NULLIF(clicks, 0)) * impressions)")]
        [TestCase("Spend / (L0 + Connections + Clicks)", "((1.0 * Spend) / NULLIF((ISNULL((ISNULL(L0,0) + ISNULL(Connections,0)),0) + ISNULL(Clicks,0)), 0))")]
        [TestCase("(Page_Likes + Actions) / Impressions", "((1.0 * (ISNULL(Page_Likes,0) + ISNULL(Actions,0))) / NULLIF(Impressions, 0))")]

        // functions
        [TestCase("MAXOF(1, Spend)", "(CASE WHEN 1 > Spend THEN 1 ELSE Spend END)")]
        [TestCase("MAXOF(1, Spend + 2) / Impressions ", "((1.0 * (CASE WHEN 1 > (ISNULL(Spend,0) + ISNULL(2,0)) THEN 1 ELSE (ISNULL(Spend,0) + ISNULL(2,0)) END)) / NULLIF(Impressions, 0))")]
        [TestCase("(MAXOF(1, Spend) + 1 ) / Impressions ", "((1.0 * (ISNULL((CASE WHEN 1 > Spend THEN 1 ELSE Spend END),0) + ISNULL(1,0))) / NULLIF(Impressions, 0))")]
        [TestCase("MAXOF(1, MAXOF(Clicks,Impressions))", "(CASE WHEN 1 > (CASE WHEN Clicks > Impressions THEN Clicks ELSE Impressions END) THEN 1 ELSE (CASE WHEN Clicks > Impressions THEN Clicks ELSE Impressions END) END)")]
       
        [TestCase("IFTHENELSE(1 > 2, Spend, Clicks)", "(CASE WHEN 1 > 2 THEN Spend ELSE Clicks END)")]
        [TestCase("IFTHENELSE(1 == 2, Spend, Clicks)", "(CASE WHEN 1 = 2 THEN Spend ELSE Clicks END)")]
        [TestCase("IFTHENELSE(1 > 2, Spend, IFTHENELSE(Clicks > 2, Clicks, NULL))", "(CASE WHEN 1 > 2 THEN Spend ELSE (CASE WHEN Clicks > 2 THEN Clicks ELSE NULL END) END)")]
        [TestCase("IFTHENELSE(ISNULL(Revenue,0) == 0, 0, Revenue - Spend)", "(CASE WHEN ISNULL(Revenue,0) = 0 THEN  0 ELSE (ISNULL(Revenue,0) - ISNULL(Spend,0)) END)")]
        [TestCase("IFTHENELSE(1 > 2 && Clicks == Spend, Spend, Clicks)", "(CASE WHEN ((1 > 2) AND (Clicks = Spend)) THEN Spend ELSE Clicks END)")]


        [TestCase("((1.0 * stats.Spend) / NULLIF(stats.Clicks, 0))", "((1.0 * (1 * stats.Spend)) / NULLIF(NULLIF(stats.Clicks, 0), 0))")]
        [TestCase("(ISNULL(stats.SocialSpend,0) + ISNULL(stats.Spend,0))", "(ISNULL(ISNULL(stats.SocialSpend,0),0) + ISNULL(ISNULL(stats.Spend,0),0))")]
         
        public void ConvertToSql_WithCase_ReturnsExpected(string input, string expected)
        {
            var result = new SqlExpressionParser().ConvertToSql(input);

            Console.WriteLine(result);

            Assert.AreEqual(expected, result);
        }
         

        [TestCase("abc ( 123 ) def", 4, 10)]
        [TestCase("abc ( (2) ) def", 4, 10)]
        [TestCase("abc ( b + (2 * 5) ) def", 4, 18)]
        [TestCase("abc ( b + (2 * 5) def", 4, -1)]
        public void GetClosingBracketIndex_WithCase_ReturnsExpected(string input, int openingIndex, int expected)
        {
            var result = new SqlExpressionParser().GetClosingBracketIndex(input, openingIndex);

            Assert.AreEqual(expected, result);
        }

        [TestCase("dbo.func(2)", "2")]
        [TestCase("dbo.func(Clicks + 1)", "Clicks + 1")]
        public void ExtractFunctionExpressions_WithCase_ReturnsExpected(string input, string expected)
        {
            // it doesnt like single arguments as params
            ExtractFunctionExpressions_WithCase_ReturnsExpected(input, new[] {expected});
        }

        [TestCase("dbo.func(1,2)", "1","2")]
        [TestCase("dbo.func(1,Spend)", "1","Spend")]
        [TestCase("dbo.func(Spend,1,Clicks)", "Spend","1","Clicks")]
        [TestCase("dbo.func(Spend,1,Clicks+2)", "Spend", "1", "Clicks+2")]
        [TestCase("dbo.func(Spend,(1 + 2),Clicks+2)", "Spend", "(1 + 2)", "Clicks+2")]
        [TestCase("dbo.func(Spend,1 + (2 * 3),Clicks+2)", "Spend", "1 + (2 * 3)", "Clicks+2")]
        public void ExtractFunctionExpressions_WithCase_ReturnsExpected(string input, params string[] expected)
        {
            var result = new SqlExpressionParser().ExtractFunctionExpressions(input, 8);

            Assert.AreEqual(expected.Count(), result.Count);
            
            for(int i = 0; i<expected.Count(); i++)
            {
                Assert.AreEqual(expected[i], result[i]);
            } 
        }

    }
}