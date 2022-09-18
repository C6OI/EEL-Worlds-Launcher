using Avalonia.Controls;

namespace EELauncher.Extensions; 

public static class TextBoxExtensions {
    public static void AddPlaceholder(this TextBox textBox, string placeholder, bool isPassword) {
        if (textBox.Text?.Trim() == "") {
            if (isPassword) textBox.PasswordChar = char.MinValue;
            
            textBox.Text = placeholder;
        } 
    }
    
    public static void RemovePlaceholder(this TextBox textBox, string placeholder, bool isPassword) {
        if (textBox.Text == placeholder) {
            textBox.Text = "";
            
            if (isPassword) textBox.PasswordChar = '*';
        }
    }
}
