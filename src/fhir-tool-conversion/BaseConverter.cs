﻿using System;
using System.Collections.Generic;
using System.Linq;
using Hl7.Fhir.Model;
using Hl7.Fhir.Utility;

namespace FhirTool.Conversion
{
    internal abstract class BaseConverter
    {
        protected Dictionary<(Type, Type), Delegate> Map { get; set; } = new Dictionary<(Type, Type), Delegate>();
        protected Dictionary<Type, Type> ComponentTargetToSourceTypeMap { get; set; }
        protected Dictionary<Type, Type> ComponentSourceToTargetTypeMap { get; set; }

        public BaseConverter()
        {
            InitComponentTypeMap();
        }

        protected abstract void InitComponentTypeMap();

        public abstract Type GetTargetCodeType();

        public abstract Type GetSourceCodeType();
        protected abstract string GetFhirTypeNameForTargetType(Type targetType);

        protected abstract Type GetTargetTypeForFhirTypeName(string targetTypeName);

        protected abstract string GetFhirTypeNameForSourceType(Type sourceType);

        protected abstract Type GetSourceTypeForFhirTypeName(string sourceTypeName);

        private Type GetSourceFhirComponentType(Type targetType)
        {
            return ComponentTargetToSourceTypeMap.GetOrDefault(targetType);
        }

        private Type GetTargetFhirComponentType(Type sourceType)
        {
            return ComponentSourceToTargetTypeMap.GetOrDefault(sourceType);
        }

        private Type GetSourceStandardFhirType(Type targetType)
        {
            var name = GetFhirTypeNameForTargetType(targetType);
            return name != null ? GetSourceTypeForFhirTypeName(name) : null;
        }

        private Type GetTargetStandardFhirType(Type sourceType)
        {
            var name = GetFhirTypeNameForSourceType(sourceType);
            return name != null ? GetTargetTypeForFhirTypeName(name) : null;
        }

        public Type GetSourceFhirType(Type targetType)
        {
            return GetSourceStandardFhirType(targetType) ?? GetSourceFhirComponentType(targetType);
        }

        public Type GetTargetFhirType(Type sourceType)
        {
            return GetTargetStandardFhirType(sourceType) ?? GetTargetFhirComponentType(sourceType);
        }

        public bool IsTargetFhirType(Type type)
        {
            return GetTargetFhirType(type) != null;
        }

        public bool IsSourceFhirType(Type type)
        {
            return GetSourceFhirType(type) != null;
        }

        public void Convert(FhirConverter converter, Base to, Base from)
        {
            var toType = to.GetType();
            var fromType = from.GetType();
            var key = (toType, fromType);
            if (Map.TryGetValue(key, out var convert))
            {
                convert.DynamicInvoke(converter, to, from);
            }
        }

        public TTo? ConvertEnum<TTo>(Enum from) where TTo : struct
        {
            if (from == null) return default;

            return EnumUtility.ParseLiteral<TTo>(from.GetLiteral());
        }

        public Enum ConvertEnum(Enum from, Type toType)
        {
            if (from == null) return null;
            return EnumUtility.ParseLiteral(from.GetLiteral(), toType) as Enum;
        }
    }
}
