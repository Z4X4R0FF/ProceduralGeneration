using System;
using Unity.Properties.Editor;
using Unity.Properties.UI;
using UnityEditor;
using UnityEditor.Searcher;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Build.Editor
{
    /// <summary>
    /// Base inspector class for <see cref="Type"/> searcher field.
    /// </summary>
    /// <typeparam name="T">Type to populate the searcher with.</typeparam>
    public abstract class TypeInspector<T> : Inspector<T>
    {
        TextElement m_Text;

        /// <summary>
        /// The title displayed on the searcher window.
        /// </summary>
        public virtual string SearcherTitle => $"Select {typeof(T).Name}";

        /// <summary>
        /// A function that returns whether or not the type should be filtered.
        /// </summary>
        public virtual Func<Type, bool> TypeFilter { get; }

        /// <summary>
        /// A function that returns the display name of the type.
        /// </summary>
        public virtual Func<Type, string> TypeNameResolver { get; }

        /// <summary>
        /// A function that returns the display category name of the type.
        /// </summary>
        public virtual Func<Type, string> TypeCategoryResolver { get; }

        public override VisualElement Build()
        {
            var typeField = Assets.LoadVisualTreeAsset(nameof(TypeInspector<T>)).CloneTree();

            var label = typeField.Q<Label>("label");
            label.text = DisplayName;

            var input = typeField.Q<VisualElement>("input");
            input.RegisterCallback<MouseUpEvent>(mouseUpEvent =>
            {
                var database = TypeSearcherDatabase.Populate<T>(TypeFilter, TypeNameResolver, TypeCategoryResolver);
                var searcher = new Searcher(database, new AddTypeSearcherAdapter(SearcherTitle));
                var position = input.worldBound.min + Vector2.up * (input.worldBound.height + 19f);
                var alignment = new SearcherWindow.Alignment(SearcherWindow.Alignment.Vertical.Top, SearcherWindow.Alignment.Horizontal.Left);
                SearcherWindow.Show(EditorWindow.focusedWindow, searcher, OnTypeSelected, position, null);
            });

            var type = Target?.GetType();
            if (type != null)
            {
                m_Text = typeField.Q<TextElement>("text");
                m_Text.text = TypeNameResolver?.Invoke(type) ?? type.Name;
            }

            return typeField;
        }

        public override void Update()
        {
            var type = Target?.GetType();
            if (type != null)
            {
                var text = TypeNameResolver?.Invoke(type) ?? type.Name;
                if (m_Text.text != text)
                {
                    m_Text.text = TypeNameResolver?.Invoke(type) ?? type.Name;
                    NotifyChanged();
                }
            }
        }

        bool OnTypeSelected(SearcherItem item)
        {
            if (item is TypeSearcherItem typeItem && TypeConstruction.TryConstruct<T>(typeItem.Type, out var instance))
            {
                Target = instance;
                return true;
            }
            return false;
        }
    }
}
