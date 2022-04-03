using System.Collections.Generic;
using System.ComponentModel;
using Amg.Extensions;
using Amg.GetOpt;

namespace Amg.OnceImpl.Example;

[Description("describe what the script does")]
public class MyBuild
{
    protected MyBuild(string? result)
    {
        if (result != null)
        {
            this.result.Enqueue(result);
        }
    }

    protected MyBuild()
    {
    }

    [Default, Description("Compile, link, and pack")]
    public virtual async Task All()
    {
        await Task.WhenAll(
            Pack(),
            Test()
            );
    }

    [Description("Release or Debug")]
    public virtual string Configuration { get; set; } = "Release";

    protected virtual MyBuild Nested => Once.Create<MyBuild>();

    [Description("Compile source code")]
    public virtual async Task Compile()
    {
        await Task.CompletedTask;
        await Task.Delay(200);
        result.Enqueue(nameof(Compile));
    }

    [Description("Link object files")]
    public virtual async Task Link()
    {
        await Compile();
        await Task.Delay(200);
        result.Enqueue(nameof(Link));
    }

    [Description("Run unit tests")]
    public virtual async Task Test()
    {
        await Link();
        await Task.Delay(200);
        result.Enqueue(nameof(Test));
    }

    [Description("Say hello")]
    public virtual async Task<string> SayHello(string name)
    {
        await Task.CompletedTask;
        return $"Hello, {name}";
    }

    [Description("Demonstrate the use of params")]
    public virtual string UseParams(params string[] items)
    {
        return items.Join();
    }

    [Description("Say something")]
    public virtual async Task SaySomething(string? something = null)
    {
        if (something != null)
        {
            Console.WriteLine(something);
        }
        await Task.CompletedTask;
    }

    [Description("Pack nuget package")]
    public virtual async Task Pack()
    {
        await Compile();
        await Link();
        await Task.Delay(200);
        result.Enqueue(nameof(Pack));
    }

    public virtual async Task<int> Times2(int a)
    {
        await Task.CompletedTask;
        args.Add(a);
        return a * 2;
    }

    public virtual async Task<int> Div2(int a)
    {
        await Task.Delay(100);
        return await Times2(a) / 4;
    }

    readonly IList<int> args = new List<int>();
    public readonly Queue<string> result = new Queue<string>();

    public virtual async Task WhatCouldGoWrong()
    {
        await Task.CompletedTask;
        throw new Exception("epic fail");
    }

    [Description("Calls a tool that always fails.")]
    public virtual async Task ToolFails()
    {
        await Task.CompletedTask;
    }

    [Description("Always fails.")]
    public virtual async Task AlwaysFails()
    {
        await WhatCouldGoWrong();
    }

    [System.ComponentModel.Description("Get information")]
    public virtual async Task<object> GetInfo()
    {
        return await Task.FromResult(Environment.GetLogicalDrives());
    }
}
