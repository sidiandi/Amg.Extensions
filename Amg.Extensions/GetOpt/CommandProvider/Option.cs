using Amg.Extensions;
using System;
using System.Linq;
using System.Reflection;

namespace Amg.GetOpt
{
    internal class Option : IOption
    {
        private readonly object instance;
        public PropertyInfo Property { get; private set; }

        public Option(object instance, PropertyInfo property)
        {
            this.instance = instance;
            this.Property = property;
            Long = Parser.LongName(property);
        }

        public void Set(string? value, ParserState args, IValueParser valueParser)
        {
            if (value == null)
            {
                value = DefaultValue();
            }

            if (value == null && args.HasCurrent)
            {
                value = args.Consume();
            }

            if (value == null)
            {
                throw new CommandLineException(args, $"Value is missing for {this}.");
            }

            Set(value, valueParser);
        }

        public void Set(ref string? appendedValue, ParserState args, IValueParser valueParser)
        {
            var defaultValue = DefaultValue();

            if (defaultValue != null)
            {
                Set(defaultValue, valueParser);
            }
            else
            {
                if (appendedValue != null)
                {
                    Set(appendedValue, valueParser);
                    appendedValue = null;
                }
                else
                {
                    Set(args, valueParser);
                }
            }
        }

        void Set(string valueString, IValueParser valueParser)
        {
            Set(new ParserState(new[] { valueString }), valueParser);
        }

        void Set(ParserState args, IValueParser valueParser)
        {
            var value = valueParser.Parse(args, Property.PropertyType);
            Property.SetValue(this.instance, value);
        }

        string? DefaultValue()
        {
            if (Property.PropertyType == typeof(bool))
            {
                return true.ToString();
            }
            else
            {
                return null;
            }
        }

        public void SetValue(object? value)
        {
            Property.SetValue(instance, new object?[] { value });
        }

        public string Name => Property.Name;

        public string Long { get; }

        public string? Short => ShortAttribute.Get(Property);

        string ShortSyntax => (Short == null ? String.Empty : $"{Parser.shortOptionPrefix}{Short}|");

        string ValueSyntax
        {
            get
            {
                var type = Property.PropertyType;
                if (type == typeof(bool))
                {
                    return String.Empty;
                }
                else if (type.IsEnum)
                {
                    return $"<{Enum.GetNames(type).Select(_ => Parser.LongNameForCsharpIdentifier(_)).Join("|")}>";
                }
                else
                {
                    return $"<{Parser.LongNameForCsharpIdentifier(Property.PropertyType.Name)}>";
                }
            }
        }

        public string Syntax => $"{ShortSyntax}{Parser.longOptionPrefix}{Long} {ValueSyntax}";

        public string? Description => Property.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>()?.Description;

        public override string ToString()
            => $"{Parser.longOptionPrefix}{Long}";
    }

}

