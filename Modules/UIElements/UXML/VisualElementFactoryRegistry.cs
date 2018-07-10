// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using System;
using System.Collections.Generic;
using System.Reflection;

namespace UnityEngine.Experimental.UIElements
{
    static class VisualElementFactoryRegistry
    {
        internal static Dictionary<string, List<IUxmlFactory>> factories { get; private set; }

        internal static void RegisterFactory(IUxmlFactory factory)
        {
            DiscoverFactories();
            List<IUxmlFactory> factoryList;
            if (factories.TryGetValue(factory.uxmlQualifiedName, out factoryList))
            {
                foreach (IUxmlFactory f in factoryList)
                {
                    if (f.GetType() == factory.GetType())
                    {
                        throw new ArgumentException("A factory of this type was already registered");
                    }
                }
                factoryList.Add(factory);
            }
            else
            {
                factoryList = new List<IUxmlFactory>();
                factoryList.Add(factory);
                factories.Add(factory.uxmlQualifiedName, factoryList);
            }
        }

        internal static void DiscoverFactories()
        {
            if (factories != null)
                return;

            factories = new Dictionary<string, List<IUxmlFactory>>();
            RegisterEngineFactories();

            AppDomain currentDomain = AppDomain.CurrentDomain;
            HashSet<string> userAssemblies = new HashSet<string>(ScriptingRuntime.GetAllUserAssemblies());
            foreach (Assembly assembly in currentDomain.GetAssemblies())
            {
                if (!userAssemblies.Contains(assembly.GetName().Name + ".dll"))
                    continue;

                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (typeof(IUxmlFactory).IsAssignableFrom(type))
                        {
                            var factory = (IUxmlFactory)Activator.CreateInstance(type);
                            RegisterFactory(factory);
                        }
                    }
                }
                catch (TypeLoadException e)
                {
                    Debug.LogWarningFormat("Error while loading types from assembly {0}: {1}", assembly.FullName, e);
                }
            }
        }

        internal static bool TryGetValue(string fullTypeName, out List<IUxmlFactory> factoryList)
        {
            DiscoverFactories();

            factoryList = null;
            return factories != null && factories.TryGetValue(fullTypeName, out factoryList);
        }

        static void RegisterEngineFactories()
        {
            IUxmlFactory[] factories =
            {
                new UxmlRootElementFactory(),
                new Button.UxmlFactory(),
                new VisualElement.UxmlFactory(),
                new IMGUIContainer.UxmlFactory(),
                new Image.UxmlFactory(),
                new Label.UxmlFactory(),
                new RepeatButton.UxmlFactory(),
                new ScrollerButton.UxmlFactory(),
                new ScrollView.UxmlFactory(),
                new Scroller.UxmlFactory(),
                new Slider.UxmlFactory(),
                new SliderInt.UxmlFactory(),
                new MinMaxSlider.UxmlFactory(),
                new TextField.UxmlFactory(),
                new Toggle.UxmlFactory(),
                new VisualContainer.UxmlFactory(),
                new TemplateContainer.UxmlFactory(),
                new Box.UxmlFactory(),
                new PopupWindow.UxmlFactory(),
                new ListView.UxmlFactory(),
            };

            foreach (var factory in factories)
            {
                RegisterFactory(factory);
            }
        }
    }
}
