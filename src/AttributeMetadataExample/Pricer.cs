using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Features.Indexed;

namespace Quantifi;

public interface IPricer
{
    Trade Trade{get;}

    DateTime PricingDate {get;}

    double Pv();

}

public class SingleQuotePricer : IPricer
{
    private IDiscountCurve _dc;
    private Quote _quote;


    public SingleQuotePricer(Trade t, DateTime pricingDt, IDiscountCurve dc, Quote price){
        Trade = t;
        PricingDate = pricingDt;
        _dc = dc;
        _quote = price;
    }
    public Trade Trade {get; }

    public DateTime PricingDate {get;}

    public double Pv() => Trade.Notional * _quote.Value;
}


public class MultiQuotePricer : IPricer
{
    private IDiscountCurve _dc;
    private IList<Quote> _quotes;


    public MultiQuotePricer(Trade t, DateTime pricingDt, IDiscountCurve dc, IList<Quote> quotes){
        Trade = t;
        PricingDate = pricingDt;
        _dc = dc;
        _quotes = quotes;
    }
    public Trade Trade {get; }

    public DateTime PricingDate {get;}

    public double Pv() => _quotes.Sum( q => q.Value);
}


public interface IPricerFactory{
    IPricer GetPricer(Trade trade, DateTime pricingDt);
}


public class SingleQuotePricerFactory : IPricerFactory
{
    private Func<string, DateTime, IDiscountCurve> _dcGetter;
    private IQuoteGetter _quoteGetter;

    public SingleQuotePricerFactory(Func<string, DateTime, IDiscountCurve> dcGetter, IQuoteGetter quoteGetter){
        _dcGetter = dcGetter;
        _quoteGetter = quoteGetter;

    }

    public IPricer GetPricer(Trade trade, DateTime pricingDt){
        var singleQuoteTrade = trade as SingleQuoteTrade;
        var quote = _quoteGetter.GetQuote(pricingDt, trade.ProductId);
        var dc = _dcGetter(singleQuoteTrade.ReferencIndexName, pricingDt);
        return new SingleQuotePricer(trade, pricingDt, dc, quote );
    }
}



public class MultiQuotePricerFactory : IPricerFactory
{
    private Func<string, DateTime, IDiscountCurve> _dcGetter;
    private IQuoteGetter _quoteGetter;

    public MultiQuotePricerFactory(Func<string, DateTime, IDiscountCurve> dcGetter, IQuoteGetter quoteGetter){
        _dcGetter = dcGetter;
        _quoteGetter = quoteGetter;

    }

    public IPricer GetPricer(Trade trade, DateTime pricingDt){
        var multiQuoteTrade = trade as MultiQuoteTrade;
        var quotes = _quoteGetter.FindQuotes(pricingDt, multiQuoteTrade.QuoteNames);
        var dc = _dcGetter(multiQuoteTrade.ReferencIndexName, pricingDt);
        return new MultiQuotePricer(trade, pricingDt, dc, quotes );
    }
}


public class PricerSelector : IPricerFactory
{
    private IIndex<Type, IPricerFactory> _index;

    public PricerSelector(IIndex<Type, IPricerFactory> index){
        _index = index;

    }
    public IPricer GetPricer(Trade trade, DateTime pricingDt) {
        var factory = _index[trade.GetType()];
        return factory.GetPricer(trade, pricingDt);
    }
}
