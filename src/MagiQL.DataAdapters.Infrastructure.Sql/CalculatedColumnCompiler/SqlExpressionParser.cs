using System;
using System.Collections.Generic;
using System.Linq;
using MagiQL.DataAdapters.Infrastructure.Sql.Functions;
using MagiQL.Expressions;

namespace MagiQL.DataAdapters.Infrastructure.Sql.CalculatedColumnCompiler
{
    // hint:  this is derived from MagiQL.Expressions.Parser and extended to support functions

    public class SqlExpressionParser
    {
       
        public string ConvertToSql(string input)
        {
            try
            {

                var functions = SplitByFunction(input);

                var interpreter = new SqlEvaluator();

                string replacedExpressionText = input;

                int i = 0;

                // parse all expressions found inside functions and replace the text before parsing
                foreach (var fn in functions.Where(x => x.Function != null))
                {
                    fn.ParsedArgumentExpressions = new List<string>();
                    foreach (var fEx in fn.OriginalArgumentExpressions)
                    {
                        //var expression = new Parser(fEx).Parse();
                        //var parsed = interpreter.Evaluate(expression);
                        var parsed = ConvertToSql(fEx);

                        fn.ParsedArgumentExpressions.Add(parsed ?? fEx);
                        // numbers return null, so just use the original
                    }

                    replacedExpressionText = replacedExpressionText.Replace(fn.OriginalText, "FUNCTION_" + i);
                    i++;
                }

                // parse the expression without the functions
                var replacedExpression = new Parser(replacedExpressionText).Parse();
                replacedExpressionText = interpreter.Evaluate(replacedExpression) ?? replacedExpressionText;

                var result = ReJoinExpressions(functions, replacedExpressionText);
                return result;
            }
            catch(Exception ex)
            {
                throw new Exception("Error parsing column expression :  " + ex.Message + "\n for expression : " + input );
            }
        }

        public string ReJoinExpressions(List<FunctionExpressions> functions, string replacedExpressionText)
        {
            var result = replacedExpressionText;

            int i = 0;
            foreach (var fn in functions)
            {
                if (fn.Function != null)
                {
                    fn.ParsedText = fn.Function.Parse(fn.ParsedArgumentExpressions.ToArray());

                    result = result.Replace("FUNCTION_" + i, fn.ParsedText);

                    i++;
                } 
            }

            return result;
        }

        /// <summary>
        /// If the input expression contains functions, each function must be split out for seperate evaluation
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private List<FunctionExpressions> SplitByFunction(string input)
        {
            var result = new List<FunctionExpressions>();
             

            var functionExpressions = FindFunctions(input);

            int inputIndex = 0;
            if (functionExpressions.Any())
            {
                foreach (var fe in functionExpressions.OrderBy(x=>x.StartIndex))
                {
                    // any expression that comes before the function
                    if (inputIndex < fe.StartIndex)
                    {
                        string leftExpression = input.Substring(inputIndex, fe.StartIndex - inputIndex );
                        if (!string.IsNullOrEmpty(leftExpression))
                        {
                            result.Add(new FunctionExpressions { OriginalText = leftExpression });
                        }
                    }

                    result.Add(fe); 
                }


                // any expression that comes after the last function 
                var rightIndex = functionExpressions.Max(x => x.EndIndex + 1);
                string rightExpression = input.Substring(rightIndex);
                if (!string.IsNullOrEmpty(rightExpression))
                {
                    result.Add(new FunctionExpressions { OriginalText = rightExpression });
                }
            }

            else
            {
                // no functions found
                result.Add(new FunctionExpressions { OriginalText = input });
            }

            return result;
        }

        private List<FunctionExpressions> FindFunctions(string input)
        { 
            var result = new List<FunctionExpressions>();

            var sqlFunctions = FunctionRegistry.All;

            int currentIndex = 0; 
            var nextMatch = sqlFunctions.Select(x => new { index = input.IndexOf(x.Name + "(", currentIndex), function = x }).Where(x => x.index > -1).OrderBy(x => x.index).FirstOrDefault();

            while (nextMatch != null)
            {
                var originalText = input.Substring(nextMatch.index);
                var openingBracketIndex = originalText.IndexOf('(');
                var closingBracketIndex = GetClosingBracketIndex(originalText, openingBracketIndex);

                if (closingBracketIndex == -1)
                {
                    throw new Exception("Closing bracket not found in" + input);    
                } 

                originalText = originalText.Substring(0, closingBracketIndex + 1);

                var originalArgumentExpressions = ExtractFunctionExpressions(originalText, openingBracketIndex);

                result.Add(new FunctionExpressions()
                {
                    OriginalText = originalText,
                    StartIndex = nextMatch.index,
                    EndIndex = closingBracketIndex,
                    Function = nextMatch.function,
                    OriginalArgumentExpressions = originalArgumentExpressions
                });

                currentIndex = nextMatch.index + closingBracketIndex + 1;
 
                nextMatch = sqlFunctions.Select(x => new { index = input.IndexOf(x.Name + "(", currentIndex), function = x }).Where(x => x.index > -1).OrderBy(x => x.index).FirstOrDefault();
            }

            return result;
        }

        public List<string> ExtractFunctionExpressions(string originalText, int openingBracketIndex)
        {
            var result = new List<string>();

            var expressionsText = originalText.Substring(openingBracketIndex + 1);
            expressionsText = expressionsText.Substring(0, expressionsText.Length - 1);

            int commaIndex = 0;
            int splitIndex = 0;
            while (commaIndex < expressionsText.Length)
            {
                var nextCommaIndex = expressionsText.IndexOf(',', commaIndex);
                 
                if (nextCommaIndex == -1)
                {
                    result.Add(expressionsText.Substring(splitIndex));
                    break;
                }

                var firstOpenedIndex = expressionsText.IndexOf('(', commaIndex);

                if (firstOpenedIndex > -1 && firstOpenedIndex < nextCommaIndex)
                {
                    // make sure the bracket is closed off first
                    var firstClosedIndex = GetClosingBracketIndex(expressionsText, firstOpenedIndex);
                    if (firstClosedIndex > nextCommaIndex)
                    {
                        commaIndex = firstClosedIndex;
                        continue;
                    }
                }

                result.Add(expressionsText.Substring(splitIndex, nextCommaIndex - splitIndex));
                splitIndex = nextCommaIndex + 1;
                commaIndex = nextCommaIndex + 1;
            }


            return result;
        }
         
        /// <summary>
        /// Find the index of the closing bracket by specifying the index of the opening bracket
        /// </summary>
        /// <param name="input"></param>
        /// <param name="openingBracketIndex"></param>
        /// <returns></returns>
        public int GetClosingBracketIndex(string input, int openingBracketIndex)
        { 
            input = input.Substring(openingBracketIndex);

            int charIndex = 0;
            var openingBracket = input[charIndex];

            char closingBracket;
            switch (openingBracket)
            {
                case '(': closingBracket = ')';
                    break;
                case '[': closingBracket = ']';
                    break;
                case '{': closingBracket = '}';
                    break;
                case '<': closingBracket = '>';
                    break;
                default: throw new Exception("No closing bracket known for '" + openingBracket + "'");
            } 

            int firstClosedIndex = input.IndexOf(closingBracket, charIndex + 1);
            int firstOpenedIndex = input.IndexOf(openingBracket, charIndex + 1);

            while (firstOpenedIndex < firstClosedIndex)
            {  
                if (firstOpenedIndex == -1)
                {
                    break;
                }

                firstClosedIndex = input.IndexOf(closingBracket, firstClosedIndex + 1);
                firstOpenedIndex = input.IndexOf(openingBracket, firstOpenedIndex + 1);
            }

            if (firstClosedIndex == -1)
            {
                return -1;
            }

            return openingBracketIndex + firstClosedIndex;  
        }
    }
}
