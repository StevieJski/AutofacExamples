using System;
using System.Diagnostics;
using Autofac;
using Autofac.Extras.AttributeMetadata;
using Autofac.Features.AttributeFilters;
using Autofac.Integration.Mef;
using System.Linq;
using Quantifi;

namespace AttributeMetadataExample;

public class Program
{
    private static void Main()
    {
        var tup = new {Name = "Nameo", Date = DateTime.Now};
        var props = tup.GetType().GetProperties();
        Console.WriteLine(props[0].Name );
        Console.WriteLine(props[0].GetValue(tup));
        // Attribute metadata documentation can be found here:
        // https://autofac.readthedocs.io/en/latest/advanced/metadata.html
        var builder = new ContainerBuilder();
        builder.RegisterType<QuoteGetter1>().As<IQuoteGetter>().InstancePerLifetimeScope();
        builder.Register((string name) => new DiscountCurveConfig(name, new[]{"1Y", "2Y", "3Y", "5Y"}));
        builder.RegisterType<DiscountCurveFactory>().As<IDiscountCurveFactory>().InstancePerLifetimeScope();
        builder.Register((string name, DateTime dt, IComponentContext context) => context.Resolve<IDiscountCurveFactory>().GetDiscountCurve(name, dt));
        builder.RegisterType<KeyValueStore>().As<IKeyValueStore>().InstancePerLifetimeScope();

        builder.RegisterServiceMiddleware<IDiscountCurve>(Autofac.Core.Resolving.Pipeline.PipelinePhase.Sharing, (context, next) => {
            var kvStore = context.Resolve<IKeyValueStore>();
            var name = context.Parameters.OfType<NamedParameter>().Any() ? context.Parameters.Named<string>("name") : context.Parameters.TypedAs<string>();
            if(kvStore.ContainsKey(name))
            {
                var instance = kvStore.Get<IDiscountCurve>(name);
                Console.WriteLine($"Instance Found {instance.Name}");
                context.Instance = instance;
            }
            else{
                next(context);
                if(context.Instance is IDiscountCurve dc)
                    kvStore.Add<IDiscountCurve>(name, dc);
            }
        });

        builder.RegisterServiceMiddleware<DiscountCurveConfig>(Autofac.Core.Resolving.Pipeline.PipelinePhase.Sharing, (context, next) => {
            var kvStore = context.Resolve<IKeyValueStore>();
            var name = context.Parameters.OfType<NamedParameter>().Any() ? context.Parameters.Named<string>("name") : context.Parameters.TypedAs<string>();
            next(context);
            var instance = context.Instance as DiscountCurveConfig;
            Console.WriteLine(instance.Name);

        });

        builder.RegisterType<SingleQuotePricerFactory>().Keyed<IPricerFactory>(typeof(SingleQuoteTrade));
        builder.RegisterType<MultiQuotePricerFactory>().Keyed<IPricerFactory>(typeof(MultiQuoteTrade));
        builder.RegisterType<PricerSelector>().As<IPricerFactory>();
        // builder.RegisterDecorator<IDiscountCurve>((context, parameters, instance) => {
        //     var kvStore = context.Resolve<IKeyValueStore>();
        //     var name = parameters.Named<string>("name");
        //     if(kvStore.ContainsKey(name))
        //         return kvStore.Get<IDiscountCurve>(name);
        //     else{
        //         kvStore.Add<IDiscountCurve>(name, instance);
        //     }
        //     return instance;

        // });

        // builder.RegisterType<Log>();

        // // Example: Attach string-based metadata manually through registration.
        // builder.RegisterType<StringManualMetadataAppender>()
        //     .As<ILogAppender>()
        //     .WithMetadata("AppenderName", "string-manual");

        // // Example: Attach strongly-typed metadata manually through registration.
        // builder.RegisterType<TypedManualMetadataAppender>()
        //     .As<ILogAppender>()
        //     .WithMetadata<AppenderMetadata>(m => m.For(am => am.AppenderName, "typed-manual"));

        // // Example: Attach interface-based metadata manually through registration.
        // // Requires use of the RegisterMetadataRegistrationSources extension from Autofac.Mef.
        // builder.RegisterType<InterfaceManualMetadataAdapter>()
        //     .As<ILogAppender>()
        //     .WithMetadata<IAppenderMetadata>(m => m.For(am => am.AppenderName, "interface-manual"));
        // builder.RegisterMetadataRegistrationSources();

        // // Example: Attribute-based metadata.
        // // Requires registration of the AttributedMetadataModule.
        // builder.RegisterType<AttributeMetadataAppender>()
        //     .As<ILogAppender>();
        // builder.RegisterModule<AttributedMetadataModule>();

        // // Example: Consuming component using metadata filter attribute in constructor.
        // builder.RegisterType<LogWithFilter>()
        //     .WithAttributeFiltering();

        using (var container = builder.Build())
        {
            using var scope = container.BeginLifetimeScope();

            var dcFactory = scope.Resolve<IDiscountCurveFactory>();
            var dc1 = dcFactory.GetDiscountCurve("Test1", DateTime.Now);
            var dc2 = dcFactory.GetDiscountCurve("Test2", DateTime.Now);
            Console.WriteLine(dc1.Name);
            Console.WriteLine(dc2.Name);
            //var k = new Autofac.Core.ResolvedParameter()
            var dc3 = scope.Resolve<IDiscountCurve>( new NamedParameter("name", "Test3"), new NamedParameter("dt", DateTime.Now));
            Console.WriteLine(dc3.Name);
            var dc3_again = scope.Resolve<IDiscountCurve>( new NamedParameter("name", "Test3"), new NamedParameter("dt", DateTime.Now));
            Console.WriteLine($"{dc3.GetHashCode()} == {dc3_again.GetHashCode()}" );

            var trade = new SingleQuoteTrade(){TradeId = "Trade1", ProductId = "Bond1", ReferencIndexName = "DC1", Notional = 1000.0};
            var pricerSelector = scope.Resolve<IPricerFactory>();
            var pricer = pricerSelector.GetPricer(trade, DateTime.Now);
            Console.WriteLine(pricer.Pv());

            // The standard logger will choose which appender to use
            // based on the specified metadata value.
            // var log = scope.Resolve<Log>();
            // log.Write("string-manual", "Message 1");
            // log.Write("typed-manual", "Message 2");
            // log.Write("interface-manual", "Message 3");
            // log.Write("attributed", "Message 4");

            // // This logger selects the appropriate appender by
            // // applying a filter attribute on the constructor parameter.
            // var filteredLog = scope.Resolve<LogWithFilter>();
            // filteredLog.Write("Message from filtered log");
        }

        if (Debugger.IsAttached)
        {
            Console.WriteLine("[press any key to exit]");
            Console.ReadKey();
        }
    }
}
