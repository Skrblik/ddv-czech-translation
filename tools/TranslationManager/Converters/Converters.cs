using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using TranslationManager.Models;

namespace TranslationManager.Converters;

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is Visibility.Visible;
}

public class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is Visibility.Collapsed;
}

public class SourceToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TranslationSource source)
        {
            return source switch
            {
                TranslationSource.PreExisting => new SolidColorBrush(Color.FromRgb(255, 193, 7)),     // amber
                TranslationSource.OurTranslation => new SolidColorBrush(Color.FromRgb(76, 175, 80)),  // green
                TranslationSource.Untranslated => new SolidColorBrush(Color.FromRgb(244, 67, 54)),    // red
                TranslationSource.DoNotTranslate => new SolidColorBrush(Color.FromRgb(158, 158, 158)),// gray
                TranslationSource.Improved => new SolidColorBrush(Color.FromRgb(33, 150, 243)),       // blue
                _ => new SolidColorBrush(Colors.White)
            };
        }
        return new SolidColorBrush(Colors.White);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class SourceToLabelConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TranslationSource source)
        {
            return source switch
            {
                TranslationSource.PreExisting => "Původní",
                TranslationSource.OurTranslation => "Náš překlad",
                TranslationSource.Untranslated => "Nepřeloženo",
                TranslationSource.DoNotTranslate => "Nepřekládat",
                TranslationSource.Improved => "Vylepšeno",
                _ => "?"
            };
        }
        return "?";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class IsModifiedToBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is true)
            return new SolidColorBrush(Color.FromArgb(30, 33, 150, 243));
        return new SolidColorBrush(Colors.Transparent);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
