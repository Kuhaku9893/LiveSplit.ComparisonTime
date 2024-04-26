using LiveSplit.Model;
using System;
using System.Reflection;

namespace LiveSplit.UI.Components
{
    public class ComparisonTimeFactory : IComponentFactory
    {
        public string ComponentName => "Comparison Time";

        public string Description => "Displays a comparison time.";

        public ComponentCategory Category => ComponentCategory.Information; 

        public IComponent Create(LiveSplitState state) => new ComparisonTime(state);

        public string UpdateName => ComponentName;

        public string XMLURL => "";

        public string UpdateURL => "";

        public Version Version => Assembly.GetExecutingAssembly().GetName().Version;
    }
}
