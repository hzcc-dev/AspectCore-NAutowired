# AspectCore-NAutowired

An extension library to integrate NAutowired and AspectCore framework into Asp.NET Core DI system.

一个基于AspectCore.DependencyInjection的扩展库，使NAutowired可以和AspectCore一起使用。
被代理的原始服务仍可以正常使用[Autowired]来将其它服务自动注入其字段。

# 安装
```Install-Package AspectCore.Extensions.Hosting.NAutowired```

# 示例
鉴于AspectCore文档较老，这里示例一下如何在.NET Core 5.0中使用AspectCore。

## 基于特性拦截
```csharp
//Intercepter
public class DefaultInterceptorAttribute : AbstractInterceptorAttribute
{
    public async override Task Invoke(AspectContext context, AspectDelegate next)
    {
        Console.WriteLine("before service call");
        await next(context);
        Console.WriteLine("after service call");
    }
}
```

```csharp
//Service
public interface IFooService
{
    [DefaultInterceptor]
    public string GetText();
}

public class FooService : IFooService
{
    [Autowired]
    private readonly IDAL dal;

    public string GetText()
    {
        return dal != null ? "Success" : "Fail";
    }
}
```

```csharp
//Program.cs
public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        //省略其它
        .UseServiceProviderFactory(new DynamicProxyServiceProviderFactory());
```

## 全局拦截

```csharp
```csharp
//Intercepter
public class DefaultInterceptorAttribute : AbstractInterceptorAttribute
{
    public async override Task Invoke(AspectContext context, AspectDelegate next)
    {
        Console.WriteLine("before service call");
        await next(context);
        Console.WriteLine("after service call");
    }
}
```

```csharp
//Service
public interface IFooService
{
    public string GetText();
}

public class FooService : IFooService
{
    [Autowired]
    private readonly IDAL dal;

    public string GetText()
    {
        return dal != null ? "Success" : "Fail";
    }
}
```

```csharp
//Program.cs
public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        //省略其它
        .UseServiceProviderFactory(new DynamicProxyServiceProviderFactory());
```

```csharp
//Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    services.ConfigureDynamicProxy(config => {
        config.Interceptors.AddTyped<DefaultInterceptorAttribute>(Predicates.ForMethod("Execute*"));

        config.Interceptors.AddTyped<CustomInterceptorAttribute>(Predicates.ForService("*Service"));
        //App1命名空间下的Service不会被代理
        config.NonAspectPredicates.AddNamespace("App1");

        //最后一级为App1的命名空间下的Service不会被代理
        config.NonAspectPredicates.AddNamespace("*.App1");

        //ICustomService接口不会被代理
        config.NonAspectPredicates.AddService("ICustomService");

        //后缀为Svc的接口和类不会被代理
        config.NonAspectPredicates.AddService("*Svc");

        //命名为Query的方法不会被代理
        config.NonAspectPredicates.AddMethod("Query");

        //后缀为Query的方法不会被代理
        config.NonAspectPredicates.AddMethod("*Query");
    });
}
```

