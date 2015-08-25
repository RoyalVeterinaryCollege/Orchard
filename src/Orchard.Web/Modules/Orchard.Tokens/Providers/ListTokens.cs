using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Orchard.ContentManagement;
using Orchard.Environment;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using System.Linq;
	 
namespace Orchard.Tokens.Providers {
	 
	public class ListTokens : ITokenProvider {
	    private readonly Func<ITokenizer> _tokenizer;
	 
	    public ListTokens(Func<ITokenizer> tokenizer) {
	        _tokenizer = tokenizer;
	        T = NullLocalizer.Instance;
	    }
	 
	    public Localizer T { get; set; }
	 
	    public void Describe(DescribeContext context) {
	        context.For("List", T("Lists"), T("Handles lists of Content Items"))
	            .Token("ForEach:*", T("ForEach:<Tokenised string to apply to each element>"), T("Will loop each element in the list and apply the tokenised string supplied."))
	            .Token("SumInt:*", T("SumInt:<Tokenised string that return a integer value to sum>"), T("Will loop each element in the list and apply the tokenised string supplied and sum the results."))
	            .Token("SumFloat:*", T("SumFloat:<Tokenised string that return a float value to sum>"), T("Will loop each element in the list and apply the tokenised string supplied and sum the results."))
                .Token("SumDecimal:*", T("SumDecimal:<Tokenised string that return a decimal value to sum>"), T("Will loop each element in the list and apply the tokenised string supplied and sum the results."))
                .Token("First:*", T("First:<Tokenised string to apply to the first element>"), T("Will apply the tokenised string supplied to the first element."))
                .Token("Last:*", T("Last:<Tokenised string to apply to the last element>"), T("Will apply the tokenised string supplied to the last element."))
                .Token("Count", T("Count"), T("Gets the list count."))
                ;
	    }
	 
	    public void Evaluate(EvaluateContext context) {
            context.For<IList<IContent>>("List", () => new List<IContent>())
                    .Token( // {List.ForEach:<string>}
	                token =>
	                {
	                    if (token.StartsWith("ForEach:", StringComparison.OrdinalIgnoreCase))
	                    {
	                        // html decode to stop double encoding.
	                        return HttpUtility.HtmlDecode(token.Substring("ForEach:".Length));
	                    }
	                    return null;
	                },
	                (token, collection) =>
	                {
	                    var builder = new StringBuilder();
	                    foreach (var content in collection)
	                    {
	                        builder.Append(_tokenizer().Replace(token, new { content }, new ReplaceOptions { Encoding = ReplaceOptions.NoEncode }));
	                    }
	                    return builder.ToString();
	                })
                    .Token( // {List.SumInt:<string>}
	                token =>
	                {
	                    if (token.StartsWith("SumInt:", StringComparison.OrdinalIgnoreCase))
	                    {
	                        // html decode to stop double encoding.
	                        return HttpUtility.HtmlDecode(token.Substring("SumInt:".Length));
	                    }
	                    return null;
	                },
	                (token, collection) => collection.Sum(i => long.Parse(_tokenizer().Replace(token, new { Content = i }, new ReplaceOptions { Encoding = ReplaceOptions.NoEncode }))))
                    .Token( // {List.SumFloat:<string>}
	                token =>
	                {
	                    if (token.StartsWith("SumFloat:", StringComparison.OrdinalIgnoreCase))
	                    {
	                        // html decode to stop double encoding.
	                        return HttpUtility.HtmlDecode(token.Substring("SumFloat:".Length));
	                    }
	                    return null;
	                },
	                (token, collection) => collection.Sum(i => double.Parse(_tokenizer().Replace(token, new { Content = i }, new ReplaceOptions { Encoding = ReplaceOptions.NoEncode }))))
	                .Token( // {List.SumDecimal:<string>}
	                token =>
	                {
	                    if (token.StartsWith("SumDecimal:", StringComparison.OrdinalIgnoreCase))
	                    {
	                        // html decode to stop double encoding.
	                        return HttpUtility.HtmlDecode(token.Substring("SumDecimal:".Length));
	                    }
	                    return null;
	                },
                    (token, collection) => collection.Sum(i => decimal.Parse(_tokenizer().Replace(token, new { Content = i }, new ReplaceOptions { Encoding = ReplaceOptions.NoEncode }))))
                    .Token( // {List.First:<string>}
                    token => {
                        if (token.StartsWith("First:", StringComparison.OrdinalIgnoreCase)) {
                            // html decode to stop double encoding.
                            return HttpUtility.HtmlDecode(token.Substring("First:".Length));
                        }
                        return null;
                    },
                    (token, list) => list.Any() ? _tokenizer().Replace(token, new { Content = list.First() }, new ReplaceOptions { Encoding = ReplaceOptions.NoEncode }) : String.Empty)
                    .Token( // {List.Last:<string>}
                    token => {
                        if (token.StartsWith("Last:", StringComparison.OrdinalIgnoreCase)) {
                            // html decode to stop double encoding.
                            return HttpUtility.HtmlDecode(token.Substring("Last:".Length));
                        }
                        return null;
                    },
                    (token, list) => list.Any() ? _tokenizer().Replace(token, new { Content = list.Last() }, new ReplaceOptions { Encoding = ReplaceOptions.NoEncode }) : String.Empty)
                    .Token( // {List.Count}
                    "Count",
                    list => list.Count)
	            ;
	    }
	}
}