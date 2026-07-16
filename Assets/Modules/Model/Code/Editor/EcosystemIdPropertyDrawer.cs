using BeauUtil;
using BeauUtil.Editor;
using UnityEditor;
using UnityEngine;

namespace AIS.Model.Editor
{
    [CustomPropertyDrawer(typeof(EcosystemIdAttribute))]
    public class EcosystemIdPropertyDrawer : PropertyDrawer
    {
        private const string EcosystemArrayName = "Ecosystems";
        private const string EcosystemIdName = "EcosystemId";
        private const string NullName = "[None]";

        static private readonly NamedItemList<string> s_Items = new NamedItemList<string>();

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty hashProp = property.FindPropertyRelative("m_HashValue");
            SerializedProperty sourceProp = property.FindPropertyRelative("m_Source");

            if (hashProp == null || sourceProp == null)
            {
                EditorGUI.LabelField(position, label, new GUIContent("[EcosystemId] requires a SerializedHash32"));
                return;
            }

            label = EditorGUI.BeginProperty(position, label, property);
            EditorGUI.showMixedValue = hashProp.hasMultipleDifferentValues;

            SerializedProperty ecosystems = property.serializedObject.FindProperty(EcosystemArrayName);
            string current = sourceProp.stringValue;

            if (ecosystems == null || !ecosystems.isArray)
            {
                // no sibling ecosystem list to choose from - let the author type an id
                EditorGUI.BeginChangeCheck();
                string typed = EditorGUI.TextField(position, label, current);
                if (EditorGUI.EndChangeCheck())
                {
                    Write(hashProp, sourceProp, typed);
                }
            }
            else
            {
                FillItems(ecosystems, current);

                EditorGUI.BeginChangeCheck();
                string next = ListGUI.Popup(position, label, current, s_Items);
                if (EditorGUI.EndChangeCheck() && next != current)
                {
                    Write(hashProp, sourceProp, next);
                }
            }

            EditorGUI.showMixedValue = false;
            EditorGUI.EndProperty();
        }

        static private void FillItems(SerializedProperty ecosystems, string current)
        {
            s_Items.Clear();
            s_Items.Add(string.Empty, NullName, -1);

            bool foundCurrent = string.IsNullOrEmpty(current);

            for (int i = 0; i < ecosystems.arraySize; i++)
            {
                string id = ecosystems.GetArrayElementAtIndex(i)
                    .FindPropertyRelative(EcosystemIdName)
                    .FindPropertyRelative("m_Source").stringValue;

                if (string.IsNullOrEmpty(id))
                {
                    continue;
                }

                s_Items.Add(id, id);
                foundCurrent |= id == current;
            }

            if (!foundCurrent)
            {
                // keep the stale id visible and selectable instead of silently blanking the field
                s_Items.Add(current, current + " (missing)", -1);
            }
        }

        static private void Write(SerializedProperty hashProp, SerializedProperty sourceProp, string value)
        {
            hashProp.longValue = new StringHash32(value).HashValue;
            sourceProp.stringValue = value ?? string.Empty;
        }
    }
}
