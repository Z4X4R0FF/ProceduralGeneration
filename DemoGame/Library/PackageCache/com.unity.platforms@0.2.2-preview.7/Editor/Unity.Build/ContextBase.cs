using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Properties.Editor;
using UnityEditor;
using UnityEngine;

namespace Unity.Build
{
    public abstract class ContextBase : IDisposable
    {
        readonly Dictionary<Type, object> m_Values = new Dictionary<Type, object>();
        internal BuildPipeline BuildPipeline { get; }
        internal BuildConfiguration BuildConfiguration { get; }

        /// <summary>
        /// List of all values stored.
        /// </summary>
        public object[] Values => m_Values.Values.ToArray();

        /// <summary>
        /// The build configuration name.
        /// </summary>
        /// <returns>The build configuration name.</returns>
        public string BuildConfigurationName => BuildConfiguration.name;

        /// <summary>
        /// The build configuration asset path.
        /// </summary>
        /// <returns>The build configuration asset path.</returns>
        public string BuildConfigurationAssetPath => AssetDatabase.GetAssetPath(BuildConfiguration);

        /// <summary>
        /// The build configuration asset GUID.
        /// </summary>
        /// <returns>The build configuration asset GUID.</returns>
        public string BuildConfigurationAssetGUID => AssetDatabase.AssetPathToGUID(BuildConfigurationAssetPath);

        /// <summary>
        /// Determine if the value of type <typeparamref name="T"/> exists.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <returns><see langword="true"/> if value is found, <see langword="false"/> otherwise.</returns>
        public bool HasValue<T>() where T : class => m_Values.Keys.Any(type => typeof(T).IsAssignableFrom(type));

        /// <summary>
        /// Get value of type <typeparamref name="T"/> if found, otherwise <see langword="null"/>.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <returns>The value of type <typeparamref name="T"/> if found, otherwise <see langword="null"/>.</returns>
        public T GetValue<T>() where T : class => m_Values.FirstOrDefault(pair => typeof(T).IsAssignableFrom(pair.Key)).Value as T;

        /// <summary>
        /// Get value of type <typeparamref name="T"/> if found.
        /// Otherwise a new instance of type <typeparamref name="T"/> constructed using <see cref="TypeConstruction"/> utility and then set on this build context.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <returns>The value or new instance of type <typeparamref name="T"/>.</returns>
        public T GetOrCreateValue<T>() where T : class
        {
            var value = GetValue<T>();
            if (value == null)
            {
                value = TypeConstruction.Construct<T>();
                SetValue(value);
            }
            return value;
        }

        /// <summary>
        /// Get value of type <typeparamref name="T"/> if found.
        /// Otherwise a new instance of type <typeparamref name="T"/> constructed using <see cref="TypeConstruction"/> utility.
        /// The build context is not modified.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <returns>The value or new instance of type <typeparamref name="T"/>.</returns>
        public T GetValueOrDefault<T>() where T : class => GetValue<T>() ?? TypeConstruction.Construct<T>();

        /// <summary>
        /// Set value of type <typeparamref name="T"/> to this build context.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="value">The value to set.</param>
        public void SetValue<T>(T value) where T : class
        {
            if (value == null)
            {
                return;
            }

            var type = value.GetType();
            if (type == typeof(object))
            {
                return;
            }

            m_Values[type] = value;
        }

        /// <summary>
        /// Set value of type <typeparamref name="T"/> to this build context to its default using <see cref="TypeConstruction"/> utility.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        public void SetValue<T>() where T : class => SetValue(TypeConstruction.Construct<T>());

        /// <summary>
        /// Remove value of type <typeparamref name="T"/> from this build context.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <returns><see langword="true"/> if the value was removed, otherwise <see langword="false"/>.</returns>
        public bool RemoveValue<T>() where T : class => m_Values.Remove(typeof(T));

        /// <summary>
        /// Provides a mechanism for releasing unmanaged resources.
        /// </summary>
        public virtual void Dispose() { }

        internal ContextBase() { }

        internal ContextBase(BuildPipeline pipeline, BuildConfiguration config)
        {
            BuildPipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
            BuildConfiguration = config ?? throw new ArgumentNullException(nameof(config));

            // Prevent the build pipeline/configuration asset from being destroyed during a build
            BuildPipeline.hideFlags |= HideFlags.DontUnloadUnusedAsset | HideFlags.HideAndDontSave;
            BuildConfiguration.hideFlags |= HideFlags.DontUnloadUnusedAsset | HideFlags.HideAndDontSave;
        }
    }
}
