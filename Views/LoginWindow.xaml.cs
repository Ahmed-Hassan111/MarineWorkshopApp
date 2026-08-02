using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MarineWorkshopApp.Views
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // تحقق بسيط من بيانات الدخول (يمكن ربطها بداتا بيز مستقبلاً)
            string username = TxtUsername.Text.Trim();
            string password = TxtPassword.Password.Trim();

            if (username == "admin" && (password == "admin" || password == "123456" || string.IsNullOrEmpty(password)))
            {
                // فتح الشاشة الرئيسية (Dashboard)
                MainWindow mainWin = new MainWindow();
                mainWin.Show();

                // إغلاق شاشة الدخول
                this.Close();
            }
            else
            {
                MessageBox.Show("اسم المستخدم أو كلمة المرور غير صحيحة!", "خطأ في الدخول", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
