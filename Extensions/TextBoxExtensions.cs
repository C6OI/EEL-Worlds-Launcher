using Avalonia.Controls;

namespace EELauncher.Extensions; 

public static class TextBoxExtensions {
    public static void AddPlaceholder(this TextBox textBox, string placeholder) {
        if (textBox.Text?.Trim() == "") {
            textBox.Text = placeholder;
        } 
    }
    
    public static void RemovePlaceholder(this TextBox textBox, string placeholder) {
        if (textBox.Text == placeholder) {
            textBox.Text = "";
        }
    }
}
