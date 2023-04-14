using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using AspectCore.Extensions.DependencyInjection.NAutowired;
using Microsoft.Extensions.DependencyInjection;
using NAutowired.Core.Attributes;

namespace TestConsoleApp
{
    class Program
    {

        static void Main(string[] args)
        {
            // sample for property injection
            var services = new ServiceCollection();
            services.AddTransient<ILogger, ConsoleLogger>();
            // inteface-impl-style regist
            services.AddTransient<ISampleService, SampleService>(); // OK
            //services.AddTransient<ISampleService, SampleService>(sp => new SampleService()); // OK 自行注入
            //services.AddSingleton<ISampleService>(new SampleService()); // OK 自行注入
            //services.AddTransient<ISampleService>(sp => new SampleService()); // 无法推断实现类型
            // class-impl-style regist
            services.AddTransient<BaseCls, SubCls>(); // ok
            //services.AddTransient<BaseCls, SubCls>(sp => new SubCls()); // desc实现类被覆盖，无法代理
            //services.AddSingleton<BaseCls>(new SubCls()); // desc无实现类无法代理
            //services.AddTransient<BaseCls>(sp => new SubCls()); // desc无实现类无法代理
            // instance-style regist
            //services.AddSingleton(new SubCls()); // desc无实现类无法代理
            // class-style regist
            //services.AddTransient<SubCls>(); // OK-depend 子类匹配则父类会被代理，但方法是否拦截需要从子类继承或声明 需要考虑注入

            var serviceProvider = new DynamicProxyServiceProviderFactory().CreateServiceProvider(services);
            var sampleService = serviceProvider.GetService<ISampleService>();
            sampleService.Invoke("le");
            var clsService = serviceProvider.GetService<BaseCls>();
            clsService.Echo("le");
            Console.ReadKey();
        }
    }

    public interface ILogger
    {
        void Info(string message);
    }

    public class ConsoleLogger : ILogger
    {
        public void Info(string message)
        {
            Console.WriteLine(message);
        }
    }

    public interface ISampleService
    {
        [Intercept]
        void Invoke(string name);
    }

    public class SampleService : ISampleService
    {
        [Autowired]
        public ILogger Logger { get; set; }

        public void Invoke(string name)
        {
            Logger?.Info("sample service invoke.");
            Logger?.Info($"{name}");
        }
    }

    public abstract class BaseCls
    {
        [Intercept(Inherited = false)]
        public abstract void Echo(string msg);
    }

    public class SubCls : BaseCls
    {
        [Autowired]
        ILogger Logger { get; set; }

        public override void Echo(string msg)
        {
            Logger?.Info($"Msg Logged: {msg}");
            Console.WriteLine(msg);
            Console.WriteLine("hello from subcls");
        }
    }


    public class Intercept : AbstractInterceptorAttribute
    {
        [FromServiceContext]
        public ILogger Logger { get; set; }

        public override Task Invoke(AspectContext context, AspectDelegate next)
        {
            Logger?.Info($"{nameof(Intercept)}");
            context.Parameters[0] = "lemon";
            return context.Invoke(next);
        }
    }
}