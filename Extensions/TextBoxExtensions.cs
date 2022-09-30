using Avalonia.Controls;

namespace EELauncher.Extensions; 

public static class TextBoxExtensions {
    public static void AddPlaceholder(this TextBox textBox, string placeholder, bool isPassword) {
        if (textBox.Text?.Trim() != "") return;
        if (isPassword) textBox.PasswordChar = char.MinValue;
            
        textBox.Text = placeholder;
    }
    
    public static void RemovePlaceholder(this TextBox textBox, string placeholder, bool isPassword) {
        if (textBox.Text != placeholder) return;
        textBox.Text = "";
            
        if (isPassword) textBox.PasswordChar = '*';
    }

    public static void ToggleVisible(this TextBox textBox, char passChar) =>
        textBox.PasswordChar = textBox.PasswordChar == passChar ? char.MinValue : passChar;
}
