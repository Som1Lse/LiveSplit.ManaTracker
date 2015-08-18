using System;
using System.Reflection;

using LiveSplit.Model;
using LiveSplit.UI.Components;

[assembly: ComponentFactory(typeof(LiveSplit.ManaTracker.ManaTrackerFactory))]

namespace LiveSplit.ManaTracker {

public class ManaTrackerFactory:IComponentFactory {
    public string ComponentName { get { return "ManaTracker"; } }
    public string Description { get { return "Keeps track of your mana and mana potions and displays a graph of their "+
                                             "usage (in Dishonored)."; } }
    public ComponentCategory Category { get{ return ComponentCategory.Media; } }

    public IComponent Create(LiveSplitState State){ return new ManaTrackerComponent(State); }
    
    public Version Version { get { return Assembly.GetExecutingAssembly().GetName().Version; } }

    public string UpdateName { get { return ComponentName; } }
    public string UpdateURL { get { return ""; } }
    public string XMLURL { get { return ""; } }
}

}
