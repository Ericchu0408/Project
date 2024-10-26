using System.Windows.Input;

namespace SafeDriver.Controls;

public partial class FrameSection : ContentView
{
    // �j�w�ݩʡG���D
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(FrameSection), default(string), propertyChanged: OnTitleChanged);

    // �j�w�ݩʡG�I���R�O�]�ϥ� ICommand�^
    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(FrameSection));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public FrameSection()
    {
        InitializeComponent(); // �T�O XAML ��l��
    }

    private static void OnTitleChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (FrameSection)bindable;
        if (control.TitleLabel != null) // Null �ˬd
        {
            control.TitleLabel.Text = newValue?.ToString();
        }
    }

    private void OnFrameSectionClicked(object sender, EventArgs e)
    {
        if (Command?.CanExecute(null) == true) // Null �ˬd�P CanExecute �P�_
        {
            Command.Execute(null);
        }
    }
}
