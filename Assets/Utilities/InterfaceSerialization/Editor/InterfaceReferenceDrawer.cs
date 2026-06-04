using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Object = UnityEngine.Object;

namespace Utilities.InterfaceSerialization.EditorHandling
{
#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(InterfaceReference<>))]
    [CustomPropertyDrawer(typeof(InterfaceReference<,>))]
    public class InterfaceReferenceDrawer : PropertyDrawer
    {
        private const string UnderlyingValueFieldName = "underlyingValue";

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty underlyingValueProp = property.FindPropertyRelative(UnderlyingValueFieldName);
            var args = GetArguments(fieldInfo);

            EditorGUI.BeginProperty(position, label, property);

            var assignedObject = EditorGUI.ObjectField(position, label, underlyingValueProp.objectReferenceValue, args.ObjectType, true);

            if (assignedObject != null)
            {
                Object component = null;

                if (assignedObject is GameObject gameObject)
                {
                    component = gameObject.GetComponent(args.InterfaceType);
                }
                else if (args.InterfaceType.IsAssignableFrom(assignedObject.GetType()))
                {
                    component = assignedObject;
                }

                if (component != null)
                {
                    ValidateAndAssignObject(underlyingValueProp, component, component.name, args.InterfaceType.Name);
                }
                else
                {
                    Debug.LogWarning($"Assigned object does not implement required interface '{args.InterfaceType.Name}'.");
                    underlyingValueProp.objectReferenceValue = null;
                }
            }
            else
            {
                underlyingValueProp.objectReferenceValue = null;
            }

            EditorGUI.EndProperty();
            InterfaceReferenceUtil.OnGUI(position, underlyingValueProp, label, args);
        }

        static InterfaceArgs GetArguments(FieldInfo fieldInfo)
        {
            Type objectType = null;
            Type interfaceType = null;

            Type fieldType = fieldInfo.FieldType;

            bool TryGetTypesFromInterfaceReference(Type type, out Type objectType,  out Type interfaceType)
            {
                objectType = interfaceType = null;

                if (type?.IsGenericType != true)
                {
                    return false;
                }

                var genericType = type.GetGenericTypeDefinition();

                if (genericType == typeof(InterfaceReference<>))
                {
                    type = type.BaseType;
                }

                if(type?.GetGenericTypeDefinition() == typeof(InterfaceReference<,>))
                {
                    var types = type.GetGenericArguments();
                    interfaceType = types[0];
                    objectType = types[1];

                    return true;
                }
                return false;
            }
        
            void GetTypesFromList(Type type, out Type objectType, out Type interfaceType)
            {
                objectType = interfaceType = null;

                var listInterface = type.GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IList<>));

                if(listInterface != null)
                {
                    var elementType = listInterface.GetGenericArguments()[0];
                    TryGetTypesFromInterfaceReference(elementType, out objectType, out interfaceType);
                }
            }
        
            if(!TryGetTypesFromInterfaceReference(fieldType, out  objectType, out interfaceType))
            {
                GetTypesFromList(fieldType, out objectType, out interfaceType);
            }

            return new InterfaceArgs(objectType, interfaceType);
        }
    
        static void ValidateAndAssignObject(SerializedProperty property, Object targetObject, string componentNameOrType, string interfaceName = null)
        {
            if(targetObject != null)
            {
                property.objectReferenceValue = targetObject;
            }
            else
            {
                Debug.LogWarning(
                    @$"The {(interfaceName != null ? 
                             $"GameObject '{componentNameOrType}'" : 
                             $"assigned object")} does not have a component that implements '{componentNameOrType}'"
                );

                property.objectReferenceValue = null;
            }
        }
    }

    public struct InterfaceArgs
    {
        public readonly Type ObjectType;
        public readonly Type InterfaceType;

        public InterfaceArgs(Type objectType, Type interfaceType)
        {
            Debug.Assert(typeof(Object).IsAssignableFrom(objectType), $"{nameof(objectType)}({objectType}) needs to be of type {typeof(Object)}.");
            Debug.Assert(interfaceType.IsInterface, $"{nameof(interfaceType)} needs to be an interface");
            ObjectType = objectType;
            InterfaceType = interfaceType;
        }
    }
    #endif
}
