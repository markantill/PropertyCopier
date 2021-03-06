﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using PropertyCopier.Comparers;
using PropertyCopier.Data;

namespace PropertyCopier.Generators
{
    /// <summary>
    /// Remove any properties that the mapping data has marked to ignore.
    /// </summary>
    internal class IgnoreTargetPropertiesGenerator : IExpressionGenerator
    {
        public ExpressionGeneratorResult GenerateExpressions(
            Expression sourceExpression,
            ICollection<PropertyInfo> targetProperties,
            MappingData mappingData,
            IEqualityComparer<string> memberNameComparer)
        {
            var alreadyMatched = mappingData.PropertyIgnoreLambdaExpressions == null
                ? new HashSet<PropertyInfo>()
                : new HashSet<PropertyInfo>(mappingData.PropertyIgnoreLambdaExpressions.Select(ExpressionBuilder.GetMemberInfo)
                    .OfType<PropertyInfo>());

            var newTargetProperties = targetProperties.Except(alreadyMatched, new PropertyInfoComparer()).ToArray();

            return new ExpressionGeneratorResult
            {
                UnmappedTargetProperties = newTargetProperties,
                Expressions = new List<PropertyAndExpression>(),
            };
        }

        public IEqualityComparer<string> MemberNameComparer { get; set; }
    }
}
