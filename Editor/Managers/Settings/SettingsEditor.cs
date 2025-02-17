﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using Devdog.General;
using Devdog.General.Editors;
using UnityEditor;
using UnityEngine;
using EditorStyles = Devdog.General.Editors.EditorStyles;
using EditorUtility = UnityEditor.EditorUtility;

namespace Devdog.QuestSystemPro.Editors
{
    public class SettingsEditor : EditorCrudBase<SettingsEditor.CategoryLookup>
    {
        public const string PrefabSaveKey = QuestSystemPro.ProductName + "_QuestSavePath";


        private SerializedObject _serializedObject;


        protected SettingsDatabase _settings;

        public SettingsEditor(string singleName, string pluralName, EditorWindow window)
            : base(singleName, pluralName, window)
        {
            canCreateItems = false;
            canDeleteItems = false;
            canReOrderItems = false;
            canDuplicateItems = false;
            hideCreateItem = true;
        }

        public SerializedObject serializedObject
        {
            get
            {
                if (_serializedObject == null)
                    _serializedObject = new SerializedObject(settings);

                return _serializedObject;
            }
        }

        protected SettingsDatabase settings
        {
            get
            {
                if (_settings == null)
                {
                    var manager = Resources.FindObjectsOfTypeAll<QuestManager>().FirstOrDefault();
                    if (manager != null) _settings = manager.settingsDatabase;
                }

                return _settings;
            }
        }

        protected override List<CategoryLookup> crudList
        {
            get
            {
                var list = new List<CategoryLookup>(8);
                if (settings != null)
                {
                    var fields = settings.GetType().GetFields();

                    CategoryLookup currentCategory = null;
                    foreach (var field in fields)
                    {
                        var cat = (CategoryAttribute)field.GetCustomAttributes(typeof(CategoryAttribute), true)
                            .FirstOrDefault();
                        if (cat != null)
                        {
                            // Got a category marker
                            currentCategory = new CategoryLookup(cat.category);
                            list.Add(currentCategory);
                        }

                        if (currentCategory != null)
                        {
                            var prop = serializedObject.FindProperty(field.Name);
                            if (prop != null)
                                currentCategory.serializedProperties.Add(prop);
                        }
                    }
                }

                return list;
            }
            set
            {
                // Doesn't do anything...
            }
        }

        protected override void CreateNewItem()
        {
        }

        public override void DuplicateItem(int index)
        {
        }

        protected override bool MatchesSearch(CategoryLookup category, string searchQuery)
        {
            var search = searchQuery.ToLower();
            return category.name.ToLower().Contains(search) ||
                   category.serializedProperties.Any(o => o.displayName.ToLower().Contains(search));
        }

        public override void Draw()
        {
            //InventoryEditorUtility.ErrorIfEmpty(EditorPrefs.GetString("InventorySystem_ItemPrefabPath") == string.Empty, "Inventory item prefab folder is not set, items cannot be saved! Click Set path to a set a save folder.");
            if (EditorPrefs.GetString(PrefabSaveKey) == string.Empty ||
                Directory.Exists(EditorPrefs.GetString(PrefabSaveKey)) == false)
                GUI.color = Color.red;

            EditorGUILayout.BeginHorizontal(EditorStyles.boxStyle);

            EditorGUILayout.LabelField("Quest system pro save folder: " + EditorPrefs.GetString(PrefabSaveKey));

            GUI.color = Color.white;
            if (GUILayout.Button("Set path", GUILayout.Width(100)))
            {
                var absolutePath = EditorUtility.SaveFolderPanel("Choose a folder to save your quests", "", "");
                EditorPrefs.SetString(PrefabSaveKey, "Assets" + absolutePath.Replace(Application.dataPath, ""));
            }

            EditorGUILayout.EndHorizontal();

            GUI.color = Color.white;


            base.Draw();
        }

        protected override void DrawSidebarRow(CategoryLookup category, int i)
        {
            BeginSidebarRow(category, i);

            DrawSidebarRowElement(category.name, 400);

            EndSidebarRow(category, i);
        }

        protected override void DrawDetail(CategoryLookup category, int index)
        {
            EditorGUILayout.BeginVertical(EditorStyles.boxStyle);
            EditorGUIUtility.labelWidth = EditorStyles.labelWidth;


            serializedObject.Update();
            foreach (var setting in category.serializedProperties) EditorGUILayout.PropertyField(setting, true);
            serializedObject.ApplyModifiedProperties();


            EditorGUIUtility.labelWidth = 0; // Resets it to the default
            EditorGUILayout.EndVertical();
        }

        protected override bool IDsOutOfSync()
        {
            return false;
        }

        protected override void SyncIDs()
        {
        }

        public class CategoryLookup
        {
            public List<SerializedProperty> serializedProperties = new(8);


            public CategoryLookup()
            {
            }

            public CategoryLookup(string name)
            {
                this.name = name;
            }

            public string name { get; set; }

            public override bool Equals(object obj)
            {
                var o = obj as CategoryLookup;
                return o != null && o.name == name;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }
    }
}