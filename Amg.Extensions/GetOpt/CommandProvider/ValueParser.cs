using Amg.Extensions;

namespace Amg.GetOpt
{
    internal class ValueParser : IValueParser
    {
        public object? Parse(ParserState args, Type toType)
        {
            if (toType.IsArray)
            {
                return ParseArray(args, toType);
            }

            var temp = args.Clone();

            string Consume()
            {
                if (!temp.HasCurrent)
                {
                    throw new CommandLineException(args, "Value is missing.");
                }
                return temp.Consume();
            }

            try
            {
                if (toType == typeof(bool))
                {
                    return bool.Parse(Consume());
                }
                else if (toType == typeof(int))
                {
                    return int.Parse(Consume());
                }
                else if (toType == typeof(double))
                {
                    return double.Parse(Consume());
                }
                else if (toType == typeof(string))
                {
                    return Consume();
                }
                else if (toType.IsEnum)
                {
                    var enumName = Enum.GetNames(toType).FindByName(_ => Parser.LongNameForCsharpIdentifier(_), Consume(), "values");
                    return Enum.Parse(toType, enumName);
                }
                throw new ArgumentException($"Cannot parse {args.Current.Quote()} as {toType.Name}.");
            }
            catch (Exception e)
            {
                temp.SetPos(args);
                if (e is CommandLineException)
                {
                    throw;
                }
                throw new CommandLineException(args, $"{args.Current.Quote()} is not a value of type {toType.Name}.", e);
            }
            finally
            {
                args.SetPos(temp);
            }
        }

        public object? ParseArray(ParserState args, Type toType)
        {
            var temp = args.Clone();

            var elementType = toType.GetElementType();
            if (elementType is null)
            {
                throw new InvalidOperationException();
            }

            var items = new List<object?>();

            while (temp.HasCurrent)
            {
                var item = Parse(temp, elementType);
                items.Add(item);
            }

            args.SetPos(temp);

            var a = Array.CreateInstance(elementType, items.Count);
            for (int i=0;i<a.Length; ++i)
            {
                a.SetValue(items[i], i);
            }
            return a;
        }
    }
}