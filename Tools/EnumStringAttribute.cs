using System;
using System.Reflection;

namespace Tools
{
    /// <summary>
    ///  Used on enumerations for providing alternative string value
    /// </summary>
    /// 18/11/2009
    /// <remarks>These string values are accesible by using the <see cref="EnumerationExtensions.StringValue"/> extension method</remarks>
    /// <example>
    /// <code>
    /// public enum CharSet
    /// {
    ///     [StringValue("abcdefgijkmnopqrstwxyz")]
    ///     Letters,
    ///     [StringValue("ABCDEFGHIJKLMNOPQRSTUVWXYZ")]
    ///     CapLetters,
    ///     [StringValue("0123456789")]
    ///     Numbers,
    ///     [StringValue("!@#$%^*_-")]
    ///     SpecialCharacters
    /// }
    /// </code>
    /// </example>
    public class StringValueAttribute : Attribute
    {
        private string _Value;

        ///<summary>
        ///   Attribute function that defines a string value
        ///</summary>
        ///<param name="value">The string value of the enumeration member</param>
        public StringValueAttribute(string value)
        { _Value = value; }

        ///<summary>
        ///   Returns the string value
        ///</summary>
        public string Value
        {
            get { return _Value; }
        }
    }

    /// <summary>
    ///   Extension methods for enumeration types
    /// </summary>
    public static class EnumerationExtensions
    {
        /// <summary>
        /// Returns the string value of <see cref="StringValueAttribute"/>
        ///  </summary>
        /// <param name="value">The enumerator value.</param>
        /// <returns>The string alternative</returns>
        /// 18/11/2009
        /// <example>
        /// <code>
        ///   string letters = CharSet.Letters.StringValue()
        /// </code>
        /// </example>
        /// <exception cref="InvalidOperationException">An appropritate exception is raised if accessed enumeration value does not have a <see cref="StringValueAttribute"/> defined</exception>
        public static string StringValue(this Enum value)
        {
            string output = null;
            Type type = value.GetType();
            FieldInfo fi = type.GetField(value.ToString());
            StringValueAttribute[] attrs = fi.GetCustomAttributes(typeof(StringValueAttribute), false) as StringValueAttribute[];
            if (attrs != null)
                if (attrs.Length > 0)
                {
                    output = attrs[0].Value;
                }
                else
                {
                    throw new InvalidOperationException("The StringValue can just be invoked on String enumerations having the '[StringValue(...)]' attribute.");
                }
            return output;
        }

        /// <summary>
        /// Parses the string and checks if it matches a value of the specified T enumeration.
        /// </summary>
        /// <typeparam name="T">An enumeration</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>The enumeration value if matched otherwise an exception will be thrown</returns>
        /// <example>
        /// <code>
        ///   CharacterSet MySet = "Numbers".ParseAsEnum&lt;CharacterSet&gt;();
        /// </code>
        /// </example>
        public static T ParseAsEnum<T>(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException
                    ("Can't parse an empty string");
            }

            Type enumType = typeof(T);
            if (!enumType.IsEnum)
            {
                throw new InvalidOperationException
                    ("Here's why you need enum constraints!!!");
            }

            // warning, can throw
            return (T)Enum.Parse(enumType, value);
        }

        /// <summary>
        /// Parses the string and checks if it the specified T enumeration contains a member with the relevant StringValue attribute.
        /// </summary>
        /// <typeparam name="T">An enumeration</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>The enumeration value if matched otherwise an exception will be thrown</returns>
        /// <example>
        /// <code>
        ///   CharacterSet MySet = "0123456789".ParseAsStringEnum&lt;CharacterSet&gt;();
        /// </code>
        /// </example>

        public static T ParseAsStringEnum<T>(this string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException("Can't parse an empty string");

            Type enumType = typeof(T);
            if (!enumType.IsEnum)
                throw new InvalidOperationException("Here's why you need enum constraints!!!");

            FieldInfo[] fiArray = enumType.GetFields();
            foreach (FieldInfo fi in fiArray)
            {
                StringValueAttribute[] attrs = fi.GetCustomAttributes(typeof(StringValueAttribute), false) as StringValueAttribute[];
                if ((attrs != null) && (attrs.Length == 1) && (attrs[0].Value.Equals(value)))
                        return (T)Enum.Parse(enumType, fi.Name);
            }
            throw new ArgumentOutOfRangeException(String.Format("StringValueAttribute with value {0} not exist in specified enum", value));
        }

    }

}
