using System;
using System.Collections.Generic;

namespace Quantifi;


public interface IDiscountCurve{
    string Name{get;}

    DateTime AsOf{get;}
    DiscountCurveConfig Config{get;}

    IList<Quote> Quotes{get;}
}


public class DiscountCurve : IDiscountCurve {

    public DiscountCurve(string name, DateTime dt, DiscountCurveConfig config, IList<Quote> quotes){
        Name = name;
        AsOf = dt;
        Config = config;
        Quotes = quotes;

    }

    public DateTime AsOf{get;set;}
    public string Name{get;set;}
    public DiscountCurveConfig Config {get;set;}
    public IList<Quote> Quotes { get ;set; }

}


public interface IDiscountCurveFactory{

    IDiscountCurve GetDiscountCurve(string name, DateTime asOf);
}


public class DiscountCurveFactory : IDiscountCurveFactory
{
    private Func<string, DiscountCurveConfig> _configGetter;
    private IQuoteGetter _quoteGetter;
    public DiscountCurveFactory(Func<string, DiscountCurveConfig> configGetter, IQuoteGetter quoteGetter){
        _configGetter = configGetter;
        _quoteGetter = quoteGetter;

    }
    public IDiscountCurve GetDiscountCurve(string name, DateTime asOf){
        var config = _configGetter(name);
        var quotes = _quoteGetter.FindQuotes(asOf, config.Tenors);
        return new DiscountCurve(name, asOf, config, quotes);

    }
}
