﻿using System.Text;
using Microsoft.CodeAnalysis;

namespace Knapcode.ExplorePackages
{
    public class ReadBuilder : IPropertyVisitor
    {
        private readonly StringBuilder _builder;
        private readonly int _indent;

        public ReadBuilder(int indent)
        {
            _indent = indent;
            _builder = new StringBuilder();
        }

        public void OnProperty(GeneratorExecutionContext context, INamedTypeSymbol nullable, IPropertySymbol symbol, string prettyPropType)
        {
            if (_builder.Length > 0)
            {
                _builder.AppendLine();
            }

            _builder.Append(' ', _indent);

            var propertyType = symbol.Type.ToString();
            switch (propertyType)
            {
                case "bool":
                case "short":
                case "int":
                case "long":
                case "System.Guid":
                case "System.TimeSpan":
                    _builder.AppendFormat("{0} = {1}.Parse(getNextField()),", symbol.Name, prettyPropType);
                    break;
                case "bool?":
                case "short?":
                case "int?":
                case "long?":
                case "System.Guid?":
                case "System.TimeSpan?":
                    _builder.AppendFormat("{0} = CsvUtility.ParseNullable(getNextField(), {1}.Parse),", symbol.Name, prettyPropType);
                    break;
                case "System.Version":
                    _builder.AppendFormat("{0} = CsvUtility.ParseReference(getNextField(), {1}.Parse),", symbol.Name, prettyPropType);
                    break;
                case "string":
                    _builder.AppendFormat("{0} = getNextField(),", symbol.Name);
                    break;
                case "System.DateTimeOffset":
                    _builder.AppendFormat("{0} = CsvUtility.ParseDateTimeOffset(getNextField()),", symbol.Name);
                    break;
                case "System.DateTimeOffset?":
                    _builder.AppendFormat("{0} = CsvUtility.ParseNullable(getNextField(), CsvUtility.ParseDateTimeOffset),", symbol.Name);
                    break;
                default:
                    if (symbol.Type.TypeKind == TypeKind.Enum)
                    {
                        _builder.AppendFormat("{0} = Enum.Parse<{1}>(getNextField()),", symbol.Name, prettyPropType);
                    }
                    else if (PropertyHelper.IsNullableEnum(nullable, symbol))
                    {
                        _builder.AppendFormat("{0} = CsvUtility.ParseNullable(getNextField(), Enum.Parse<{1}>),", symbol.Name, prettyPropType);
                    }
                    else
                    {
                        _builder.AppendFormat("{0} = Parse{0}(getNextField()),", symbol.Name);
                    }
                    break;
            }
        }

        public void Finish(GeneratorExecutionContext context)
        {
        }

        public string GetResult()
        {
            return _builder.ToString();
        }
    }
}
