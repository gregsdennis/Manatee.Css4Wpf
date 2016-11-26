using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Markup;

namespace Manatee.Css4Wpf
{
	[ContentProperty("Styles")]
	[MarkupExtensionReturnType(typeof(Style))]
	public class StyleExtension : StaticResourceExtension
	{
		private readonly string _styleNames;
		private Style _finalStyle;

		public StyleCollection Styles { get; }

		public StyleExtension(string styleNames)
		{
			if (styleNames == null) throw new ArgumentNullException(nameof(styleNames));

			_styleNames = styleNames;
		}
		public StyleExtension()
		{
			Styles = new StyleCollection();
		}

		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			if (_finalStyle == null)
			{
				// get the target of the extension from the IServiceProvider interface
				var ipvt = (IProvideValueTarget)serviceProvider.GetService(typeof(IProvideValueTarget));

				var targetObject = ipvt.TargetObject as FrameworkElement;
				if (targetObject == null)
					throw new InvalidOperationException("MultiStyle can only be applied to the Style property");

				var styles = _styleNames == null ? Styles : ResolveStyles(targetObject, _styleNames.Split(' '));
				_finalStyle = BuildStyle(styles);
			}
			return _finalStyle;
		}

		private static IEnumerable<Style> ResolveStyles(FrameworkElement element, IEnumerable<string> names)
		{
			var styles = new List<Style>();
			var styleNames = new List<string>(names);

			while (styleNames.Any() && element != null)
			{
				var toRemove = new List<string>();
				foreach (var styleName in styleNames)
				{
					var style = element.TryFindResource(styleName) as Style;
					if (style == null) continue;

					styles.Add(style);
					toRemove.Add(styleName);
				}
				foreach (var name in toRemove)
				{
					styleNames.Remove(name);
				}
				element = element.Parent as FrameworkElement;
			}

			if (styleNames.Any())
				throw new XamlParseException($"Cannot find resource named '{styleNames[0]}'. Resource names are case sensitive.");

			return styles;
		}

		private static Style BuildStyle(IEnumerable<Style> styles)
		{
			var style = new Style();

			foreach (var sourceStyle in styles)
			{
				foreach (var setter in sourceStyle.Setters)
				{
					style.Setters.Add(setter);
				}
				foreach (var trigger in sourceStyle.Triggers)
				{
					style.Triggers.Add(trigger);
				}
			}

			return style;
		}
	}
}
