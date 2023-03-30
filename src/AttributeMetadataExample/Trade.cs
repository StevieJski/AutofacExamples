namespace Quantifi;

public abstract class Trade{

    public string TradeId {get;set;}


    public string ProductId {get;set;}

    public double Notional {get;set;}

}

public class SingleQuoteTrade : Trade {
    public string ReferencIndexName {get;set;}

}


public class MultiQuoteTrade : Trade{
    public string ReferencIndexName {get;set;}

    public string[] QuoteNames {get;set;}
}


