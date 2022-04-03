using Amg.Extensions;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Amg.GetOpt
{
    internal class Command : ICommand
    {
        private readonly object instance;
        public MethodInfo Method { get; private set; }

        public Command(object instance, MethodInfo method)
        {
            this.instance = instance;
            this.Method = method;
            this.Name = Parser.LongName(method);
        }

        public Task<object?> Invoke(ParserState args, IValueParser valueParser)
        {
            object? Get(ParameterInfo parameter)
            {
                if (!args.HasCurrent)
                {
                    if (parameter.HasDefaultValue)
                    {
                        return parameter.DefaultValue;
                    }
                    else
                    {
                        throw new CommandLineException(args, $"missing parameter {parameter.Name} for command {this.Name}.");
                    }
                }

                return valueParser.Parse(args, parameter.ParameterType);
            }

            var parameters = Method.GetParameters().Select(Get).ToArray();
            return AsTask(Method.Invoke(instance, parameters));
        }

        /// <summary>
        /// Waits if returnValue is Task
        /// </summary>
        /// <param name="returnValue"></param>
        /// <returns></returns>
        static Task<object?> AsTask(object? returnValue)
        {
            if (returnValue is Task task)
            {
                var type = returnValue.GetType();
                var resultProperty = type.GetProperty("Result");
                return task.ContinueWith((_) =>
                {
                    try
                    {
                        var result = resultProperty == null
                            ? null
                            : resultProperty.GetValue(task);
                        return result;
                    }
                    catch (TargetInvocationException targetInvocationException)
                    {
                        var inner = targetInvocationException.InnerException?.InnerException;
                        if (inner is { })
                        {
                            throw inner;
                        }
                        else
                        {
                            throw;
                        }
                    }
                });
            }
            else
            {
                return Task.FromResult<object?>(returnValue);
            }
        }

        public string Name { get; }

        static string TypeSyntax(Type type)
            => type == typeof(string)
                ? String.Empty
                : $": {Parser.LongNameForCsharpIdentifier(type.Name)}";

        static string GetSyntax(ParameterInfo p)
        {
            var s = $"{Parser.LongNameForCsharpIdentifier(p.Name!)}{TypeSyntax(p.ParameterType)}";
            return p.HasDefaultValue 
                ? s.Quote("[", "]")
                : s.Quote("<", ">");
        }

        public string Syntax => new[] { Name }.Concat(Method.GetParameters().Select(GetSyntax)).Join(" ");
        public string ParameterSyntax => Method.GetParameters().Select(GetSyntax).Join(" ");

        public string? Description => Method.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>()?.Description;

        public bool IsDefault => this.Method.Has<DefaultAttribute>();
    }

}

