using UnityEngine;
using System.Linq;
using UnityEditor;
using UnityEngine.Profiling;
using System.Collections.Generic;

namespace PrefabEvolution
{
	static class PEModificationsExt
	{
		private class PropertyCouple
		{
			public SerializedProperty prefabProperty;
			public SerializedProperty objectProperty;
		}

		static internal void GetProperties(this PEModifications.PropertyData _this, out SerializedProperty prefabProperty, out SerializedProperty objectProperty, PEPrefabScript script)
		{
			var couple = _this.UserData as PropertyCouple;
			if (couple == null)
			{
				couple = new PropertyCouple();
				if (couple.objectProperty == null)
				{
					var so = new SerializedObject(_this.Object);

					if (so != null)
						couple.objectProperty = so.FindProperty(_this.PropertyPath);

					if (couple.objectProperty == null)
					{
						if (PEPrefs.DebugLevel > 0)
							Debug.Log(string.Format("Property {0} not found on Object {1}", _this.PropertyPath, _this.Object));
					}
				}

				if (couple.prefabProperty == null)
				{
					var prefabObject = script.Links.GetPrefabObject(script.GetDiffWith().gameObject, _this.Object);

					if (prefabObject != null)
					{
						var so = new SerializedObject(prefabObject);
						if (so != null)
							couple.prefabProperty = so.FindProperty(_this.PropertyPath);
					}
					else
					{
						Debug.LogWarning("Prefab object for prefab property modifications is not found");
					}
				}
			}

			prefabProperty = couple.prefabProperty;
			objectProperty = couple.objectProperty;
			_this.UserData = couple;
		}

		
		static private bool CheckChild(SerializedProperty property)
		{
			var ptype = property.propertyType;
			return !(ptype == SerializedPropertyType.String ||
					ptype == SerializedPropertyType.Color ||
					ptype == SerializedPropertyType.Vector2 ||
					ptype == SerializedPropertyType.Vector3 ||
					ptype == SerializedPropertyType.Quaternion ||
					ptype == SerializedPropertyType.ObjectReference ||
					false);
		}

		static internal SerializedProperty FindPropertyFast(this SerializedProperty property, string path)
		{
			var i = 1000;
			var resetAndRestart = true;
			var restart = false;
			do
			{
				while (i-- > 0)
				{
					restart = false;
					Profiler.BeginSample("Next");
					var next = property.Next(CheckChild(property));
					Profiler.EndSample();
					if (!next)
					{
						break;
					}

					Profiler.BeginSample("GetPropertyPath");
					var propertyPath = property.propertyPath;
					Profiler.EndSample();
					if (propertyPath == path)
					{
						return property;
					}
				}
				if (resetAndRestart)
				{
					resetAndRestart = false;
					restart = true;
					
				}

				Profiler.BeginSample("PropertyReset");
				property.Reset();
				Profiler.EndSample();
			} while (restart);
			return null;
		}

		static private Dictionary<Object, SerializedProperty> serializedObjectsCache = new Dictionary<Object, SerializedProperty>();
		static private SerializedProperty GetPropertyIterator(Object obj)
		{
			Profiler.BeginSample("GetPropertyIterator");
			SerializedProperty result;
			if (!serializedObjectsCache.TryGetValue(obj, out result))
			{
				serializedObjectsCache[obj] = result = new SerializedObject(obj).GetIterator();
			}
			else
			{
				result.serializedObject.Update();
				result.Reset();
			}
			Profiler.EndSample();
			return result;
		}

		static internal void CalculateModifications(this PEModifications _this, PEPrefabScript prefab, PEPrefabScript instance)
		{
			Profiler.BeginSample("CalculateModifications");

			Profiler.BeginSample("RemoveAll");
			instance.Modifications.Modificated.RemoveAll(m => m.Mode == PEModifications.PropertyData.PropertyMode.Default);
			Profiler.EndSample();
			var counter = 0;
			foreach (var link in instance.Links.Links)
			{
				if (link == null || link.InstanceTarget == null || link.InstanceTarget == instance || link.InstanceTarget is PEPrefabScript)
					continue;

				var prefabObjectLink = prefab.Links[link];
				if (prefabObjectLink == null)
					continue;

				var prefabObject = prefabObjectLink.InstanceTarget;

				if (prefabObject == null)
					continue;

				var prefabRootProperty = GetPropertyIterator(prefabObject);

				var property = GetPropertyIterator(link.InstanceTarget);

				while (true)
				{
					Profiler.BeginSample("MoveIn");

					var moveIn = CheckChild(property);
					Profiler.EndSample();

					Profiler.BeginSample("Next");

					var hasNext = property.Next(moveIn);
					Profiler.EndSample();
					if (!hasNext)
						break;
					counter++;
					if (PEUtils.PropertyFilter(property))
					{
						continue;
					}

					string propertyPath = property.propertyPath;


					Profiler.BeginSample("Property Lookup");
					var prefabProperty = prefabRootProperty.FindPropertyFast(propertyPath);
					Profiler.EndSample();

					var isArray = propertyPath.Contains(".Array.data[");
					var isInherited = link.InstanceTarget.GetType().IsSubclassOf(prefabObject.GetType());
					if (prefabProperty == null && !isArray && !isInherited)
					{
						if (PEPrefs.DebugLevel > 0)
							Debug.Log("Property not found(Some times its happens) " + propertyPath);
						continue;
					}
					Profiler.BeginSample("Equality check");
					

					var prefabValue = prefabProperty == null ? null : prefabProperty.GetPropertyValue();

					var instanceValue = property.GetPropertyValue();
					var isChanged = !object.Equals(instanceValue, prefabValue);
					Profiler.EndSample();

					Profiler.BeginSample("Compare");

					if (isChanged)
					{
						if (property.propertyType == SerializedPropertyType.ObjectReference)
						{
							var instanceLink = instance.Links[instanceValue as Object];
							var prefabLink = prefab.Links[prefabValue as Object];

							if (prefabLink != null && instanceLink != null)
								isChanged = prefabLink.LIIF != instanceLink.LIIF;
						}
						else
						{
							var animationCurve = instanceValue as AnimationCurve;
							if (animationCurve != null)
							{
								isChanged = !PEUtils.Compare(animationCurve, prefabValue as AnimationCurve);
							}
						}
					}
					Profiler.EndSample();

					if (!isChanged)
						continue;

					Profiler.BeginSample("AddModification");

					instance.Modifications.AddModification(new PEModifications.PropertyData
					{
						Object = link.InstanceTarget,
						PropertyPath = propertyPath,
						ObjeckLink = link.LIIF,
					});
					Profiler.EndSample();
				}
			}

			Profiler.BeginSample("CalculateStructureDiff");
			instance.Modifications.CalculateStructureDiff(prefab, instance);
			Profiler.EndSample();
			Profiler.EndSample();
		}

		static private void AddModification(this PEModifications _this, PEModifications.PropertyData data)
		{
			foreach (var mod in _this.Modificated)
				if (mod.PropertyPath == data.PropertyPath && mod.Object == data.Object && mod.ObjeckLink == data.ObjeckLink)
					return;

			_this.Modificated.Add(data);
		}

		static private void CalculateStructureDiff(this PEModifications _this, PEPrefabScript prefab, PEPrefabScript instance)
		{
			_this.NonPrefabObjects.Clear();
			var hierarchy = EditorUtility.CollectDeepHierarchy(new[] { instance });
			foreach (var transform in hierarchy.OfType<Transform>())
			{
				if (transform.parent == null)
					continue;

				var link = prefab.Links[instance.Links[transform]];
				if (link != null)
					continue;

				_this.NonPrefabObjects.Add(new PEModifications.HierarchyData {
					child = transform,
					parent = transform.parent
				});
			}

			_this.NonPrefabComponents.Clear();
			foreach (var component in hierarchy.Where(obj => !(obj is Transform)).OfType<Component>())
			{
				var link = prefab.Links[instance.Links[component]];
				if (link != null || prefab.Links[instance.Links[component.gameObject.transform]] == null)
					continue;

				_this.NonPrefabComponents.Add(new PEModifications.ComponentsData {
					child = component,
					parent = component.gameObject
				});
			}

			_this.RemovedObjects.Clear();
			foreach (var link in prefab.Links.Links)
			{
				if (link.InstanceTarget is Transform)
					continue;

				if (instance.Links[link] == null || instance.Links[link].InstanceTarget == null)
					_this.RemovedObjects.Add(link.LIIF);
			}

			_this.TransformParentChanges.Clear();
			foreach (var link in instance.Links.Links)
			{
				var transform = link.InstanceTarget as Transform;
				if (transform == null)
					continue;

				var currentTransform = transform;
				if (currentTransform == instance.transform)
					continue;

				var currentTransformParent = currentTransform.parent;
				if (prefab.Links[link] == null)
					continue;

				var otherTransform = prefab.Links[link].InstanceTarget as Transform;

				var otherTransformParent = otherTransform.parent;

				if (prefab.Links[otherTransformParent] == null || instance.Links[currentTransformParent] == null
				    || prefab.Links[otherTransformParent].LIIF
				    != instance.Links[currentTransformParent].LIIF)
					_this.TransformParentChanges.Add(new PEModifications.HierarchyData {
						child = currentTransform,
						parent = currentTransformParent
					});
			}
		}
	}
}