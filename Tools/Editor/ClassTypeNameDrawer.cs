using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ClassTypeName))]
public class ClassTypeNameDrawer :PropertyDrawer
{
    //当前属性对应的实例
    protected ClassTypeName m_classTypeName;
    //子类完整命名空间名称
    protected List<string> m_names;
    //格式化后的子类名称列表
    protected List<string> m_formatedNames;
    protected bool m_initialized = false;

    protected virtual void Initialize()
    {
        m_classTypeName = (ClassTypeName)attribute;

        //获取当前AppDomain中所有程序集的所有类型，并筛选出指定类型的子类
        var classes = System.AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsSubclassOf(m_classTypeName.type));
        m_names = classes
            .Select(type => type.ToString())
            .ToList();
        m_formatedNames = classes
            .Select(type=>type.Name)
            .Select(name=>Regex.Replace(name,"(\\B[A-Z])", " $1"))
            .ToList();
    }

    /// <summary>
    /// 若属性为空字符串，默认选择列表中第一个子类
    /// </summary>
    /// <param name="property">SerializedProperty</param>
    protected virtual void InitializeProperty(SerializedProperty property)
    {
        if (property.stringValue.Length == 0)
        {
            property.stringValue = m_names[0];
        }
    }

    protected virtual void HandleGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (!m_names.Contains(property.stringValue)) return;
        
        var current=m_names.IndexOf(property.stringValue);
        //绘制前缀标签
        position = EditorGUI.PrefixLabel(position, label);
        //绘制下拉列表，选择返回的索引
        var selected = EditorGUI.Popup(position, current, m_formatedNames.ToArray());
        //更新属性值为选择的完整类型名字
        property.stringValue = m_names[selected];
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (!m_initialized)
        {
            m_initialized = true;
            Initialize();
        }

        if (m_names.Count > 0)
        {
            InitializeProperty(property);
            HandleGUI(position, property, label);
        }
    }
}
