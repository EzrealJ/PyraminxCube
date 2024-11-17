using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PyraminxCube.Extensions.Core
{
    //[DebuggerStepThrough]
    public static class StringExtensions
    {
        /// <summary>
        /// 字符串是否为【null或""】
        /// <para>
        /// <see cref="string.IsNullOrEmpty(string?)"/>的扩展方法版本
        /// </para>
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this string? value) => string.IsNullOrEmpty(value);
        /// <summary>
        /// 字符串不为【null或""】
        /// <para>
        /// <see cref="string.IsNullOrEmpty(string?)"/>的否定
        /// </para>
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsNotNullOrEmpty(this string? value) => !string.IsNullOrEmpty(value);
        /// <summary>
        /// 字符串是否为【null或空白字符串】
        /// <para>
        /// <see cref="string.IsNullOrWhiteSpace(string?)"/>的扩展方法版本"/>
        /// </para>
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsNullOrWhiteSpace(this string? value) => string.IsNullOrWhiteSpace(value);
        /// <summary>
        /// 字符串不为【null或空白字符串】
        /// <para>
        /// <see cref="string.IsNullOrWhiteSpace(string?)"/>的否定
        /// </para>
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsNotNullOrWhiteSpace(this string? value) => !string.IsNullOrWhiteSpace(value);
        /// <summary>
        /// 确保字符串以指定字符串开始
        /// </summary>
        /// <param name="str"></param>
        /// <param name="arg"></param>
        /// <param name="comparisonType"></param>
        /// <returns></returns>
        public static string EnsureStartsWith(this string? str, string arg, StringComparison comparisonType = StringComparison.Ordinal)
        {
            ArgumentException.ThrowIfNullOrEmpty(str, nameof(str));
            if (str.StartsWith(arg, comparisonType))
            {
                return str;
            }
            return arg + str;
        }
        /// <summary>
        /// 确保字符串以指定字符串结束
        /// </summary>
        /// <param name="str"></param>
        /// <param name="arg"></param>
        /// <param name="comparisonType"></param>
        /// <returns></returns>
        public static string EnsureEndsWith(this string? str, string arg, StringComparison comparisonType = StringComparison.Ordinal)
        {
            ArgumentException.ThrowIfNullOrEmpty(str, nameof(str));
            if (str.EndsWith(arg, comparisonType))
            {
                return str;
            }
            return str + arg;
        }
        /// <summary>
        /// 截取字符串的前n个字符作为新的字符串
        /// </summary>
        /// <param name="str"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string Left(this string? str, int length)
        {
            ArgumentException.ThrowIfNullOrEmpty(str, nameof(str));
            if (length <= 0)
            {
                return string.Empty;
            }
            return str.Length <= length ? str : str[..length];
        }
        /// <summary>
        /// 截取字符串的后n个字符作为新的字符串
        /// </summary>
        /// <param name="str"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string Right(this string? str, int length)
        {
            ArgumentException.ThrowIfNullOrEmpty(str, nameof(str));
            if (length <= 0)
            {
                return string.Empty;
            }
            return str.Length <= length ? str : str.Substring(str.Length - length, length);
        }
    }
}
