using AutoMapper;
using Siffrum.Ecom.DomainModels.Foundation.Base;
using Siffrum.Ecom.ServiceModels.Foundation.Base;
using Siffrum.Ecom.ServiceModels.Foundation.Base.CommonResponseRoot;
using System.Reflection;

namespace Siffrum.Ecom.Foundation.AutoMapperBindings
{
    public static class AutoMapperExtensions
    {
        public static AutoMappingResponse RegisterAutoMapperFromDmToSm<DmT, SmT>(this IProfileExpression expression, Action<IMappingExpression, Type, Type> afterAutoMapAction = null, string dmNameExtension = "DM", string smNameExtension = "SM", Action<IMappingExpression, Type, Type> additionMapAction = null)
        {
            Action<IMappingExpression, Type, Type> additionMapAction2 = additionMapAction;
            string dmNameExtension2 = dmNameExtension;
            string smNameExtension2 = smNameExtension;
            AutoMappingResponse mappingResp = new AutoMappingResponse();
            if (afterAutoMapAction == null)
            {
                afterAutoMapAction = delegate (IMappingExpression map, Type source, Type destination)
                {
                    mappingResp.SuccessDmToSmMaps.Add(source.FullName + "_____" + destination.FullName);
                    map.IgnoreAllVirtualsForType(source).IgnoreAllVirtualsForType(destination).IgnorePropertiesFromConversionIfMarked(source, destination)
                        .HandleLocalDateTimeConversionProperties(source, destination)
                        .HandleFilePathToUriProperties(source, destination)
                        .MaxDepth(10);
                    if (additionMapAction2 != null)
                    {
                        additionMapAction2(map, source, destination);
                    }

                    IMappingExpression arg = map.ReverseMap().IgnoreAllVirtualsForType(source).IgnoreAllVirtualsForType(destination)
                        .IgnorePropertiesFromConversionIfMarked(destination, source)
                        .MaxDepth(10);
                    mappingResp.SuccessSmToDmMaps.Add(destination.FullName + "_____" + source.FullName);
                    if (additionMapAction2 != null)
                    {
                        additionMapAction2(arg, destination, source);
                    }
                };
            }

            Func<Type, bool> filter = (x) => x.IsClass && !x.IsAbstract && x.Name.EndsWith(dmNameExtension2) && x.IsAssignableTo(typeof(DomainModelRootBase)) && x.GetCustomAttributesData().FirstOrDefault((y) => y.AttributeType == typeof(IgnoreClassAutoMapAttribute)) == null;
            expression.CreateMapsForSourceTypes(typeof(DmT).Assembly, filter, delegate (Type srcType)
            {
                string name = typeof(SmT).Assembly.GetName().Name;
                name += srcType.Namespace.Replace(typeof(DmT).Assembly.GetName().Name, "");
                name = name + "." + srcType.Name.Replace(dmNameExtension2, smNameExtension2);
                Type type = typeof(SmT).Assembly.GetType(name);
                if (type != null && type.IsAssignableTo(typeof(BaseServiceModelRoot)) && type.GetCustomAttributesData().FirstOrDefault((y) => y.AttributeType == typeof(IgnoreClassAutoMapAttribute)) == null)
                {
                    return type;
                }

                mappingResp.UnsuccessfullPaths.Add(srcType.FullName + "_____" + type);
                return null;
            }, afterAutoMapAction);
            return mappingResp;
        }

        public static void CreateMapsForSourceTypes(this IProfileExpression configuration, Assembly srcAssembly, Func<Type, bool> filter, Func<Type, Type> destinationType, Action<IMappingExpression, Type, Type> mappingConfiguration)
        {
            Type[] exportedTypes = srcAssembly.GetExportedTypes();
            configuration.CreateMapsForSourceTypes(exportedTypes.Where(filter), destinationType, mappingConfiguration);
        }

        public static void CreateMapsForSourceTypes(this IProfileExpression configuration, IEnumerable<Type> typeSource, Func<Type, Type> destinationType, Action<IMappingExpression, Type, Type> mappingConfiguration)
        {
            foreach (Type item in typeSource)
            {
                Type type = destinationType(item);
                if (!(type == null))
                {
                    IMappingExpression arg = configuration.CreateMap(item, type);
                    mappingConfiguration(arg, item, type);
                }
            }
        }

        public static IMappingExpression IgnoreAllVirtualsForType(this IMappingExpression expression, Type targetType)
        {
            foreach (PropertyInfo property in from p in targetType.GetProperties()
                                              where p.GetGetMethod().IsVirtual && !p.GetGetMethod().IsFinal
                                              select p)
            {
                expression.ForAllMembers(delegate (IMemberConfigurationExpression opt)
                {
                    if (opt.DestinationMember.Name == property.Name)
                    {
                        opt.Ignore();
                    }
                });
            }

            return expression;
        }

        public static IMappingExpression<TSource, TDestination> IgnoreAllDestinationVirtual<TSource, TDestination>(this IMappingExpression<TSource, TDestination> expression)
        {
            return (IMappingExpression<TSource, TDestination>)((IMappingExpression)expression).IgnoreAllVirtualsForType(typeof(TDestination));
        }

        public static IMappingExpression<TSource, TDestination> IgnoreAllSourceVirtual<TSource, TDestination>(this IMappingExpression<TSource, TDestination> expression)
        {
            return (IMappingExpression<TSource, TDestination>)((IMappingExpression)expression).IgnoreAllVirtualsForType(typeof(TSource));
        }

        public static IMappingExpression IgnorePropertiesFromConversionIfMarked(this IMappingExpression expression, Type sourceType, Type destinationType)
        {
            Type sourceType2 = sourceType;
            Type destinationType2 = destinationType;
            List<string> list = (from x in sourceType2.GetProperties()
                                 where x.CustomAttributes.Any((y) => y.AttributeType == typeof(IgnorePropertyOnReadAttribute) && IsConversionValid(y, sourceType2, destinationType2) ? true : false)
                                 select x.Name).ToList();
            foreach (string item in list)
            {
                expression.ForMember(item, delegate (IMemberConfigurationExpression x)
                {
                    x.Ignore();
                });
            }

            List<string> list2 = (from x in destinationType2.GetProperties()
                                  where x.CustomAttributes.Any((y) => y.AttributeType == typeof(IgnorePropertyOnWriteAttribute) && IsConversionValid(y, sourceType2, destinationType2) ? true : false)
                                  select x.Name).ToList();
            foreach (string item2 in list2)
            {
                expression.ForMember(item2, delegate (IMemberConfigurationExpression x)
                {
                    x.Ignore();
                });
            }

            return expression;
        }

        private static bool IsConversionValid(CustomAttributeData customData, Type sourceType, Type destinationType)
        {
            object value = customData.ConstructorArguments.First().Value;
            object obj = value;
            if (obj is int)
            {
                switch ((int)obj)
                {
                    case 0:
                        return true;
                    case 1:
                        return typeof(BaseServiceModelRoot).IsAssignableFrom(sourceType) && typeof(DomainModelRootBase).IsAssignableFrom(destinationType);
                    case 2:
                        return typeof(DomainModelRootBase).IsAssignableFrom(sourceType) && typeof(BaseServiceModelRoot).IsAssignableFrom(destinationType);
                }
            }

            throw new NotImplementedException();
        }

        public static IMappingExpression HandleLocalDateTimeConversionProperties(this IMappingExpression expression, Type sourceType, Type destinationType)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            PropertyInfo[] properties = destinationType.GetProperties();
            foreach (PropertyInfo propertyInfo in properties)
            {
                CustomAttributeData customAttributeData = propertyInfo.CustomAttributes.Where((x) => x.AttributeType == typeof(ConvertFromUtcToLocalDateAttribute)).FirstOrDefault();
                if (customAttributeData == null)
                {
                    continue;
                }

                if (customAttributeData.NamedArguments != null && customAttributeData.NamedArguments.Count <= 0)
                {
                    dictionary.Add(propertyInfo.Name, propertyInfo.Name);
                    continue;
                }

                dictionary.Add(propertyInfo.Name, customAttributeData.NamedArguments?.FirstOrDefault((z) => z.MemberName == "SourcePropertyName").TypedValue.Value?.ToString());
            }

            foreach (KeyValuePair<string, string> item in dictionary)
            {
                expression.ForMember(item.Key, delegate (IMemberConfigurationExpression opt)
                {
                    opt.ConvertUsing(new LocalDateTimeValueConverter(), item.Value ?? item.Key);
                });
            }

            return expression;
        }

        public static IMappingExpression HandleFilePathToUriProperties(this IMappingExpression expression, Type sourceType, Type destinationType)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            PropertyInfo[] properties = destinationType.GetProperties();
            foreach (PropertyInfo propertyInfo in properties)
            {
                CustomAttributeData customAttributeData = propertyInfo.CustomAttributes.Where((x) => x.AttributeType == typeof(ConvertFilePathToUriAttribute)).FirstOrDefault();
                if (customAttributeData == null)
                {
                    continue;
                }

                if (customAttributeData.NamedArguments != null && customAttributeData.NamedArguments.Count <= 0)
                {
                    dictionary.Add(propertyInfo.Name, propertyInfo.Name);
                    continue;
                }

                dictionary.Add(propertyInfo.Name, customAttributeData.NamedArguments?.FirstOrDefault((z) => z.MemberName == "SourcePropertyName").TypedValue.Value?.ToString());
            }

            foreach (KeyValuePair<string, string> item in dictionary)
            {
                expression.ForMember(item.Key, delegate (IMemberConfigurationExpression opt)
                {
                    opt.ConvertUsing(new FilePathToUrlConverter(), item.Value ?? item.Key);
                });
            }

            return expression;
        }
    }
}
