using System.Windows;
using System.Windows.Controls;

namespace BeerZdec.Helpers
{
    public static class PasswordBoxHelper
    {
        // 1. Регистрируем свойство.
        // FrameworkPropertyMetadataOptions.BindsTwoWayByDefault оставляем,
        // это удобно, чтобы не писать Mode=TwoWay в XAML каждый раз.
        public static readonly DependencyProperty BoundPasswordProperty =
            DependencyProperty.RegisterAttached(
                "BoundPassword",
                typeof(string),
                typeof(PasswordBoxHelper),
                new FrameworkPropertyMetadata(
                    string.Empty,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnBoundPasswordChanged));

        public static string GetBoundPassword(DependencyObject dp)
            => (string)dp.GetValue(BoundPasswordProperty);

        public static void SetBoundPassword(DependencyObject dp, string value)
            => dp.SetValue(BoundPasswordProperty, value);

        // Флаг-защита от рекурсии
        public static readonly DependencyProperty IsUpdatingProperty =
            DependencyProperty.RegisterAttached(
                "IsUpdating",
                typeof(bool),
                typeof(PasswordBoxHelper),
                new PropertyMetadata(false));

        // 2. Вызывается, когда меняется значение свойства (из ViewModel)
        private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var box = d as PasswordBox;
            if (box == null) return;

            // ️ ВАЖНО: Отписываемся от события перед изменением пароля.
            // Если этого не сделать, установка box.Password вызовет PasswordChanged,
            // и мы получим бесконечный цикл или обрезание данных.
            box.PasswordChanged -= HandlePasswordChanged;

            // Если это обновление НЕ от нас (от пользователя), то синхронизируем UI
            if (!(bool)box.GetValue(IsUpdatingProperty))
            {
                box.Password = (string)e.NewValue;
            }

            // Подписываемся обратно
            box.PasswordChanged += HandlePasswordChanged;
        }

        // 3. Вызывается, когда пользователь печатает (событие UI)
        private static void HandlePasswordChanged(object sender, RoutedEventArgs e)
        {
            var box = sender as PasswordBox;
            if (box == null) return;

            // Поднимаем флаг: "Я сейчас обновляю свойство, коллбэк не дергайся"
            box.SetValue(IsUpdatingProperty, true);

            // Обновляем свойство привязки (уходит в ViewModel)
            SetBoundPassword(box, box.Password);

            // Опускаем флаг
            box.SetValue(IsUpdatingProperty, false);
        }
    }
}