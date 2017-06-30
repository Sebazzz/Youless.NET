namespace YoulessNet.Internal {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using JetBrains.Annotations;

    /// <summary>
    ///     Helper utilities for manupilating objects
    /// </summary>
    internal static class ObjectUtils {
        /// <summary>
        ///     Gets all properties from the specified object.
        /// </summary>
        /// <remarks>
        ///     This method is primarely to be used with anyonymous objects.
        /// </remarks>
        /// <param name="o"></param>
        /// <returns></returns>
        public static IDictionary<string, object> GetProperties([NotNull] object o) {
            if (o == null) {
                throw new ArgumentNullException("o");
            }
            
            // if it is a generic dictionary, return it
            IDictionary<string, object> gDictionary = o as IDictionary<string, object>;
            if (gDictionary != null) {
                return gDictionary;
            }

            // if it is a dictionary, return it
            IDictionary oDirectionary = o as IDictionary;
            if (oDirectionary != null) {
                return oDirectionary.Keys.OfType<object>()
                    .ToDictionary(e => (e as String ?? e.ToString()), e => oDirectionary[e]);
            }

            // get properties of anonymous object
            PropertyInfo[] props = o.GetType().GetTypeInfo().DeclaredProperties.ToArray();
            IDictionary<string, object> values = new Dictionary<string, object>(props.Length);

            foreach (PropertyInfo prop in props) {
                object val = prop.GetValue(o, null);
                if (val != null) {
                    values.Add(prop.Name, val);
                }
            }

            return values;
        }

        /// <summary>
        /// Gets the object as a culture-neutral string
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static string GetCultureNeutralString(object o) {
            if (o == null) {
                return null;
            }

            IFormattable formattableObject = o as IFormattable;

            return formattableObject != null
                ? formattableObject.ToString(null, CultureInfo.InvariantCulture)
                : o.ToString();
        }
    }
}