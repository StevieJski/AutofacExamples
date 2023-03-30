using System.Collections.Generic;

namespace Quantifi;

public class DiscountCurveConfig{

    public DiscountCurveConfig(string name, IList<string> tenors){
        Name = name;
        Tenors = tenors;
    }
    public string Name{get;set;}

    public IList<string> Tenors{get;set;}
}
