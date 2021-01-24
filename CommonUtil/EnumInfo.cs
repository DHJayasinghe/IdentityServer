using CommonUtil.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace CommonUtil
{
    public static class EnumInfo
    {
        /// <summary>
        /// Get list of available enum values and theier description + group attributes
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <returns></returns>
        public static IEnumerable<EnumDescription> GetList<TEnum>() where TEnum : struct =>
            Enum.GetValues(typeof(TEnum)).Cast<TEnum>()
                .Select(x => new EnumDescription(
                    name: x.ToString(),
                    description: x.GetType().GetMember(x.ToString()).FirstOrDefault()?.GetCustomAttribute<DisplayAttribute>()?.Description, //retrieve description attribute
                    group: x.GetType().GetMember(x.ToString()).FirstOrDefault()?.GetCustomAttribute<DisplayAttribute>()?.GroupName //retrieve group name attribute
                )).ToList();

        /// <summary>
        /// Get name of provided <paramref name="enumValue"/>
        /// </summary>
        /// <param name="enumValue"></param>
        /// <returns></returns>
        public static string GetName<TEnum>(this TEnum enumValue) where TEnum : struct => enumValue.ToString();

        /// <summary>
        /// Get description attribute value of provided <paramref name="enumValue"/>
        /// </summary>
        /// <param name="enumValue"></param>
        /// <returns></returns>
        public static string GetDescription<TEnum>(this TEnum enumValue) where TEnum : struct =>
            enumValue.GetType().GetMember(enumValue.ToString()).FirstOrDefault()?.GetCustomAttribute<DisplayAttribute>()?.Description;

        /// <summary>
        /// Get description attribute value of provided <paramref name="enumValue"/>
        /// </summary>
        /// <param name="enumValue"></param>
        /// <returns></returns>
        public static string GetAudience<TEnum>(this TEnum enumValue) where TEnum : struct =>
            enumValue.GetType().GetMember(enumValue.ToString()).FirstOrDefault()?.GetCustomAttribute<AudienceAttribute>()?.Audience;

        /// <summary>
        /// Convert provided <paramref name="value"/> to specified <typeparamref name="TEnum"/>, and returns its int value
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int GetValue<TEnum>(string value) where TEnum : struct, IComparable, IFormattable, IConvertible =>
            (int)Enum.Parse(typeof(TEnum), value);

        /// <summary>
        /// Get name of provided <paramref name="enumValue"/>
        /// </summary>
        /// <param name="enumValue"></param>
        /// <returns></returns>
        public static int GetValue<TEnum>(this TEnum enumValue) where TEnum : struct => (int)Enum.Parse(typeof(TEnum), enumValue.ToString());

        public static string GetDescriptionByInt<TEnum>(string value) where TEnum : struct, IComparable, IFormattable, IConvertible =>
             ((TEnum)Enum.Parse(typeof(TEnum), value)).ToString();
    }

    public sealed class EnumDescription
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Group { get; set; }
        public EnumDescription() { }

        public EnumDescription(string name, string description, string group)
        {
            Name = name;
            Description = description;
            Group = group;
        }
    }
}
