using System;
using System.Collections.Generic;
using System.Linq;

namespace Quantifi;

public class Quote{

    public Quote(string name, DateTime dt, double value){
        Name=name;
        Date = dt;
        Value=value;
    }

    public DateTime Date{get;set;}

    public string Name{get;set;}

    public double Value{get;set;}
}


public interface IQuoteGetter{

    IList<Quote> FindQuotes(DateTime quoteDate, IList<string> names);

    Quote GetQuote(DateTime quoteDate, string name);

}

public class QuoteGetter1 : IQuoteGetter
{
    public IList<Quote> FindQuotes(DateTime quoteDate, IList<string> names) => names.Select( n => new Quote(n, quoteDate, 1.0)).ToList();
    public Quote GetQuote(DateTime quoteDate, string name) => new Quote(name, quoteDate, 1.0);
}
